﻿using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using UnityEngine;

namespace CodeExplorinator
{
    public class ClassData
    {
        public INamedTypeSymbol ClassInformation { get; private set; }

        /// <summary>
        /// the graph node that belongs to this classData
        /// </summary>
        public ClassNode ClassNode;

        /// <summary>
        /// the class from which this class inherits
        /// </summary>
        public ClassData ParentClass;

        /// <summary>
        /// all the interfaces this class is implementing
        /// </summary>
        public List<ClassData> ImplementingInterfaces { get; private set; }

        /// <summary>
        /// All interfaces or classes this class is implementing/extending
        /// </summary>
        public List<ClassData> ExtendingOrImplementingClasses
        {
            get
            {
                List<ClassData> temp = new List<ClassData>();
                temp.AddRange(ImplementingInterfaces);
                if (ParentClass != null)
                {
                    temp.Add(ParentClass);
                }
                return temp;
            }
        }

        /// <summary>
        /// child classes of this class, only possible if this class is a parent class or an interface
        /// </summary>
        public List<ClassData> ChildClasses { get; private set; }

        public List<FieldData> PublicVariables { get; private set; }
        public List<FieldData> PrivateVariables { get; private set; }
        public List<PropertyData> PublicProperties { get; private set; }
        public List<PropertyData> PrivateProperties { get; private set; }
        public List<MethodData> PublicMethods { get; private set; }
        public List<MethodData> PrivateMethods { get; private set; }

        /// <summary>
        /// All references to this class in other classes within the project saved as a field
        /// </summary>
        public List<ClassFieldReferenceData> ReferencedByExternalClassField { get; private set; }

        /// <summary>
        /// All references to this class in other classes within the project saved as a property
        /// </summary>
        public List<ClassPropertyReferenceData> ReferencedByExternalClassProperty { get; private set; }

        /// <summary>
        /// All references to this class within itself saved as a field
        /// </summary>
        public List<ClassFieldReferenceData> InternalClassFieldReference { get; private set; }

        /// <summary>
        /// All references to this class within itself saved as a property
        /// </summary>
        public List<ClassPropertyReferenceData> InternalClassPropertyReference { get; private set; }

        /// <summary>
        /// All references this class contains of other classes saved as a field
        /// </summary>
        public List<ClassFieldReferenceData> IsReferencingExternalClassAsField { get; private set; }

        /// <summary>
        /// All references this class contains of other classes saved as a property
        /// </summary>
        public List<ClassPropertyReferenceData> IsReferencingExternalClassAsProperty { get; private set; }

        /// <summary>
        /// A list of all classes referencing or are referenced by this class
        /// </summary>
        public HashSet<ClassData> AllConnectedClasses { get; private set; }

        public List<ClassModifiers> ClassModifiersList { get; private set; }

        /// <summary>
        /// This is used by the ReferenceFinder class to find the parent and child classes and interfaces implemented
        /// </summary>
        public List<TypeInfo> AllParentsAndInheritanceTypes { get; private set; }

        public string ClassModifiersAsString
        {
            get
            {
                bool isStruct = false, isRecord = false, isInterface = false;

                string result = "";
                foreach (ClassModifiers modifier in ClassModifiersList)
                {
                    result += modifier.ToString().ToLower() + " ";
                    if (modifier == ClassModifiers.STRUCT) { isStruct = true; }
                    if (modifier == ClassModifiers.RECORD) { isRecord = true; }
                    if (modifier == ClassModifiers.INTERFACE) { isInterface = true; }
                }

                //If it is a normal class:
                if (!isStruct && !isRecord && !isInterface && !isInterface)
                {
                    result += "class ";
                }

                //If not empty, remove the last space
                if (!result.Equals(""))
                {
                    result = result.Substring(0, result.Length - 1);
                }

                return result;
            }
        }

