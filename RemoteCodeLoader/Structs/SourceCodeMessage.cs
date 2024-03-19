using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RemoteCodeLoader.Structs
{
    public struct SourceCodeMessage
    {
        [JsonInclude]
        public string XamlViewModelCode;
        [JsonInclude]
        public string XamlSourceCode;
    }
}
