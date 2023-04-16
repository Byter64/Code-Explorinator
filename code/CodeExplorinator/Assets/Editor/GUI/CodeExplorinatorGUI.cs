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
using UnityEditor.Graphs;

namespace CodeExplorinator
{
    public class CodeExplorinatorGUI : EditorWindow
    {
        [SerializeField, Tooltip("This file will be overwritten by the CodeExplorinator with the code graph")]
        private VisualTreeAsset uxmlFile;

        //Only exists to prevent garbage collection from deleting the dragBehaviour object
        private DragBehaviour dragBehaviour;
        //Only exists to prevent garbage collection from deleting the zoomBehaviour object
        private ZoomBehaviour zoomBehaviour;
        private SearchRadiusSliderBehaviour sliderBehaviour;
        private GraphManager graphManager;
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
            dragBehaviour = new DragBehaviour(graph);
            zoomBehaviour = new ZoomBehaviour(graph, 1.05f);
            #region Create Background
            rootVisualElement.Add(graph);
            graph.style.position = new StyleEnum<Position>(Position.Absolute);
            graph.style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(0b11111111111111111111, 0b11111111111111111111));
            graph.style.width = 0b11111111111111111111;
            graph.style.height = 0b11111111111111111111;
            graph.style.backgroundImage = Background.FromTexture2D(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Graphics/TEST_GraphBackground.png"));
            graph.style.marginLeft = -0b1111111111111111111; //Bigger numbers resulted in the background being not on the start view anymore :(
            graph.style.marginTop = -0b1111111111111111111;
            #endregion


            List<ClassData> classData = GenerateClassDataFromProject();
            graphManager = new GraphManager(classData, graph, 0);
            sliderBehaviour = CreateSearchRadiusSlider(rootVisualElement, graphManager);
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
            //    classGUI testVisualElement = testClass.GenerateVisualElement();
            //    graph.Add(testVisualElement);
            //    xpos += testClass.Size.x;
            //}

            #endregion
        }

        private SearchRadiusSliderBehaviour CreateSearchRadiusSlider(VisualElement root, GraphManager graphManager)
        {
            SliderInt slider = new SliderInt(0, 10);
            SearchRadiusSliderBehaviour sliderBehaviour = new SearchRadiusSliderBehaviour(slider, graphManager, 0);
            slider.style.position = new StyleEnum<Position>(Position.Absolute);
            slider.style.marginLeft = 20;
            slider.style.marginTop = 20;
            root.Add(slider);

            return sliderBehaviour;
        }

        private List<ClassData> GenerateClassDataFromProject()
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