using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CodeExplorinator; 

[Generator]
public class SourceGenerator : ISourceGenerator { 

    public void Initialize(GeneratorInitializationContext context) {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context) {
        var receiver = (SyntaxReceiver) context.SyntaxReceiver!;
        
        var source = $@"using System;
using System.Collections.Generic;

namespace CodeExplorinator {{
    public static class HelloWorld {{
        public static string Message => ""{SomeSharedClass.HelloWorld}"";

        public static readonly IReadOnlyList<string> AllClassNames = new[] {{
            {string.Join(",\n            ", receiver.AllDeclaredClasses.Select(c => "\"" + c.Identifier + "\""))}
        }};
    }}
}}
";
        
        context.AddSource("hello.g.cs", SourceText.From(source, Encoding.UTF8));
    }
    
    public sealed class SyntaxReceiver : ISyntaxReceiver {
        
        // TODO: Collect useful information here to parse in `Execute`

        public readonly List<ClassDeclarationSyntax> AllDeclaredClasses = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode) {
            if (syntaxNode is ClassDeclarationSyntax cds) {
                AllDeclaredClasses.Add(cds);
            }
        }
    }
}