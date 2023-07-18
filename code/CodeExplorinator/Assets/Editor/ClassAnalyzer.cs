using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeExplorinator
{
    public static class ClassAnalyzer
    {
        /// <summary>
        /// checks if the file has multiple class declarations and generates the information for each class
        /// </summary>
        /// <param name="root">the root of the cs file</param>
        /// <param name="model">the semantic model of the assembly</param>
        /// <returns></returns>
        public static List<ClassData> GenerateAllClassInfo(CompilationUnitSyntax root, SemanticModel model)
        {
            IEnumerable<TypeDeclarationSyntax> classDeclarations = root.DescendantNodes()
                .OfType<TypeDeclarationSyntax>();

            List<ClassData> classDatas = new List<ClassData>();

            foreach (var classDeclaration in classDeclarations)
            {
                classDatas.Add(GenerateClassInfo(classDeclaration, model));
            }

            return classDatas;
        }

        /// <summary>
        /// finds all variable and method declarations, saves them as FieldData/MethodData and sorts them for Accessibility, generates a ClassData
        /// </summary>
        /// <param name="root">root of the class, aka class declaration</param>
        /// <param name="model">the semantic model of the assembly</param>
        /// <returns></returns>
        private static ClassData GenerateClassInfo(TypeDeclarationSyntax root, SemanticModel model)
        {
            ClassData classData = new ClassData(model.GetDeclaredSymbol(root));

            List<TypeInfo> allParentsAndInheritance = FindAllParentInformation(root, model);
            classData.AllParentsAndInheritanceTypes.AddRange(allParentsAndInheritance);

            List<IFieldSymbol> allVariables = FindAllFieldDeclarations(root, model);
            List<FieldData> publicVariables = getAllPublicFieldSymbols(allVariables, classData);
            List<FieldData> privateVariables = getAllPrivateFieldSymbols(allVariables, classData);

            classData.PublicVariables.AddRange(publicVariables);
            classData.PrivateVariables.AddRange(privateVariables);


            List<IPropertySymbol> allProperties = FindAllPropertyDeclarations(root, model);
            List<PropertyData> publicProperties = getAllPublicPropertySymbols(allProperties, classData);
            List<PropertyData> privateProperties = getAllPrivatePropertySymbols(allProperties, classData);

            classData.PublicProperties.AddRange(publicProperties);
            classData.PrivateProperties.AddRange(privateProperties);


            List<IMethodSymbol> allMethods = FindAllMethods(root, model);
            List<MethodData> publicMethods = getAllPublicMethodSymbols(allMethods, classData);
            List<MethodData> privateMethods = getAllPrivateMethodSymbols(allMethods, classData);

            classData.PublicMethods.AddRange(publicMethods);
            classData.PrivateMethods.AddRange(privateMethods);

            return classData;
        }


        #region AnalyzingInheritance

        private static List<TypeInfo> FindAllParentInformation(SyntaxNode root, SemanticModel model)
        {
            IEnumerable<SimpleBaseTypeSyntax> simpleBaseTypeSyntaxes = root.DescendantNodes()
                .OfType<SimpleBaseTypeSyntax>();

            List<TypeInfo> parentTypes = new List<TypeInfo>();
            foreach (var simpleBaseTypeSyntax in simpleBaseTypeSyntaxes)
            {
                IEnumerable<IdentifierNameSyntax> parents =
                    simpleBaseTypeSyntax.DescendantNodes().OfType<IdentifierNameSyntax>();
                foreach (var parent in parents)
                {
                    parentTypes.Add(model.GetTypeInfo(parent));
                }
            }


            return parentTypes;
        }

        #endregion

        #region AnalyzingMethods

        private static List<IMethodSymbol> FindAllMethods(SyntaxNode root, SemanticModel model)
        {
            // Use the syntax model to find all methoddeclarations
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

                return methodDatas;
            }
            catch (NullReferenceException)
            {
                
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

                return methodDatas;
            }
            catch (NullReferenceException)
            {
                
            }

            return null;
        }

        #endregion

        #region AnalyzingFields

        private static List<IFieldSymbol> FindAllFieldDeclarations(SyntaxNode root, SemanticModel model)
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

                return fieldDatas;
            }
            catch (NullReferenceException)
            {
                
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

                return fieldDatas;
            }
            catch (NullReferenceException)
            {
                
            }

            return null;
        }

        #endregion

        #region AnalyzingProperties

        private static List<IPropertySymbol> FindAllPropertyDeclarations(SyntaxNode root,
            SemanticModel model)
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

        private static List<PropertyData> getAllPublicPropertySymbols(List<IPropertySymbol> propertySymbols,
            ClassData classData)
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

                return propertyDatas;
            }
            catch (NullReferenceException)
            {
                
            }

            return null;
        }

        private static List<PropertyData> getAllPrivatePropertySymbols(List<IPropertySymbol> propertySymbols,
            ClassData classData)
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

                return propertyDatas;
            }
            catch (NullReferenceException)
            {
                
            }

            return null;
        }

        #endregion
    }
}