using Microsoft.Extensions.Logging;
using RemoteCodeLoader.Code;
using RemoteCodeLoader.Models;
using RemoteCodeLoader.Shared;
using RemoteCodeLoader.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RemoteCodeLoader
{
    public class RemoteCodeLoader
    {
        private WebSocket _websocket;
        public RemoteCodeLoader(WebSocket connectedWebsocket)
        {
            _websocket = connectedWebsocket;
        }

        private async Task<T> ReadMessage<T>() where T : struct
        {
            var frameBuffer = new byte[8192];

            var receiveResult = await _websocket.ReceiveAsync(frameBuffer.AsMemory(), CancellationToken.None);
            int totalSize = receiveResult.Count;
            while (!receiveResult.EndOfMessage && !_websocket.CloseStatus.HasValue)
            {
                Array.Resize(ref frameBuffer, frameBuffer.Length * 2);
                receiveResult = await _websocket.ReceiveAsync(frameBuffer.AsMemory(totalSize..), CancellationToken.None);
                totalSize += receiveResult.Count;
            }

            return new ReadOnlySpan<byte>(frameBuffer, 0, totalSize).ToStruct<T>(Encoding.UTF8);
        }

        private async Task<bool> RequestResource(string resource)
        {
            var requestMessage = new RequestMessage()
            {
                RequestedResource = resource
            };
            var messageJson = requestMessage.ToJsonBytes(Encoding.UTF8);
            await _websocket.SendAsync(messageJson, WebSocketMessageType.Binary, true, CancellationToken.None);

            var response = await ReadMessage<ResponseMessage>();
            return response.Accepted;
        }

        public void AddLocalAssembly(Type type)
        {
            CodeAssembler.AddReference(type.Assembly.Location);
        }

        private async Task<LoadedCodeBoundle<T>?> ReadRemoteCode<T>(string requestedResource) where T : Element
        {
            var loadedBundle = new LoadedCodeBoundle<T>();

            var requestAccepted = await RequestResource(requestedResource);

            if (!requestAccepted)
                return null;

            var xaml = await ReadMessage<XamlMessage>();

            var elementInstance = (Element?)Activator.CreateInstance(typeof(T));
            if (elementInstance == null)
                return null;

            loadedBundle.LoadedXaml = (T)elementInstance.LoadFromXaml(xaml.Xaml);

            var codeBehindAssemblyHeader = await ReadMessage<AssemblyHeaderMessage>();

            for (int i = 0; i < codeBehindAssemblyHeader.ReferenceCount; i++)
            {
                var referenceMessage = await ReadMessage<ReferenceMessage>();
                CodeAssembler.AddReference(referenceMessage.Payload);
            }

            loadedBundle.XamlClassName = codeBehindAssemblyHeader.XamlClassName;
            loadedBundle.ViewModelClassName = codeBehindAssemblyHeader.ViewModelClassName;

            var sourceCodeMessage = await ReadMessage<SourceCodeMessage>();

            loadedBundle.XamlCodeAssembly = CodeAssembler.Compile(sourceCodeMessage.XamlSourceCode);
            loadedBundle.XamlViewModelAssembly = CodeAssembler.Compile(sourceCodeMessage.XamlViewModelCode);

            return loadedBundle;
        }

        public async Task<T?> CreateXamlElement<T>(string requestedResource, IServiceProvider serviceProvider) where T : Element
        {
            var bundle = await ReadRemoteCode<T>(requestedResource);

            if (bundle == null)
                return null;

            if (bundle.XamlCodeAssembly == null)
                return null;

            dynamic? hookInstance = CreateInstance(bundle.XamlCodeAssembly.GetType(bundle.XamlClassName)!, serviceProvider);
            var viewModelInstance = CreateInstance(bundle.XamlViewModelAssembly.GetType(bundle.ViewModelClassName)!, serviceProvider);

            if (hookInstance == null)
                return null;

            hookInstance.RegisterXaml(bundle.LoadedXaml, viewModelInstance);
 
            bundle.LoadedXaml.BindingContext = viewModelInstance;
            return bundle.LoadedXaml;
        }


        private static object? CreateInstance(Type type, IServiceProvider serviceProvider)
        {
            var primaryCtor = type.GetConstructors()[0];
            var ctorParams = primaryCtor.GetParameters();

            List<object?> parameters = [];

            foreach (var param in ctorParams)
            {
                var paramType = param.ParameterType;
                var service = serviceProvider.GetService(paramType);
                parameters.Add(service);
            }

            if (parameters.Count > 0)
                return Activator.CreateInstance(type, parameters.ToArray());
            else return Activator.CreateInstance(type);
        }

    }
}
