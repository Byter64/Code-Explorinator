using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System;
using UnityEngine.Assertions.Must;

namespace CodeExplorinator
{
    /// <summary>
    ///!!!THIS CLASS SHOULD PROBABLY BE IN CLASSDATA!!!
    /// </summary>
    public static class ReferenceFinder
    {

        #region MethodReferences
        /// <summary>
        /// Clears all references in all ClassData, 
        /// generates MethodInvocationData for all accesses in the whole compilation and sorts them into all ClassDatas.
        /// </summary>
        public static void ReFillAllPublicMethodReferences(IEnumerable<ClassData> classDatas, Compilation compilation)
        {
            foreach(ClassData classData in classDatas)
            {
                classData.ClearAllPublicMethodInvocations();
            }

            IEnumerable<MethodInvocationData> allinvocations = GenerateAllInvocationDataForCompilation(classDatas, compilation);

            //Add the invocation data to the correct MethodData
            foreach(MethodInvocationData invocation in allinvocations)
            {
                foreach (ClassData classData in classDatas)
                {
                    foreach (MethodData methodData in classData.PublicMethods)
                    {
                        if (invocation.ReferencedMethod == methodData)
                        {
                            if(invocation.ContainingMethod.ContainingClass == invocation.ReferencedMethod.ContainingClass)
                            {
                                methodData.InternalInvocations.Add(invocation);
                            }
                            else
                            {
                                methodData.ExternalInvocations.Add(invocation);
                            }
                        }
                    }
                }
            }
        }

        private static IEnumerable<MethodInvocationData> GenerateAllInvocationDataForCompilation(IEnumerable<ClassData> classDatas, Compilation compilation)
        {
            List<MethodInvocationData> allInvocations = new List<MethodInvocationData>();

            foreach(SyntaxTree syntaxTree in compilation.SyntaxTrees)
            {
                allInvocations.AddRange(GenerateAllInvocationDataForSyntaxTree(classDatas, compilation, syntaxTree));
            }
            allInvocations.RemoveAll(invocations => invocations == null);
            return allInvocations;
        }

        private static IEnumerable<MethodInvocationData> GenerateAllInvocationDataForSyntaxTree(IEnumerable<ClassData> classDatas, Compilation compilation, SyntaxTree syntaxTree)
        {
            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
            if(semanticModel== null) { return null; }

            List<MethodInvocationData> allInvocations = new List<MethodInvocationData>();

            IEnumerable<InvocationExpressionSyntax> invocations = syntaxTree.GetCompilationUnitRoot().DescendantNodes().OfType<InvocationExpressionSyntax>();
            foreach(InvocationExpressionSyntax invocation in invocations)
            {
                allInvocations.Add(GenerateMethodInvocationData(classDatas, semanticModel, invocation));
            }

            return allInvocations;
        }

        private static MethodInvocationData GenerateMethodInvocationData(IEnumerable<ClassData> classDatas, SemanticModel semanticModel, InvocationExpressionSyntax invocation)
        {
            IMethodSymbol method = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            MethodData referencedMethod = null;
            MethodData containingMethod = null;

            //███████████████████████████████████████████████████████████████████████████████
            //Searching for the declaration of the method in which access is invoked
            //███████████████████████████████████████████████████████████████████████████████
            SyntaxNode syntaxNode = invocation;
            while(syntaxNode != null && syntaxNode.GetType() != typeof(MethodDeclarationSyntax))
            {
                syntaxNode = syntaxNode.Parent;
            }

            IMethodSymbol invocator = semanticModel.GetDeclaredSymbol(syntaxNode) as IMethodSymbol;
            containingMethod = FindMethodData(classDatas,invocator);

            //███████████████████████████████████████████████████████████████████████████████
            //Searching for declaration of the invoked method
            //███████████████████████████████████████████████████████████████████████████████
            foreach (ClassData classData in classDatas)
            {
                IEnumerable<MethodData> combinedList = classData.PublicMethods.Concat(classData.PrivateMethods);
                foreach (MethodData methodData in combinedList)
                {
                    if (methodData.MethodSymbol == method)
                    {
                        referencedMethod = methodData;
                        goto EndOfGenerateMethodInvocationData;
                    }
                }
            }
            return null;

        EndOfGenerateMethodInvocationData:
            return new MethodInvocationData(containingMethod, referencedMethod);
        }
        #endregion

