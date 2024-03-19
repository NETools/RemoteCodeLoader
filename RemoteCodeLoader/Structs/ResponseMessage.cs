using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RemoteCodeLoader.Structs
{
    public struct ResponseMessage
    {
        [JsonInclude]
        public bool Accepted;

        [JsonInclude]
        public string Resource;
    }
}
