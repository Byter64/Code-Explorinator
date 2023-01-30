using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;

namespace CodeExplorinator
{
    public class FieldData
    {
        
        public IFieldSymbol fieldSymbol { get; private set; }
        public ClassData ContainingClass { get; private set; }
        public List<FieldAccessData> Accesses { get; private set; }
        public FieldData(IFieldSymbol fieldSymbol, ClassData containingClass)
        {
            this.fieldSymbol = fieldSymbol;
            ContainingClass = containingClass;
            Accesses = new List<FieldAccessData>();
        }
    }
}