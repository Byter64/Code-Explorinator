using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;
using static CodeExplorinator.Color;

namespace CodeExplorinator
{
    public class PropertyData
    {
        
        public IPropertySymbol PropertySymbol { get; private set; }
        public ClassData ContainingClass { get; private set; }
        public List<PropertyAccessData> AllAccesses
        {
            get
            {
                return AccessedByExternalMethod.Concat(AccessedByInternalMethod).ToList();
            }
        }
        
        /// <summary>
        /// All accesses to this property outside of the class it is declared in within the project
        /// </summary>
        public List<PropertyAccessData> AccessedByExternalMethod { get; private set; }
        
        /// <summary>
        /// All accesses to this property inside the class it is declared in within the project
        /// </summary>
        public List<PropertyAccessData> AccessedByInternalMethod { get; private set; }
        
        /// <summary>
        /// If the property is a reference to the class it is declared in, the class is inserted here; can be null
        /// </summary>
        public List<ClassPropertyReferenceData> ReferencingContainingClass { get; private set; }
        
        /// <summary>
        /// If the property is a reference to another class, the class is inserted here; can be null
        /// </summary>
        public List<ClassPropertyReferenceData> ReferencingExternalClass { get; private set; }
        
        public List<PropertyModifiers> PropertyModifiersList { get; private set; }

        public string PropertyModifiersAsString
        {
            get
            {
                string result = "";
                foreach (PropertyModifiers modifier in PropertyModifiersList)
                {
                    result += modifier.ToString().ToLower() + " ";
                }
                //If not empty, remove the last space
                if (!result.Equals(""))
                {
                    result = result.Substring(0, result.Length - 1);
                }

                return result;
            }
        }

        public PropertyData(IPropertySymbol propertySymbol, ClassData containingClass)
        {
            PropertySymbol = propertySymbol;
            ContainingClass = containingClass;
            AccessedByExternalMethod = new List<PropertyAccessData>();
            AccessedByInternalMethod = new List<PropertyAccessData>();
            ReferencingContainingClass = new List<ClassPropertyReferenceData>();
            ReferencingExternalClass = new List<ClassPropertyReferenceData>();
            PropertyModifiersList = new List<PropertyModifiers>();
            DetermineModifiers();
        }
        public override string ToString()
        {
            string result = PropertySymbol.DeclaredAccessibility + " ";
            result += PropertyModifiersAsString;
            if (PropertyModifiersList.Count != 0)
            {
                result += " ";
            }

            result += GetType() + " ";

            result += GetName() + ";";
            return result;
        }
        public string ToRichString()
        {
            string result = ColorText(PropertySymbol.DeclaredAccessibility.ToString(), accessebility) + " ";
            result += ColorText(PropertyModifiersAsString, modifier);
            if (PropertyModifiersList.Count != 0)
            {
                result += " ";
            }
            if (PropertySymbol.Type.IsValueType)
            {
                result += ColorText(GetType().ToString(), structType) + " ";
            }
            else
            {
                result += ColorText(GetType().ToString(), classType) + " ";
            }
            result += ColorText(GetName(), variableName) + ColorText(";", rest);

            return result;
        }

        public string GetName()
        {
            return PropertySymbol.Name;
        }

        public Accessibility GetAccessibility()
        {
            return PropertySymbol.DeclaredAccessibility;
        }
        
        public string GetAccessibilityAsString()
        {
            if (PropertySymbol.DeclaredAccessibility == Accessibility.ProtectedOrInternal)
            {
                return "protected internal";
            }

            if (PropertySymbol.DeclaredAccessibility == Accessibility.ProtectedAndInternal)
            {
                return "private protected";
            }
            
            //irgendwelche weirden spezialfälle sind nicht bedacht

            return PropertySymbol.DeclaredAccessibility.ToString().ToLower();
        }
        
        public ITypeSymbol GetType()
        {
            return PropertySymbol.Type;
        }

        public IPropertySymbol GetIPropertySymbol()
        {
            return PropertySymbol;
        }

        private void DetermineModifiers()
        {
            if (PropertySymbol.IsStatic)
            {
                PropertyModifiersList.Add(PropertyModifiers.STATIC);
            }
            
            if (PropertySymbol.IsAbstract)
            {
                PropertyModifiersList.Add(PropertyModifiers.ABSTRACT);
            }

            if (PropertySymbol.IsExtern)
            {
                PropertyModifiersList.Add(PropertyModifiers.EXTERN);
            }

            if ( PropertySymbol.IsOverride)
            {
                PropertyModifiersList.Add(PropertyModifiers.OVERRIDE);
            }

            if (PropertySymbol.IsSealed)
            {
                PropertyModifiersList.Add(PropertyModifiers.SEALED);
            }

            if (PropertySymbol.IsVirtual)
            {
                PropertyModifiersList.Add(PropertyModifiers.VIRTUAL);
            }
            
            if ( PropertySymbol.IsReadOnly)
            {
                PropertyModifiersList.Add(PropertyModifiers.READONLY);
            }


            //dunno what this does but it sounds like we could use it:
            //MethodSymbol.IsConditional;
            //MethodSymbol.IsVararg;
            //MethodSymbol.IsExtensionMethod;
        }

        public enum PropertyModifiers
        {
            STATIC,
            ABSTRACT,
            CONST, //doesnt work
            OVERRIDE,
            READONLY,
            SEALED,
            VOLATILE, //doesnt work
            VIRTUAL,
            EXTERN,
            
            NEW, //not implemented
            FIXED, //not implemented
            UNSAFE, //not implemented
            
            //not sure if these count:
            EVENT
            
        }
    }
}