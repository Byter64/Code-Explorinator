using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace CodeExplorinator
{
    public static class ReferenceFinder
    {
        public static void RefillAllReferences(List<ClassData> classDatas, Compilation compilation)
        {
            ReFillAllPublicMethodReferences(classDatas, compilation);
            ReFillAllPublicAccesses(classDatas, compilation);
            ReFillAllPublicPropertyAccesses(classDatas, compilation);
            ReFillAllClassReferences(classDatas, compilation);
        }


        #region MethodReferences

        /// <summary>
        /// Clears all references in all ClassData, 
        /// generates MethodInvocationData for all accesses in the whole compilation and sorts them into all ClassDatas.
        /// </summary>
        public static void ReFillAllPublicMethodReferences(IEnumerable<ClassData> classDatas, Compilation compilation)
        {
            foreach (ClassData classData in classDatas)
            {
                classData.ClearAllPublicMethodInvocations();
            }

            IEnumerable<MethodInvocationData> allinvocations =
                GenerateAllInvocationDataForCompilation(classDatas, compilation);

            //Add the invocation data to the correct MethodData
            foreach (MethodInvocationData invocation in allinvocations)
            {
                foreach (ClassData classData in classDatas)
                {
                    foreach (MethodData methodData in classData.PublicMethods)
                    {
                        if (invocation.ReferencedMethod == methodData)
                        {
                            if (invocation.ContainingMethod.ContainingClass ==
                                invocation.ReferencedMethod.ContainingClass)
                            {
                                methodData.InvokedByInternal.Add(invocation);
                                invocation.ContainingMethod.IsInvokingInternalMethods.Add(invocation);
                            }
                            else
                            {
                                methodData.InvokedByExternal.Add(invocation);
                                invocation.ContainingMethod.IsInvokingExternalMethods.Add(invocation);
                            }

                            methodData.AllConnectedMethods.Add(invocation.ContainingMethod);
                            invocation.ContainingMethod.AllConnectedMethods.Add(methodData);
                        }
                    }

                    foreach (var methodData in classData.PrivateMethods)
                    {
                        if (invocation.ReferencedMethod == methodData)
                        {
                            methodData.InvokedByInternal.Add(invocation);
                            invocation.ContainingMethod.IsInvokingInternalMethods.Add(invocation);

                            methodData.AllConnectedMethods.Add(invocation.ContainingMethod);
                            invocation.ContainingMethod.AllConnectedMethods.Add(methodData);
                        }
                    }
                }
            }
        }

        private static IEnumerable<MethodInvocationData> GenerateAllInvocationDataForCompilation(
            IEnumerable<ClassData> classDatas,
            Compilation compilation)
        {
            List<MethodInvocationData> allInvocations = new List<MethodInvocationData>();

            foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
            {
                allInvocations.AddRange(GenerateAllInvocationDataForSyntaxTree(classDatas, compilation, syntaxTree));
            }

            return allInvocations;
        }

        private static IEnumerable<MethodInvocationData> GenerateAllInvocationDataForSyntaxTree(
            IEnumerable<ClassData> classDatas,
            Compilation compilation, SyntaxTree syntaxTree)
        {
            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
            if (semanticModel == null)
            {
                return null;
            }

            List<MethodInvocationData> allInvocations = new List<MethodInvocationData>();

            IEnumerable<InvocationExpressionSyntax> invocations = syntaxTree.GetCompilationUnitRoot().DescendantNodes()
                .OfType<InvocationExpressionSyntax>();
            foreach (InvocationExpressionSyntax invocation in invocations)
            {
                MethodInvocationData data = GenerateMethodInvocationData(classDatas, semanticModel, invocation);
                allInvocations.Add(data);
            }

            allInvocations.RemoveAll(invocations => invocations == null);
            return allInvocations;
        }

        private static MethodInvocationData GenerateMethodInvocationData(IEnumerable<ClassData> classDatas,
            SemanticModel semanticModel, InvocationExpressionSyntax invocation)
        {
            IMethodSymbol method = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            MethodData referencedMethod = null;
            MethodData containingMethod = null;

            //███████████████████████████████████████████████████████████████████████████████
            //Searching for the declaration of the method in which access is invoked
            //███████████████████████████████████████████████████████████████████████████████
            SyntaxNode syntaxNode = invocation;
            while (syntaxNode != null && syntaxNode.GetType() != typeof(MethodDeclarationSyntax))
            {
                syntaxNode = syntaxNode.Parent;
            }

            if (syntaxNode == null)
            {
                return null;
            }

            IMethodSymbol invocator = semanticModel.GetDeclaredSymbol(syntaxNode) as IMethodSymbol;
            if (invocator == null)
            {
                return null;
            }

            containingMethod = FindMethodData(classDatas, invocator);
            if (containingMethod == null)
            {
                return null;
            }

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
        /// Clears all accesses in all ClassData, 
        ///  generates FieldAccessData for all accesses in the whole compilation and sorts them into all ClassDatas.
        /// </summary>
        public static void ReFillAllPublicAccesses(IEnumerable<ClassData> classDatas, Compilation compilation)
        {
            foreach (ClassData classData in classDatas)
            {
                classData.ClearAllPublicFieldAccesses();
            }

            IEnumerable<FieldAccessData>
                allAccesses = GenerateAllFieldAccessDataForCompilation(classDatas, compilation);

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
                                fieldData.AccessedByInternalMethod.Add(access);
                                access.ContainingMethod.IsAccessingInternalField.Add(access);
                            }
                            else
                            {
                                fieldData.AccessedByExternalMethod.Add(access);
                                access.ContainingMethod.IsAccessingExternalField.Add(access);
                            }
                        }
                    }

                    foreach (FieldData fieldData in classData.PrivateVariables)
                    {
                        if (access.ReferencedField == fieldData)
                        {
                            fieldData.AccessedByInternalMethod.Add(access);
                            access.ContainingMethod.IsAccessingInternalField.Add(access);
                        }
                    }
                }
            }
        }

        private static IEnumerable<FieldAccessData> GenerateAllFieldAccessDataForCompilation(
            IEnumerable<ClassData> classDatas, Compilation compilation)
        {
            List<FieldAccessData> allAccesses = new List<FieldAccessData>();

            foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
            {
                allAccesses.AddRange(GenerateAllFieldAccessDataForSyntaxTree(classDatas, compilation, syntaxTree));
            }

            return allAccesses;
        }

        private static IEnumerable<FieldAccessData> GenerateAllFieldAccessDataForSyntaxTree(
            IEnumerable<ClassData> classDatas, Compilation compilation, SyntaxTree syntaxTree)
        {
            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
            if (semanticModel == null)
            {
                return null;
            }

            List<FieldAccessData> allAccesses = new List<FieldAccessData>();

            IEnumerable<IdentifierNameSyntax> accesses =
                syntaxTree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>();
            foreach (IdentifierNameSyntax access in accesses)
            {
                FieldAccessData accessData = TryGenerateFieldAccessData(classDatas, compilation, semanticModel, access);
                allAccesses.Add(accessData);
            }

            allAccesses.RemoveAll(invocations => invocations == null);
            return allAccesses;
        }

        private static FieldAccessData TryGenerateFieldAccessData(IEnumerable<ClassData> classDatas,
            Compilation compilation, SemanticModel semanticModel, IdentifierNameSyntax access)
        {
            IFieldSymbol field = semanticModel.GetSymbolInfo(access).Symbol as IFieldSymbol;
            if (field == null)
            {
                return null;
            }


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

            if (syntaxNode == null)
            {
                return null;
            }

            IMethodSymbol accessor = semanticModel.GetDeclaredSymbol(syntaxNode) as IMethodSymbol;
            if (accessor == null)
            {
                return null;
            }

            containingMethod = FindMethodData(classDatas, accessor);
            if (containingMethod == null)
            {
                return null;
            }

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


        #region PropertyAccesses

        /// <summary>
        /// Clears all accesses in all ClassData, 
        ///  generates PropertyAccessData for all accesses in the whole compilation and sorts them into all ClassDatas.
        /// </summary>
        public static void ReFillAllPublicPropertyAccesses(IEnumerable<ClassData> classDatas, Compilation compilation)
        {
            foreach (ClassData classData in classDatas)
            {
                classData.ClearAllPublicPropertyAccesses();
            }

            IEnumerable<PropertyAccessData>
                allAccesses = GenerateAllPropertyAccessDataForCompilation(classDatas, compilation);

            //Add the accesses to the correct FieldData
            foreach (PropertyAccessData access in allAccesses)
            {
                foreach (ClassData classData in classDatas)
                {
                    foreach (PropertyData propertyData in classData.PublicProperties)
                    {
                        if (access.ReferencedProperty == propertyData)
                        {
                            if (access.ContainingMethod.ContainingClass == access.ReferencedProperty.ContainingClass)
                            {
                                propertyData.AccessedByInternalMethod.Add(access);
                                access.ContainingMethod.IsAccessingInternalProperty.Add(access);
                            }
                            else
                            {
                                propertyData.AccessedByExternalMethod.Add(access);
                                access.ContainingMethod.IsAccessingExternalProperty.Add(access);
                            }
                        }
                    }

                    foreach (PropertyData propertyData in classData.PrivateProperties)
                    {
                        if (access.ReferencedProperty == propertyData)
                        {
                            propertyData.AccessedByInternalMethod.Add(access);
                            access.ContainingMethod.IsAccessingInternalProperty.Add(access);
                        }
                    }
                }
            }
        }

        private static IEnumerable<PropertyAccessData> GenerateAllPropertyAccessDataForCompilation(
            IEnumerable<ClassData> classDatas, Compilation compilation)
        {
            List<PropertyAccessData> allAccesses = new List<PropertyAccessData>();

            foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
            {
                allAccesses.AddRange(GenerateAllPropertyAccessDataForSyntaxTree(classDatas, compilation, syntaxTree));
            }

            return allAccesses;
        }

        private static IEnumerable<PropertyAccessData> GenerateAllPropertyAccessDataForSyntaxTree(
            IEnumerable<ClassData> classDatas, Compilation compilation, SyntaxTree syntaxTree)
        {
            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
            if (semanticModel == null)
            {
                return null;
            }

            List<PropertyAccessData> allAccesses = new List<PropertyAccessData>();

            IEnumerable<IdentifierNameSyntax> accesses =
                syntaxTree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>();
            foreach (IdentifierNameSyntax access in accesses)
            {
                PropertyAccessData accessData =
                    TryGeneratePropertyAccessData(classDatas, compilation, semanticModel, access);
                allAccesses.Add(accessData);
            }

            allAccesses.RemoveAll(invocations => invocations == null);
            return allAccesses;
        }

        private static PropertyAccessData TryGeneratePropertyAccessData(IEnumerable<ClassData> classDatas,
            Compilation compilation, SemanticModel semanticModel, IdentifierNameSyntax access)
        {
            IPropertySymbol property = semanticModel.GetSymbolInfo(access).Symbol as IPropertySymbol;
            if (property == null)
            {
                return null;
            }


            PropertyData referencedProperty = null;
            MethodData containingMethod = null;

            //███████████████████████████████████████████████████████████████████████████████
            //Searching for the declaration of the method in which the field is accessed
            //███████████████████████████████████████████████████████████████████████████████
            SyntaxNode syntaxNode = access;
            while (syntaxNode != null && syntaxNode.GetType() != typeof(MethodDeclarationSyntax))
            {
                syntaxNode = syntaxNode.Parent;
            }

            if (syntaxNode == null)
            {
                return null;
            }

            IMethodSymbol accessor = semanticModel.GetDeclaredSymbol(syntaxNode) as IMethodSymbol;
            if (accessor != null)
            {
                return null;
            }

            containingMethod = FindMethodData(classDatas, accessor);
            if (containingMethod == null)
            {
                return null;
            }

            //███████████████████████████████████████████████████████████████████████████████
            //Searching for declaration of the accessed property
            //███████████████████████████████████████████████████████████████████████████████
            foreach (ClassData classData in classDatas)
            {
                foreach (PropertyData propertyData in classData.PublicProperties)
                {
                    if (propertyData.PropertySymbol == property)
                    {
                        referencedProperty = propertyData;
                        goto EndOfGeneratePropertyAccessData;
                    }
                }
            }

            return null;

            EndOfGeneratePropertyAccessData:

            return new PropertyAccessData(containingMethod, referencedProperty);
        }

        #endregion


        #region ClassReferences

        public static void ReFillAllClassReferences(IEnumerable<ClassData> classDatas, Compilation compilation)
        {
            //creates all ClassFieldReferenceData and ClassPropertyReferenceData and
            //inserts these references into the ClassData, FieldData and PropertyData
            foreach (ClassData classData in classDatas)
            {
                FindAllClassFields(classData, classDatas);
                FindAllClassProperties(classData, classDatas);
                FindAllInheritance(classData, classDatas);
            }
        }

        private static void FindAllClassFields(ClassData classData, IEnumerable<ClassData> classDatas)
        {
            foreach (FieldData fieldData in classData.PublicVariables.Concat(classData.PrivateVariables).ToList())
            {

                foreach (ClassData referencedClass in classDatas)
                {
                    //if the class is referenced by this field, set information
                    if (SymbolEqualityComparer.Default.Equals(referencedClass.ClassInformation, fieldData.GetType()))
                    {
                        SetFieldReferenceInformation(referencedClass, fieldData);
                    }
                }
            }
        }

        private static void SetFieldReferenceInformation(ClassData referencedClass, FieldData fieldData)
        {
            ClassFieldReferenceData reference = new ClassFieldReferenceData(referencedClass, fieldData);

            //if the class has declared an instance of itself it is sorted in "internal" lists, otherwise in "external" lists
            if (referencedClass == fieldData.ContainingClass)
            {
                referencedClass.InternalClassFieldReference.Add(reference);
                fieldData.ReferencingContainingClass.Add(reference);
            }
            else
            {
                referencedClass.ReferencedByExternalClassField.Add(reference);
                fieldData.ReferencingExternalClass.Add(reference);
                fieldData.ContainingClass.IsReferencingExternalClassAsField.Add(reference);

                referencedClass.AllConnectedClasses.Add(fieldData.ContainingClass);
                fieldData.ContainingClass.AllConnectedClasses.Add(referencedClass);
            }
        }

        private static void FindAllClassProperties(ClassData classData, IEnumerable<ClassData> classDatas)
        {
            foreach (PropertyData propertyData in classData.PublicProperties.Concat(classData.PrivateProperties)
                         .ToList())
            {
                foreach (ClassData referencedClass in classDatas)
                {
                    //if the class is referenced by this property set information
                    if (SymbolEqualityComparer.Default.Equals(referencedClass.ClassInformation, propertyData.GetType()))
                    {
                        ClassPropertyReferenceData reference =
                            new ClassPropertyReferenceData(referencedClass, propertyData);

                        //if the class has declared an instance of itself it is sorted in "internal" lists, otherwise in "external" lists
                        if (referencedClass == propertyData.ContainingClass)
                        {
                            referencedClass.InternalClassPropertyReference.Add(reference);
                            propertyData.ReferencingContainingClass.Add(reference);
                        }
                        else
                        {
                            referencedClass.ReferencedByExternalClassProperty.Add(reference);
                            propertyData.ReferencingExternalClass.Add(reference);
                            propertyData.ContainingClass.IsReferencingExternalClassAsProperty.Add(reference);

                            if (!referencedClass.AllConnectedClasses.Contains(propertyData.ContainingClass))
                            {
                                referencedClass.AllConnectedClasses.Add(propertyData.ContainingClass);
                            }

                            if (!propertyData.ContainingClass.AllConnectedClasses.Contains(referencedClass))
                            {
                                propertyData.ContainingClass.AllConnectedClasses.Add(referencedClass);
                            }
                        }
                    }
                }
            }
        }

        private static void FindAllInheritance(ClassData analyzedClass, IEnumerable<ClassData> classDatas)
        {
            foreach (TypeInfo type in analyzedClass.AllParentsAndInheritanceTypes)
            {
                foreach (ClassData randomClass in classDatas)
                {
                    if (SymbolEqualityComparer.Default.Equals(randomClass.ClassInformation, type.Type))
                    {
                        if (randomClass.ClassInformation.TypeKind == TypeKind.Interface)
                        {
                            analyzedClass.ImplementingInterfaces.Add(randomClass);
                            analyzedClass.AllConnectedClasses.Add(randomClass);

                            randomClass.AllConnectedClasses.Add(analyzedClass);
                            randomClass.ChildClasses.Add(analyzedClass);
                        }
                        else
                        {
                            analyzedClass.ParentClass = randomClass;
                            analyzedClass.AllConnectedClasses.Add(randomClass);

                            randomClass.AllConnectedClasses.Add(analyzedClass);
                            randomClass.ChildClasses.Add(analyzedClass);
                        }
                    }
                }
            }
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