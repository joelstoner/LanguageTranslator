namespace LanguageTranslator;

public class Program
{
    static void Main(string[] args)
    {
        TokenGenerator tokenGenerator = new TokenGenerator();
        tokenGenerator.Run();
        SymbolTableGenerator symbolTableGenerator = new SymbolTableGenerator();
        symbolTableGenerator.Run();
        SyntaxAnalyzer syntaxAnalyzer = new SyntaxAnalyzer();
        syntaxAnalyzer.Run();
    }
}