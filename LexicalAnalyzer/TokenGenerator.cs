using System.Runtime.InteropServices;

namespace LanguageTranslator;

class TokenGenerator
{
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
        if (ch == '+')
            return 13;
        if (ch == '–')
            return 14;
        if (char.IsWhiteSpace(ch))
            return 15;
        return 16;
    }

    static void GenerateToken(ref string token, ref string state, string flag, StreamWriter w, StreamReader r)
    { 
        w.WriteLine($"{token} {flag}");
        Console.WriteLine($"Token Generated: {token} {flag}");
        state = "0";
        token = "";
    }

    static void GenerateToken(char token, string flag, StreamWriter w)
    {
        w.WriteLine($"{token} {flag}");
        Console.WriteLine($"Token Generated: {token} {flag}");
    }

    static string GetDelimiter(char d)
    {
        if (d == '*' || d == '/') 
            return "$mop";
        if (d == ',') 
            return "$comma";
        if (d == ';') 
            return "$semi";
        if (d == '{') 
            return "$LB";
        if (d == '}') 
            return "$RB";
        if (d == '(') 
            return "$LP";
        if (d == ')') 
            return "$RP";
        if (d == '+' || d == '–') 
            return "$addop";
        return "poop";
    }

    static string ReservedWordCheck(string w)
    {
        if (w == "CONST")
            return "$const";
        if (w == "IF")
            return "$if";
        if (w == "VAR")
            return "$var";
        if (w == "THEN")
            return "$then";
        if (w == "PROCEDURE")
            return "$procedure";
        if (w == "WHILE")
            return "$while";
        if (w == "CALL")
            return "$call";
        if (w == "DO")
            return "$do";
        if (w == "ODD")
            return "$odd";
        if (w == "CLASS")
            return "$class";
        return "var";

    }
    internal void Run()
    {
        string[,] fsa = new FiniteStateTable().GetFiniteStateTable(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FSA1.xlsx"));
        
        for (int i = 0; i < fsa.GetLength(0); i++)
        {
            for (int j = 0; j < fsa.GetLength(1); j++)
            {
                Console.Write("{0,-10}", fsa[i, j] + " ");
            }
            Console.WriteLine();
        }

        string parentDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
        using (StreamReader reader = new StreamReader(Path.Combine(parentDir, "PGM1.txt")))
        using (StreamWriter writer = new StreamWriter(Path.Combine(parentDir, "tokens.txt")))
        {
            string buffer = "";
            string currentState = "0";
            int intChar;
            char newChar;
            bool breaker = false;
            while ((intChar = reader.Read()) != -1 && !breaker)
            {
                newChar = (char)intChar;
                int newCharType = GetCharType(newChar);
                string nextState = fsa[int.Parse(currentState)+1, newCharType];
                bool readNextChar = true;
                Console.WriteLine($"Moved from state {currentState} to {nextState}: {newChar} read in");
                switch(int.Parse(currentState))
                {
                    case 0: // starting state
                        switch (int.Parse(nextState))
                        {
                            case 0: // white space field
                                currentState = nextState;
                                break;
                            case 1: // invalid character
                                Console.WriteLine("Error: Invalid character " + newChar);
                                break;
                            case 2 or 20 or 21 or 22 or 23 or 24 or 25 or 26 or 27: // final state reached
                                buffer += newChar;
                                GenerateToken(ref buffer, ref currentState, GetDelimiter(newChar), writer, reader);
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
                            case 4: // end of numlitbreakbreak
                                GenerateToken(ref buffer, ref currentState, "numlit", writer, reader);
                                if(GetCharType(newChar) > 3 && GetCharType(newChar) < 15)
                                    GenerateToken(newChar, GetDelimiter(newChar), writer);
                                break;
                            case 6:
                                GenerateToken(ref buffer, ref currentState, ReservedWordCheck(buffer), writer, reader);
                                if(GetCharType(newChar) > 3 && GetCharType(newChar) < 15)
                                    GenerateToken(newChar, GetDelimiter(newChar), writer);
                                break;
                        }
                        break;
                    case 7: // slash is found
                        switch (int.Parse(nextState))
                        {
                            case 10:
                                GenerateToken(ref buffer, ref currentState, "$mop", writer, reader);
                                break;
                            case 8 or 9:
                                currentState = nextState;
                                break;
                        }
                        break;
                    case 9: // comment state
                        currentState = nextState;
                        break;
                    case 11 or 14 or 17:
                        switch (int.Parse(nextState))
                        {
                            case 12 or 13 or 15 or 16 or 18 or 19:
                                buffer += newChar;
                                GenerateToken(ref buffer, ref currentState, $"${buffer}", writer, reader);
                                break;
                        }
                        break;
                    default:
                        GenerateToken(ref buffer, ref currentState, "$", writer, reader);
                        break;
                }
            }
        }
        
    }
}
