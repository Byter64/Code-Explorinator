using System.Linq;
using Microsoft.CodeAnalysis;
using UnityEngine;
using Microsoft.CodeAnalysis.CSharp;
using UnityEditor;

public static class CanIUseRoslyn
{
    [MenuItem("Test/GO BRR")]
    public static void ItJustWorks() {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(@"");

        var allAssemblies = System.AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && a.Location.Length > 0) // Note: This might exclude unexpected stuff
            .Select(a => MetadataReference.CreateFromFile(a.Location));
        
        CSharpCompilation compilation = CSharpCompilation.Create("MyCompilation", syntaxTrees: new[] { tree }, references: allAssemblies);

        INamedTypeSymbol typeByName = compilation.GetTypeByMetadataName("HelloWorld.MyLameUnityScript");
        
        SemanticModel sem = compilation.GetSemanticModel(tree, true);
        
        Debug.Log(string.Join(", ", typeByName.MemberNames));
        //Debug.Log(sem.GetConstantValue();
    }
}