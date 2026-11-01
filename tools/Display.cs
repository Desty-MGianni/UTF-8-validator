namespace Utf8Validator.tools;

static class Display
{
    internal static void Info(string msg)
    {
        Console.Write("\u001b[34m[INFO]\u001b[0m ");
        Console.Write(msg + "\n");
    }

    // Here, section is either the name of the file/directory or the string
    // Raw Bytes for when we hande CLI / stdin arguments
    internal static void Error(string section, string msg)
    {
        Console.Write("\u001b[31m[ERROR]\u001b[0m");
        Console.Write($"[{section}] {msg}\n");
    }

    internal static void Success(string section)
    {
        Console.Write("\u001b[32m[OK]\u001b[0m");
        Console.Write($"[{section}] All the tests are successful\n");
    }

    internal static void SuccessSummary()
    {
        Console.WriteLine();
        Console.Write("\u001b[32mAll test passed successfuly\u001b[0m\n");
    }

    internal static void ErrorSummary(int ok, int total)
    {
        Console.WriteLine();
        Console.Write(ok);
        Console.Write("\u001b[33m/\u001b[0m");
        Console.Write(total);
        Console.Write($" \u001b[33mtests has been successful\u001b[0m\n");
    }

    //TODO: Pretty and consise Usage instructions.
    internal static void Usage() { }
}
