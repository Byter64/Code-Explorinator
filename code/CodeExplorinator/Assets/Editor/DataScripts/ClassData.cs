﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
                string result = "";
                foreach (ClassModifiers modifier in ClassModifiersList)
                {
                    result += modifier.ToString() + " ";
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

        public string GetName()
        {
            return ClassInformation.Name;
        }

        public Accessibility GetAccessibility()
        {
            return ClassInformation.DeclaredAccessibility;
        }

        public string GetAccessibilityAsString()
        {
            if (ClassInformation.DeclaredAccessibility == Accessibility.ProtectedOrInternal)
            {
                return "protected internal";
            }

            if (ClassInformation.DeclaredAccessibility == Accessibility.ProtectedAndInternal)
            {
                return "private protected";
            }

            return ClassInformation.DeclaredAccessibility.ToString().ToLower();
        }

        public void ReadOutMyInformation()
        {
            string classString = "Class: ".ToUpper() + GetAccessibilityAsString() + " ";

            foreach (var classModifier in ClassModifiersList)
            {
                classString += classModifier.ToString().ToLower() + " ";
            }

            classString += GetName() + "\n";

            string publicVariableString = "has out of class accessible Variables:\n".ToUpper();
            foreach (var publicVariable in PublicVariables)
            {
                publicVariableString += publicVariable.GetAccessibilityAsString() + " ";

                foreach (var fieldModifier in publicVariable.FieldModifiersList)
                {
                    publicVariableString += fieldModifier.ToString().ToLower() + " ";
                }

                publicVariableString += publicVariable.FieldSymbol.Type.Name + " " + publicVariable.GetName() + "\n";
            }

            string privateVariableString = "has private Variables:\n".ToUpper();
            foreach (var privateVariable in PrivateVariables)
            {
                privateVariableString += privateVariable.GetAccessibilityAsString() + " ";

                foreach (var fieldModifier in privateVariable.FieldModifiersList)
                {
                    privateVariableString += fieldModifier.ToString().ToLower() + " ";
                }

                privateVariableString += privateVariable.FieldSymbol.Type.Name + " " + privateVariable.GetName() + "\n";
            }

            string publicMethodString = "has out of class accessible Methods:\n".ToUpper();
            foreach (var publicMethod in PublicMethods)
            {
                publicMethodString += publicMethod.GetAccessibilityAsString() + " ";

                foreach (var methodModifier in publicMethod.MethodModifiersList)
                {
                    publicMethodString += methodModifier.ToString().ToLower() + " ";
                }

                publicMethodString += publicMethod.GetReturnType().Name + " " + publicMethod.GetName() + " (";

                foreach (var parameter in publicMethod.GetParameters())
                {
                    publicMethodString += parameter.Type.Name + " " + parameter.Name + ", ";
                }

                if (publicMethod.GetParameters().Length > 0)
                {
                    publicMethodString = publicMethodString.Remove(publicMethodString.Length - 2);
                }

                publicMethodString += ")\n";
            }

            string privateMethodString = "has private Methods:\n".ToUpper();
            foreach (var privateMethod in PrivateMethods)
            {
                privateMethodString += privateMethod.GetAccessibilityAsString() + " ";

                foreach (var methodModifier in privateMethod.MethodModifiersList)
                {
                    privateMethodString += methodModifier.ToString().ToLower() + " ";
                }

                privateMethodString += privateMethod.GetReturnType().Name + " " + privateMethod.GetName() + " (";

                foreach (var parameter in privateMethod.GetParameters())
                {
                    privateMethodString += parameter.Type.Name + " " + parameter.Name + ", ";
                }

                if (privateMethod.GetParameters().Length > 0)
                {
                    privateMethodString = privateMethodString.Remove(privateMethodString.Length - 2);
                }


                privateMethodString += ")\n";
            }

            string result = classString + publicVariableString + privateVariableString + publicMethodString +
                            privateMethodString;

            Debug.Log(result);
        }

        public enum ClassModifiers
        {
            STATIC,
            ABSTRACT,
            SEALED,
            RECORD,
            STRUCT,
            INTERFACE,

            PARTIAL //not implemented
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

        public string ToRichString()
        {
            return ToString();
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
                ClassModifiersList.Add(ClassModifiers.SEALED);
            }

            if (ClassInformation.IsRecord)
            {
                ClassModifiersList.Add(ClassModifiers.RECORD);
            }

            if (!ClassInformation.IsReferenceType)
            {
                ClassModifiersList.Add(ClassModifiers.STRUCT);
            }
        }
    }
}