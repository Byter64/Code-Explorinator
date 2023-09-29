using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public class CodeExplorinatorGUI : EditorWindow
    {
        public static bool isControlDown = false;
        public static Vector2 Scale { get { return zoomBehaviour.Scale; } }

        private const string settingsKey = "CodeExplorinatorSettings";

        private static DragBehaviour dragBehaviour;
        private static ZoomBehaviour zoomBehaviour;
        private static GraphManager graphManager;
        private static VisualElement graph;
        private static MenuGUI menu;
        [MenuItem("Window/Code Explorinator")]
        public static void OnShowWindow()
        {
            EditorWindow editorWindow = GetWindow(typeof(CodeExplorinatorGUI));
            editorWindow.titleContent = new GUIContent("Code Explorinator");
        }

        private void CreateGUI()
        {
            if (graphManager == null)
            {
                Initialize();
            }
            rootVisualElement.Add(graph);
            rootVisualElement.Add(menu.VisualElement);
        }

        private void OnGUI()
        {
            CheckControlKeys();

            if (Event.current.keyCode == KeyCode.F5)
            {
                Reinitialize();
            }
        }

        //This is called everytime the window is closed (therefore destroyed)
        private void OnDestroy()
        {
            string saveData = graphManager.Serialize(true);
            EditorPrefs.SetString(settingsKey, saveData);
        }

        public List<ClassData> GenerateClassDataFromProject()
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

            ReferenceFinder.RefillAllReferences(classDatas, compilation);

            return classDatas;
        }

        public void Reinitialize()
        {
            List<ClassData> classData = GenerateClassDataFromProject();
            graphManager.Reinitialize(classData);
            menu.Reinitialize();
        }

        private void Initialize()
        {
            graph = new VisualElement();
            graph.style.scale = Vector2.one;
            dragBehaviour = new DragBehaviour(graph);
            zoomBehaviour = new ZoomBehaviour(graph, 1.05f);

            #region Create Background
            graph.style.position = new StyleEnum<Position>(Position.Absolute);
            graph.style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(0xFFFFF, 0xFFFFF));
            graph.style.width = 0xFFFFF;
            graph.style.height = 0xFFFFF;
            graph.style.backgroundImage = Background.FromTexture2D(AssetDatabase.LoadAssetAtPath<Texture2D>(Utilities.pathroot + "Editor/Graphics/TEST_GraphBackground.png"));
            graph.style.marginLeft = -0x7FFFF;
            graph.style.marginTop = -0x7FFFF;
            #endregion


            List<ClassData> classData = GenerateClassDataFromProject();
            graphManager = new GraphManager(classData, graph, 0);
            menu = new MenuGUI(graphManager, new Vector2Int(300, 600), this);
            menu.GenerateVisualElement();

            string settings = EditorPrefs.GetString(settingsKey);
            if (settings != null && settings != string.Empty)
            {
                GraphManager.SerializationData data = graphManager.DeSerialize(settings);

                menu.SetClassDepth(data.shownClassDepth);
                menu.SetMethodDepth(data.shownMethodDepth);
                graphManager.AddSelectedClasses(GraphManager.SerializationData.ToClassNodes(data.focusedClassNodes));
                graphManager.ApplySelectedClasses();

                if (data.state == GraphManager.State.MethodLayer)
                {
                    graphManager.AddSelectedMethods(GraphManager.SerializationData.ToMethodNodes(data.focusedMethodNodes));
                    graphManager.ApplySelectedMethods();
                }
            }
        }

        public static void CheckControlKeys()
        {
            Event @event = Event.current;
            if (@event.type == EventType.KeyDown && (@event.keyCode == KeyCode.LeftControl || @event.keyCode == KeyCode.RightControl))
            {
                isControlDown = true;
            }
            else if (@event.type == EventType.KeyUp && (@event.keyCode == KeyCode.LeftControl || @event.keyCode == KeyCode.RightControl))
            {
                isControlDown = false;
            }
        }

        public static void SetControlKey(bool isPressed)
        {
            isControlDown = isPressed;
        }
    }
}