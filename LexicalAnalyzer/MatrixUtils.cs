using System;

namespace LanguageTranslator
{
    public static class MatrixUtils
    {
        public static int[,] Warshall(int[,] matrix)
        {
            int V = matrix.GetLength(0);
            for (int k = 0; k < V; k++)
                for (int i = 0; i < V; i++)
                    for (int j = 0; j < V; j++)
                        if (matrix[i, k] == 1 && matrix[k, j] == 1)
                            matrix[i, j] = 1;
            return matrix;
        }

        public static int[,] AddIdentity(int[,] matrix)
        {
            int n = matrix.GetLength(0);
            for (int i = 0; i < n; i++)
                matrix[i, i] = 1;
            return matrix;
        }

        public static int[,] Transpose(int[,] matrix)
        {
            int n = matrix.GetLength(0);
            var result = new int[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    result[j, i] = matrix[i, j];
            return result;
        }

        public static int[,] BMatrix(int[,] gte, int[,] lte)
        {
            var translte = Transpose(lte);
            int size = gte.GetLength(0) * 2;
            var bm = new int[size, size];
            int half = size / 2;

            for (int i = 0; i < half; i++)
                for (int j = 0; j < half; j++)
                {
                    bm[i, j] = 0;
                    bm[half + i, half + j] = 0;
                    bm[half + i, j] = translte[i, j];
                    bm[i, half + j] = gte[i, j];
                }

            bm = Warshall(bm);
            return AddIdentity(bm);
        }

        public static int[,] PrecedenceFunction(int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int half = rows / 2;
            var pf = new int[2, half];
            
            for (int i = 0; i < rows; i++)
            {
                int sum = 0;
                for (int j = 0; j < cols; j++)
                    sum += matrix[i, j];

                if (i < half)
                    pf[0, i] = sum;
                else
                    pf[1, i - half] = sum;
            }
            
            return pf;
        }

        public static int[,] GetInitTable(char init, char[,] precTable)
        {
            int n = precTable.GetLength(0), m = precTable.GetLength(1);
            var equalTable = new int[n, m];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                    equalTable[i, j] = (precTable[i, j] == init || precTable[i, j] == '=') ? 1 : 0;
            return equalTable;
        }

        public static void PrintMatrix(int[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                Console.Write("{ ");
                for (int j = 0; j < matrix.GetLength(1); j++)
                    Console.Write($"{matrix[i, j],3}");
                Console.WriteLine(" }");
            }
            Console.WriteLine();
        }
    }
}
