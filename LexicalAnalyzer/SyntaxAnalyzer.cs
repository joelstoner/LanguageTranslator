namespace LanguageTranslator;

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
        using (StreamReader reader = new StreamReader("SymbolTable.txt"))
        {
            
        }
        
    }
    
}