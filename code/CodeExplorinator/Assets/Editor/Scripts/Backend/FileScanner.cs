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
        //todo: maybe integrate fileanalyzer in this class as a method or something
        public static List<INamedTypeSymbol> ScanAllFilesForClasses()
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

            List<INamedTypeSymbol> classSymbols = new List<INamedTypeSymbol>();
            
            foreach (SyntaxTree tree in compilation.SyntaxTrees)
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(tree);

                CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
                classSymbols.AddRange(FileAnalyzer.GenerateAllClassInfo(root, semanticModel));
                //classDatas.AddRange(ClassAnalyzer.GenerateAllClassInfo(root, semanticModel));
            }

            return classSymbols;
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

