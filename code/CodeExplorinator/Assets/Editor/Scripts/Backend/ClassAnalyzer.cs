using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using UnityEngine;

public static class ClassAnalyzer
{

    public static HashSet<INamedTypeSymbol> AnalyzeConnectionsOfClass(INamedTypeSymbol focussedClassSymbol, ImmutableHashSet<INamedTypeSymbol> allClasses)
    {
        ImmutableArray<ISymbol> members = focussedClassSymbol.GetMembers();
        Debug.Log(focussedClassSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));


        HashSet<INamedTypeSymbol> connectedClasses = new HashSet<INamedTypeSymbol>();
                
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
                    foreach (var classSymbol in allClasses)
                    {
                        if (SymbolEqualityComparer.Default.Equals(fieldSymbol.Type, classSymbol))
                        {
                            connectedClasses.Add(classSymbol);
                            //add class to dictionary
                            break;
                        }
                    }
                    break;
                
                case IPropertySymbol propertySymbol:
                    Debug.Log("propertySymbol: " + propertySymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                    foreach (var classSymbol in allClasses)
                    {
                        if (SymbolEqualityComparer.Default.Equals(propertySymbol.Type, classSymbol))
                        {
                            connectedClasses.Add(classSymbol);
                            //add to dictionary
                            break;
                        }
                    }
                    break;
                
                default:
                    break;
            }
                        
        }

        return connectedClasses;
    }
    
}
