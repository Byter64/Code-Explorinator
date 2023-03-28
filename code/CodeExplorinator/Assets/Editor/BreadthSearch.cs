using Microsoft.CodeAnalysis;
using UnityEngine;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace CodeExplorinator
{
    public class BreadthSearch
    {
        public int searchRadius = 3;
        public List<ClassData> AnalysedClasses;
        public List<MethodData> AnalysedMethods;

        private ClassData GetStartingClass(List<ClassData> classDatas)
        {
            //do something accually reasonable here
            return classDatas[0];
        }

        private MethodData GetStartingMethod(List<MethodData> methodDatas)
        {
            //do something accually reasonable here
            return methodDatas[0];
        }

        public void Start()
        {
            List<ClassData> classDatas = GenerateClasses();
            AnalysedClasses = new List<ClassData>();
            AnalysedMethods = new List<MethodData>();
            
            ClassData startingClass = GetStartingClass(classDatas);
            GenerateClassGraph(startingClass, searchRadius);
            
            foreach (var startingMethod in startingClass.PublicMethods.Concat(startingClass.PrivateMethods))
            {
                GenerateMethodGraph(startingMethod,searchRadius);
            }
        }

        public void GenerateClassGraph(ClassData startingClass, int searchRadius)
        {
            if (searchRadius <= 0)
            {
                return;
            }

            //hopefully this doesnt need to be synchronized?
            if (AnalysedClasses.Contains(startingClass))
            {
                return;
            }

            AnalysedClasses.Add(startingClass);

            //checking incoming and outgoing references

            foreach (var fieldReference in startingClass.IsReferencingExternalClassField)
            {
                //instantiate fieldReference.FieldContainingReference.ContainingClass
                GenerateClassGraph(fieldReference.FieldContainingReference.ContainingClass, searchRadius - 1);
            }

            foreach (var propertyReference in startingClass.IsReferencingExternalClassProperty)
            {
                //instantiate propertyReference.PropertyContainingReference.ContainingClass
                GenerateClassGraph(propertyReference.PropertyContainingReference.ContainingClass, searchRadius - 1);
            }

            foreach (var fieldReference in startingClass.ReferencedByExternalClassField)
            {
                //instantiate fieldReference.FieldContainingReference.ContainingClass
                GenerateClassGraph(fieldReference.FieldContainingReference.ContainingClass, searchRadius - 1);
            }

            foreach (var propertyReference in startingClass.ReferencedByExternalClassProperty)
            {
                //instantiate propertyReference.PropertyContainingReference.ContainingClass
                GenerateClassGraph(propertyReference.PropertyContainingReference.ContainingClass, searchRadius - 1);
            }
            
            
            /*or just iterate though AllContainingClasses:
            foreach (var connectedClass in startingClass.AllConnectedClasses)
            {
                GenerateClassGraph(connectedClass,searchRadius - 1);
            }
            */
        }

        public void GenerateMethodGraph(MethodData startingMethod, int searchRadius)
        {
            if (searchRadius <= 0)
            {
                return;
            }

            //hopefully this doesnt need to be synchronized?
            if (AnalysedMethods.Contains(startingMethod))
            {
                return;
            }
            
            AnalysedMethods.Add(startingMethod);

            //checking incoming and outgoing references

            foreach (var methodInvocation in startingMethod.InvokedByExternal)
            {
                //instantiate reference to method and maybe even the containing class
                GenerateMethodGraph(methodInvocation.ContainingMethod, searchRadius - 1);
            }

            foreach (var methodInvocation in startingMethod.IsInvokingExternalMethods)
            {
                //instantiate reference to method and maybe even the containing class
                GenerateMethodGraph(methodInvocation.ContainingMethod, searchRadius - 1);
            }

            foreach (var fieldAccess in startingMethod.IsAccessingExternalField)
            {
                // just instantiate the class if needed, this is a dead end and not currently shown
            }

            foreach (var propertyAccess in startingMethod.IsAccessingExternalProperty)
            {
                // just instantiate the class if needed, this is a dead end and not currently shown
            }
        }


        private List<ClassData> GenerateClasses()
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

            foreach (SyntaxTree tree in compilation.SyntaxTrees)
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(tree);

                CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
                classDatas.AddRange(ClassAnalyzer.GenerateAllClassInfo(root, semanticModel));
            }

            ReferenceFinder.ReFillAllPublicMethodReferences(classDatas, compilation);
            ReferenceFinder.ReFillAllPublicAccesses(classDatas, compilation);
            ReferenceFinder.ReFillAllPublicPropertyAccesses(classDatas, compilation);
            ReferenceFinder.ReFillAllClassReferences(classDatas, compilation);

            return classDatas;
        }
    }
}