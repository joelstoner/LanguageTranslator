namespace LanguageTranslator;

public class SymbolTableGenerator
{
    static bool IsReservedWord(string w)
    {
        if (w == "$const")
            return true;
        if (w == "$if")
            return true;
        if (w == "$var")
            return true;
        if (w == "$then")
            return true;
        if (w == "$procedure")
            return true;
        if (w == "$while")
            return true;
        if (w == "$call")
            return true;
        if (w == "$do")
            return true;
        if (w == "$odd")
            return true;
        if (w == "$class")
            return true;
        return false;
    }
    static int GetFlagType(int s, string f)
    {
        if (f == "$class")
            return 0;
        if (f == "var" && s != 3)
            return 1;
        if (f == "$LB")
            return 2;
        if (f == "$const")
            return 3;
        if (f == "$=")
            return 5;
        if (f == "numlit")
            return 6;
        if (f == "$semi")
            return 7;
        if (f == "$comma")
            return 8;
        if (f == "$var")
            return 9;
        if (s == 3 && (f == "var" || IsReservedWord(f)))
            return 4;
        if (f == "$RB")
            return 11;
        return 10;
    }
    
    internal void Run()
    {
        string[,] fsa = new FiniteStateTable().GetFiniteStateTable
            (Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FSA2.xlsx"));
        for (int i = 0; i < fsa.GetLength(0); i++) // displays 2nd fsa
        {
            for (int j = 0; j < fsa.GetLength(1); j++)
            {
                Console.Write("{0,-10}", fsa[i, j]); 
            }
            Console.WriteLine();
        }
        
        string token, flag, varName = "", line;
        int state = 0, flagType, symbolCount = 1, codeAddress = 0, dataAddress = 0, varValue = 0, nextState;
        bool keepRunning = true;
        
        string parentDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
        using (StreamReader reader = new StreamReader(Path.Combine(parentDir, "tokens.txt")))
        using (StreamWriter writer = new StreamWriter(Path.Combine(parentDir, "SymbolTable.txt")))
        {
            try
            {
                while ((line = reader.ReadLine().Trim()) != null && keepRunning)
                {
                    token = line.Substring(0, line.IndexOf(' '));
                    flag = line.Substring(line.LastIndexOf(' ') + 1);
                    flagType = GetFlagType(state, flag);
                    nextState = int.Parse(fsa[state + 1, flagType]);
                    switch (state)
                    {
                        case 0:
                            switch (nextState)
                            {
                                case 1:
                                    Console.WriteLine($"Moved from state {state} to {nextState}: {token} {flag}");
                                    state = nextState;
                                    break;
                                default:
                                    keepRunning = false;
                                    Console.WriteLine($"Invalid state {flagType} from state {state}: {token} {flag}");
                                    break;
                            }

                            break;
                        case 1:
                            switch (nextState)
                            {
                                case 2: // class generated
                                    writer.Write("{0,-3}", symbolCount);
                                    writer.Write("{0,-15}", token);
                                    writer.Write("{0,-15}", "$pgmname");
                                    writer.Write("{0,-6}", "n/a");
                                    writer.Write("{0,-6}", codeAddress);
                                    writer.WriteLine("{0,-2}", "CS");
                                    Console.WriteLine($"Moved from state {state} to {nextState}: {token} {flag}");
                                    state = nextState;
                                    symbolCount++;
                                    codeAddress += 2;
                                    break;
                                default:
                                    keepRunning = false;
                                    Console.WriteLine(
                                        $"Error! State: {state} || Next State: {nextState} || FlagType: {flagType}");
                                    break;
                            }

                            break;
                        case 2:
                            switch (nextState)
                            {
                                case 3:
                                    Console.WriteLine($"Moved from state {state} to {nextState}: {token} {flag}");
                                    state = nextState;
                                    break;
                                default:
                                    keepRunning = false;
                                    Console.WriteLine(
                                        $"Error! State: {state} || Next State: {nextState} || FlagType: {flagType}");
                                    break;
                            }

                            break;
                        case 3:
                            switch (nextState)
                            {
                                case 4 or 8 or 10:
                                    Console.WriteLine($"Moved from state {state} to {nextState}: {token} {flag}");
                                    state = nextState;
                                    //Console.WriteLine($"Debug: {token} {flag} from {state} to {nextState}");
                                    break;
                                default:
                                    keepRunning = false;
                                    Console.WriteLine(
                                        $"Error! State: {state} || Next State: {nextState} || FlagType: {flagType}");
                                    break;
                            }

                            break;
                        case 4:
                            switch (nextState)
                            {
                                case 5:
                                    Console.WriteLine($"Moved from state {state} to {nextState}: {token} {flag}");
                                    varName = token;
                                    state = nextState;
                                    break;
                                default:
                                    keepRunning = false;
                                    Console.WriteLine($"Invalid state {flagType} from state {state}: {token} {flag}");
                                    break;
                            }

                            break;
                        case 5:
                            switch (nextState)
                            {
                                case 6:
                                    Console.WriteLine($"Moved from state {state} to {nextState}: {token} {flag}");
                                    state = nextState;
                                    break;
                                default:
                                    keepRunning = false;
                                    Console.WriteLine(
                                        $"Error! State: {state} || Next State: {nextState} || FlagType: {flagType}");
                                    break;
                            }
                            break;
                        case 6:
                            switch (nextState)
                            {
                                case 7:
                                    Console.WriteLine($"Moved from state {state} to {nextState}: {token} {flag}");
                                    state = nextState;
                                    varValue = int.Parse(token);
                                    break;
                                default:
                                    keepRunning = false;
                                    Console.WriteLine(
                                        $"Error! State: {state} || Next State: {nextState} || FlagType: {flagType}");
                                    break;
                            }

                            break;
                        case 7:
                            switch (nextState)
                            {
                                case 3 or 4:
                                    writer.Write("{0,-3}", symbolCount);
                                    writer.Write("{0,-15}", varName);
                                    writer.Write("{0,-15}", "constvar");
                                    writer.Write("{0,-6}", varValue);
                                    writer.Write("{0,-6}", dataAddress);
                                    writer.WriteLine("{0,-2}", "DS");
                                    Console.WriteLine($"Moved from state {state} to {nextState}: {token} {flag}");
                                    state = nextState;
                                    symbolCount++;
                                    dataAddress += 2;
                                    break;
                                default:
                                    keepRunning = false;
                                    Console.WriteLine(
                                        $"Error! State: {state} || Next State: {nextState} || FlagType: {flagType}");
                                    break;
                            }

                            break;
                        case 8:
                            switch (nextState)
                            {
                                case 9:
                                    Console.WriteLine($"Moved from state {state} to {nextState}: {token} {flag}");
                                    state = nextState;
                                    varName = token;
                                    break;
                                default:
                                    keepRunning = false;
                                    Console.WriteLine(
                                        $"Error! State: {state} || Next State: {nextState} || FlagType: {flagType}");
                                    break;
                            }

                            break;
                        case 9:
                            switch (nextState)
                            {
                                case 3 or 8:
                                    writer.Write("{0,-3}", symbolCount);
                                    writer.Write("{0,-15}", varName);
                                    writer.Write("{0,-15}", "var");
                                    writer.Write("{0,-6}", "null");
                                    writer.Write("{0,-6}", dataAddress);
                                    writer.WriteLine("{0,-2}", "DS");
                                    Console.WriteLine($"Moved from state {state} to {nextState}: {token} {flag}");
                                    state = nextState;
                                    symbolCount++;
                                    dataAddress += 2;
                                    break;
                                default:
                                    keepRunning = false;
                                    Console.WriteLine(
                                        $"Error! State: {state} || Next State: {nextState} || FlagType: {flagType}");
                                    Console.WriteLine($"{token} || {flag}");
                                    break;
                            }

                            break;
                        case 10:
                            switch (nextState)
                            {
                                case 10:
                                    Console.WriteLine($"Moved from state {state} to {nextState}: {token} {flag}");
                                    state = nextState;
                                    break;
                                case 11:
                                    writer.Write("{0,-3}", symbolCount);
                                    writer.Write("{0,-15}", token);
                                    writer.Write("{0,-15}", flag);
                                    writer.Write("{0,-6}", token);
                                    writer.Write("{0,-6}", dataAddress);
                                    writer.WriteLine("{0,-2}", "DS");
                                    Console.WriteLine($"Moved from state {state} to {nextState}: {token} {flag}");
                                    state = nextState;
                                    symbolCount++;
                                    dataAddress += 2;
                                    break;
                                case 12:
                                    keepRunning = false;
                                    break;
                                default:
                                    keepRunning = false;
                                    Console.WriteLine(
                                        $"Error! State: {state} || Next State: {nextState} || FlagType: {flagType}");
                                    Console.WriteLine($"{token} || {flag}");
                                    break;
                            }

                            break;
                        case 11:
                            switch (nextState)
                            {
                                case 10:
                                    Console.WriteLine($"Moved from state {state} to {nextState}: {token} {flag}");
                                    state = nextState;
                                    break;
                                case 12:
                                    keepRunning = false;
                                    break;
                                default:    //Console.WriteLine($"Line: {line}");
                                    keepRunning = false;
                                    Console.WriteLine(
                                        $"Error! State: {state} || Next State: {nextState} || FlagType: {flagType}");
                                    Console.WriteLine($"{token} || {flag}");
                                    break;
                            }

                            break;
                        case 12:
                            keepRunning = false;
                            break;
                        default:
                            Console.WriteLine($"Default Invalid state {flagType} from state {state}: {token} {flag}");
                            keepRunning = false;
                            break;
                    }

                }
            }
            catch (NullReferenceException) // catches to EOF condition
            {
                Console.WriteLine("EOF reached");
            }

            for (int a = 1; a <= 3; a++) // generate temp variables
            {
                writer.Write("{0,-3}", symbolCount);
                writer.Write("{0,-15}", $"T{a}");
                writer.Write("{0,-15}", "var");
                writer.Write("{0,-6}", "null");
                writer.Write("{0,-6}", dataAddress);
                writer.WriteLine("{0,-2}", "DS");
                symbolCount++;
                dataAddress += 2;
            }

        }
    }
}