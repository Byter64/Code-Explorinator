using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;

namespace CodeExplorinator
{
    public class MethodData
    {
        public  IMethodSymbol MethodSymbol { get; private set; }
        public ClassData ContainingClass { get; private set;}
        public List<MethodInvocationData> Invocations { get; private set; }


        //to be implemented: get modifiers

        public MethodData(IMethodSymbol symbol, ClassData containingClass)
        {
            MethodSymbol = symbol;
            this.ContainingClass = containingClass;
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