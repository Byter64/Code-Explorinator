using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEngine;


namespace CodeExplorinator
{
    public static class FileScanner
    {
        public static void ScanAllFiles()
        {
            string[] allCSharpScripts = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);

            
            CSharpCompilation compilation = CSharpCompilation.Create("myAssembly");
            //List<ClassData> classDatas = new List<ClassData>();

        
        
            foreach (string cSharpScript in allCSharpScripts){
            
                StreamReader streamReader = new StreamReader(cSharpScript);
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(streamReader.ReadToEnd());
                streamReader.Close();

                compilation = compilation.AddSyntaxTrees(syntaxTree);
            }

            foreach (SyntaxTree tree in compilation.SyntaxTrees)
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(tree);

                CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
                FileAnalyzer.GenerateAllClassInfo(root, semanticModel);
                //classDatas.AddRange(ClassAnalyzer.GenerateAllClassInfo(root, semanticModel));
            }

            
            /*
            TODO: we need for that: Microsoft.CodeAnalysis.Workspaces.MSBuild

            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            Solution solution = await workspace.OpenSolutionAsync(nancyApp);

            var solution = Solution.Create(SolutionId.CreateNewId()).AddCSharpProject("Foo", "Foo").Solution;

            Roslyn.Services.Workspace.LoadSolution
            */
            
        }
        
    }
    
}

