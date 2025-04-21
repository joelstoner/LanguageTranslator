namespace LanguageTranslator;

public class AssemblyGenerator
{
    internal void Run()
    {
        string parentDir = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..")
        );
        using (StreamReader reader = new StreamReader(Path.Combine(parentDir, "Quads.txt")))
        using (StreamWriter writer = new StreamWriter(Path.Combine(parentDir, "Final.txt")))
        {
            string line, op, arg1, arg2, res;
            while ((line = reader.ReadLine()) != null)
            {
                string[] quad = line.Split(' ');
                op = quad[0];
                arg1 = quad[1];
                arg2 = quad[2];
                res = quad[3];
                switch (op)
                {
                    case "+":
                        writer.WriteLine($"    mov ax, [{arg1}]");
                        writer.WriteLine($"    add ax, [{arg2}]");
                        writer.WriteLine($"    mov [{res}], ax");
                        break;
                    case "-":
                        writer.WriteLine($"    mov ax, [{arg1}]");
                        writer.WriteLine($"    sub ax, [{arg2}]");
                        writer.WriteLine($"    mov [{res}], ax");
                        break;
                    case "*":
                        writer.WriteLine($"    mov ax, [{arg1}]");
                        writer.WriteLine($"    mul [{arg2}]");
                        writer.WriteLine($"    mov [{res}], ax");
                        break;
                    case "/":
                        writer.WriteLine($"    mov dx, 0");
                        writer.WriteLine($"    mov ax, [{arg1}]");
                        writer.WriteLine($"    mov bx, [{arg2}]");
                        writer.WriteLine( "    div bx");
                        writer.WriteLine($"    mov [{res}], ax");
                        break;
                    case "=":
                        writer.WriteLine($"    mov ax, [{arg1}]");
                        writer.WriteLine($"    mov [{res}], ax");
                        break;
                    case ">":
                        writer.WriteLine($"    mov ax, [{arg1}]");
                        writer.WriteLine($"    cmp ax, [{arg2}]");
                        writer.WriteLine($"    jle [{res}], ax");
                        break;
                    case ">=":
                        writer.WriteLine($"    mov ax, [{arg1}]");
                        writer.WriteLine($"    cmp ax, [{arg2}]");
                        writer.WriteLine($"    jl [{res}], ax");
                        break;
                    case "JUMP":
                        writer.WriteLine($"    jmp {arg1}");
                        break;
                    default:
                        if (op.First() == 'L')
                        {
                            writer.WriteLine($"{op.Substring(0, 2)}: ");
                        }
                        Console.WriteLine($"Unknown op: {op}");
                        break;
                }
            }
        }
    }
}