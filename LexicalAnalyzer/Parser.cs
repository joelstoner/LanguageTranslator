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
        if (Type == "$LP") return 3;
        if (Type == "$RP") return 4;
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

    public Quad()
    {
        Operator  = new Symbol("N", "N");
        Argument1 = new Symbol("N", "N");
        Argument2 = new Symbol("N", "N");
        Result    = new Symbol("N", "N");
    }

    public void AddSymbol(Symbol symbol)
    {
        if (symbol.IsOperator() || symbol.Name == "=")
            Operator = symbol;
        else if (Argument2.Name == "N")
            Argument2 = symbol;
        else if (Argument1.Name == "N")
            Argument1 = symbol;
        else
            Result = symbol;

        Count++;
    }

    public void Print()
    {
        string parentDir = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..")
        );
        using (StreamWriter writer = new StreamWriter(Path.Combine(parentDir,"Quads.txt")))
        {
            try
            {
                Console.WriteLine($"QUAD: {Operator.Name} {Argument1.Name} {Argument2.Name} {Result.Name}");
                //writer.WriteLine($"{Operator.Name} {Argument1.Name} {Argument2.Name} {Result.Name}");
            }
            catch
            {
                Console.WriteLine($"QUAD Error: {Operator.Name}");
            }
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
        { ' ', '<', ' ', ' ', ' ', ' ', ' ', '<', ' ', ' ', ' ', '>' }, // then
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

    static bool ShouldStartStack(string s)
    {
        if (s == "var" || s == "$if" || s == "$then" || s == "$output" || s == "$while" || s == "$do") 
            return true;
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
        string parentDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
        using (var reader = new StreamReader(Path.Combine(parentDir, "tokens.txt")))
        {
            string line, token, flag;
            Symbol symbol0, symbol1 = new Symbol("", "");
            //var varList   = new List<Symbol>();
            var quadStack = new Queue<Quad>();
            // finds the {
            while ((line = reader.ReadLine()) != null)
            {
                token = line.Substring(0, line.IndexOf(' '));
                flag = line.Substring(line.LastIndexOf(' ') + 1);
                if (token == "{")
                    break;
            }
            
            bool compoundMode = false, whileMode = false;
            var labelStack = new Queue<Symbol>();
            var waitLabelStack = new Queue<Symbol>();
            int labelCount = 0;
            for (int i = 1; i <= 12; i++)
                labelStack.Enqueue(new Symbol($"L{i}", "N"));
            for (int i = 1; i <= 12; i++)
                labelStack.Enqueue(new Symbol($"L{i}", "N"));
            // searches for operation sentences
            while ((line = reader.ReadLine()) != null)
            {
                Console.WriteLine($"Check point 1: {line}");
                // if not valid sentence, continues running 
                token = line.Substring(0, line.IndexOf(' '));
                flag = line.Substring(line.LastIndexOf(' ') + 1);
                Symbol symbol = new Symbol(token, flag);
                var stackSymbols = new Stack<Symbol>();
                var stringSymbols = new Queue<Symbol>();
                var tempStack = new Queue<Symbol>();
                
                
                if (compoundMode && token == "}")
                {
                    Console.WriteLine("THE CODE RAN!");
                    Quad q = new Quad();
                    q.Operator = waitLabelStack.Dequeue();
                    quadStack.Enqueue(q);
                    q.Print();
                    compoundMode = false;
                    continue;
                }
                if (whileMode && token == "}")
                {
                    Console.WriteLine("THE CODE RAN!");
                    Quad q = new Quad();
                    q.Operator = new Symbol("JUMP", "$jump");
                    q.Argument1 = waitLabelStack.Dequeue();
                    quadStack.Enqueue(q);
                    q.Print();
                    whileMode = false;
                    
                    q = new Quad();
                    q.Operator = waitLabelStack.Dequeue();
                    quadStack.Enqueue(q);
                    q.Print();
                    whileMode = false;
                    continue;
                }
                if (!ShouldStartStack(flag))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        Console.WriteLine($"Check point 2: {line}");
                        token = line.Substring(0, line.IndexOf(' '));
                        if (token == ";")
                            break;
                    }
                    //continue;
                }
                else //if (flag == "var")
                {
                    if (flag == "$if")
                        compoundMode = true;
                    // sentence with operations found
                    Console.WriteLine($"MAKING STACK! @{token}");
                    for (int i = 1; i <= 12; i++)
                        tempStack.Enqueue(new Symbol($"T{i}", "numlit"));

                    // construct string
                    stringSymbols.Enqueue(symbol);
                    while ((line = reader.ReadLine()) != null)
                    {
                        Console.WriteLine($"Check point 3: {line}");
                        token = line.Substring(0, line.IndexOf(' '));
                        flag = line.Substring(line.LastIndexOf(' ') + 1);
                        symbol = new Symbol(token, flag);
                        stringSymbols.Enqueue(symbol);
                        if (token == ";")
                            break;
                    }

                    stackSymbols.Push(symbol);
                    Console.Write("HERES THE STRING:");
                    PrintList(stringSymbols.ToList());
                    Console.Write("HERES THE STACK:");
                    PrintList(stackSymbols.ToList());

                    while (true)
                    {
                        // both semicolons? done.
                        try
                        {
                            if (stackSymbols.Peek().GetSymbolType() == 11 &&
                                stringSymbols.Peek().GetSymbolType() == 11)
                            {
                                Console.WriteLine("this code ran");
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                        }
                        
                        // while shit
                        if (stringSymbols.Peek().Name == "WHILE")
                        {
                            Quad quad = new Quad();
                            quad.Operator = stringSymbols.Dequeue();
                            quad.Print();
                            quadStack.Enqueue(quad);

                            quad = new Quad();
                            quad.Operator = labelStack.Peek();
                            waitLabelStack.Enqueue(labelStack.Dequeue());
                            quad.Print();
                            quadStack.Enqueue(quad);
                            
                            quad = new Quad();
                            quad.Argument1 = (stringSymbols.Dequeue());
                            quad.Operator = stringSymbols.Dequeue();
                            quad.Argument2 = (stringSymbols.Dequeue());
                            //quad.AddSymbol(stringSymbols.Dequeue());
                            quad.Result = labelStack.Peek();
                            waitLabelStack.Enqueue(labelStack.Dequeue());
                            quad.Print();
                            
                            quadStack.Enqueue(quad);
                            stringSymbols.Dequeue();
                            stringSymbols.Dequeue();
                            whileMode = true;
                            continue;
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
                        
                        
                        Console.Write("CURRENT STACK: ");
                        PrintList(stackSymbols.Reverse().ToList());
                        Console.Write("CURRENT STRING: ");
                        PrintList(stringSymbols.ToList());

                        
                        int fvalue = precFunc[0, fsymbol.GetSymbolType()];
                        int gvalue = precFunc[1, stringSymbols.Peek().GetSymbolType()];
                        Console.WriteLine($"f({fsymbol.Name})={fvalue} AND g({stringSymbols.Peek().Name})={gvalue}");
                        if (stringSymbols.Peek().Name == ")")
                        {
                            stringSymbols.Dequeue();
                            var items = stackSymbols.Reverse().ToList();
                            items.Remove(new Symbol("(", "$LP"));
                            stackSymbols = new Stack<Symbol>(items);
                        }
                        if (stringSymbols.Peek().Name == "IF" )
                        {
                            Quad quad = new Quad();
                            stackSymbols.Push(stringSymbols.Dequeue());
                            quad.AddSymbol(stackSymbols.Pop());
                            quad.Print();
                            quadStack.Enqueue(quad);
                            //stackSymbols.Pop();
                            continue;
                        }
                        
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
                            {
                                if (stackSymbols.Peek().GetSymbolType() != 3 && stackSymbols.Peek().GetSymbolType() != 4)
                                    quad.AddSymbol(stackSymbols.Pop());
                                else
                                    stackSymbols.Pop();
                            }
                            if (quad.Operator.Name == ";" || quad.Operator.Name == "IF")
                                continue;
                            if (quad.Operator.Name == ">")
                                quad.Result = new Symbol("LE", "$stuff");
                            if (quad.Operator.Name == ">=")
                                quad.Result = new Symbol("L", "$stuff");
                                    
                            
                            stackSymbols.Push(tempStack.Peek());
                            quad.AddSymbol(tempStack.Dequeue());
                            if (quad.Operator.Name.Contains("="))
                                quad.Result = new Symbol("N", "N");
                            
                            quadStack.Enqueue(quad);
                            quad.Print();
                            if (stringSymbols.Peek().Name == "THEN")
                            {
                                quad = new Quad();
                                stackSymbols.Push(stringSymbols.Dequeue());
                                quad.AddSymbol(stackSymbols.Pop());
                                quadStack.Last().Result = labelStack.Peek();
                                Console.WriteLine($"{labelStack.Peek().Name} THIS FUCLONG");
                                waitLabelStack.Enqueue(labelStack.Dequeue());
                                Console.WriteLine($"{waitLabelStack.Peek().Name} THIS FUCLONG");
                                quad.Print();
                                quadStack.Enqueue(quad);
                                //labelCount+=2;
                                compoundMode = true;
                                stringSymbols.Dequeue();
                            }
                            /*if (labelCount > 0)
                            {
                                labelCount--;
                                if (labelCount == 0)
                                {
                                    Quad q = new Quad();
                                    q.Operator = waitLabelStack.Dequeue();
                                    quadStack.Enqueue(q);
                                    q.Print();
                                }
                            }*/
                        }
                    }
                }
            }

            using (StreamWriter writer = new StreamWriter(Path.Combine(parentDir, "Quads.txt")))
            {
                foreach (var q in quadStack)
                {
                    try
                    {
                        Console.WriteLine(
                            $"QUAD: {q.Operator.Name} {q.Argument1.Name} {q.Argument2.Name} {q.Result.Name}");
                        writer.WriteLine($"{q.Operator.Name} {q.Argument1.Name} {q.Argument2.Name} {q.Result.Name}");
                    }
                    catch
                    {
                        Console.WriteLine($"QUAD Error: {q.Operator.Name}");
                    }

                }
            }
        }
    }
    
    }
    