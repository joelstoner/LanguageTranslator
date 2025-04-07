namespace LanguageTranslator;

public class Symbol
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Value { get; set; }

    public Symbol(string name, string type, string value)
    {
        Name = name;
        Type = type;
        Value = value;
    }
}
public class SyntaxAnalyzer
{
    static char[,] precTable = new char[,]
    {
        { ' ', '<', '<', '<', ' ', '<', '<' },
        { ' ', '>', '>', '<', '>', '<', '<' },
        { ' ', '>', '>', '<', '>', '<', '<' },
        { ' ', '<', '<', '<', '=', '<', '<' },
        { ' ', '>', '>', ' ', '>', '>', '>' },
        { ' ', '>', '>', '<', '>', '>', '>' },
        { ' ', '>', '>', '<', '>', '>', '>'}
    };

    static bool IsPrecSymbol(string f)
    {
        f = f.Trim();
        if (f == "var")
            return true;
        if (f == "numlit")
            return true;
        if (f == "$=")
            return true;
        if  (f == "$addop")
            return true;
        if (f == "$mop")
            return true;
        if (f == "$RP")
            return true;
        if (f == "$LP")
            return true;
        return false;

    }
    static int[,] GetInitTable(char init)
    {
        int[,] equalTable = new int[precTable.GetLength(0), precTable.GetLength(1)];
        for (int i = 0; i < equalTable.GetLength(0); i++)
        {
            for (int j = 0; j < equalTable.GetLength(1); j++)
            {
                if (precTable[i, j] == init) equalTable[i, j] = 1;
                else equalTable[i, j] = 0;
            }
        }
        return equalTable;
    }
    
    
    internal void Run()
    {
        string parentDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
        List<Symbol> symbols = new List<Symbol>();
        HashSet<string> varSet = new HashSet<string>();
        Stack<Symbol> precStack = new Stack<Symbol>();
        using (StreamReader reader = new StreamReader(Path.Combine(parentDir, "SymbolTable.txt")))
        {
            string line;
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine().Trim();
                string[] parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                if (parts[5].Trim() != "CS")
                    symbols.Add(new Symbol(parts[1], parts[2], parts[3]));
            }
        }
        
        using (StreamReader reader = new StreamReader(Path.Combine(parentDir, "tokens.txt")))
        {
            string line;
            string[] parts = new string[] { };
            bool endOfDec = false;
            while (!reader.EndOfStream && !endOfDec)
            {
                line = reader.ReadLine().Trim();
                parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                if (varSet.Contains(parts[0]))
                {
                    endOfDec = true;
                    Console.WriteLine($"LANDED HERE : {parts[0]}");
                }
                else if (parts[1].Trim() == "var")
                    varSet.Add(parts[0]);
            }

            while (!reader.EndOfStream)
            {
                if (IsPrecSymbol(parts[1]))
                    precStack.Push(new Symbol(parts[0], parts[1], ""));
                line = reader.ReadLine().Trim();
                parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
            }

            foreach (Symbol symbol in precStack)
            {
                Console.WriteLine($"{symbol.Name} {symbol.Type} {symbol.Value}");
            }
        }

        
    }
    
}