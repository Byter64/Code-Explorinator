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
        

        [MenuItem("Window/Backend Test")]
        public static void Init()
        {
            classSymbols = FileScanner.ScanAllFilesForClasses().ToImmutableHashSet();
            //ClassAnalyzer.AnalyzeConnectionsOfClass(classSymbols.First(),classSymbols);
            INamedTypeSymbol mytestclass = null;
            foreach (var classSymbol in classSymbols)
            {
                if (classSymbol.Name == "ATestClass")
                {
                    mytestclass = classSymbol;
                }
            }
            var dictionary = DephtsSearch.Start(new ClassData(mytestclass),2,classSymbols);

            Debug.Log("FINISHED; NOW PRINTING");
            foreach (var key in dictionary.Keys)
            {
                
                Debug.Log(key.typeData.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                Debug.Log("AND ITS COMPONENTS:");
                foreach (var classData in dictionary[key])
                {
                    
                    Debug.Log(classData.typeData.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                }
            }
        }

        public static ImmutableHashSet<INamedTypeSymbol> GetSetOfAllClasses()
        {
            return classSymbols;
        }
        
        public static Dictionary<IClassData, ImmutableHashSet<IClassData>> GenerateGraph(ClassData focusedClass, int radius)
        {
            classSymbols = FileScanner.ScanAllFilesForClasses().ToImmutableHashSet();
            return DephtsSearch.Start(focusedClass, radius, classSymbols);
        }
        /*
        
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
        */
    }
}


