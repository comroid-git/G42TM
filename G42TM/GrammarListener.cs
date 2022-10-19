using G42TM.Antlr;
using TextMateSharp.Grammars;

namespace G42TM;

public class GrammarListener : ANTLRv4ParserBaseListener
{
    public GrammarDefinition Definition { get; }

    public GrammarListener(GrammarDefinition definition)
    {
        Definition = definition;
    }
}