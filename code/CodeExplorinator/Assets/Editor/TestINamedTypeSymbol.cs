using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using UnityEngine;

public class TestINamedTypeSymbol : INamedTypeSymbol
{
    public int Arity => throw new System.NotImplementedException();

    public bool IsGenericType => throw new System.NotImplementedException();

    public bool IsUnboundGenericType => throw new System.NotImplementedException();

    public bool IsScriptClass => throw new System.NotImplementedException();

    public bool IsImplicitClass => throw new System.NotImplementedException();

    public bool IsComImport => throw new System.NotImplementedException();

    public IEnumerable<string> MemberNames => throw new System.NotImplementedException();

    public ImmutableArray<ITypeParameterSymbol> TypeParameters => throw new System.NotImplementedException();

    public ImmutableArray<ITypeSymbol> TypeArguments => throw new System.NotImplementedException();

    public ImmutableArray<NullableAnnotation> TypeArgumentNullableAnnotations => throw new System.NotImplementedException();

    public INamedTypeSymbol OriginalDefinition => throw new System.NotImplementedException();

    public IMethodSymbol DelegateInvokeMethod => throw new System.NotImplementedException();

    public INamedTypeSymbol EnumUnderlyingType => throw new System.NotImplementedException();

    public INamedTypeSymbol ConstructedFrom => throw new System.NotImplementedException();

    public ImmutableArray<IMethodSymbol> InstanceConstructors => throw new System.NotImplementedException();

    public ImmutableArray<IMethodSymbol> StaticConstructors => throw new System.NotImplementedException();

    public ImmutableArray<IMethodSymbol> Constructors => throw new System.NotImplementedException();

    public ISymbol AssociatedSymbol => throw new System.NotImplementedException();

    public bool MightContainExtensionMethods => throw new System.NotImplementedException();

    public INamedTypeSymbol TupleUnderlyingType => throw new System.NotImplementedException();

    public ImmutableArray<IFieldSymbol> TupleElements => throw new System.NotImplementedException();

    public bool IsSerializable => throw new System.NotImplementedException();

    public INamedTypeSymbol NativeIntegerUnderlyingType => throw new System.NotImplementedException();

    public TypeKind TypeKind => throw new System.NotImplementedException();

    public INamedTypeSymbol BaseType => throw new System.NotImplementedException();

    public ImmutableArray<INamedTypeSymbol> Interfaces => throw new System.NotImplementedException();

    public ImmutableArray<INamedTypeSymbol> AllInterfaces => throw new System.NotImplementedException();

    public bool IsReferenceType => throw new System.NotImplementedException();

    public bool IsValueType => throw new System.NotImplementedException();

    public bool IsAnonymousType => throw new System.NotImplementedException();

    public bool IsTupleType => throw new System.NotImplementedException();

    public bool IsNativeIntegerType => throw new System.NotImplementedException();

    public SpecialType SpecialType => throw new System.NotImplementedException();

    public bool IsRefLikeType => throw new System.NotImplementedException();

    public bool IsUnmanagedType => throw new System.NotImplementedException();

    public bool IsReadOnly => throw new System.NotImplementedException();

    public bool IsRecord => throw new System.NotImplementedException();

    public NullableAnnotation NullableAnnotation => throw new System.NotImplementedException();

    public bool IsNamespace => throw new System.NotImplementedException();

    public bool IsType => throw new System.NotImplementedException();

    public SymbolKind Kind => throw new System.NotImplementedException();

    public string Language => throw new System.NotImplementedException();

    public string Name  { get { return "Kuhmelker"; } }

    public string MetadataName => throw new System.NotImplementedException();

    public ISymbol ContainingSymbol => throw new System.NotImplementedException();

    public IAssemblySymbol ContainingAssembly => throw new System.NotImplementedException();

    public IModuleSymbol ContainingModule => throw new System.NotImplementedException();

    public INamedTypeSymbol ContainingType => throw new System.NotImplementedException();

