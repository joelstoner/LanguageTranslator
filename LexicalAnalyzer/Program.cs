namespace LanguageTranslator;


public class Program
{
    static void Main(string[] args)
    {
        var tokenGenerator = new TokenGenerator();
        tokenGenerator.Run();
        var symbolTableGenerator = new SymbolTableGenerator();
        symbolTableGenerator.Run();
        var parser = new Parser();
        parser.Run();
    }
}