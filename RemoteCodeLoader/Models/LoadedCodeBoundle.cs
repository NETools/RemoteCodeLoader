using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RemoteCodeLoader.Models
{
    public class LoadedCodeBoundle<T> where T : Element
    {
        public T LoadedXaml { get; set; }

        public string XamlClassName { get; set; }
        public string ViewModelClassName { get; set; }

        public Assembly XamlCodeAssembly { get; set; }
        public Assembly XamlViewModelAssembly { get; set; }
    }
}
