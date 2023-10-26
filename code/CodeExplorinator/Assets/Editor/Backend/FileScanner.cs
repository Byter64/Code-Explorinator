using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEngine;


namespace CodeExplorinator
{
    public static class FileScanner
    {
        //todo: maybe integrate fileanalyzer in this class as a method or something
        public static ImmutableHashSet<ClassData> ScanAllFilesForClasses()
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

            List<ClassData> classSymbols = new List<ClassData>();
            
            foreach (SyntaxTree tree in compilation.SyntaxTrees)
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(tree);

                CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
                classSymbols.AddRange(GenerateClassesForFile(root, semanticModel));
                //classDatas.AddRange(ClassAnalyzer.GenerateAllClassInfo(root, semanticModel));
            }

            return classSymbols.ToImmutableHashSet();
            /*
            TODO: we need for that: Microsoft.CodeAnalysis.Workspaces.MSBuild

            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            Solution solution = await workspace.OpenSolutionAsync(nancyApp);

            var solution = Solution.Create(SolutionId.CreateNewId()).AddCSharpProject("Foo", "Foo").Solution;

            Roslyn.Services.Workspace.LoadSolution
            */

        }
        
         public static List<ClassData> GenerateClassesForFile(CompilationUnitSyntax root, SemanticModel model)
        {
            IEnumerable<TypeDeclarationSyntax> classDeclarations = root.DescendantNodes()
                .OfType<TypeDeclarationSyntax>();

            List<ClassData> classSymbols = new List<ClassData>();
            
            foreach (TypeDeclarationSyntax classDeclaration in classDeclarations)
            {
                INamedTypeSymbol classSymbol = model.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
                
                classSymbols.Add(new ClassData(classSymbol));
                
                /*
                ImmutableArray<ISymbol> members = classSymbol.GetMembers();
                Debug.Log(classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

                
                    foreach (ISymbol member  in members)
                    {
                        
                        switch (member)
                        {
                            case IMethodSymbol method:
                                Debug.Log("methodSymbol: " + method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                                break;
                            case IFieldSymbol fieldSymbol:
                                Debug.Log("fieldSymbol: " + fieldSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                                Debug.Log("fieldSymbol contains: " + fieldSymbol.ContainingSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                                Debug.Log(fieldSymbol.);
                                break;
                            case IPropertySymbol propertySymbol:
                                Debug.Log("propertySymbol: " + propertySymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                                break;
                            default:
                                break;
                        }
                        
                    }
                */
            }
            return classSymbols;

        }
        
    }
    
}

