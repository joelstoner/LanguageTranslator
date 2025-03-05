namespace LanguageTranslator;

class TokenGenerator
{
    enum CharType
    {
        LETTER, DIGIT, MULT, DIV, ASSIGN, CARROT, SPACE, OTHER
    }

    /*
    class Token
    {
        public char ch;
        public TokenType type;
        Token(char c, TokenType t)
        {
            ch = c;
            type = t;
        }
    }*/

    static CharType GetCharType(char ch)
    {
        if (char.IsLetter(ch))
            return CharType.LETTER;
        if (char.IsDigit(ch))
            return CharType.DIGIT;
        if (ch == '*')
            return CharType.MULT;
        if (ch == '/')
            return CharType.DIV;
        if (ch == '=')
            return CharType.ASSIGN;
        if (ch == '<')
            return CharType.CARROT;
        if (char.IsWhiteSpace(ch))
            return CharType.SPACE;
        return CharType.OTHER;
    }
    /*
    int GetNewState(char ch, int s)
    {
        switch(state)
        {
            case 0:
                
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            case 6:
                break;
            case 7:
                break;
            case 8:
                break;
            case 9:
                break;
            case 10:
                break;
            case 11:
                break;
            case 12:
                break;
            case 13:
                break;
            case 14:
                break;
            case 15:
                break;
            case 16:
                break;
            default:
                break;
    }
    */
    
    
    static void Main(string[]args)
    {
        string[,] fsa = new FiniteStateTable().GetFiniteStateTable();
        List<string> tokens = new List<string>();
        for (int i = 0; i < fsa.GetLength(0); i++)
        {
            for (int j = 0; j < fsa.GetLength(1); j++)
            {
                Console.Write("{0,-10}", fsa[i, j] + " ");
            }
            Console.WriteLine();
        }
        
        using (StreamReader reader = new StreamReader("/home/joelstoner/RiderProjects/TestProject/LexicalAnalyzer/java0.txt"))
        {
            string buffer = "";
            char c;
            CharType type;
            string state = "0";
            while (reader.EndOfStream == false)
            {
                c = (char)reader.Read();
                Console.Write("Char read: " + c);
                type = GetCharType(c);
                Console.Write(" || Char type: " + type);
                state = fsa[int.Parse(state) + 1, (int)type];
                Console.WriteLine(" || Current State: " + state);
                if (int.TryParse(state, out int result))
                {
                    if (result == -1)
                    {
                        Console.WriteLine("Error: Invalid Character");
                        break;
                    }
                    buffer.Append(c);
                }
                else
                {
                    tokens.Add(buffer);
                    Console.WriteLine(buffer);
                    buffer = "";
                    state = "0";
                }
            }
        }
        
    }
}