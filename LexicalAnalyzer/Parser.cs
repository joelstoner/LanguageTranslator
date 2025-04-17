using static LanguageTranslator.MatrixUtils;

namespace LanguageTranslator;

public class Symbol
{
    public string Name { get; set; }
    public string Type { get; set; }

    public Symbol(string name, string type)
    {
        Name = name;
        Type = type;
    }

    public int GetSymbolType()
    {
        if (Type == "$=") return 0;
        if (Name == "+" && Type == "$addop") return 1;
        if (Name == "-" && Type == "$addop") return 2;
        if (Type == "$LB") return 3;
        if (Type == "$RB") return 4;
        if (Name == "*" && Type == "$mop") return 5;
        if (Name == "/" && Type == "$mop") return 6;
        if (Name == "IF" && Type == "$if") return 7;
        if (Name == "THEN" && Type == "$then") return 8;
        if (Name == ">") return 9;
        if (Name == ">=") return 10;
        if (Type == "$semi") return 11;
        return 12;
    }

    public bool IsOperator() => GetSymbolType() < 12;
}

public class Quad
{
    public Symbol Operator { get; set; }
    public Symbol Argument1 { get; set; }
    public Symbol Argument2 { get; set; }
    public Symbol Result { get; set; }
    public int Count { get; set; } = 0;

    public void AddSymbol(Symbol symbol)
    {
        if (symbol.IsOperator())
            Operator = symbol;
        else if (Argument2 == null)
            Argument2 = symbol;
        else if (Argument1 == null)
            Argument1 = symbol;
        else
            Result = symbol;

        Count++;
    }

    public void Print()
    {
        try
        {
            Console.WriteLine($"QUAD: {Operator.Name} {Argument1.Name} {Argument2.Name} {Result.Name}");
        }
        catch
        {
            Console.WriteLine($"QUAD Error: {Operator.Name}");
        }
    }
}

public class Parser
{
    // precedence table for terminals: =  +  -  (  )  *  /  ;
    static char[,] precTable =
    {
        { ' ', '<', '<', '<', ' ', '<', '<', ' ', ' ', ' ', ' ', '>' },
        { ' ', '>', '>', '<', '>', '<', '<', '>', '>', '>', '>', '>' },
        { ' ', '>', '>', '<', '>', '<', '<', '>', '>', '>', '>', '>' },
        { ' ', '<', '<', '<', '=', '<', '<', ' ', ' ', ' ', ' ', ' ' },
        { ' ', '>', '>', ' ', '>', '>', '>', ' ', ' ', ' ', ' ', '>' },
        { ' ', '>', '>', '<', '>', '>', '>', '>', '>', '>', '>', '>' },
        { ' ', '>', '>', '<', '>', '>', '>', '>', '>', '>', '>', '>' },
        { ' ', '<', '<', '<', ' ', '<', '<', ' ', '=', '<', '<', '>' }, // if
        { ' ', '<', ' ', ' ', ' ', ' ', ' ', '<', ' ', ' ', ' ', ' ' }, // then
        { ' ', '<', '<', '<', ' ', '<', '<', ' ', '>', ' ', ' ', '>' }, // >
        { ' ', '<', '<', '<', ' ', '<', '<', ' ', '>', ' ', ' ', '>' }, // >=
        { '<', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '=' }
    };

    static bool StartStack(Symbol token, List<Symbol> list)
    {
        foreach (var s in list)
        {
            if (s.Name == token.Name)
                return true;
        }
        return false;
    }

    static void PrintList(List<Symbol> list)
    {
        foreach (var s in list)
            Console.Write(s.Name);
        Console.WriteLine();
    }

