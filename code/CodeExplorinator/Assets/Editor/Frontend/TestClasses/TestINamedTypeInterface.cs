using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using UnityEngine;

public class TestINamedTypeInterface : INamedTypeSymbol
{
    string name;
    public List<IPropertySymbol> properties = new();
    public List<IFieldSymbol> fields = new();
    public List<IMethodSymbol> methods = new();
    public TestINamedTypeInterface(string name)
    {
        this.name = name;
    }

    public int Arity { get; set; }

    public bool IsGenericType { get; set; }

    public bool IsUnboundGenericType { get; set; }

    public bool IsScriptClass { get; set; }

    public bool IsImplicitClass { get; set; }

    public bool IsComImport { get; set; }

    public IEnumerable<string> MemberNames { get; set; }

    public ImmutableArray<ITypeParameterSymbol> TypeParameters { get; set; }

    public ImmutableArray<ITypeSymbol> TypeArguments { get; set; }

    public ImmutableArray<NullableAnnotation> TypeArgumentNullableAnnotations { get; set; }

    public INamedTypeSymbol OriginalDefinition { get; set; }

    public IMethodSymbol DelegateInvokeMethod { get; set; }

    public INamedTypeSymbol EnumUnderlyingType { get; set; }

    public INamedTypeSymbol ConstructedFrom { get; set; }

    public ImmutableArray<IMethodSymbol> InstanceConstructors { get; set; }

    public ImmutableArray<IMethodSymbol> StaticConstructors { get; set; }

    public ImmutableArray<IMethodSymbol> Constructors { get; set; }

    public ISymbol AssociatedSymbol { get; set; }

    public bool MightContainExtensionMethods { get; set; }

    public INamedTypeSymbol TupleUnderlyingType { get; set; }

    public ImmutableArray<IFieldSymbol> TupleElements { get; set; }

    public bool IsSerializable { get; set; }

    public INamedTypeSymbol NativeIntegerUnderlyingType { get; set; }

    public TypeKind TypeKind { get; set; }

    public INamedTypeSymbol BaseType { get; set; }

    public ImmutableArray<INamedTypeSymbol> Interfaces { get; set; }

    public ImmutableArray<INamedTypeSymbol> AllInterfaces { get; set; }

    public bool IsReferenceType { get; set; }

    public bool IsValueType { get; set; }

    public bool IsAnonymousType { get; set; }

    public bool IsTupleType { get; set; }

    public bool IsNativeIntegerType { get; set; }

    public SpecialType SpecialType { get; set; }

    public bool IsRefLikeType { get; set; }

    public bool IsUnmanagedType { get; set; }

    public bool IsReadOnly { get; set; }

    public bool IsRecord { get; set; }

    public NullableAnnotation NullableAnnotation { get; set; }

    public bool IsNamespace { get; set; }

    public bool IsType { get; set; }

    public SymbolKind Kind { get; set; }

    public string Language { get; set; }

    public string Name { get; set; }

    public string MetadataName { get; set; }

    public ISymbol ContainingSymbol { get; set; }

    public IAssemblySymbol ContainingAssembly { get; set; }

    public IModuleSymbol ContainingModule { get; set; }

    public INamedTypeSymbol ContainingType { get; set; }

    public INamespaceSymbol ContainingNamespace { get; set; }

    public bool IsDefinition { get; set; }

    public bool IsStatic { get; set; }

    public bool IsVirtual { get; set; }

    public bool IsOverride { get; set; }

    public bool IsAbstract { get; set; }

    public bool IsSealed { get; set; }

    public bool IsExtern { get; set; }

    public bool IsImplicitlyDeclared { get; set; }

    public bool CanBeReferencedByName { get; set; }

    public ImmutableArray<Location> Locations { get; set; }

    public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; set; }

    public Accessibility DeclaredAccessibility { get; set; }

    public bool HasUnsupportedMetadata { get; set; }

    ITypeSymbol ITypeSymbol.OriginalDefinition { get; }

    ISymbol ISymbol.OriginalDefinition { get; }

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
        List<ISymbol> deineMutter = new();
        foreach (IFieldSymbol symbol in fields)
        {
            deineMutter.Add(symbol);
        }
        foreach (IPropertySymbol symbol in properties)
        {
            deineMutter.Add(symbol);
        }
        foreach (IMethodSymbol symbol in methods)
        {
            deineMutter.Add(symbol);
        }
        return deineMutter.ToImmutableArray();
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
        return name;
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
