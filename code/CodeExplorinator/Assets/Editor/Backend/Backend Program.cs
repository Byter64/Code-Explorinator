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
        private static ImmutableHashSet<ClassData> classSymbols;
        
        
        public BackendProgram()
        {
            
        }
        

        [MenuItem("Window/Backend Test")]
        public static void Init()
        {
            classSymbols = FileScanner.ScanAllFilesForClasses();
            //ClassAnalyzer.AnalyzeConnectionsOfClass(classSymbols.First(),classSymbols);
            INamedTypeSymbol mytestclass = null;
            foreach (var classSymbol in classSymbols)
            {
                if (classSymbol.typeData.Name == "ATestClass")
                {
                    mytestclass = classSymbol.typeData;
                }
            }
            var dictionary = DephtsSearch.Start(new ClassData(mytestclass),2,classSymbols);

            Debug.Log("FINISHED; NOW PRINTING");
            foreach (var key in dictionary.Keys)
            {
                
                Debug.Log("FOUND: " + key.typeData.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                Debug.Log("AND ITS COMPONENTS:");
                foreach (var classData in dictionary[key])
                {
                    
                    Debug.Log(classData.typeData.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                }
            }
        }
        
        [MenuItem("Window/Backend Test 2")]
        public static void BackendTest2()
        {
            classSymbols = FileScanner.ScanAllFilesForClasses();
            //ClassAnalyzer.AnalyzeConnectionsOfClass(classSymbols.First(),classSymbols);
            INamedTypeSymbol mytestclass = null;
            foreach (var classSymbol in classSymbols)
            {
                if (classSymbol.typeData.Name == "ATestClass")
                {
                    mytestclass = classSymbol.typeData;
                }
            }
            var dictionary = GenerateGraph(new ClassData(mytestclass), 10);

            Debug.Log("FINISHED; NOW PRINTING");
            foreach (var key in dictionary.Keys)
            {
                
                Debug.Log("FOUND: " + key.typeData.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                Debug.Log("AND ITS COMPONENTS:");
                foreach (var classData in dictionary[key])
                {
                    
                    Debug.Log(classData.typeData.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                }
            }
        }

        public static ImmutableHashSet<ClassData> GetSetOfAllClasses()
        {
            classSymbols = FileScanner.ScanAllFilesForClasses();
            return classSymbols;
        }
        
        public static ImmutableDictionary<IClassData, ImmutableHashSet<IClassData>> GenerateGraph(ClassData focusedClass, int radius)
        {
            return DephtsSearch.Start(focusedClass, radius, classSymbols).ToImmutableDictionary();
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


