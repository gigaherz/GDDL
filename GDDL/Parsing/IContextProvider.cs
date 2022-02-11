
namespace GDDL.Parsing
{
    public interface IContextProvider
    {
        /**
         * @return An object containing the location being parsed, to be used in error messages and debugging.
         */
        ParsingContext ParsingContext { get; }
    }
}
