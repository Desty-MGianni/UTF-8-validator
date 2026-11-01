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

//TODO: Add debug flag so that only the errors or the finished message is shown.

//TODO: Flag to show the text read from source ("reconstructed" from the bytes we read)

namespace Utf8Validator;

using Utf8Validator.Tools;

class Program
{
    static void Main(string[] args)
    {
        int total = 0;
        int ok = 0;

        if (args.Length == 0)
        {
            // this check is for printf piping into this program.
            if (Console.IsInputRedirected)
            {
                total++;
                (bool success, string errorMsg) = Handlers.ReadStdin();

                if (!success)
                    Display.Error("stdin piping", errorMsg);
                else
                {
                    Display.Success("stdin piping");
                    ok++;
                }

                CheckResult(ok, total);
                return;
            }
            // if the user enter noting, we show how to use the program
            else
            {
                Display.Usage();
                return;
            }
        }

        (ok, total) = args[0] switch
        {
            "-d" => Handlers.Dirs(args[1..]),
            "-f" => Handlers.Files(args[1..]),
            "-t" => Handlers.Text(args[1..]),
            _ => (0, 0),
        };

        // TODO: Add 2nd argument logic for `--debug` flag
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
