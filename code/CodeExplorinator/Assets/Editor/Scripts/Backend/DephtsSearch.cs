using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using UnityEngine;

public static class DephtsSearch
{

    public static Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>> Start(INamedTypeSymbol focusClass, int radius, ImmutableHashSet<INamedTypeSymbol> allClasses)
    {
        Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>> connections =
            new Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>>();
        
        DepthSearch(focusClass, radius - 1);
        
        return connections;
        
        void DepthSearch(INamedTypeSymbol focusClass, int radius)
        {
            //todo: continue here with a temporary dictionary and utilize the class analyzer class to get the connections of the classes
        
            if (radius < 0) return;
            if (connections.ContainsKey(focusClass)) return;

            HashSet<INamedTypeSymbol> connectedClasses = ClassAnalyzer.AnalyzeConnectionsOfClass(focusClass, allClasses);
            
            if (radius - 1 < 0)
            {
                foreach (var VARIABLE in allClasses)
                {
                    
                }
                //idk careful cuz incomplete class
            }


            
            connections.Add(focusClass,connectedClasses);
        

            foreach (var connectedClass in connectedClasses)
            {
                DepthSearch(connectedClass, radius - 1);
            }
        
        }
        
    }
    

}
