using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteCodeLoader.Models
{
    public class SourceCode
    {
        public ClassSource XamlCode { get; set; }
        public ClassSource ViewModelCode { get; set; }

        public void XamlFromFile(string xamlSourcePath)
        {
            XamlCode = LoadSource(xamlSourcePath);
        }

        public void ViewModelFromFile(string viewModelSourcePath)
        {
            ViewModelCode = LoadSource(viewModelSourcePath);
        }

        private static ClassSource LoadSource(string path)
        {
            var source = File.ReadAllText(path);
            return new ClassSource()
            {
                Source = source,
                ClassName = ParseNamespaceAndClassName(source)
            };
        }

        private static string ParseNamespaceAndClassName(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var root = syntaxTree.GetRoot();

            var namespaceName = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().First().Name.ToString();
            var className = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First().Identifier.ValueText;

            return $"{namespaceName}.{className}";
        }

        public override string ToString()
        {
            return XamlCode.Source + ViewModelCode.Source;
        }

    }
}
