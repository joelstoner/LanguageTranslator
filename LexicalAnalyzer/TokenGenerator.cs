using System.Runtime.InteropServices;

namespace LanguageTranslator;


class TokenGenerator
{
    enum CharType
    {
        LETTER, DIGIT, MULT, DIV, ASSIGN, CARROT, SPACE, OTHER
    }

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
        // Windows Dir: C:\\Users\\jston\\RiderProjects\\LanguageTranslator\\LexicalAnalyzer\\java0.txt
        // Linux Dir: /home/joelstoner/RiderProjects/TestProject/LexicalAnalyzer/java0.txt
        using (StreamReader reader = new StreamReader(GetOsDir()))
        {
            string buffer = "";
            string state = "0";  // starting state
            while (!reader.EndOfStream)
            {
                int nextCharInt = reader.Peek();
                if (nextCharInt == -1)
                    break;
                char nextChar = (char)nextCharInt;
                CharType type = GetCharType(nextChar);
                
                string newState = fsa[int.Parse(state) + 1, (int)type];
                
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
                    if (!string.IsNullOrEmpty(buffer))
                    {
                        tokens.Add(buffer.Trim());
                        Console.WriteLine(buffer.Trim());
                        buffer = "";
                        state = "0";
                    }
                    else
                    {
                        reader.Read();
                    }
                }
            }
            
            if (!string.IsNullOrEmpty(buffer))
            {
                tokens.Add(buffer);
                Console.WriteLine("Token: " + buffer);
            }
        }

        
    }
}