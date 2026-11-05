namespace Utf8Validator.Tools;

static class Handlers
{
    internal static (int ok, int total) Dirs(ReadOnlySpan<string> args, GlobalFlags flags)
    {
        string section = "Directories";
        bool debugMode = Utils.CheckFlagValue(flags, GlobalFlags.Debug);
        int ok = 0;
        int total = 0;
        if (args.Length == 0)
        {
            if (debugMode)
            {
                Display.Error(
                    section,
                    $"No arguments given for directories. Use -h to show the usage"
                );
            }
            return (0, 0);
        }
        foreach (string arg in args)
        {
            if (debugMode)
            {
                Display.Info($"Searching through {arg}");
            }

            if (!Directory.Exists(arg))
            {
                if (debugMode)
                {
                    Display.Error(arg, $"Directory {arg} is not found");
                }
                total++;
                continue;
            }
            string[] files = Directory.GetFiles(arg);
            (int currentOk, int currentTotal) = Files(files, flags);
            ok += currentOk;
            total += currentTotal;
        }
        return (ok, total);
    }

    internal static (int ok, int total) Files(ReadOnlySpan<string> filePath, GlobalFlags flags)
    {
        bool debugMode = Utils.CheckFlagValue(flags, GlobalFlags.Debug);
        bool showCharMode = Utils.CheckFlagValue(flags, GlobalFlags.ShowChar);

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
            Result(debugMode, success, filename, errorMsg, ref ok);
        }

        return (ok, total);
    }

    // C# only permets a string[] for the CLI arguments input.
    // This method bypass the C# logic and permit 2 use cases :
    //    - with argument after the flag -> uses manual parsing
    //    - without argument after the flag -> we prompt the user via stdin.
    internal static (int ok, int total) Text(ReadOnlySpan<string> remArgs, GlobalFlags flags)
    {
        bool debugMode = Utils.CheckFlagValue(flags, GlobalFlags.Debug);
        bool showCharMode = Utils.CheckFlagValue(flags, GlobalFlags.ShowChar);

        int ok = 0;
        int total = 0;

        bool success;
        string errorMsg;
        string section = "CLI";

        // If the user type "bytes" in the CLI Arguments,
        // we manually parse each character from the string
        // into a byte following the printf command syntax
        //
        // (\xFA become real 0xFA byte)
        if (remArgs.Length >= 1)
        {
            total = 1;
            if (debugMode)
                Display.Info("Reading cli arguments");

            (success, errorMsg) = Readers.CLI(remArgs, showCharMode);
            Result(debugMode, success, section, errorMsg, ref ok);
        }
        return (ok, total);
    }

    // Parameter noArgs is a state for when the user used -t but didn't enter anything
    internal static (int ok, int total) Piping(bool debugMode, bool showCharMode)
    {
        int ok = 0;
        if (debugMode)
            Display.Info("Reading stdin");

        (bool success, string errorMsg) = Readers.Stream(showCharMode);
        Result(debugMode, success, "Stdin", errorMsg, ref ok);
        return (ok, 1);
    }

    internal static void Result(
        bool debugMode,
        bool success,
        string section,
        string errorMsg,
        ref int ok
    )
    {
        if (success)
        {
            ok++;
            if (debugMode)
                Display.Success(section);
        }
        else if (debugMode)
        {
            Display.Error(section, errorMsg);
        }
    }
}
