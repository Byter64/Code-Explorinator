using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection.Metadata;
using System.Threading;
using UnityEngine;

public class TestIMethodSymbol : IMethodSymbol
{
    string name;
    ITypeSymbol symbol;
    public TestIMethodSymbol(string name, ITypeSymbol symbol)
    {
        this.name = name;
        this.symbol = symbol;
    }

    public MethodKind MethodKind => throw new System.NotImplementedException();

    public int Arity => throw new System.NotImplementedException();

    public bool IsGenericMethod => throw new System.NotImplementedException();

    public bool IsExtensionMethod => throw new System.NotImplementedException();

    public bool IsAsync => throw new System.NotImplementedException();

    public bool IsVararg => throw new System.NotImplementedException();

    public bool IsCheckedBuiltin => throw new System.NotImplementedException();

    public bool HidesBaseMethodsByName => throw new System.NotImplementedException();

    public bool ReturnsVoid => throw new System.NotImplementedException();

    public bool ReturnsByRef => throw new System.NotImplementedException();

    public bool ReturnsByRefReadonly => throw new System.NotImplementedException();

    public RefKind RefKind => throw new System.NotImplementedException();

    public ITypeSymbol ReturnType => symbol;

    public NullableAnnotation ReturnNullableAnnotation => throw new System.NotImplementedException();

    public ImmutableArray<ITypeSymbol> TypeArguments => throw new System.NotImplementedException();

    public ImmutableArray<NullableAnnotation> TypeArgumentNullableAnnotations => throw new System.NotImplementedException();

    public ImmutableArray<ITypeParameterSymbol> TypeParameters => throw new System.NotImplementedException();

    public ImmutableArray<IParameterSymbol> Parameters => throw new System.NotImplementedException();

    public IMethodSymbol ConstructedFrom => throw new System.NotImplementedException();

    public bool IsReadOnly => throw new System.NotImplementedException();

    public bool IsInitOnly => throw new System.NotImplementedException();

    public IMethodSymbol OriginalDefinition => throw new System.NotImplementedException();

    public IMethodSymbol OverriddenMethod => throw new System.NotImplementedException();

    public ITypeSymbol ReceiverType => throw new System.NotImplementedException();

    public NullableAnnotation ReceiverNullableAnnotation => throw new System.NotImplementedException();

    public IMethodSymbol ReducedFrom => throw new System.NotImplementedException();

    public ImmutableArray<IMethodSymbol> ExplicitInterfaceImplementations => throw new System.NotImplementedException();

    public ImmutableArray<CustomModifier> ReturnTypeCustomModifiers => throw new System.NotImplementedException();

    public ImmutableArray<CustomModifier> RefCustomModifiers => throw new System.NotImplementedException();

    public SignatureCallingConvention CallingConvention => throw new System.NotImplementedException();

    public ImmutableArray<INamedTypeSymbol> UnmanagedCallingConventionTypes => throw new System.NotImplementedException();

    public ISymbol AssociatedSymbol => throw new System.NotImplementedException();

    public IMethodSymbol PartialDefinitionPart => throw new System.NotImplementedException();

    public IMethodSymbol PartialImplementationPart => throw new System.NotImplementedException();

    public INamedTypeSymbol AssociatedAnonymousDelegate => throw new System.NotImplementedException();

    public bool IsConditional => throw new System.NotImplementedException();

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

    public IMethodSymbol Construct(params ITypeSymbol[] typeArguments)
    {
        throw new System.NotImplementedException();
    }

    public IMethodSymbol Construct(ImmutableArray<ITypeSymbol> typeArguments, ImmutableArray<NullableAnnotation> typeArgumentNullableAnnotations)
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

    public DllImportData GetDllImportData()
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

    public ImmutableArray<AttributeData> GetReturnTypeAttributes()
    {
        throw new System.NotImplementedException();
    }

    public ITypeSymbol GetTypeInferredDuringReduction(ITypeParameterSymbol reducedFromTypeParameter)
    {
        throw new System.NotImplementedException();
    }

    public IMethodSymbol ReduceExtensionMethod(ITypeSymbol receiverType)
    {
        throw new System.NotImplementedException();
    }

    public ImmutableArray<SymbolDisplayPart> ToDisplayParts(SymbolDisplayFormat format = null)
    {
        throw new System.NotImplementedException();
    }

    public string ToDisplayString(SymbolDisplayFormat format = null)
    {
        return "public " + ReturnType.ToDisplayString()  + " "+ name + "()";
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
