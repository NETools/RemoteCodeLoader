using RemoteCodeLoader.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteCodeLoader.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RemoteSourceCode : Attribute
    {
        public string Id { get; private set; }
        public string XamlPath { get; private set; }
        public RemoteCodeType RemoteCodeType { get; private set; }
        public RemoteSourceCode(string id, string xamlPath, RemoteCodeType remoteCodeType)
        {
            Id = id;
            XamlPath = xamlPath;
            RemoteCodeType = remoteCodeType;
        }
        
    }
}