        #region VariableAccesses
        /// <summary>
        /// !!THIS METHOD SHOULD PROBABLY BE IN ANOTHER CLASS AND NOT HERE!!!
        /// Clears all accesses in all ClassData, 
        ///  generates FieldAccessData for all accesses in the whole compilation and sorts them into all ClassDatas.
        /// </summary>
        public static void ReFillAllPublicAccesses(IEnumerable<ClassData> classDatas, Compilation compilation)
        {
            foreach (ClassData classData in classDatas)
            {
                classData.ClearAllPublicFieldAccesses();
            }

            IEnumerable<FieldAccessData> allAccesses = GenerateAllFieldAccessDataForCompilation(classDatas, compilation);

            //Add the accesses to the correct FieldData
            foreach (FieldAccessData access in allAccesses)
            {
                foreach (ClassData classData in classDatas)
                {
                    foreach (FieldData fieldData in classData.PublicVariables)
                    {
                        if (access.ReferencedField == fieldData)
                        {
                            if (access.ContainingMethod.ContainingClass == access.ReferencedField.ContainingClass)
                            {
                                fieldData.InternalAccesses.Add(access);
                            }
                            else
                            {
                                fieldData.ExternalAccesses.Add(access);
                            }
                        }
                    }
                }
            }
        }

        private static IEnumerable<FieldAccessData> GenerateAllFieldAccessDataForCompilation(IEnumerable<ClassData> classDatas, Compilation compilation)
        {
            List<FieldAccessData> allAccesses = new List<FieldAccessData>();

            foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
            {
                allAccesses.AddRange(GenerateAllFieldAccessDataForSyntaxTree(classDatas, compilation, syntaxTree));
            }
            allAccesses.RemoveAll(invocations => invocations == null);
            return allAccesses;
        }

        private static IEnumerable<FieldAccessData> GenerateAllFieldAccessDataForSyntaxTree(IEnumerable<ClassData> classDatas, Compilation compilation, SyntaxTree syntaxTree)
        {
            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
            if (semanticModel == null) { return null; }

            List<FieldAccessData> allAccesses = new List<FieldAccessData>();

            IEnumerable<IdentifierNameSyntax> accesses = syntaxTree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>();
            foreach (IdentifierNameSyntax access in accesses)
            {
                FieldAccessData accessData = TryGenerateFieldAccessData(classDatas, compilation, semanticModel, access);
                if (accessData != null) { allAccesses.Add(accessData); }
            }

            return allAccesses;
        }

        private static FieldAccessData TryGenerateFieldAccessData(IEnumerable<ClassData> classDatas, Compilation compilation, SemanticModel semanticModel, IdentifierNameSyntax access)
        {
            IFieldSymbol field = semanticModel.GetSymbolInfo(access).Symbol as IFieldSymbol;
            if (field == null) { return null; }


            FieldData referencedField = null;
            MethodData containingMethod = null;

            //███████████████████████████████████████████████████████████████████████████████
            //Searching for the declaration of the method in which the field is accessed
            //███████████████████████████████████████████████████████████████████████████████
            SyntaxNode syntaxNode = access;
            while (syntaxNode != null && syntaxNode.GetType() != typeof(MethodDeclarationSyntax))
            {
                syntaxNode = syntaxNode.Parent;
            }

            IMethodSymbol accessor = semanticModel.GetDeclaredSymbol(syntaxNode) as IMethodSymbol;
            containingMethod = FindMethodData(classDatas, accessor);

            //███████████████████████████████████████████████████████████████████████████████
            //Searching for declaration of the accessed field
            //███████████████████████████████████████████████████████████████████████████████
            foreach (ClassData classData in classDatas)
            {
                foreach (FieldData fieldData in classData.PublicVariables)
                {
                    if (fieldData.FieldSymbol == field)
                    {
                        referencedField = fieldData;
                        goto EndOfGenerateFieldAccessData;
                    }
                }
            }
            return null;

        EndOfGenerateFieldAccessData:
            return new FieldAccessData(containingMethod, referencedField);
        }
        #endregion


        private static MethodData FindMethodData(IEnumerable<ClassData> classDatas, IMethodSymbol method)
        {
            foreach (ClassData classData in classDatas)
            {
                IEnumerable<MethodData> combinedList = classData.PublicMethods.Concat(classData.PrivateMethods);
                foreach (MethodData methodData in combinedList)
                {
                    if (methodData.MethodSymbol == method)
                    {
                        return methodData;
                    }
                }
            }

            return null;
        }
    }
}