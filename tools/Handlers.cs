namespace Utf8Validator.tools;

static class Handlers
{
    internal static (int ok, int total) Dirs(string[] args)
    {
        int ok = 0;
        int total = 0;
        if (args.Length == 0)
        {
            Display.Error(
                "Directory",
                $"No arguments given for directories. Use -h to show the usage"
            );
            return (0, 0);
        }
        foreach (string dir in args)
        {
            if (!Directory.Exists(dir))
            {
                Display.Error(dir, $"Directory {dir} is not found");
                total++;
            }
            string[] Files = Directory.GetFiles(dir);
            (int currentOk, int currentTotal) = Handlers.Files(Files);
            ok += currentOk;
            total += currentTotal;
        }
        return (ok, total);
    }

    internal static (int ok, int total) Files(string[] filePath)
    {
        int total = 0;
        int ok = 0;
        foreach (string filename in filePath)
        {
            total++;
            if (!File.Exists(filename))
            {
                Display.Error(filename, $"File does not exist in the current directory");
                continue;
            }

            Display.Info($"Reading {filename}");

            Stream data = new FileStream(filename, FileMode.Open);
            (bool success, string errorMsg) = Validator.Run(data);
            if (success)
            {
                Display.Success(filename);
                ok++;
            }
            else
            {
                Display.Error(filename, errorMsg);
            }
        }

        return (ok, total);
    }

    internal static (int ok, int total) Text(string[] args)
    {
        // C# only permets a string[] for the CLI arguments input.
        // This method bypass the C# logic and permit 2 use cases :
        //    - with argument after the flag -> uses manual parsing
        //    - without argument after the flag -> uses stdin ??????

        int ok = 0;
        int total = 1;
        bool success;
        string errorMsg;

        // If the user type bytes as CLI Arguments,
        // we manually parse each character in the string
        // into a byte following the printf command syntax
        //
        // (\xFA become real 0xFA byte)
        if (args.Length >= 1)
        {
            (success, errorMsg) = ReadCLI(args[1..]);
            if (success)
            {
                Display.Success("CLI Bytes");
                ok++;
            }
            else
            {
                Display.Error("CLI bytes", errorMsg);
            }
        }
        else
        {
            (success, errorMsg) = ReadStdin();
            if (!success)
            {
                Display.Error("Stdin bytes", errorMsg);
            }
            else
            {
                Display.Success("stdin");
                ok++;
            }
        }
        // No other argument in args : we prompt the user via stdin
        return (ok, total);
    }

    internal static (bool success, string errorMsg) ReadCLI(string[] text)
    {
        Display.Info("reading cli arguments");
        (bool success, byte[] output, string errormsg) = ParseTextToBytes(text[0]);
        if (!success)
        {
            return (false, errormsg);
        }

        Stream inputstream = new MemoryStream(output);
        return Validator.Run(inputstream);
    }

    internal static (bool success, string errorMsg) ReadStdin()
    {
        Display.Info("reading stdin");
        Stream stdin = Console.OpenStandardInput();
        return Validator.Run(stdin);
    }

    static (bool, byte[], string errormsg) ParseTextToBytes(string text)
    {
        static byte? convertchartobyte(char c) =>
            c switch
            {
                '0' => 0,
                '1' => 1,
                '2' => 2,
                '3' => 3,
                '4' => 4,
                '5' => 5,
                '6' => 6,
                '7' => 7,
                '8' => 8,
                '9' => 9,
                'a' or 'a' => 10,
                'b' or 'b' => 11,
                'c' or 'c' => 12,
                'd' or 'd' => 13,
                'e' or 'e' => 14,
                'f' or 'f' => 15,
                _ => null,
            };

        /*
         * for a result of 0xaf in hex :
         * e1 = a
         * e0 = f
         */
        byte? e0 = null,
            e1 = null;
        byte[] buffer = new byte[text.Length];
        int idx = 0;
        bool success = default;
        string errormsg = string.Empty;

        foreach (char c in text)
        {
            if (c == '\\' || c == 'x')
                continue;

            errormsg = $"invalid character detected when parsing text to byte: {c}";
            // this initial null check is necessary to "advance the pointer".
            if (e1 is null)
            {
                e1 = convertchartobyte(c);
                if (e1 is null)
                {
                    success = false;
                    break;
                }
                else
                    continue;
            }

            e0 ??= convertchartobyte(c);
            if (e0 is null)
            {
                success = false;
                break;
            }

            byte value = (byte)(e1 * 16 + e0);
            buffer[idx] = value;
            idx++;

            e0 = null;
            e1 = null;

            //todo: debug and show detection here to display each byte
            Console.Write($"{value:x2} ");

            success = true;
        }
        Console.Write("\n");
        return (success, buffer, errormsg);
    }
}
