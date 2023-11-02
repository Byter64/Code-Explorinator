using Microsoft.CodeAnalysis;

namespace CodeExplorinator
{
    public record PlaceholderClassData(INamedTypeSymbol typeData) : IClassData;
}
