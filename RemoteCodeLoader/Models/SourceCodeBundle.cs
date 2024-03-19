using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteCodeLoader.Models
{
    public class SourceCodeBundle
    {
        public string Xaml { get; set; }
        public SourceCode SourceCode { get; set; } = new SourceCode();
        public List<byte[]> Assemblies { get; set; } = new List<byte[]>();
    }
}
