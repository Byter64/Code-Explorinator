using System;
using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;
using static CodeExplorinator.ClassData;

namespace CodeExplorinator
{
    public class MethodData
    {
        /// <summary>
        /// The underlying IMethodSymbol which contains all important information about the method itself
        /// </summary>
        public  IMethodSymbol MethodSymbol { get; private set; }
        /// <summary>
        /// The class in which this method is defined and declared in
        /// </summary>
        public ClassData ContainingClass { get; private set;}
        /// <summary>
        /// All invocations to this method within the project (Concatination of in and external invocations)
        /// </summary>
        public List<MethodInvocationData> AllInvocations
        {
            get
            {
                return InvokedByExternal.Concat(InvokedByInternal).ToList();
            }
        }

        /// <summary>
        /// All invocations to this method outside of the class it is declared in within the project
        /// </summary>
        public List<MethodInvocationData> InvokedByExternal { get; private set; }

        /// <summary>
        /// All invocations to this method inside the class it is declared in within the project
        /// </summary>
        public List<MethodInvocationData> InvokedByInternal { get; private set; }

        /// <summary>
        /// All invocations this method makes to other methods in its own class
        /// </summary>
        public List<MethodInvocationData> IsInvokingInternalMethods { get; private set; }
        
        /// <summary>
        /// All invocations this method makes to other methods outside its own class
        /// </summary>
        public List<MethodInvocationData> IsInvokingExternalMethods { get; private set; }

        /// <summary>
        /// All accesses to a field outside of the class this method is declared in within the project
        /// </summary>
        public List<FieldAccessData> IsAccessingExternalField { get; private set; }
        
        /// <summary>
        /// All accesses to a field inside the class this method is declared in within the project
        /// </summary>
        public List<FieldAccessData> IsAccessingInternalField { get; private set; }
        
        /// <summary>
        /// All accesses to a property outside of the class this method is declared in within the project
        /// </summary>
        public List<PropertyAccessData> IsAccessingExternalProperty{ get; private set; }
        
        /// <summary>
        /// All accesses to a property inside the class this method is declared in within the project
        /// </summary>
        public List<PropertyAccessData> IsAccessingInternalProperty { get; private set; }
        
        /// <summary>
        /// A list of all Methods that are referenced by or are referencing this method NOT IMPLEMENTED!!!
        /// </summary>
        public List<MethodData> AllConnectedMethods { get; private set; }
        
        public List<MethodModifiers> MethodModifiersList { get; private set; }
        
        //maybe create this once the method is created, else this is probably generated each time this string is accessed
        public string MethodModifiersAsString
        {
            get
            {
                string result = "";
                foreach (MethodModifiers modifier in MethodModifiersList)
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

        public MethodData(IMethodSymbol symbol, ClassData containingClass)
        {
            MethodSymbol = symbol;
            ContainingClass = containingClass;
            InvokedByExternal = new List<MethodInvocationData>();
            InvokedByInternal = new List<MethodInvocationData>();
            IsInvokingInternalMethods = new List<MethodInvocationData>();
            IsInvokingExternalMethods = new List<MethodInvocationData>();
            IsAccessingInternalField = new List<FieldAccessData>();
            IsAccessingExternalField = new List<FieldAccessData>();
            IsAccessingInternalProperty = new List<PropertyAccessData>();
            IsAccessingExternalProperty = new List<PropertyAccessData>();
            MethodModifiersList = new List<MethodModifiers>();
            DetermineModifiers();
        }

        public void AddInvocation(MethodInvocationData invocation)
        {
            AllInvocations.Add(invocation);
        }

        public MethodInvocationData[] GetAllInvocations()
        {
            return AllInvocations.ToArray();
        }

        public string GetName()
        {
            return MethodSymbol.Name;
        }

        public Accessibility GetAccessibility()
        {
            return MethodSymbol.DeclaredAccessibility;
        }
        
        public string GetAccessibilityAsString()
        {
            if (MethodSymbol.DeclaredAccessibility == Accessibility.ProtectedOrInternal)
            {
                return "protected internal";
            }

            if (MethodSymbol.DeclaredAccessibility == Accessibility.ProtectedAndInternal)
            {
                return "private protected";
            }

            return MethodSymbol.DeclaredAccessibility.ToString().ToLower();
        }

        public ImmutableArray<IParameterSymbol> GetParameters()
        {
            return MethodSymbol.Parameters;
        }

        public ITypeSymbol GetReturnType()
        {
            return MethodSymbol.ReturnType;
        }

        public IMethodSymbol GetIMethodSymbol()
        {
            return MethodSymbol;
        }

        public override string ToString()
        {
            string result = MethodSymbol.DeclaredAccessibility + " ";
            result += MethodModifiersAsString;
            if(MethodModifiersList.Count != 0)
            {
                result += " ";
            }
            result += GetName() + "(";

            ImmutableArray<IParameterSymbol> parameters = GetParameters();
            foreach(IParameterSymbol parameter in parameters)
            {
                result += parameter.Type + " " + parameter.Name + ", ";
            }
            if (parameters.Length != 0)
            {
                result = result.Substring(0, result.Length - 2);
            }
            result += ");";

            return result;
        }

        private void DetermineModifiers()
        {
            if (MethodSymbol.IsStatic)
            {
                MethodModifiersList.Add(MethodModifiers.STATIC);
            }
            
            if (MethodSymbol.IsAbstract)
            {
                MethodModifiersList.Add(MethodModifiers.ABSTRACT);
            }

            if (MethodSymbol.IsAsync)
            {
                MethodModifiersList.Add(MethodModifiers.ASYNC);
            }

            if (MethodSymbol.IsExtern)
            {
                MethodModifiersList.Add(MethodModifiers.EXTERN);
            }

            if ( MethodSymbol.IsOverride)
            {
                MethodModifiersList.Add(MethodModifiers.OVERRIDE);
            }

            if (MethodSymbol.IsSealed)
            {
                MethodModifiersList.Add(MethodModifiers.SEALED);
            }

            if (MethodSymbol.IsVirtual)
            {
                MethodModifiersList.Add(MethodModifiers.VIRTUAL);
            }


            if ( MethodSymbol.IsReadOnly)
            {
                MethodModifiersList.Add(MethodModifiers.READONLY);
            }

            //dunno what this does but it sounds like we could use it:
            //MethodSymbol.IsConditional;
            //MethodSymbol.IsVararg;
            //MethodSymbol.IsExtensionMethod;
        }

        public enum MethodModifiers
        {
            STATIC,
            ABSTRACT,
            ASYNC,
            EXTERN,
            OVERRIDE,
            SEALED,
            VIRTUAL,
            READONLY,
            
            RECORD, //not implemented
            NEW, //not implemented
            UNSAFE, //not implemented
            
            //not sure if this counts:
            DELEGATE //not implemented

        }
    };
}