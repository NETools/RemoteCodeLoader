using RemoteCodeLoader.Models;
using RemoteCodeLoader.Shared;
using RemoteCodeLoader.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace RemoteCodeLoader
{
    public class RemoteCodeWriter(WebSocket websocket, Dictionary<string, SourceCodeBundle> resources)
    {
        private async Task<T> ReadMessage<T>() where T : struct
        {
            var frameBuffer = new byte[8192];

            var receiveResult = await websocket.ReceiveAsync(frameBuffer.AsMemory(), CancellationToken.None);
            int totalSize = receiveResult.Count;
            while (!receiveResult.EndOfMessage && !websocket.CloseStatus.HasValue)
            {
                Array.Resize(ref frameBuffer, frameBuffer.Length * 2);
                receiveResult = await websocket.ReceiveAsync(frameBuffer.AsMemory(totalSize..), CancellationToken.None);
                totalSize += receiveResult.Count;
            }

            return new ReadOnlySpan<byte>(frameBuffer, 0, totalSize).ToStruct<T>(Encoding.UTF8);
        }

        public async Task HandleRequests()
        {
            while (true)
            {
                var resourceRequest = await ReadMessage<RequestMessage>();

                if (resources.ContainsKey(resourceRequest.RequestedResource))
                {
                    await Send(new ResponseMessage()
                    {
                        Accepted = true,
                        Resource = resourceRequest.RequestedResource
                    });
                }
                else
                {
                    await Send(new ResponseMessage()
                    {
                        Accepted = false,
                        Resource = $"{resourceRequest.RequestedResource} does not exist on this server."
                    });

                    continue;
                }

                await SendCode(resourceRequest.RequestedResource);
            }
        }

        private async Task Send<T>(T message) where T : struct
        {
            var messageBuffer = message.ToJsonBytes(Encoding.UTF8);
            await websocket.SendAsync(messageBuffer, WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        private async Task SendCode(string resource)
        {
            var xaml = new XamlMessage()
            {
               Xaml = resources[resource].Xaml,
            };

            await Send(xaml);

            var hookHeader = new AssemblyHeaderMessage()
            {
                XamlClassName = resources[resource].SourceCode.XamlCode.ClassName,
                ViewModelClassName = resources[resource].SourceCode.ViewModelCode.ClassName,
                ReferenceCount = resources[resource].Assemblies.Count
            };

            await Send(hookHeader);

            foreach (var assembly in resources[resource].Assemblies)
            {
                var reference = new ReferenceMessage()
                {
                    Payload = assembly
                };

                await Send(reference);
            }

            var sourceCodeMessage = new SourceCodeMessage()
            {
                XamlSourceCode = resources[resource].SourceCode.XamlCode.Source,
                XamlViewModelCode = resources[resource].SourceCode.ViewModelCode.Source
            };

            await Send(sourceCodeMessage);
        }


    }
}
