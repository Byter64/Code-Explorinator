using System;
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
        //
        
        public static List<INamedTypeSymbol> GenerateAllClassInfo(CompilationUnitSyntax root, SemanticModel model)
        {
            IEnumerable<TypeDeclarationSyntax> classDeclarations = root.DescendantNodes()
                .OfType<TypeDeclarationSyntax>();

            List<INamedTypeSymbol> classSymbols = new List<INamedTypeSymbol>();
            
            foreach (TypeDeclarationSyntax classDeclaration in classDeclarations)
            {
                INamedTypeSymbol classSymbol = model.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
                
                classSymbols.Add(classSymbol);
                
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


