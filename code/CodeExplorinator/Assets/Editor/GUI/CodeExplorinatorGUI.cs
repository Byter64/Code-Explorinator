using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public class CodeExplorinatorGUI : EditorWindow
    {
        [SerializeField, Tooltip("This file will be overwritten by the CodeExplorinator with the code graph")]
        private VisualTreeAsset uxmlFile;
        [SerializeField]
        private Texture2D lineTexture;

        [MenuItem("Window/CodeExplorinator")]
        public static void OnShowWindow()
        {
            EditorWindow editorWindow = GetWindow(typeof(CodeExplorinatorGUI));
            editorWindow.titleContent = new GUIContent("Code Explorinator");
        }

        private void CreateGUI()
        {
            ClassData testData = CreateTestData();
            
            GUIStyle classStyle = new GUIStyle();
            classStyle.alignment = TextAnchor.MiddleCenter;
            classStyle.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Editor/Fonts/DroidSansMono.ttf");
            classStyle.fontSize = 20;

            GUIStyle methodStyle = new GUIStyle(classStyle);
            methodStyle.alignment = TextAnchor.UpperLeft;
            
            ClassGUI testClass = new ClassGUI(new Vector2(50, 50), testData, classStyle, classStyle, methodStyle, lineTexture);
            VisualElement testVisualElement = testClass.CreateVisualElement();
            rootVisualElement.Add(testVisualElement);
            
            // VisualElements objects can contain other VisualElement following a tree hierarchy.
            //VisualElement label = new Label("Hello World! From C#");
            //root1.Add(label);

            // Instantiate UXML
            //VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
            //root1.Add(labelFromUXML);
        }

        /// <summary>
        /// DEBUG. Will analyze the whole project and instantiate the first class it found as GUI element0
        /// </summary>
        private ClassData CreateTestData()
        {
            string[] allCSharpScripts =
                Directory.GetFiles(Application.dataPath, "*.cs",
                    SearchOption.AllDirectories); //maybe searching all directories not needed?

            CSharpCompilation compilation = CSharpCompilation.Create("myAssembly")
                .AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location));
            List<ClassData> allClasses = new List<ClassData>();
            //goes through all files and generates the syntax trees and the semantic model
            foreach (var cSharpScript in allCSharpScripts)
            {
                StreamReader streamReader = new StreamReader(cSharpScript);
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(streamReader.ReadToEnd());
                streamReader.Close();

                compilation = compilation.AddSyntaxTrees(syntaxTree);

                SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);

                CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();
                allClasses.AddRange(ClassAnalyzer.GenerateAllClassInfo(root, semanticModel));
            }

            return allClasses.First();
        }
    }
}