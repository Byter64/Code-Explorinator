using Microsoft.CodeAnalysis;
using UnityEngine;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public class BreadthSearch
    {
        public int searchRadius = 7;
        public List<ClassData> AnalysedClasses;
        public List<MethodData> AnalysedMethods;
        public List<Node> Nodes;
        private VisualElement Graph;
        private CodeExplorinatorGUI CodeExplorinatorGUI;

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

        public void Init(CodeExplorinatorGUI codeExplorinatorGUI, VisualElement graph)
        {
            CodeExplorinatorGUI = codeExplorinatorGUI;
            Graph = graph;

            Nodes = new List<Node>();

            List<ClassData> classDatas = GenerateClasses();
            AnalysedClasses = new List<ClassData>();
            AnalysedMethods = new List<MethodData>();
            
            ClassData startingClass = GetStartingClass(classDatas);
            GenerateClassGraph(startingClass, searchRadius);
            
            foreach (var startingMethod in startingClass.PublicMethods.Concat(startingClass.PrivateMethods))
            {
                GenerateMethodGraph(startingMethod,searchRadius);
            }
            
            SpringEmbedderAlgorithm.Calculate(Nodes, 1000, 1000);
            
            ConnectionGUI connectionGUI = new ConnectionGUI(Nodes, graph, CodeExplorinatorGUI.lineTexture);
            connectionGUI.DrawConnections();
        }

        private void GenerateNode(ClassData classData, bool isLeaf)
        {
            GUIStyle classStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Editor/Fonts/DroidSansMono.ttf"),
                fontSize = 20
            };
            
            GUIStyle methodStyle = new GUIStyle(classStyle);
            methodStyle.alignment = TextAnchor.UpperLeft;

                ClassGUI testClass = new ClassGUI(new Vector2(Random.Range(-50,50) - Graph.style.marginLeft.value.value, Random.Range(-50,50)-Graph.style.marginTop.value.value), classData, classStyle, methodStyle, methodStyle, CodeExplorinatorGUI.lineTexture);
                VisualElement testVisualElement = testClass.CreateVisualElement();
                Debug.Log("Visualelement: " + testVisualElement.style.marginLeft + "/" + testVisualElement.style.marginTop);
                Nodes.Add(new Node(classData, testVisualElement, isLeaf));
                Graph.Add(testVisualElement);
        }

        public void GenerateClassGraph(ClassData startingClass, int searchRadius)
        {
            if (searchRadius <= 0)
            {
                return;
            }

            /*
            //hopefully this doesnt need to be synchronized?
            if (AnalysedClasses.Contains(startingClass))
            {
                return;
            }
            */

            //if the class was already analysed but we can still search, the node is not generated but the tree explored further
            //impacts the searchtime negatively tho
            //if we wanted to perfectly run through all nodes we would have to save the highest searchradius that was gone trough, and go through
            //the node again if our current searchradius is higher. i think that could cause performance issuses if a lot of circular references are present
            foreach (var node in Nodes)
            {
                if (node.ClassData == startingClass)
                {
                    if (node.IsLeaf && searchRadius>1)
                    {
                        node.IsLeaf = false; 
                        goto SkipOverGeneration;
                    }

                    return;
                }
            }
            

            AnalysedClasses.Add(startingClass);

            if (searchRadius == 1)
            {
                GenerateNode(startingClass,true);
            }
            else
            {
                GenerateNode(startingClass,false);
            }
            
            SkipOverGeneration:

            //checking incoming and outgoing references

            /*
            
            foreach (var fieldReference in startingClass.IsReferencingExternalClassAsField)
            {
                //instantiate fieldReference.FieldContainingReference.ContainingClass
                GenerateClassGraph(fieldReference.ReferencedClass, searchRadius - 1);
            }

            foreach (var propertyReference in startingClass.IsReferencingExternalClassAsProperty)
            {
                //instantiate propertyReference.PropertyContainingReference.ContainingClass
                GenerateClassGraph(propertyReference.ReferencedClass, searchRadius - 1);
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
            */
            
            //or just iterate though AllContainingClasses:

            foreach (var connectedClass in startingClass.AllConnectedClasses)
            {
                GenerateClassGraph(connectedClass,searchRadius - 1);
            }
            
            
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
                GenerateMethodGraph(methodInvocation.ReferencedMethod, searchRadius - 1);
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