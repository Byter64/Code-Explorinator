using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using static CodeExplorinator.Color;

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
                return AccessedByExternalMethod.Concat(AccessedByInternalMethod).ToList();
            }
        }

        /// <summary>
        /// All accesses to this field outside of the class it is declared in within the project
        /// </summary>
        public List<FieldAccessData> AccessedByExternalMethod { get; private set; }

        /// <summary>
        /// All accesses to this field inside the class it is declared in within the project
        /// </summary>
        public List<FieldAccessData> AccessedByInternalMethod { get; private set; }

        /// <summary>
        /// If the field is a reference to the class it is declared in, the class is inserted here; can be null
        /// </summary>
        public List<ClassFieldReferenceData> ReferencingContainingClass { get; private set; }

        /// <summary>
        /// If the field is a reference to another class, the class is inserted here; can be null
        /// </summary>
        public List<ClassFieldReferenceData> ReferencingExternalClass { get; private set; }

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
            AccessedByExternalMethod = new List<FieldAccessData>();
            AccessedByInternalMethod = new List<FieldAccessData>();
            ReferencingContainingClass = new List<ClassFieldReferenceData>();
            ReferencingExternalClass = new List<ClassFieldReferenceData>();
            FieldModifiersList = new List<FieldModifiers>();
            DetermineModifiers();
        }

        public override string ToString()
        {
            string accessibility = FieldSymbol.DeclaredAccessibility.ToString();
            accessibility = char.ToLower(accessibility[0]) + accessibility.Substring(1);

            string result = accessibility + " ";
            result += FieldModifiersAsString;
            if (FieldModifiersList.Count != 0)
            {
                result += " ";
            }

            result += ClassData.RemoveNameSpace(GetFieldType()) + " ";

            result += GetName() + ";";
            return result;
        }
        public string ToRichString()
        {
            string accessibility = FieldSymbol.DeclaredAccessibility.ToString();
            accessibility = char.ToLower(accessibility[0]) + accessibility.Substring(1);

            string result = ColorText(accessibility, Color.accessibility) + " ";
            result += ColorText(FieldModifiersAsString, modifier);
            if (FieldModifiersList.Count != 0)
            {
                result += " ";
            }

            if (FieldSymbol.Type.IsValueType)
            {
                result += ColorText(ClassData.RemoveNameSpace(GetFieldType()), structType) + " ";
            }
            else
            {
                result += ColorText(ClassData.RemoveNameSpace(GetFieldType()), classType) + " ";
            }

            result += ColorText(GetName(), variableName) + ColorText(";", rest);

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

            return FieldSymbol.DeclaredAccessibility.ToString().ToLower();
        }

        public ITypeSymbol GetFieldType()
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

            if (FieldSymbol.IsExtern)
            {
                FieldModifiersList.Add(FieldModifiers.EXTERN);
            }

            if (FieldSymbol.IsReadOnly)
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

        }

        public enum FieldModifiers
        {
            STATIC,
            CONST,
            READONLY,
            VOLATILE,

            EXTERN, //not tested

            NEW, //not implemented
            FIXED, //not implemented
            UNSAFE, //not implemented
            EVENT

        }
    }
}