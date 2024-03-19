using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RemoteCodeLoader.Structs
{
    public struct AssemblyHeaderMessage
    {
        [JsonInclude]
        public string XamlClassName;
        [JsonInclude]
        public string ViewModelClassName;
        [JsonInclude]
        public int ReferenceCount;
    }
}
