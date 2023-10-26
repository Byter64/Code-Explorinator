using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using CodeExplorinator;
using Microsoft.CodeAnalysis;
using UnityEngine;

public static class DephtsSearch
{
    //todo: i need references for both ways, otherwise this does not generate a good graph
    //but for both ways i would need to check every exsisting class if it contains the class were looking at

    public static Dictionary<IClassData, ImmutableHashSet<IClassData>> Start(IClassData focusClass, int radius, ImmutableHashSet<INamedTypeSymbol> allClasses)
    {
        Dictionary<IClassData, ImmutableHashSet<IClassData>> connectionsDictionary =
            new Dictionary<IClassData, ImmutableHashSet<IClassData>>();

        List<IClassData> incompleteClasses = new List<IClassData>();
        
        DepthSearch(focusClass, radius - 1);

        //check where placeholders are needed
        foreach (var incompleteClass in incompleteClasses)
        {
            HashSet<IClassData> connectedClasses = ClassAnalyzer.AnalyzeConnectionsOfClass(incompleteClass.typeData, allClasses);
            HashSet<IClassData> connectedClassesWithPlaceholders = new HashSet<IClassData>();
            
            foreach (var connectedClass in connectedClasses)
            {
                //if the dictionary does not contain the connected class, it becomes a placeholder
                if (connectionsDictionary.ContainsKey(connectedClass))
                {
                    connectedClassesWithPlaceholders.Add(connectedClass);
                }
                else
                {
                    connectedClassesWithPlaceholders.Add(connectedClass as PlaceholderClassData);
                }
            }
            
            connectionsDictionary.Add(incompleteClass, connectedClassesWithPlaceholders.ToImmutableHashSet());
        }
        
        return connectionsDictionary;
        
        void DepthSearch(IClassData focusClass, int radius)
        {
        
            if (radius < 0) return;
            if (connectionsDictionary.ContainsKey(focusClass)) return;
            
            if (radius - 1 < 0)
            {
                incompleteClasses.Add(focusClass);
                return;
            }
            
            //will this work for every type of classdata?
            ImmutableHashSet<IClassData> connectedClasses = ClassAnalyzer.AnalyzeConnectionsOfClass(focusClass.typeData, allClasses).ToImmutableHashSet();
            connectionsDictionary.Add(focusClass,connectedClasses);

            foreach (var connectedClass in connectedClasses)
            {
                DepthSearch(connectedClass, radius - 1);
            }
            
        }
        
    }
}
