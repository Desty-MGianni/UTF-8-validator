namespace Utf8Validator.Tools;

static class Handlers
{
    internal static (int ok, int total) Dirs(string[] args, GlobalFlags flags)
    {
        bool debugMode = CheckFlagValue(flags, GlobalFlags.Debug);

        int ok = 0;
        int total = 0;
        if (args.Length == 0)
        {
            if (debugMode)
            {
                Display.Error(
                    "Directory",
                    $"No arguments given for directories. Use -h to show the usage"
                );
            }
            return (0, 0);
        }
        foreach (string dir in args)
        {
            if (debugMode)
            {
                Display.Info($"Searching through {dir}");
            }
            if (!Directory.Exists(dir))
            {
                if (debugMode)
                {
                    Display.Error(dir, $"Directory {dir} is not found");
                }
                total++;
            }
            string[] files = Directory.GetFiles(dir);
            (int currentOk, int currentTotal) = Files(files, flags);
            ok += currentOk;
            total += currentTotal;
        }
        return (ok, total);
    }

    internal static (int ok, int total) Files(string[] filePath, GlobalFlags flags)
    {
        bool debugMode = CheckFlagValue(flags, GlobalFlags.Debug);
        bool showCharMode = CheckFlagValue(flags, GlobalFlags.ShowChar);

        int total = 0;
        int ok = 0;

        foreach (string filename in filePath)
        {
            total++;
            if (!File.Exists(filename))
            {
                if (debugMode)
                    Display.Error(filename, $"File does not exist in the current directory");
                continue;
            }

            if (debugMode)
                Display.Info($"Reading {filename}");

            Stream data = new FileStream(filename, FileMode.Open);
            (bool success, string errorMsg) = Validator.Run(data, showCharMode);
            if (success)
            {
                ok++;
                if (debugMode)
                    Display.Success(filename);
            }
            else if (!success && debugMode)
            {
                Display.Error(filename, errorMsg);
            }
        }

        return (ok, total);
    }

    internal static bool PipingOrIndirection(GlobalFlags flags)
    {
        bool debugMode = CheckFlagValue(flags, GlobalFlags.Debug);
        bool showCharMode = CheckFlagValue(flags, GlobalFlags.ShowChar);

        if (debugMode)
            Display.Info("reading stdin");
        (bool success, string errorMsg) = ReadStdin(showCharMode);

        if (success)
        {
            if (debugMode)
                Display.Success("stdin piping");
            return true;
        }
        else if (debugMode)
        {
            Display.Error("stdin piping", errorMsg);
        }
        return false;
    }

    internal static (int ok, int total) Text(string[] remArgs, GlobalFlags flags)
    {
        // C# only permets a string[] for the CLI arguments input.
        // This method bypass the C# logic and permit 2 use cases :
        //    - with argument after the flag -> uses manual parsing
        //    - without argument after the flag -> uses stdin ??????

        bool debugMode = CheckFlagValue(flags, GlobalFlags.Debug);
        bool showCharMode = CheckFlagValue(flags, GlobalFlags.ShowChar);

        int ok = 0;
        int total = 1;

        bool success;
        string errorMsg;

        // If the user type bytes as CLI Arguments,
        // we manually parse each character in the string
        // into a byte following the printf command syntax
        //
        // (\xFA become real 0xFA byte)
        if (remArgs.Length >= 1)
        {
            if (debugMode)
                Display.Info("reading cli arguments");
            (success, errorMsg) = ReadCLI(remArgs, showCharMode);
            if (success)
            {
                ok++;
                if (debugMode)
                    Display.Success("CLI Bytes");
            }
            else if (debugMode)
            {
                Display.Error("CLI bytes", errorMsg);
            }
        }
        else
        {
            if (debugMode)
                Display.Info("reading stdin");
            (success, errorMsg) = ReadStdin(showCharMode);
            if (success)
            {
                ok++;
                if (debugMode)
                    Display.Success("stdin");
            }
            else if (debugMode)
            {
                Display.Error("Stdin bytes", errorMsg);
            }
        }
        // No other argument in args : we prompt the user via stdin
        return (ok, total);
    }

    //BUG: For this command Script/utf8-validator -t \x23\x22 --show \xAC, only the 1st argument is read
    //(the flags are extracted successfully)
    //TODO: Refactor this and ParseTextToByte methods
    internal static (bool success, string errorMsg) ReadCLI(string[] remArgs, bool showCharMode)
    {
        foreach (string text in remArgs)
        {
            (bool success, byte[] buffer, string errorMsg) = ParseTextToBytes(text, showCharMode);
            if (!success)
                return (false, errorMsg);

            Stream inputstream = new MemoryStream(buffer);

            (success, errorMsg) = Validator.Run(inputstream, showCharMode);
            if (!success)
                return (false, errorMsg);
        }
        return (true, string.Empty);
    }

    internal static (bool success, string errorMsg) ReadStdin(bool showCharMode)
    {
        Stream stdin = Console.OpenStandardInput();
        return Validator.Run(stdin, showCharMode);
    }

    // this method parse string from printf style (\xFF) into raw bytes.
    // In the actual implementation, the program first go through all of
    // TODO: Change method's signature to accept a Stream instead of string (so I can daisy chain it into the validator)
    private static (bool, byte[], string errormsg) ParseTextToBytes(string text, bool showCharMode)
    {
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
        string errorMsg = string.Empty;

        foreach (char c in text)
        {
            if (c == '\\' || c == 'x')
                continue;

            errorMsg = $"invalid character detected when parsing text to byte: {c}";
            // this initial null check is necessary to "advance the pointer".
            if (e1 is null)
            {
                e1 = ConvertCharToByte(c);
                if (e1 is null)
                {
                    success = false;
                    break;
                }
                else
                    continue;
            }

            e0 ??= ConvertCharToByte(c);
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

            if (showCharMode)
                Console.Write($"{value:x2} ");

            success = true;
        }
        Console.Write("\n");
        return (success, buffer, errorMsg);
    }

    private static byte? ConvertCharToByte(char c) =>
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

    // This method isn't called with value argument set to None.
    private static bool CheckFlagValue(GlobalFlags gf, GlobalFlags value) => (gf & value) != 0;
}
