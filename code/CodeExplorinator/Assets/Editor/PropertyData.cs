using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;

namespace CodeExplorinator
{
    public class PropertyData
    {
        
        public IPropertySymbol PropertySymbol { get; private set; }
        public ClassData ContainingClass { get; private set; }
        public List<FieldAccessData> AllAccesses
        {
            get
            {
                return ExternalAccesses.Concat(InternalAccesses).ToList();
            }
        }
        public List<FieldAccessData> ExternalAccesses { get; private set; }
        public List<FieldAccessData> InternalAccesses { get; private set; }
        public List<PropertyModifiers> PropertyModifiersList { get; private set; }
        
        public PropertyData(IPropertySymbol propertySymbol, ClassData containingClass)
        {
            PropertySymbol = propertySymbol;
            ContainingClass = containingClass;
            ExternalAccesses = new List<FieldAccessData>();
            InternalAccesses = new List<FieldAccessData>();
            PropertyModifiersList = new List<PropertyModifiers>();
            DetermineModifiers();
        }

        public override string ToString()
        {
            return PropertySymbol.Name;

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