    internal void Run()
    {
        // 1) show init tables
        PrintMatrix(GetInitTable('>', precTable));
        PrintMatrix(GetInitTable('<', precTable));

        // 2) build precedence‑function
        var gt       = GetInitTable('>', precTable);
        var lt       = GetInitTable('<', precTable);
        var bm       = BMatrix(gt, lt);
        var precFunc = PrecedenceFunction(bm);

        // 3) print precFunc
        for (int i = 0; i < precFunc.GetLength(0); i++)
        {
            for (int j = 0; j < precFunc.GetLength(1); j++)
                Console.Write(precFunc[i, j] + " ");
            Console.WriteLine();
        }

        // 4) drive the parser
        var parentDir = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
        var tokenFile = Path.Combine(parentDir, "tokens.txt");

        using (var reader = new StreamReader(tokenFile))
        {
            string line, token, flag;
            Symbol symbol0, symbol1 = new Symbol("", "");
            var varList   = new List<Symbol>();
            var quadStack = new Queue<Quad>();

            while ((line = reader.ReadLine()?.Trim()) != null)
            {
                token   = line.Substring(0, line.IndexOf(' '));
                flag    = line.Substring(line.LastIndexOf(' ') + 1);
                symbol0 = symbol1;
                symbol1 = new Symbol(token, flag);

                if (!StartStack(symbol1, varList))
                {
                    Console.WriteLine("NEXT!");
                    if (flag == "var")
                        varList.Add(symbol1);
                }
                else
                {
                    Console.WriteLine($"MAKING STACK! @{token}");
                    var stackSymbols  = new Stack<Symbol>();
                    var stringSymbols = new Queue<Symbol>();
                    var tempStack     = new Queue<Symbol>();

                    for (int i = 1; i <= 12; i++)
                        tempStack.Enqueue(new Symbol($"T{i}", "numlit"));

                    if (symbol0.Type == "var" || symbol0.Type == "numlit")
                        stringSymbols.Enqueue(symbol0);

                    // read until semicolon
                    while (symbol1.GetSymbolType() != 11)
                    {
                        stringSymbols.Enqueue(symbol1);
                        line = reader.ReadLine()?.Trim();
                        if (line == null) break;
                        Console.WriteLine($"Stack maker line: {line}");

                        token   = line.Substring(0, line.IndexOf(' '));
                        flag    = line.Substring(line.LastIndexOf(' ') + 1);
                        symbol1 = new Symbol(token, flag);
                    }

                    stackSymbols.Push(symbol1);
                    stringSymbols.Enqueue(new Symbol(";", "$semi"));

                    Console.WriteLine("HERES THE STRING");
                    PrintList(stringSymbols.ToList());

                    Console.WriteLine("HERES THE STACK");
                    foreach (var s in stackSymbols)
                        Console.WriteLine($"{s.Name} : {s.Type}");

                    // operator‑precedence loop
                    while (true)
                    {
                        // both semicolons? done.
                        if (stackSymbols.Peek().GetSymbolType() == 11 &&
                            stringSymbols.Peek().GetSymbolType() == 11)
                        {
                            Console.WriteLine("this code ran");
                            break;
                        }

                        // shift any operand
                        if (!stringSymbols.Peek().IsOperator())
                        {
                            Console.WriteLine($"NON TERMINAL PUSHED: {stringSymbols.Peek().Name}");
                            stackSymbols.Push(stringSymbols.Dequeue());
                            continue;
                        }

                        // find the topmost operator on stack
                        Symbol fsymbol = null;
                        foreach (var sym in stackSymbols)
                        {
                            if (sym.IsOperator())
                            {
                                fsymbol = sym;
                                break;
                            }
                        }
                        if (fsymbol == null)
                            break;

                        int fvalue = precFunc[0, fsymbol.GetSymbolType()];
                        int gvalue = precFunc[1, stringSymbols.Peek().GetSymbolType()];
                        Console.WriteLine($"f({fsymbol.Name})={fvalue} AND g({stringSymbols.Peek().Name})={gvalue}");

                        if (fvalue < gvalue)
                        {
                            Console.WriteLine($"{stringSymbols.Peek().Name} pushed to stack");
                            stackSymbols.Push(stringSymbols.Dequeue());
                        }
                        else
                        {
                            // reduce
                            var quad = new Quad();
                            while (quad.Count < 3 && stackSymbols.Count > 0)
                                quad.AddSymbol(stackSymbols.Pop());

                            if (quad.Operator.Name == ";")
                                continue;
                            if (quad.Operator.Name == "=")
                                quad.Result = new Symbol("N", "N");
                            else if (tempStack.Count > 0)
                            {
                                stackSymbols.Push(tempStack.Peek());
                                quad.AddSymbol(tempStack.Dequeue());
                            }

                            quadStack.Enqueue(quad);
                            quad.Print();
                        }
                    }
                }
            }

            foreach (var q in quadStack)
                q.Print();
        }
    }
}


