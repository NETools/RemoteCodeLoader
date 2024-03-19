using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using RemoteCodeLoader.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RemoteCodeLoader.Code
{
    public class CodeAssembler
    {
        private static List<MetadataReference> _metadataReferences = [MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(VerticalStackLayout).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Button).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(INotifyPropertyChanged).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ActivationState).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Debug).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ICommand).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Color).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(RemoteSourceCode).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location)];

        public static Assembly Compile(string source)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
            var compilation = CSharpCompilation.Create(
                Path.GetRandomFileName(),
                syntaxTrees: new[] { syntaxTree },
                references: _metadataReferences,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (MemoryStream ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    // Display compilation errors
                    foreach (Diagnostic diagnostic in result.Diagnostics)
                    {
                        Debug.WriteLine(diagnostic.GetMessage());
                    }

                    return null;
                }
                else
                {
                    // Load the dynamically compiled assembly
                    ms.Seek(0, SeekOrigin.Begin);
                    return Assembly.Load(ms.ToArray());
                }
            }
        }

        public static void AddReference(byte[] image)
        {
            _metadataReferences.Add(MetadataReference.CreateFromImage(image));
        }

        public static void AddReference(string file)
        {
            _metadataReferences.Add(MetadataReference.CreateFromFile(file));
        }
    }
}
