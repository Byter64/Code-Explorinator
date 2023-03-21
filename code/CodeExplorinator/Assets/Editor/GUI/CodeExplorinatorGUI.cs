using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace CodeExplorinator
{
    public class CodeExplorinatorGUI : EditorWindow
    {
        private static List<IDrawable> drawables = new List<IDrawable>();

        [MenuItem("Window/CodeExplorinator")]
        public static void OnShowWindow()
        {
            
            GetWindow(typeof(CodeExplorinatorGUI));

            //█████████████████████████████████
            //███████████DebugCode█████████████
            //█████████████████████████████████
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

            //ClassData testData = new ClassData(new TestINamedTypeSymbol());
            //testData.PublicMethods.Add()
            GUIStyle classStyle = new GUIStyle();
            classStyle.alignment = TextAnchor.MiddleCenter;
            classStyle.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Editor/Fonts/DroidSansMono.ttf");
            GUIStyle methodStyle = new GUIStyle(classStyle);
            methodStyle.alignment = TextAnchor.UpperLeft;
            drawables.Add(new ClassGUI(allClasses.First(), classStyle, classStyle, methodStyle));
            //█████████████████████████████████
            //█████████Ende DebugCode██████████
            //█████████████████████████████████
        }

        private void OnGUI()
        {
            //EditorGUI.DrawTextureTransparent(new Rect(10, 10, kirby.width, kirby.height), kirby);
            foreach(IDrawable drawable in drawables)
            {
                drawable.Draw();
            }
        }
    }
}