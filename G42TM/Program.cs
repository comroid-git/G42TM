using System.Xml;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using G42TM.Antlr;
using Newtonsoft.Json;
using TextMateSharp.Grammars;
using Formatting = Newtonsoft.Json.Formatting;

namespace G42TM;

public class Program
{
    public static readonly string AntlrExt = ".g4";
    public static readonly string TextMateExt = ".tmLanguage";
    
    public static void Main(string[] args)
    {
        var path = string.Join(" ", args);
        if (!File.Exists(path))
        {
            Console.WriteLine($"Error: File {path} not found");
            return;
        }

        if (!path.EndsWith(AntlrExt))
        {
            Console.WriteLine($"Error: File {path} is not an Antlr v4 file");
            return;
        }

        var parserFile = new FileInfo(path);
        var splitGrammar = parserFile.Name.EndsWith("Parser" + AntlrExt);
        var grammarName = parserFile.Name.Substring(0,
            parserFile.Name.IndexOf((splitGrammar ? "Parser" : string.Empty) + AntlrExt, StringComparison.Ordinal));
        var lexerFile = splitGrammar ? GetLexerFile(grammarName, parserFile) : null;

        var def = new GrammarDefinition()
        {
            Name = grammarName,
            DisplayName = grammarName,
            Description = "Converted from ANTLR via G42TM",
            Contributes =
            {
                Grammars = { new Grammar() { Language = grammarName } },
                Languages =
                {
                    new Language()
                    {
                        Id = grammarName,
                        Aliases = { grammarName },
                        Extensions = { "<EXTENSION>" }
                    }
                },
                Snippets = new()
            },
            Repository = new(),
            Engines = new(),
            Scripts = new()
        };
        var listener = new GrammarListener(def);
        
        foreach (var file in new[]{lexerFile, parserFile}.Where(x => x != null).Select(x => x!))
        {
            var input = new AntlrInputStream(file.OpenRead());
            var lexer = new ANTLRv4Lexer(input);
            var tokenizer = new CommonTokenStream(lexer);
            var parser = new ANTLRv4Parser(tokenizer);
            var spec = parser.grammarSpec();
            
            ParseTreeWalker.Default.Walk(listener, spec);
        }

        var data = JsonConvert.SerializeObject(listener.Definition, Formatting.Indented);
        var outputFile = new FileInfo(Path.Combine(parserFile.DirectoryName!, grammarName + TextMateExt));
        File.WriteAllText(outputFile.FullName, data);
    }

    private static FileInfo GetLexerFile(string grammarName, FileInfo parserFile) =>
        new(Path.Combine(parserFile.DirectoryName!, grammarName + "Lexer" + AntlrExt));
}