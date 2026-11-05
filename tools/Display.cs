using System.Text;

namespace Utf8Validator.Tools;

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
        Console.Write($"\u001b[2m[{section}]\u001b[0m ");
        Console.Write($"{msg}\n");
    }

    internal static void Success(string section)
    {
        Console.Write("\u001b[32m[OK]\u001b[0m");
        Console.Write($"\u001b[2m[{section}]\u001b[0m ");
        Console.Write($"Valid UTF-8 string\n");
    }

    internal static void Character(ReadOnlySpan<byte> validSequence)
    {
        // We prepare a mini-buffer.
        Span<char> renderBuffer = stackalloc char[2];

        // the encoder fill the buffer and gives the exact vue.
        int charsWritten = Utils.Encoder(validSequence, renderBuffer);

        Console.Write(renderBuffer[..charsWritten]);
    }

    internal static void SuccessSummary()
    {
        Console.Write("\n");
        Console.Write("\u001b[32mAll test passed successfuly\u001b[0m\n");
    }

    internal static void ErrorSummary(int ok, int total)
    {
        Console.Write('\n');
        Console.Write(ok);
        Console.Write("\u001b[33m/\u001b[0m");
        Console.Write(total);
        Console.Write($" \u001b[33mtests has been successful\u001b[0m\n");
    }

    internal static void Usage()
    {
        Console.Write("\u001b[36m=== UTF-8 CLI VALIDATOR ===\u001b[0m\n");
        Console.Write("A strict, high-performance RFC 3629 compliance checker.\n");
        Console.Write("\n");

        Console.Write("\u001b[33mUSAGE:\u001b[0m\n");
        Console.Write("  ./utf8-validator [GLOBAL FLAGS] [TARGET FLAG] [ARGUMENTS...]\n");
        Console.Write("  [COMMAND] | ./utf8-validator [GLOBAL FLAGS]\n");
        Console.Write("\n");

        Console.Write("\u001b[33mGLOBAL FLAGS:\u001b[0m (Can be placed anywhere)\n");
        Console.Write(
            "  \u001b[35m--debug\u001b[0m             Enable granular evaluation logs and bitmask routing traces.\n"
        );
        Console.Write(
            "  \u001b[35m--show-bytes\u001b[0m        Dump the reconstructed characters step-by-step upon validation.\n"
        );
        Console.Write("\n");

        Console.Write("\u001b[33mTARGET FLAGS:\u001b[0m\n");
        Console.Write(
            "  \u001b[32m-f <files...>\u001b[0m       Validate one or multiple specific file paths.\n"
        );
        Console.Write(
            "  \u001b[32m-d <directories...>\u001b[0m Validate all files within one or multiple directories.\n"
        );
        Console.Write(
            "  \u001b[32m-t <hex_string>\u001b[0m     Parse and validate a literal hex string using printf syntax.\n"
        );
        Console.Write("                      (e.g., \"\\xAC\\x41\\x42\")\n");
        Console.Write("  \u001b[32m-h, --help\u001b[0m          Show this help usage menu.\n");
        Console.Write("\n");

        Console.Write("\u001b[33mEXAMPLES:\u001b[0m\n");
        Console.Write("  \u001b[34m# Batch file validation:\u001b[0m\n");
        Console.Write("  ./utf8-validator -f document.txt logs/app.log\n");
        Console.Write("\n");

        Console.Write("  \u001b[34m# Directory scanning:\u001b[0m\n");
        Console.Write("  ./utf8-validator -d ./src /var/log\n");
        Console.Write("\n");

        Console.Write("  \u001b[34m# Native Unix Piping (Automatic Stdin detection):\u001b[0m\n");
        Console.Write("  printf \"\\xFF\" | ./utf8-validator\n");
        Console.Write("  ./utf8-validator < firmware.bin\n");
        Console.Write("\n");
    }
}
