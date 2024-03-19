using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RemoteCodeLoader.Attributes;
using RemoteCodeLoader.Models;
using RemoteCodeLoader.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteCodeLoader
{
    public class SolutionAnalyzer
    {
        public string RootPath { get; private set; }
        public SolutionAnalyzer(string rootPath)
        {
            RootPath = rootPath;
        }

        public async Task<Dictionary<string, SourceCodeBundle>> FindRemoteCodes(List<byte[]> assemblies)
        {
            var resources = new Dictionary<string, SourceCodeBundle>();

            Stack<string> pathStack = new Stack<string>();
            pathStack.Push(RootPath);

            while(pathStack.Count > 0)
            {
                var currentPath = pathStack.Pop();

                var directories = Directory.GetDirectories(currentPath);
                var files = Directory.GetFiles(currentPath).Where(path => Path.GetExtension(path) == ".cs");

                foreach (var directory in directories)
                {
                    pathStack.Push(directory);
                }

                foreach (var file in files)
                {
                    var relevantAttribute = await AnalyzeSource(File.ReadAllText(file));
                    if (relevantAttribute == null)
                        continue;

                    if (!resources.ContainsKey(relevantAttribute.Id))
                    {
                        resources.Add(relevantAttribute.Id, new SourceCodeBundle()
                        {
                            Assemblies = assemblies,
                            Xaml = File.ReadAllText(relevantAttribute.XamlPath)
                        });
                    }

                    if (relevantAttribute.RemoteCodeType == RemoteCodeType.XamlCodeBehind)
                        resources[relevantAttribute.Id].SourceCode.XamlFromFile(file);
                    else if (relevantAttribute.RemoteCodeType == RemoteCodeType.XamlViewModel)
                        resources[relevantAttribute.Id].SourceCode.ViewModelFromFile(file);
                }
            }

            return resources;
        }

        private static async Task<RemoteSourceCode?> AnalyzeSource(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp12));
            var root = await syntaxTree.GetRootAsync();
            var attributes = root.DescendantNodes().OfType<AttributeSyntax>();
            foreach (var attribute in attributes)
            {
                var attributeName = attribute.Name.ToString();
                if (attributeName != nameof(RemoteSourceCode))
                    continue;

                if (attribute.ArgumentList == null)
                    return null;

                var args = attribute.ArgumentList.Arguments;

                var id = args[0].ToString().Replace("\"", "");
                var xamlPath = args[1].ToString().Replace("\"", "").Replace("@", "");
                var remoteCodeType = Enum.Parse<RemoteCodeType>(Path.GetExtension(args[2].ToString()).Replace(".", ""));


                return new RemoteSourceCode(id, xamlPath, remoteCodeType);
            }

            return null;
        }
    }
}
