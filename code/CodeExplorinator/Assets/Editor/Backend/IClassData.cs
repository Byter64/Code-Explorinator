using Microsoft.CodeAnalysis;

namespace CodeExplorinator
{
    public interface IClassData
    {
        public INamedTypeSymbol typeData { get; init; }
    }

}
