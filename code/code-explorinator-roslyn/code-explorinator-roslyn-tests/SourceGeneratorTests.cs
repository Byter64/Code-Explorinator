using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace CodeExplorinator.Tests; 

// https://gist.github.com/chsienki/2955ed9336d7eb22bcb246840bfeb05c

public class SourceGeneratorTests
{
    [Test]
    public void EnsureExampleCompiles() {
        var testCode = $@"
namespace Foo {{ 
    public static class Bar {{}} 
    public static class Baz {{}} 

    public static class Program {{
        public static void Main(string[] args) {{

            // Ensure that the generated code can be used
            Console.WriteLine(HelloWorld.Message);
            foreach (var className in HelloWorld.AlLClassNames)
                Console.WriteLine(className);

            // Ensure that the shared code can be used
            Console.WriteLine(SomeSharedClass.HelloWorld);
        }}
    }}
}}
";
        
        Compilation comp = CreateCompilation(testCode);
        var newComp = RunGenerators(comp, out var generatorDiags, new SourceGenerator());

        Assert.That(generatorDiags, Is.Empty);
        
        // Note: Use debugger breakpoints here to inspect the generated code

        var sources = newComp.SyntaxTrees.Select(t => t.ToString()).ToArray();
        Console.WriteLine(string.Join("\n\n", sources));

        var diagnostics = newComp.GetDiagnostics();
        Assert.That(diagnostics, Is.Empty); // No compiler errors in generated code
    }

    #region Utils

    private static Compilation CreateCompilation(string? source = null) => 
        CSharpCompilation.Create("compilation",
            source == null 
                ? new CSharpSyntaxTree[] {} 
                : new[] { CSharpSyntaxTree.ParseText(source) },
            new[] {
                // Ensure that all relevant dependencies are properly referenced in test compilation
                MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a").Location),
                MetadataReference.CreateFromFile(typeof(System.Object).Assembly.Location),
                // TODO: replace type with another final type in shared project
                MetadataReference.CreateFromFile(typeof(SomeSharedClass).Assembly.Location) 
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

    private static Compilation RunGenerators(
        Compilation c, 
        out ImmutableArray<Diagnostic> diagnostics, 
        params ISourceGenerator[] generators
    ) {
        CSharpGeneratorDriver.Create(
            generators, 
            parseOptions: (CSharpParseOptions?) c.SyntaxTrees.FirstOrDefault()?.Options
        ).RunGeneratorsAndUpdateCompilation(c, out var d, out diagnostics);
        
        return d;
    }
    
    #endregion
}