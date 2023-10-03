using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEngine;

namespace CodeExplorinator
{
    public static class FileAnalyzer
    {
        public static void GenerateAllClassInfo(CompilationUnitSyntax root, SemanticModel model)
        {
            IEnumerable<TypeDeclarationSyntax> classDeclarations = root.DescendantNodes()
                .OfType<TypeDeclarationSyntax>();
            

            foreach (TypeDeclarationSyntax classDeclaration in classDeclarations)
            {
                INamedTypeSymbol classSymbol = model.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
                ImmutableArray<ISymbol> members = classSymbol.GetMembers();
                Debug.Log(classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

                foreach (IMethodSymbol member  in members)
                {
                    //irgendeine exception fliegt hier
                    Debug.Log(member.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                }
            }

        }
    } 
}


