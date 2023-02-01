using System.Linq;
using Microsoft.CodeAnalysis;
using UnityEngine;
using Microsoft.CodeAnalysis.CSharp;
using UnityEditor;
using CodeExplorinator;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;

public static class CanIUseRoslyn
{
    private static ReferenceFinder finder;

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

    [MenuItem("Test/Projekt analysieren")]
    public static void AnalizeProject()
    {
        string[] allCSharpScripts = Directory.GetFiles(Application.dataPath, "*.cs");

        CSharpCompilation compilation = CSharpCompilation.Create("myAssembly");
        List<ClassData> classDatas = new List<ClassData>();

        foreach (string cSharpScript in allCSharpScripts)
        {
            StreamReader streamReader = new StreamReader(cSharpScript);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(streamReader.ReadToEnd());
            streamReader.Close();

            compilation = compilation.AddSyntaxTrees(syntaxTree);
        }

        foreach(SyntaxTree tree in compilation.SyntaxTrees)
        {
            SemanticModel semanticModel = compilation.GetSemanticModel(tree);

            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            classDatas.AddRange(ClassAnalyzer.GenerateAllClassInfo(root, semanticModel));
        }

        finder = new ReferenceFinder(classDatas, compilation);
        finder.ReFillAllPublicMethodReferences();

        foreach(ClassData classData in classDatas)
        {
            
            foreach(MethodData methodData in classData.PublicMethods)
            {
                string result = "";
                result += methodData.MethodSymbol.Name + " is referenced " + methodData.AllInvocations.Count + " times: \n";

                foreach(MethodInvocationData invocationData in methodData.AllInvocations)
                {
                    result += "\t referenced method " + invocationData.ReferencedMethod + " in " + invocationData.ContainingMethod +
                        "\t\t of class " + invocationData.ContainingMethod.ContainingClass + "\n";
                }
                Debug.Log(result);
            }
        }

        finder.ReFillAllPublicAccesses();
        foreach (ClassData classData in classDatas)
        {
            foreach (FieldData fieldData in classData.PublicVariables)
            {
                string result = "";
                result += fieldData.FieldSymbol.Name + " is accessed " + fieldData.AllAccesses.Count + " times: \n";

                foreach (FieldAccessData accessData in fieldData.AllAccesses)
                {
                    result += "\t accessed field " + accessData.ReferencedField + " in " + accessData.ContainingMethod +
                        "\t\t of class " + accessData.ContainingMethod.ContainingClass + "\n";
                }
                Debug.Log(result);
            }
        }
    }
    
}