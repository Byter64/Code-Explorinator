using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CodeExplorinator
{
    public interface INode
    {

    }

    public abstract record ClassNode : INode
    {
        public INamedTypeSymbol typeData;
        public INode[] ingoings;
        public INode[] outgoings;
    }

    public abstract record PlaceholderNode : INode
    {
        public string name;
    }
}