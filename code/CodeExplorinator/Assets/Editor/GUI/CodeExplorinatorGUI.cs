using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;
using UnityEngine.TextCore.Text;

namespace CodeExplorinator
{
    public class CodeExplorinatorGUI : EditorWindow
    {
        [SerializeField, Tooltip("This file will be overwritten by the CodeExplorinator with the code graph")]
        private VisualTreeAsset uxmlFile;
        [SerializeField]
        public Texture2D lineTexture;

        private DragBehaviour dragBehaviour;
        private ZoomBehaviour zoomBehaviour;

        [MenuItem("Window/CodeExplorinator")]
        public static void OnShowWindow()
        {
            EditorWindow editorWindow = GetWindow(typeof(CodeExplorinatorGUI));
            editorWindow.titleContent = new GUIContent("Code Explorinator");
        }


        private void CreateGUI()
        {
            VisualElement graph = new VisualElement();
            graph.style.scale = Vector2.one;
            rootVisualElement.Add(graph);
            dragBehaviour = new DragBehaviour(graph);
            zoomBehaviour = new ZoomBehaviour(graph, 1.05f);
            graph.style.position = new StyleEnum<Position>(Position.Absolute);
            graph.style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(0b11111111111111111111, 0b11111111111111111111));
            graph.style.width = 0b11111111111111111111;
            graph.style.height = 0b11111111111111111111;
            graph.style.backgroundImage = Background.FromTexture2D(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Graphics/TEST_GraphBackground.png"));
            graph.style.marginLeft = -0b1111111111111111111; //Bigger numbers resulted in the background being not on the start view anymore :(
            graph.style.marginTop = -0b1111111111111111111;
            SpringEmbedderAlgorithm.Init(this, graph);

            #region YanniksAltesZeichenZeugs
            //GUIStyle classStyle = new GUIStyle
            //{
            //    alignment = TextAnchor.MiddleCenter,
            //    font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Editor/Fonts/DroidSansMono.ttf"),
            //    fontSize = 20
            //};
            //GUIStyle methodStyle = new GUIStyle(classStyle);
            //methodStyle.alignment = TextAnchor.UpperLeft;
            
            //List<ClassData> data = CreateData();
            //float xpos = 0;
            //foreach (ClassData classData in data)
            //{ 
            //    ClassGUI testClass = new ClassGUI(new Vector2(xpos - graph.style.marginLeft.value.value, -graph.style.marginTop.value.value) , classData, classStyle, methodStyle, methodStyle, lineTexture);
            //    VisualElement testVisualElement = testClass.CreateVisualElement();
            //    graph.Add(testVisualElement);
            //    xpos += testClass.Size.x;
            //}

            #endregion
        }




        /// <summary>
        /// DEBUG. Will analyze the whole project and instantiate the first class it found as GUI element0
        /// </summary>
        private ClassData CreateTestData()
        {
            return CreateData().First();
        }

        private List<ClassData> CreateData()
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

            return allClasses;
        }
    }
}