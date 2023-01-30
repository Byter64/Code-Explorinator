using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;

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
        /// All invocations to this method within the project
        /// </summary>
        public List<MethodInvocationData> Invocations { get; private set; }


        //to be implemented: get modifiers

        public MethodData(IMethodSymbol symbol, ClassData containingClass)
        {
            MethodSymbol = symbol;
            ContainingClass = containingClass;
            Invocations = new List<MethodInvocationData>();
        }

        public void AddInvocation(MethodInvocationData invocation)
        {
            Invocations.Add(invocation);
        }

        public MethodInvocationData[] GetAllInvocations()
        {
            return Invocations.ToArray();
        }

        public string GetName()
        {
            return MethodSymbol.Name;
        }

        public Accessibility GetAccessibility()
        {
            return MethodSymbol.DeclaredAccessibility;
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
    };
}