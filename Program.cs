/*
 * Copyright (c) 2026 Gianni Mucci
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

/*
 *
 * Personnal reproduction in C# of the UTF-8 validator made and shown by Dave Eddy
 * from the YouTube channel You Suck At Programming.
 * (https://www.youtube.com/watch?v=7taRssuPB8E)
 *
 * EXPLANATION OF THE UTF-8 STANDARD
 * ---------------------------------
 * To codify any characters from humankind, we uses UNICODE code points,
 * which we restranscript in computer language with the
 * UTF-8, UTF-16 or UTF-32 standards.
 *
 * UTF-8 can uses UP TO 4 octets, detailed like this :
 *
 * the first octet (or the "leftmost" octet of the series will determine
 * the number of continuation bytes necessary.
 *
 *  0XXXXXXX -> 1 octet (ASCII table)
 *  110XXXXX -> 2 octet
 *  1110XXXX -> 3 octet
 *  11110XXX -> 4 octet
 *
 * When we encode U+10FFFF in UTF-8 (the largest codepoint available
 * for the standard), the first octet will be 11110100 -> 0xF4.
 *
 * On top of that, the Values 0xC0 and 0xC1 are not permitted
 * by the UTF-8 standard.
 *
 * Any other value for the first octet is invalid UTF-8.
 *
 * The continuation bytes are all the same, following this convention :
 *     10XXXXXX
 *
 *
 * BEHAVIOUR OF THE PROGRAM
 * ---------------------------------
 *
 * The static Handlers class inside the Tools namespace allows this program to
 * be flexible. It allows
 *
 * This program will immediately stop when an error is detected,
 * showing the byte value and the position.
 *
 * C# doesn't allow raw bytes to be sent to the program .
 * (either via args or Console.OpenStandardInput / Console.In)
 *
 * To have that feature available, the program will manually
 * parse string into bytes while keeping the same syntax as the Printf command.
 *
 */

//TODO: Change each array's into Span<T> or ReadOnlySpan<T> for better performances
namespace Utf8Validator;

using Utf8Validator.Tools;

class Program
{
    static void Main(string[] args)
    {
        List<string> remArgs = [.. args];

        // Filter global flags. The user can type them where
        // he want inside the array of arguments.
        bool debugMode = remArgs.Remove("--debug");
        bool showBytesMode = remArgs.Remove("--show");

        GlobalFlags gFlags = (debugMode, showBytesMode) switch
        {
            (true, true) => GlobalFlags.All,
            (true, false) => GlobalFlags.Debug,
            (false, true) => GlobalFlags.ShowChar,
            (false, false) => GlobalFlags.None,
        };

        // CommandFlags flags
        if (remArgs.Count == 0)
        {
            if (Console.IsInputRedirected)
            {
                bool success = Handlers.PipingOrIndirection(gFlags);
                CheckResult(success ? 1 : 0, 1);
            }
            else
                Display.Usage();
            return;
        }

        string mainFlag = remArgs[0];
        string[] comArgs = remArgs.ToArray()[1..];

        (int ok, int total) = mainFlag switch
        {
            "-d" => Handlers.Dirs(comArgs, gFlags),
            "-f" => Handlers.Files(comArgs, gFlags),
            "-t" => Handlers.Text(comArgs, gFlags),
            _ => (0, 0), // Captures -h, --help or unknown flags
        };

        if (total == 0)
        {
            Display.Usage();
            return;
        }

        CheckResult(ok, total);
    }

    static void CheckResult(int ok, int total)
    {
        if (ok == total)
            Display.SuccessSummary();
        else if (total > 0)
        {
            Display.ErrorSummary(ok, total);
        }
    }
}
