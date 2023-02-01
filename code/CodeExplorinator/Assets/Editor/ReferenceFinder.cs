using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System;
using UnityEngine.Assertions.Must;

namespace CodeExplorinator
{
    public class ReferenceFinder
    {
        private List<ClassData> allClassesInCompilation;
        private Compilation compilation; 

        public ReferenceFinder(List<ClassData> allClassesInCompilation, Compilation compilation)
        {
            this.allClassesInCompilation = allClassesInCompilation;
            this.compilation = compilation;
        }

        #region MethodReferences
        /// <summary>
        /// !!THIS METHOD SHOULD PROBABLY BE IN ANOTHER CLASS AND NOT HERE!!!
        /// Clears all references in all ClassData, 
        ///  generates MethodInvocationData for all accesses in the whole compilation and sorts them into all ClassDatas.
        /// </summary>
        public void ReFillAllPublicMethodReferences()
        {
            foreach(ClassData classData in allClassesInCompilation)
            {
                classData.ClearAllPublicMethodInvocations();
            }

            IEnumerable<MethodInvocationData> allinvocations = GenerateAllInvocationDataForCompilation();

            //Add the invocation to the correct MethodData
            foreach(MethodInvocationData invocation in allinvocations)
            {
                foreach (ClassData classData in allClassesInCompilation)
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

        private IEnumerable<MethodInvocationData> GenerateAllInvocationDataForCompilation()
        {
            List<MethodInvocationData> allInvocations = new List<MethodInvocationData>();

            foreach(SyntaxTree syntaxTree in compilation.SyntaxTrees)
            {
                allInvocations.AddRange(GenerateAllInvocationDataForSyntaxTree(syntaxTree));
            }
            allInvocations.RemoveAll(invocations => invocations == null);
            return allInvocations;
        }

        private IEnumerable<MethodInvocationData> GenerateAllInvocationDataForSyntaxTree(SyntaxTree syntaxTree)
        {
            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
            if(semanticModel== null) { return null; }

            List<MethodInvocationData> allInvocations = new List<MethodInvocationData>();

            IEnumerable<InvocationExpressionSyntax> invocations = syntaxTree.GetCompilationUnitRoot().DescendantNodes().OfType<InvocationExpressionSyntax>();
            foreach(InvocationExpressionSyntax invocation in invocations)
            {
                allInvocations.Add(GenerateMethodInvocationData(semanticModel, invocation));
            }

            return allInvocations;
        }

        private MethodInvocationData GenerateMethodInvocationData(SemanticModel semanticModel, InvocationExpressionSyntax invocation)
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
            containingMethod = FindMethodData(invocator);

            //███████████████████████████████████████████████████████████████████████████████
            //Searching for declaration of the invoked method
            //███████████████████████████████████████████████████████████████████████████████
            foreach (ClassData classData in allClassesInCompilation)
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
        public void ReFillAllPublicAccesses()
        {
            foreach (ClassData classData in allClassesInCompilation)
            {
                classData.ClearAllPublicFieldAccesses();
            }

            IEnumerable<FieldAccessData> allAccesses = GenerateAllFieldAccessDataForCompilation();

            //Add the accesses to the correct FieldData
            foreach (FieldAccessData access in allAccesses)
            {
                foreach (ClassData classData in allClassesInCompilation)
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

        private IEnumerable<FieldAccessData> GenerateAllFieldAccessDataForCompilation()
        {
            List<FieldAccessData> allAccesses = new List<FieldAccessData>();

            foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
            {
                allAccesses.AddRange(GenerateAllFieldAccessDataForSyntaxTree(syntaxTree));
            }
            allAccesses.RemoveAll(invocations => invocations == null);
            return allAccesses;
        }

        private IEnumerable<FieldAccessData> GenerateAllFieldAccessDataForSyntaxTree(SyntaxTree syntaxTree)
        {
            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
            if (semanticModel == null) { return null; }

            List<FieldAccessData> allAccesses = new List<FieldAccessData>();

            IEnumerable<IdentifierNameSyntax> accesses = syntaxTree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>();
            foreach (IdentifierNameSyntax access in accesses)
            {
                FieldAccessData accessData = TryGenerateFieldAccessData(semanticModel, access);
                if (accessData != null) { allAccesses.Add(accessData); }
            }

            return allAccesses;
        }

        private FieldAccessData TryGenerateFieldAccessData(SemanticModel semanticModel, IdentifierNameSyntax access)
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
            containingMethod = FindMethodData(accessor);

            //███████████████████████████████████████████████████████████████████████████████
            //Searching for declaration of the accessed field
            //███████████████████████████████████████████████████████████████████████████████
            foreach (ClassData classData in allClassesInCompilation)
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


        private MethodData FindMethodData(IMethodSymbol method)
        {
            foreach (ClassData classData in allClassesInCompilation)
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