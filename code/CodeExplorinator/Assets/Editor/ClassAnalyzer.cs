using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CodeExplorinator
{
    public class ClassAnalyzer : MonoBehaviour
    {
        [MenuItem("Test/Test4")]
        public static void Test3()
        {
            string[] allCSharpScripts =
                Directory.GetFiles(Application.dataPath, "*.cs",
                    SearchOption.AllDirectories); //maybe searching all directories not needed?

            CSharpCompilation compilation = CSharpCompilation.Create("myAssembly")
                .AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location));

            //goes through all files and generates the syntax trees and the semantic model
            foreach (var cSharpScript in allCSharpScripts)
            {
                StreamReader streamReader = new StreamReader(cSharpScript);
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(streamReader.ReadToEnd());
                streamReader.Close();
                
                compilation = compilation.AddSyntaxTrees(syntaxTree);

                SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);

                CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();
                GenerateAllClassInfo(root, semanticModel);
            }
        }

        public static List<ClassData> GenerateAllClassInfo(CompilationUnitSyntax root, SemanticModel model)
        {
            //checks if the file has multiple class declarations and generates the information for each class

            IEnumerable<ClassDeclarationSyntax> classDeclarations = root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>();

            List<ClassData> classDatas = new List<ClassData>();

            foreach (var classDeclaration in classDeclarations)
            {
                classDatas.Add(GenerateClassInfo(classDeclaration, model));
            }

            return classDatas;
        }

        private static ClassData GenerateClassInfo(ClassDeclarationSyntax root, SemanticModel model)
        {
            //finds all variable and method declarations, saves them as FieldData/MethodData and sorts them for Accessibility,
            //generates a ClassData
            ClassData classData = new ClassData(model.GetDeclaredSymbol(root));
            Debug.Log("CLASS NAME IS:" + classData.ClassInformation.Name);

            List<IFieldSymbol>
                allVariables = FindAllFieldDeclarations(root, model); // PROPERTY DECLARATIONS MISSING!!! CONTINUE CODING HERE!!
            List<FieldData> publicVariables = getAllPublicFieldSymbols(allVariables, classData);
            List<FieldData> privateVariables = getAllPrivateFieldSymbols(allVariables, classData);

            List<IPropertySymbol> allProperties = FindAllPropertyDeclarations(root, model);
            List<PropertyData> publicProperties = getAllPublicPropertySymbols(allProperties, classData);
            List<PropertyData> privateProperties = getAllPrivatePropertySymbols(allProperties, classData);

            List<IMethodSymbol> allMethods = FindAllMethods(root, model);
            List<MethodData> publicMethods = getAllPublicMethodSymbols(allMethods, classData);
            List<MethodData> privateMethods = getAllPrivateMethodSymbols(allMethods, classData);

            classData.PublicVariables.AddRange(publicVariables);
            classData.PrivateVariables.AddRange(privateVariables);
            
            classData.PublicProperties.AddRange(publicProperties);
            classData.PrivateProperties.AddRange(privateProperties);
            
            classData.PublicMethods.AddRange(publicMethods);
            classData.PrivateMethods.AddRange(privateMethods);

            classData.ReadOutMyInformation(); //for debugging purposes
            
            return classData;
        }


        private static List<MethodData> getAllPublicMethodSymbols(List<IMethodSymbol> methodSymbols,
            ClassData classData)
        {
            //select all public, protected, internal and friend methods and display them 
            try
            {
                IEnumerable<IMethodSymbol> publicMethods = methodSymbols
                    .Where(m => m.DeclaredAccessibility != Accessibility.Private);

                List<MethodData> methodDatas = new List<MethodData>();

                foreach (var publicMethod in publicMethods)
                {
                    MethodData temp = new MethodData(publicMethod, classData);
                    methodDatas.Add(temp);
                }


                //remover later!!
                IEnumerable<string> distinctPublicMethods = publicMethods.Select(m => m.Name);

                foreach (var method in distinctPublicMethods)
                {
                    Debug.Log("this method is public: " + method);
                }

                return methodDatas;
            }
            catch (NullReferenceException e)
            {
                Debug.Log("no public methods found");
            }

            return null;
        }

        private static List<MethodData> getAllPrivateMethodSymbols(List<IMethodSymbol> methodSymbols,
            ClassData classData)
        {
            //select all private methods and display them 
            try
            {
                IEnumerable<IMethodSymbol> privateMethods = methodSymbols
                    .Where(m => m.DeclaredAccessibility == Accessibility.Private);


                List<MethodData> methodDatas = new List<MethodData>();

                foreach (var publicMethod in privateMethods)
                {
                    MethodData temp = new MethodData(publicMethod, classData);
                    methodDatas.Add(temp);
                }


                //remove later!!
                IEnumerable<string> distinctPrivateMethods = privateMethods.Select(m => m.Name);

                foreach (var method in distinctPrivateMethods)
                {
                    Debug.Log("this method is private: " + method);
                }

                return methodDatas;
            }
            catch (NullReferenceException e)
            {
                Debug.Log("no private methods found");
            }

            return null;
        }

        private static List<IMethodSymbol> FindAllMethods(ClassDeclarationSyntax root, SemanticModel model)
        {

            // Use the syntax model to find all methoddeclarations:
            IEnumerable<MethodDeclarationSyntax> methodDeclarations = root.DescendantNodes()
                .OfType<MethodDeclarationSyntax>();

            List<IMethodSymbol> methodSymbols = new List<IMethodSymbol>();


            //fill list with IMethodSymbols
            foreach (var methodDeclacation in methodDeclarations)
            {
                IMethodSymbol methodSymbol = model.GetDeclaredSymbol(methodDeclacation);
                methodSymbols.Add(methodSymbol);
            }

            return methodSymbols;
        }


        private static List<FieldData> getAllPublicFieldSymbols(List<IFieldSymbol> fieldSymbols, ClassData classData)
        {
            try
            {
                //select all public, protected, internal and friend vars and display them 
                IEnumerable<IFieldSymbol> publicVariables = fieldSymbols
                    .Where(m => m.DeclaredAccessibility != Accessibility.Private);


                List<FieldData> fieldDatas = new List<FieldData>();

                foreach (var publicVariable in publicVariables)
                {
                    FieldData temp = new FieldData(publicVariable, classData);
                    fieldDatas.Add(temp);
                }

                //remove later!!
                IEnumerable<string> distinctPublicVariables = publicVariables.Select(m => m.Name);

                foreach (var variable in distinctPublicVariables)
                {
                    Debug.Log("this var is public: " + variable);
                }

                return fieldDatas;
            }
            catch (NullReferenceException e)
            {
                Debug.Log("no public vars found");
            }

            return null;
        }

        private static List<FieldData> getAllPrivateFieldSymbols(List<IFieldSymbol> fieldSymbols, ClassData classData)
        {
            try
            {
                //select all private vars and display them 
                IEnumerable<IFieldSymbol> privateVariables = fieldSymbols
                    .Where(m => m.DeclaredAccessibility == Accessibility.Private);

                List<FieldData> fieldDatas = new List<FieldData>();

                foreach (var privateVariable in privateVariables)
                {
                    FieldData temp = new FieldData(privateVariable, classData);
                    fieldDatas.Add(temp);
                }

                //remove later!!
                IEnumerable<string> distinctPrivateVariables = privateVariables.Select(m => m.Name);

                foreach (var variable in distinctPrivateVariables)
                {
                    Debug.Log("this var is private: " + variable);
                }

                return fieldDatas;
            }
            catch (NullReferenceException e)
            {
                Debug.Log("no private vars found");
            }

            return null;
        }

        private static List<IFieldSymbol> FindAllFieldDeclarations(ClassDeclarationSyntax root, SemanticModel model)
        {
            // Use the syntax model to find all declarations:
            IEnumerable<FieldDeclarationSyntax> fieldDeclarationSyntaxes = root.DescendantNodes()
                .OfType<FieldDeclarationSyntax>();

            List<IFieldSymbol> fieldSymbols = new List<IFieldSymbol>();

            foreach (FieldDeclarationSyntax fieldDeclarationSyntax in fieldDeclarationSyntaxes)
            {
                foreach (VariableDeclaratorSyntax variableDeclaratorSyntax in fieldDeclarationSyntax.Declaration
                             .Variables)
                {
                    fieldSymbols.Add(model.GetDeclaredSymbol(variableDeclaratorSyntax) as IFieldSymbol);
                }
            }

            return fieldSymbols;

            // remove later!!
            foreach (var variableDeclaration in fieldDeclarationSyntaxes)
            {
                var f = variableDeclaration.DescendantNodes().OfType<VariableDeclaratorSyntax>();
            }

            foreach (var variableDeclaration in fieldDeclarationSyntaxes)
            {
                var vars = variableDeclaration; //hier aufgehört zu arbeiten
                var something = model.GetDeclaredSymbol(variableDeclaration) as ITypeSymbol;
                //var somethin = something.Name;
                //.Log(somethin);
                Debug.Log((model.GetSymbolInfo(variableDeclaration).Symbol.ContainingSymbol.GetType()));
                Debug.Log((model.GetSymbolInfo(variableDeclaration).Symbol as ITypeSymbol).TypeKind);
            }

            List<ISymbol> variableSymbols = new List<ISymbol>();


            //fill list with ISymbols
            foreach (var variableDeclacation in fieldDeclarationSyntaxes)
            {
                IFieldSymbol varSymbol = model.GetSymbolInfo(variableDeclacation).Symbol as IFieldSymbol;
                Debug.Log(varSymbol.Name + varSymbol.DeclaredAccessibility.ToString() + varSymbol.Kind +
                          varSymbol.ContainingType);
                //variableSymbols.Add(varSymbol);
            }

            return null;
        }

        private static List<IPropertySymbol> FindAllPropertyDeclarations(ClassDeclarationSyntax root, SemanticModel model)
        {
            // Use the syntax model to find all declarations:
            IEnumerable<PropertyDeclarationSyntax> propertyDeclarationSyntaxes = root.DescendantNodes()
                .OfType<PropertyDeclarationSyntax>();

            List<IPropertySymbol> propertySymbols = new List<IPropertySymbol>();

            foreach (PropertyDeclarationSyntax propertyDeclarationSyntax in propertyDeclarationSyntaxes)
            {
                propertySymbols.Add(model.GetDeclaredSymbol(propertyDeclarationSyntax));
            }

            return propertySymbols;
        }
        
        private static List<PropertyData> getAllPublicPropertySymbols(List<IPropertySymbol> propertySymbols, ClassData classData)
        {
            try
            {
                //select all public, protected, internal and friend vars and display them 
                IEnumerable<IPropertySymbol> publicVariables = propertySymbols
                    .Where(m => m.DeclaredAccessibility != Accessibility.Private);


                List<PropertyData> propertyDatas = new List<PropertyData>();

                foreach (var publicVariable in publicVariables)
                {
                    PropertyData temp = new PropertyData(publicVariable, classData);
                    propertyDatas.Add(temp);
                }

                //remove later!!
                IEnumerable<string> distinctPublicVariables = publicVariables.Select(m => m.Name);

                foreach (var variable in distinctPublicVariables)
                {
                    Debug.Log("this property is public: " + variable);
                }

                return propertyDatas;
            }
            catch (NullReferenceException e)
            {
                Debug.Log("no public property found");
            }

            return null;
        }

        private static List<PropertyData> getAllPrivatePropertySymbols(List<IPropertySymbol> propertySymbols, ClassData classData)
        {
            try
            {
                //select all private vars and display them 
                IEnumerable<IPropertySymbol> privateVariables = propertySymbols
                    .Where(m => m.DeclaredAccessibility == Accessibility.Private);

                List<PropertyData> propertyDatas = new List<PropertyData>();

                foreach (var privateVariable in privateVariables)
                {
                    PropertyData temp = new PropertyData(privateVariable, classData);
                    propertyDatas.Add(temp);
                }

                //remove later!!
                IEnumerable<string> distinctPrivateVariables = privateVariables.Select(m => m.Name);

                foreach (var variable in distinctPrivateVariables)
                {
                    Debug.Log("this property is private: " + variable);
                }

                return propertyDatas;
            }
            catch (NullReferenceException e)
            {
                Debug.Log("no private property found");
            }

            return null;
        }
        
    }
}