        public ClassData(INamedTypeSymbol classInformation)
        {
            ImplementingInterfaces = new List<ClassData>();
            ChildClasses = new List<ClassData>();
            PublicVariables = new List<FieldData>();
            PrivateVariables = new List<FieldData>();
            PublicProperties = new List<PropertyData>();
            PrivateProperties = new List<PropertyData>();
            PublicMethods = new List<MethodData>();
            PrivateMethods = new List<MethodData>();
            ReferencedByExternalClassField = new List<ClassFieldReferenceData>();
            ReferencedByExternalClassProperty = new List<ClassPropertyReferenceData>();
            InternalClassFieldReference = new List<ClassFieldReferenceData>();
            InternalClassPropertyReference = new List<ClassPropertyReferenceData>();
            IsReferencingExternalClassAsField = new List<ClassFieldReferenceData>();
            IsReferencingExternalClassAsProperty = new List<ClassPropertyReferenceData>();
            AllConnectedClasses = new HashSet<ClassData>();
            ClassModifiersList = new List<ClassModifiers>();
            AllParentsAndInheritanceTypes = new List<TypeInfo>();
            ClassInformation = classInformation;
            DetermineModifiers();
        }

        public static string RemoveNameSpace(ITypeSymbol typeSymbol)
        {
            string result;
            INamespaceSymbol namespaceSymbol = typeSymbol.ContainingNamespace;
            if (namespaceSymbol == null || namespaceSymbol.IsGlobalNamespace || typeSymbol.ToString().LastIndexOf('.') == -1)
            {
                result = typeSymbol.ToString();
            }
            else
            {
                int namespaceLength = namespaceSymbol.ToString().Length;
                result = typeSymbol.ToString().Substring(namespaceLength + 1);
            }
            return result;
        }

        public string GetName()
        {
            return ClassInformation.Name;
        }

        public enum ClassModifiers
        {
            STATIC,
            ABSTRACT,
            SEALED,
            RECORD,
            STRUCT,
            INTERFACE,

            PARTIAL //only implementable with var isPartial = classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
        }

        public void ClearAllPublicMethodInvocations()
        {
            foreach (MethodData method in PublicMethods)
            {
                method.InvokedByInternal.Clear();
                method.InvokedByExternal.Clear();
            }
        }

        public void ClearAllPublicFieldAccesses()
        {
            foreach (FieldData field in PublicVariables)
            {
                field.AccessedByInternalMethod.Clear();
                field.AccessedByExternalMethod.Clear();
            }
        }

        public void ClearAllPublicPropertyAccesses()
        {
            foreach (PropertyData property in PublicProperties)
            {
                property.AccessedByInternalMethod.Clear();
                property.AccessedByExternalMethod.Clear();
            }
        }

        public override string ToString()
        {
            string result = ClassModifiersList.Count == 0 ? "" : "<<" + ClassModifiersAsString + ">>";
            result += GetName();
            return result;
        }

        private void DetermineModifiers()
        {
            if (ClassInformation.IsStatic)
            {
                ClassModifiersList.Add(ClassModifiers.STATIC);
            }

            if (ClassInformation.IsAbstract)
            {
                ClassModifiersList.Add((ClassInformation.TypeKind == TypeKind.Interface) ? ClassModifiers.INTERFACE : ClassModifiers.ABSTRACT);
            }

            if (ClassInformation.IsSealed)
            {
                ClassModifiersList.Add(ClassInformation.IsReferenceType ? ClassModifiers.SEALED : ClassModifiers.STRUCT);

            }

            if (ClassInformation.IsRecord)
            {
                ClassModifiersList.Add(ClassModifiers.RECORD);
            }

            if (ClassInformation.DeclaringSyntaxReferences.Length > 1) //https://github.com/dotnet/roslyn/issues/19386 
            {
                //if the class is declared as partial but not declared twice, it doesnt get recognised as partial
                ClassModifiersList.Add(ClassModifiers.PARTIAL);
            }
        }
    }
}