    public INamespaceSymbol ContainingNamespace => throw new System.NotImplementedException();

    public bool IsDefinition => throw new System.NotImplementedException();

    public bool IsStatic { get { return false; } }

    public bool IsVirtual => throw new System.NotImplementedException();

    public bool IsOverride => throw new System.NotImplementedException();

    public bool IsAbstract { get { return true; } }

    public bool IsSealed { get { return true; } }

    public bool IsExtern => throw new System.NotImplementedException();

    public bool IsImplicitlyDeclared => throw new System.NotImplementedException();

    public bool CanBeReferencedByName => throw new System.NotImplementedException();

    public ImmutableArray<Location> Locations => throw new System.NotImplementedException();

    public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => throw new System.NotImplementedException();

    public Accessibility DeclaredAccessibility { get { return Accessibility.Public; } }

    public bool HasUnsupportedMetadata => throw new System.NotImplementedException();

    ITypeSymbol ITypeSymbol.OriginalDefinition => throw new System.NotImplementedException();

    ISymbol ISymbol.OriginalDefinition => throw new System.NotImplementedException();

    public void Accept(SymbolVisitor visitor)
    {
        throw new System.NotImplementedException();
    }

    public TResult? Accept<TResult>(SymbolVisitor<TResult> visitor)
    {
        throw new System.NotImplementedException();
    }

    public INamedTypeSymbol Construct(params ITypeSymbol[] typeArguments)
    {
        throw new System.NotImplementedException();
    }

    public INamedTypeSymbol Construct(ImmutableArray<ITypeSymbol> typeArguments, ImmutableArray<NullableAnnotation> typeArgumentNullableAnnotations)
    {
        throw new System.NotImplementedException();
    }

    public INamedTypeSymbol ConstructUnboundGenericType()
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

    public ISymbol FindImplementationForInterfaceMember(ISymbol interfaceMember)
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

    public ImmutableArray<ISymbol> GetMembers()
    {
        throw new System.NotImplementedException();
    }

    public ImmutableArray<ISymbol> GetMembers(string name)
    {
        throw new System.NotImplementedException();
    }

    public ImmutableArray<CustomModifier> GetTypeArgumentCustomModifiers(int ordinal)
    {
        throw new System.NotImplementedException();
    }

    public ImmutableArray<INamedTypeSymbol> GetTypeMembers()
    {
        throw new System.NotImplementedException();
    }

    public ImmutableArray<INamedTypeSymbol> GetTypeMembers(string name)
    {
        throw new System.NotImplementedException();
    }

    public ImmutableArray<INamedTypeSymbol> GetTypeMembers(string name, int arity)
    {
        throw new System.NotImplementedException();
    }

    public ImmutableArray<SymbolDisplayPart> ToDisplayParts(NullableFlowState topLevelNullability, SymbolDisplayFormat format = null)
    {
        throw new System.NotImplementedException();
    }

    public ImmutableArray<SymbolDisplayPart> ToDisplayParts(SymbolDisplayFormat format = null)
    {
        throw new System.NotImplementedException();
    }

    public string ToDisplayString(NullableFlowState topLevelNullability, SymbolDisplayFormat format = null)
    {
        throw new System.NotImplementedException();
    }

    public string ToDisplayString(SymbolDisplayFormat format = null)
    {
        throw new System.NotImplementedException();
    }

    public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, NullableFlowState topLevelNullability, int position, SymbolDisplayFormat format = null)
    {
        throw new System.NotImplementedException();
    }

    public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null)
    {
        throw new System.NotImplementedException();
    }

    public string ToMinimalDisplayString(SemanticModel semanticModel, NullableFlowState topLevelNullability, int position, SymbolDisplayFormat format = null)
    {
        throw new System.NotImplementedException();
    }

    public string ToMinimalDisplayString(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null)
    {
        throw new System.NotImplementedException();
    }

    public ITypeSymbol WithNullableAnnotation(NullableAnnotation nullableAnnotation)
    {
        throw new System.NotImplementedException();
    }
}
