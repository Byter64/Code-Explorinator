using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;

namespace CodeExplorinator
{
    public class MethodInvocationData
    {
        public ClassData containingClass { get; private set; }
        public MethodData referencedMethod { get; private set; }


        public MethodInvocationData(ClassData containingClass, MethodData referencedMethod)
        {
            this.containingClass = containingClass;
            this.referencedMethod = referencedMethod;
        }

    }
}