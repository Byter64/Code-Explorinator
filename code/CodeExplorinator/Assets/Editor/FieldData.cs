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
        }
    }
}