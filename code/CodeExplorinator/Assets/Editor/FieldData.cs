using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;

namespace CodeExplorinator
{
    public class FieldData
    {
        
        public IFieldSymbol FieldSymbol { get; private set; }
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

        public List<FieldAccessData> Accesses { get; private set; }
        public List<FieldModifiers> FieldModifiersList { get; private set; }
        
        public FieldData(IFieldSymbol fieldSymbol, ClassData containingClass)
        {
            FieldSymbol = fieldSymbol;
            ContainingClass = containingClass;
            ExternalAccesses = new List<FieldAccessData>();
            InternalAccesses = new List<FieldAccessData>();
        }

        public override string ToString()
        {
            return FieldSymbol.Name;
            Accesses = new List<FieldAccessData>();
            FieldModifiersList = new List<FieldModifiers>();
            DetermineModifiers();
        }

        public string GetName()
        {
            return FieldSymbol.Name;
        }

        public Accessibility GetAccessibility()
        {
            return FieldSymbol.DeclaredAccessibility;
        }
        
        public string GetAccessibilityAsString()
        {
            if (FieldSymbol.DeclaredAccessibility == Accessibility.ProtectedOrInternal)
            {
                return "protected internal";
            }

            if (FieldSymbol.DeclaredAccessibility == Accessibility.ProtectedAndInternal)
            {
                return "private protected";
            }
            
            //irgendwelche weirden spezialfälle sind nicht bedacht

            return FieldSymbol.DeclaredAccessibility.ToString().ToLower();
        }
        
        public ITypeSymbol GetType()
        {
            return FieldSymbol.Type;
        }

        public IFieldSymbol GetIMethodSymbol()
        {
            return FieldSymbol;
        }
        
        
        
        private void DetermineModifiers()
        {
            if (FieldSymbol.IsStatic)
            {
                FieldModifiersList.Add(FieldModifiers.STATIC);
            }
            
            if (FieldSymbol.IsAbstract)
            {
                FieldModifiersList.Add(FieldModifiers.ABSTRACT);
            }

            if (FieldSymbol.IsExtern)
            {
                FieldModifiersList.Add(FieldModifiers.EXTERN);
            }

            if ( FieldSymbol.IsOverride)
            {
                FieldModifiersList.Add(FieldModifiers.OVERRIDE);
            }

            if (FieldSymbol.IsSealed)
            {
                FieldModifiersList.Add(FieldModifiers.SEALED);
            }

            if (FieldSymbol.IsVirtual)
            {
                FieldModifiersList.Add(FieldModifiers.VIRTUAL);
            }
            
            if ( FieldSymbol.IsReadOnly)
            {
                FieldModifiersList.Add(FieldModifiers.READONLY);
            }

            if (FieldSymbol.IsConst)
            {
                FieldModifiersList.Add(FieldModifiers.CONST);
            }

            if (FieldSymbol.IsVolatile)
            {
                FieldModifiersList.Add(FieldModifiers.VOLATILE);
            }
            

            //dunno what this does but it sounds like we could use it:
            //MethodSymbol.IsConditional;
            //MethodSymbol.IsVararg;
            //MethodSymbol.IsExtensionMethod;
        }

        public enum FieldModifiers
        {
            STATIC,
            ABSTRACT,
            CONST,
            OVERRIDE,
            READONLY,
            SEALED,
            VOLATILE,
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