/*
     static char[,] precTable =
       {//    =    +    -    *    /    (    )    >   >=   if  then   ;
   /* = * / { ' ', '<', '<', '<', '<', '<', '<', ' ', ' ', ' ', ' ', '>' },
   /* + * / { ' ', '>', '>', '<', '>', '<', '<', ' ', ' ', ' ', '>', '>' },
   /* - * / { ' ', '>', '>', '<', '>', '<', '<', ' ', ' ', ' ', '>', '>' },
   /* * * / { ' ', '<', '<', '<', '<', '<', '<', ' ', ' ', ' ', '>', ' ' },
   /* / * / { ' ', '>', '>', '<', '>', '>', '>', ' ', ' ', ' ', '>', '>' },
   /* ( * / { ' ', '>', '>', '<', '>', '>', '>', ' ', ' ', ' ', ' ', '>' },
   /* ) * / { ' ', '>', '>', '<', '>', '>', '>', ' ', ' ', ' ', ' ', '>' },
   /* > * / { ' ', '<', '<', '<', '<', '<', ' ', ' ', ' ', ' ', '>', ' ' },
   /* >=* / { ' ', '<', '<', '<', '<', '<', ' ', ' ', ' ', ' ', '>', ' ' },
   /* if* / { ' ', '<', '<', '<', '<', '<', ' ', '<', '<', ' ', '=', ' ' },
   /*then* /{ '<', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '<', ' ', ' ' },
   /* ; * / { '<', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '=' },
       };
 // 4) drive the parser
   string parentDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
   using (var reader = new StreamReader(Path.Combine(parentDir, "tokens.txt")))
   {
       string line, token, flag;
       Symbol symbol0, symbol1 = new Symbol("", "");
       var varList = new List<Symbol>();
       var quadStack = new Queue<Quad>();
       
       while ((line = reader.ReadLine()?.Trim()) != null)
       {
           token = line.Substring(0, line.IndexOf(' '));
           flag  = line.Substring(line.LastIndexOf(' ') + 1);
           symbol0 = symbol1;
           symbol1 = new Symbol(token, flag);

           if (!StartStack(symbol1, varList))
           {
               Console.WriteLine("NEXT!");
               if (flag == "var") 
                   varList.Add(symbol1);
           }
           else
           {
               Console.WriteLine($"MAKING STACK! @{token}");
               var stackSymbols  = new Stack<Symbol>();
               var stringSymbols = new Queue<Symbol>();
               var tempStack = new Queue<Symbol>();
               for (int i = 1; i <= 12; i++)
                   tempStack.Enqueue(new Symbol($"T{i}", "numlit"));
               
               if (symbol0.Type == "var" || symbol0.Type == "numlit")
                   stringSymbols.Enqueue(symbol0);

               // read until semicolon
               while (symbol1.GetSymbolType() != 7)
               {
                   stringSymbols.Enqueue(symbol1);
                   line = reader.ReadLine()?.Trim();
                   if (line == null) break;
                   Console.WriteLine($"Stack maker line: {line}");
                   token = line.Substring(0, line.IndexOf(' '));
                   flag  = line.Substring(line.LastIndexOf(' ') + 1);
                   symbol1 = new Symbol(token, flag);
               }

               stackSymbols.Push(symbol1);
               stringSymbols.Enqueue(new Symbol(";", "$semi"));

               Console.WriteLine("HERES THE STRING");
               PrintList(stringSymbols.ToList());

               Console.WriteLine("HERES THE STACK");
               foreach (var s in stackSymbols)
                   Console.WriteLine($"{s.Name} : {s.Type}");

               // operator‐precedence loop
               while (true)
               {
                   // both semicolons? done.
                   if (stackSymbols.Peek().GetSymbolType() == 7 &&
                       stringSymbols.Peek().GetSymbolType() == 7)
                   {
                       Console.WriteLine("this fucking code ran");
                       break;
                   }
                   // shift any operand
                   if (!stringSymbols.Peek().IsOperator())
                   {
                       Console.WriteLine($"NON TERMINAL PUSHED: {stringSymbols.Peek().Name}");
                       stackSymbols.Push(stringSymbols.Dequeue());
                       continue;
                   }
                   // find the topmost operator on stack
                   // Find the topmost operator on the stack without LINQ
                   Symbol fsymbol = null;
                   foreach (var sym in stackSymbols)
                   {
                       if (sym.IsOperator())
                       {
                           fsymbol = sym;
                           break;
                       }
                   }
                   if (fsymbol == null)
                   {
                       break;
                   }
                   
                   int fvalue = precFunc[0, fsymbol.GetSymbolType()];
                   int gvalue = precFunc[1, stringSymbols.Peek().GetSymbolType()];
                   Console.WriteLine($"f({fsymbol.Name})={fvalue} AND g({stringSymbols.Peek().Name})={gvalue}");
                   
                   if (fvalue < gvalue)
                   {
                       Console.WriteLine($"{stringSymbols.Peek().Name} pushed to stack");
                       stackSymbols.Push(stringSymbols.Dequeue());
                   }
                   else if (fvalue >= gvalue)
                   {
                       // reduce
                       var quad = new Quad();
                       while (quad.Count < 3 && stackSymbols.Count > 0)
                           quad.AddSymbol(stackSymbols.Pop());
                       if (quad.Operator.Name == ";")
                           continue;
                       if (quad.Operator.Name == "=")
                           quad.Result = new Symbol("N", "N");
                       else if (tempStack.Count > 0)
                       {
                           stackSymbols.Push(tempStack.Peek());
                           quad.AddSymbol(tempStack.Dequeue());
                       }
                       quadStack.Enqueue(quad);
                       quad.Print();
                   }
               }
           }
       }

       foreach (var q in quadStack)
       {
           q.Print();
       }
    }
 */

