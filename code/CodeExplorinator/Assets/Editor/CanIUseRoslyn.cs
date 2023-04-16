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
        string[] allCSharpScripts = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);

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
        
        ReferenceFinder.ReFillAllPublicMethodReferences(classDatas, compilation);
        ReferenceFinder.ReFillAllPublicAccesses(classDatas, compilation);
        ReferenceFinder.ReFillAllPublicPropertyAccesses(classDatas, compilation);

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

                result = "";
                
                foreach (MethodInvocationData invocationData in methodData.IsInvokingInternalMethods)
                {
                    result += "\t method " + invocationData.ContainingMethod + " in class: " + methodData.ContainingClass.GetName() + " references internal: " + invocationData.ReferencedMethod +
                              "\t\t of class " + invocationData.ReferencedMethod.ContainingClass + "\n";
                }
                
                Debug.Log(result);
                result = "";
                
                foreach (MethodInvocationData invocationData in methodData.IsInvokingExternalMethods)
                {
                    result += "\t method " + invocationData.ContainingMethod + " in class: " + methodData.ContainingClass.GetName() + " references external: " + invocationData.ReferencedMethod +
                              "\t\t of class " + invocationData.ReferencedMethod.ContainingClass + "\n";
                }
                
                Debug.Log(result);
                result = "";

                foreach (var invocationData in methodData.IsAccessingExternalField)
                {
                    result += "\t FIELD: method " + invocationData.ContainingMethod + " in class: " + methodData.ContainingClass.GetName() + " references external field: " + invocationData.ReferencedField +
                              "\t\t of class " + invocationData.ReferencedField.ContainingClass + "\n";
                }
                
                Debug.Log(result);
                result = "";

                foreach (var invocationData in methodData.IsAccessingInternalField)
                {
                    result += "\t FIELD: method " + invocationData.ContainingMethod + " in class: " + methodData.ContainingClass.GetName() + " references external field: " + invocationData.ReferencedField +
                              "\t\t of class " + invocationData.ReferencedField.ContainingClass + "\n";
                }
                
                Debug.Log(result);
                result = "";

                foreach (var invocationData in methodData.IsAccessingExternalProperty)
                {
                    result += "\t PROPERTY: method " + invocationData.ContainingMethod + " in class: " + methodData.ContainingClass.GetName() + " references external property: " + invocationData.ReferencedProperty +
                              "\t\t of class " + invocationData.ReferencedProperty.ContainingClass + "\n";
                }
                
                Debug.Log(result);
                result = "";

                foreach (var invocationData in methodData.IsAccessingInternalProperty)
                {
                    result += "\t PROPERTY: method " + invocationData.ContainingMethod + " in class: " + methodData.ContainingClass.GetName() + " references internal property: " + invocationData.ReferencedProperty +
                              "\t\t of class " + invocationData.ReferencedProperty.ContainingClass + "\n";
                }
                
                Debug.Log(result);
            }
            
        }

        
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

        ReferenceFinder.ReFillAllClassReferences(classDatas,compilation);
    }
    
}