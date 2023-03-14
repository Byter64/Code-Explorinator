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
        public List<FieldData> PublicVariables { get; private set; }
        public List<FieldData> PrivateVariables { get; private set; }
        public List<MethodData> PublicMethods { get; private set; }
        public List<MethodData> PrivateMethods { get; private set; }
        public List<ClassModifiers> ClassModifiersList { get; private set; }

        public ClassData(INamedTypeSymbol classInformation)
        {
            PublicVariables = new List<FieldData>();
            PrivateVariables = new List<FieldData>();
            PublicMethods = new List<MethodData>();
            PrivateMethods = new List<MethodData>();
            ClassModifiersList = new List<ClassModifiers>();
            ClassInformation = classInformation;
            DetermineModifiers();
        }

        //braucht noch vererbung

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
            string classString = "Class: " + GetAccessibilityAsString() + " ";
            
            foreach (var classModifier in ClassModifiersList)
            {
                classString += classModifier.ToString().ToLower() + " ";
            }

            classString += GetName() + "\n";
            
            string publicVariableString = "has out of class accessible Variables:\n";
            foreach (var publicVariable in PublicVariables)
            {
                publicVariableString += publicVariable.GetAccessibilityAsString() + " ";
                
                foreach (var fieldModifier in publicVariable.FieldModifiersList)
                {
                    publicVariableString += fieldModifier.ToString().ToLower() + " ";
                }

                publicVariableString += publicVariable.FieldSymbol.Type.Name.ToLower() + " " + publicVariable.GetName() + "\n";
            }
            
            string privateVariableString = "has private Variables:\n";
            foreach (var privateVariable in PrivateVariables)
            {
                privateVariableString += privateVariable.GetAccessibilityAsString() + " ";
                
                foreach (var fieldModifier in privateVariable.FieldModifiersList)
                {
                    privateVariableString += fieldModifier.ToString().ToLower() + " ";
                }

                privateVariableString += privateVariable.FieldSymbol.Type.Name.ToLower() + " " + privateVariable.GetName() + "\n";
            }
            
            string publicMethodString = "has out of class accessible Methods:\n";
            foreach (var publicMethod in PublicMethods)
            {
                publicMethodString += publicMethod.GetAccessibilityAsString() + " ";
                
                foreach (var methodModifier in publicMethod.MethodModifiersList)
                {
                    publicMethodString += methodModifier.ToString().ToLower() + " ";
                }

                publicMethodString += publicMethod.GetReturnType().Name.ToLower() + " " + publicMethod.GetName() + " (";

                foreach (var parameter in publicMethod.GetParameters())
                {
                    publicMethodString += parameter.Name.ToLower() + ", ";
                }
                
                publicMethodString +=  ")\n";
            }
            
            string privateMethodString = "has private Methods:\n";
            foreach (var privateMethod in PrivateMethods)
            {
                privateMethodString += privateMethod.GetAccessibilityAsString() + " ";
                
                foreach (var methodModifier in privateMethod.MethodModifiersList)
                {
                    privateMethodString += methodModifier.ToString().ToLower() + " ";
                }

                privateMethodString += privateMethod.GetReturnType().Name.ToLower() + " " + privateMethod.GetName() + " (";

                foreach (var parameter in privateMethod.GetParameters())
                {
                    privateMethodString += parameter.Name.ToLower() + ", ";
                }
                
                privateMethodString +=  ")\n";
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
            PARTIAL //not implemented
        }

        public void ClearAllPublicMethodInvocations()
        {
            foreach(MethodData method in PublicMethods)
            {
                method.InternalInvocations.Clear();
                method.ExternalInvocations.Clear();
            }
        }

        public void ClearAllPublicFieldAccesses()
        {
            foreach (FieldData field in PublicVariables)
            {
                field.InternalAccesses.Clear();
                field.ExternalAccesses.Clear();
            }
        }

        public override string ToString()
        {
            return ClassInformation.Name;
        }
        private void DetermineModifiers()
        {
            if (ClassInformation.IsStatic)
            {
                ClassModifiersList.Add(ClassModifiers.STATIC);
            }

            if (ClassInformation.IsAbstract)
            {
                ClassModifiersList.Add(ClassModifiers.ABSTRACT);
            }

            if (ClassInformation.IsSealed)
            {
                ClassModifiersList.Add(ClassModifiers.SEALED);
            }

            /*
            if (ClassInformation.IsVirtual)
            {
                ClassModifiersList.Add(ClassModifiers.VIRTUAL);
            }

            if (ClassInformation.IsReadOnly)
            {
                ClassModifiersList.Add(ClassModifiers.READONLY);
            }

            if (ClassInformation.IsExtern)
            {
                ClassModifiersList.Add(ClassModifiers.EXTERN);
            }

            if (ClassInformation.IsOverride)
            {
                ClassModifiersList.Add(ClassModifiers.OVERRIDE);
            }
            //dunno what this does but it sounds like we could use it:
            //MethodSymbol.IsConditional;
            //MethodSymbol.IsVararg;
            //MethodSymbol.IsExtensionMethod;
            
            */
        }
    }
}