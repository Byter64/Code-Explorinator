using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;
using static CodeExplorinator.MethodData;

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
                return AccessedByExternal.Concat(AccessedByInternal).ToList();
            }
        }
        
        /// <summary>
        /// All accesses to this field outside of the class it is declared in within the project
        /// </summary>
        public List<FieldAccessData> AccessedByExternal { get; private set; }
        
        /// <summary>
        /// All accesses to this field inside the class it is declared in within the project
        /// </summary>
        public List<FieldAccessData> AccessedByInternal { get; private set; }

        public List<FieldModifiers> FieldModifiersList { get; private set; }

        public string FieldModifiersAsString
        {
            get
            {
                string result = "";
                foreach (FieldModifiers modifier in FieldModifiersList)
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

        public FieldData(IFieldSymbol fieldSymbol, ClassData containingClass)
        {
            FieldSymbol = fieldSymbol;
            ContainingClass = containingClass;
            AccessedByExternal = new List<FieldAccessData>();
            AccessedByInternal = new List<FieldAccessData>();
            FieldModifiersList = new List<FieldModifiers>();
            DetermineModifiers();
        }

        public override string ToString()
        {
            string result = FieldSymbol.DeclaredAccessibility + " ";
            result += FieldModifiersAsString;
            if (FieldModifiersList.Count != 0)
            {
                result += " ";
            }

            result += GetType() + " ";

            result += GetName() + ";";
            return result;
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

        public IFieldSymbol GetIFieldSymbol()
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