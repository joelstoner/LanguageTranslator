using System.Runtime.InteropServices;

namespace LanguageTranslator;


class TokenGenerator
{
    private static string GetOsDir()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "C:\\Users\\jston\\RiderProjects\\LanguageTranslator\\LexicalAnalyzer\\java0.txt";
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "/home/joelstoner/RiderProjects/TestProject/LexicalAnalyzer/java0.txt";
        }
        return null;
    }
    static int GetCharType(char ch)
    {
        if (char.IsLetter(ch))
            return 0;
        if (char.IsDigit(ch))
            return 1;
        if (ch == '*')
            return 2;
        if (ch == '/')
            return 3;
        if (ch == '=')
            return 4;
        if (ch == '<')
            return 5;
        if (ch == '>')
            return 6;
        if (ch == ',')
            return 7;
        if (ch == ';')
            return 8;
        if (ch == '{')
            return 9;
        if (ch == '}')
            return 10;
        if (ch == '(')
            return 11;
        if (ch == ')')
            return 12;
        if (char.IsWhiteSpace(ch))
            return 13;
        return 14;
    }

    static void GenerateToken(ref string token, string flag)
    {
        Console.WriteLine($"Token generated: {token}, {flag}\n");
        token = "";
    }
    
    static void Main(string[]args)
    {
        string[,] fsa = new FiniteStateTable().GetFiniteStateTable();
        //List<string> tokens = new List<string>();
        for (int i = 0; i < fsa.GetLength(0); i++)
        {
            for (int j = 0; j < fsa.GetLength(1); j++)
            {
                Console.Write("{0,-10}", fsa[i, j] + " ");
            }
            Console.WriteLine();
        }
        // Windows Dir: C:\\Users\\jston\\RiderProjects\\LanguageTranslator\\LexicalAnalyzer\\java0.txt
        // Linux Dir: /home/joelstoner/RiderProjects/TestProject/LexicalAnalyzer/java0.txt
        using (StreamReader reader = new StreamReader(GetOsDir()))
        {
            string buffer = "";
            string currentState = "0";
            int intChar;
            char newChar;
            bool breaker = true;
            //newChar = reader.Read();
            while ((intChar = reader.Read()) != -1 && breaker)
            {
                newChar = (char)intChar;
                Console.WriteLine($"{newChar} read in");
                int newCharType = GetCharType(newChar);
                string nextState = fsa[int.Parse(currentState)+1, newCharType];
                Console.WriteLine($"State moved to {nextState}");
                switch(int.Parse(currentState))
                {
                    case 0: // starting state
                        switch (int.Parse(nextState))
                        {
                            case 0: // white space field
                                currentState = nextState;
                                break;
                            case 1: // invalid character
                                Console.WriteLine("Error: Invalid character");
                                GenerateToken(ref buffer, "<inval>");
                                break;
                            case 2 or 20 or 21 or 22 or 23 or 24 or 25: // final state reached
                                buffer += newChar;
                                GenerateToken(ref buffer, "<something>");
                                break;
                            case 3 or 5 or 7 or 9 or 11 or 14: // char added to buffer
                                currentState = nextState;
                                buffer += newChar;
                                break;
                        }
                        break;
                    case 3 or 5: // numeric literal is found
                        switch (int.Parse(nextState))
                        {
                            case 3 or 5: // adds each digit to buffer
                                buffer += newChar;
                                currentState = nextState;
                                break;
                            case 4 or 6: // end of numlitbreakbreak
                                GenerateToken(ref buffer, "<>");
                                break;
                        }
                        break;
                    /* case 5: // variable or reserved word is found
                        switch (int.Parse(nextState))
                        {
                            case 5:
                                buffer += newChar;
                                currentState = nextState;
                                break;
                            case 6:
                                GenerateToken(ref buffer, nextState);
                                break;
                        }
                        break; */
                    case 7: // slash is found
                        switch (int.Parse(nextState))
                        {
                            case 10:
                                GenerateToken(ref buffer, "<>");
                                break;
                            case 8 or 9:
                                currentState = nextState;
                                break;
                        }
                        break;
                    case 9:
                        currentState = nextState;
                        break;
                    case 11 or 14 or 17:
                        switch (int.Parse(nextState))
                        {
                            case 12 or 15 or 18:
                                buffer += newChar;
                                GenerateToken(ref buffer, "<>");
                                break;
                            case 13 or 16 or 19:
                                buffer += newChar + '=';
                                GenerateToken(ref buffer, "<>");
                                break;
                        }
                        break;
                        
                }
                
            }
        }
        
    }
}
/*
        using (StreamReader reader = new StreamReader(GetOsDir()))
        {
            string buffer = "";
            string state = "0";  // starting state
            int read = reader.Read();
            while (!reader.EndOfStream)
            {
                int nextCharInt = reader.Peek();
                if (nextCharInt == -1)
                    break;
                char nextChar = (char)nextCharInt;
                CharType type = GetCharType(nextChar);
                
                string newState = fsa[int.Parse(state) + 1, (int)type];
                Console.WriteLine("State: " + newState);
                
                if (int.TryParse(newState, out int numericState))
                {
                    reader.Read(); 
                    if (numericState == -1)
                    {
                        Console.WriteLine("Error: Invalid Character");
                        break;
                    }
                    buffer += nextChar;
                    state = newState;
                }
                else
                {
                    // We have hit a final state.
                    if (!string.IsNullOrEmpty(buffer))
                    {
                        // Finalize the token that was being accumulated.
                        tokens.Add(buffer.Trim());
                        Console.WriteLine(buffer.Trim());
                        buffer = "";
                    }
                    else
                    {
                        // If the buffer is empty, it might be a punctuation token.
                        // Here, you can choose to add the final state's label (newState) as a token.
                        tokens.Add(newState);
                        Console.WriteLine(newState);
                    }
                    reader.Read(); // consume the punctuation char
                    state = "0";
                    Console.WriteLine();
                }

            }
            
            if (!string.IsNullOrEmpty(buffer))
            {
                tokens.Add(buffer);
                Console.WriteLine("Token: " + buffer);
            }
        }
        */