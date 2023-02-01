using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
        /// All invocations to this method within the project (Concatination of in and external invocations)
        /// </summary>
        public List<MethodInvocationData> AllInvocations
        {
            get
            {
                return ExternalInvocations.Concat(InternalInvocations).ToList();
            }
        }

        /// <summary>
        /// All invocations to this method outside of the class it is declared in within the project
        /// </summary>
        public List<MethodInvocationData> ExternalInvocations { get; private set; }

        /// <summary>
        /// All invocations to this method inside the class it is declared in within the project
        /// </summary>
        public List<MethodInvocationData> InternalInvocations { get; private set; }


        //to be implemented: get modifiers

        public MethodData(IMethodSymbol symbol, ClassData containingClass)
        {
            MethodSymbol = symbol;
            ContainingClass = containingClass;
            ExternalInvocations = new List<MethodInvocationData>();
            InternalInvocations = new List<MethodInvocationData>();
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
            return MethodSymbol.Name;
        }
    }
}