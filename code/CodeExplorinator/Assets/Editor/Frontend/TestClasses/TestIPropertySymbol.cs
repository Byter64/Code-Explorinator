using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using UnityEngine;

public class TestIPropertySymbol : IPropertySymbol
{
    string name;
    ITypeSymbol symbol;

    public TestIPropertySymbol(string name, ITypeSymbol symbol)
    {
        this.name = name;
        this.symbol = symbol;
    }

    public bool IsIndexer => throw new System.NotImplementedException();

    public bool IsReadOnly => throw new System.NotImplementedException();

    public bool IsWriteOnly => throw new System.NotImplementedException();

    public bool IsWithEvents => throw new System.NotImplementedException();

    public bool ReturnsByRef => throw new System.NotImplementedException();

    public bool ReturnsByRefReadonly => throw new System.NotImplementedException();

    public RefKind RefKind => throw new System.NotImplementedException();

    public ITypeSymbol Type => symbol;

    public NullableAnnotation NullableAnnotation => throw new System.NotImplementedException();

    public ImmutableArray<IParameterSymbol> Parameters => throw new System.NotImplementedException();

    public IMethodSymbol GetMethod => throw new System.NotImplementedException();

    public IMethodSymbol SetMethod => throw new System.NotImplementedException();

    public IPropertySymbol OriginalDefinition => throw new System.NotImplementedException();

    public IPropertySymbol OverriddenProperty => throw new System.NotImplementedException();

    public ImmutableArray<IPropertySymbol> ExplicitInterfaceImplementations => throw new System.NotImplementedException();

    public ImmutableArray<CustomModifier> RefCustomModifiers => throw new System.NotImplementedException();

    public ImmutableArray<CustomModifier> TypeCustomModifiers => throw new System.NotImplementedException();

    public SymbolKind Kind => throw new System.NotImplementedException();

    public string Language => throw new System.NotImplementedException();

    public string Name => throw new System.NotImplementedException();

    public string MetadataName => throw new System.NotImplementedException();

    public ISymbol ContainingSymbol => throw new System.NotImplementedException();

    public IAssemblySymbol ContainingAssembly => throw new System.NotImplementedException();

    public IModuleSymbol ContainingModule => throw new System.NotImplementedException();

    public INamedTypeSymbol ContainingType => throw new System.NotImplementedException();

    public INamespaceSymbol ContainingNamespace => throw new System.NotImplementedException();

    public bool IsDefinition => throw new System.NotImplementedException();

    public bool IsStatic => throw new System.NotImplementedException();

    public bool IsVirtual => throw new System.NotImplementedException();

    public bool IsOverride => throw new System.NotImplementedException();

    public bool IsAbstract => throw new System.NotImplementedException();

    public bool IsSealed => throw new System.NotImplementedException();

    public bool IsExtern => throw new System.NotImplementedException();

    public bool IsImplicitlyDeclared => throw new System.NotImplementedException();

    public bool CanBeReferencedByName => throw new System.NotImplementedException();

    public ImmutableArray<Location> Locations => throw new System.NotImplementedException();

    public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => throw new System.NotImplementedException();

    public Accessibility DeclaredAccessibility => throw new System.NotImplementedException();

    public bool HasUnsupportedMetadata => throw new System.NotImplementedException();

    ISymbol ISymbol.OriginalDefinition => throw new System.NotImplementedException();

    public void Accept(SymbolVisitor visitor)
    {
        throw new System.NotImplementedException();
    }

    public TResult? Accept<TResult>(SymbolVisitor<TResult> visitor)
    {
        throw new System.NotImplementedException();
    }

    public bool Equals(ISymbol other, SymbolEqualityComparer equalityComparer)
    {
        throw new System.NotImplementedException();
    }

    public bool Equals(ISymbol other)
    {
        throw new System.NotImplementedException();
    }

    public ImmutableArray<AttributeData> GetAttributes()
    {
        throw new System.NotImplementedException();
    }

    public string GetDocumentationCommentId()
    {
        throw new System.NotImplementedException();
    }

    public string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    public ImmutableArray<SymbolDisplayPart> ToDisplayParts(SymbolDisplayFormat format = null)
    {
        throw new System.NotImplementedException();
    }

    public string ToDisplayString(SymbolDisplayFormat format = null)
    {
        return "public" + Type.ToDisplayString() + name;
    }

    public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null)
    {
        throw new System.NotImplementedException();
    }

    public string ToMinimalDisplayString(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null)
    {
        throw new System.NotImplementedException();
    }
}
