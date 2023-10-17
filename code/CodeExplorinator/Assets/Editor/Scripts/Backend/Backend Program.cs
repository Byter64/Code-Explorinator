using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using UnityEditor;
using UnityEngine;

namespace CodeExplorinator
{
    public class BackendProgram
    {
        //todo: maybe cast to IClassData cuz thats what ill get the first time interacting with frontend
        static ImmutableHashSet<INamedTypeSymbol> classSymbols;
        
        
        public BackendProgram()
        {
            
        }
        

        [MenuItem("Window/Code Explorinator")]
        public static void Init()
        {
            classSymbols = FileScanner.ScanAllFilesForClasses().ToImmutableHashSet();
            ClassAnalyzer.AnalyzeConnectionsOfClass(classSymbols.First(),classSymbols);
            //DephtsSearch.Start(classSymbols.First(),2,classSymbols);
        }
        
        
        
        public void GimmeTheGraph(INamedTypeSymbol focusedClassSymbol, int radius)
        {
            ClassAnalyzer.AnalyzeConnectionsOfClass(focusedClassSymbol,classSymbols);
        }
        
        public State state
        {
            get;
            private set;
        }
        
        public record State{
            
        }

        public record Input
        {
            //referenztiefe placeholder
            public int searchRadius = 3;
            //fokusklasse
            //oder fokusklassen

        }
    }
}


