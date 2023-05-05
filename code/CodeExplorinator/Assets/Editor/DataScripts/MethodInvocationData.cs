using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;

namespace CodeExplorinator
{
    /// <summary>
    /// Class that represents the invocation of a method
    /// </summary>
    public class MethodInvocationData
    {
        /// <summary>
        /// The method in which this invocation takes place
        /// </summary>
        public MethodData ContainingMethod { get; private set; }

        /// <summary>
        /// The method that is called by this invocation
        /// </summary>
        public MethodData ReferencedMethod { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containingMethod">The method in which this invocation takes place</param>
        /// <param name="referencedMethod">The method that is called by this invocation</param>
        public MethodInvocationData(MethodData containingMethod, MethodData referencedMethod)
        {
            ContainingMethod = containingMethod;
            ReferencedMethod = referencedMethod;
        }

    }
}