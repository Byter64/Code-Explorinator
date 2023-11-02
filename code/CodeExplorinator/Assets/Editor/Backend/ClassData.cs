using Microsoft.CodeAnalysis;

namespace CodeExplorinator
{
    public record ClassData(INamedTypeSymbol typeData) : IClassData;

}