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

namespace Utf8Validator;

using System.Buffers;
using Utf8Validator.Tools;

class Program
{
    static void Main(string[] args)
    {
        ReadOnlySpan<string> argsSpan = args.AsSpan();
        int ok = 0;
        int total = 0;

        // Loop through the args to find the flags
        (GlobalFlags gf, TargetFlags tf, int realArgsCount) = ExtractFlags(argsSpan);

        if (gf == GlobalFlags.Invalid || tf == TargetFlags.Invalid)
        {
            Display.Usage();
            return;
        }

        // No target flags detected + input redirected = piping used
        if (tf == TargetFlags.None && Console.IsInputRedirected)
        {
            bool showCharMode = Utils.CheckFlagValue(gf, GlobalFlags.ShowChar);
            bool debugMode = Utils.CheckFlagValue(gf, GlobalFlags.Debug);

            (ok, total) = Handlers.Piping(debugMode, showCharMode);
            CheckResult(ok, total);

            return;
        }

        // This ArrayPool usage avoids temporary heap allocations
        // and reduces Garbage Collection pressure by recycling the array.
        string[] sharedArray = ArrayPool<string>.Shared.Rent(realArgsCount);

        // As we need to return the array, we need a scope in which we
        // can convert the array into a Span without data corruption
        try
        {
            // logic to extract all the arguments that aren't the flags.
            int WriteIdx = 0;
            for (int i = 0; i < argsSpan.Length; i++)
            {
                string arg = argsSpan[i];
                if (arg.StartsWith('-'))
                    continue;
                sharedArray[WriteIdx] = arg;
                WriteIdx++;
            }

            // slicing of the argument array.
            ReadOnlySpan<string> remArgs = sharedArray.AsSpan(0, realArgsCount);

            // We don't need to care if gf == none or invalid
            (ok, total) = tf switch
            {
                TargetFlags.Directory => Handlers.Dirs(remArgs, gf),
                TargetFlags.File => Handlers.Files(remArgs, gf),
                TargetFlags.Text => Handlers.Text(remArgs, gf),
                _ => (0, 0),
            };
        }
        finally
        {
            ArrayPool<string>.Shared.Return(sharedArray);
        }

        if (total == 0)
        {
            Display.Usage();
            return;
        }

        CheckResult(ok, total);
    }

    // For a an enum with the flag attribute :
    // To add a flag        : gf |= GlobalFlags.X
    // to remove a flag     : gf &= GlobalFlags.X
    static (GlobalFlags, TargetFlags, int realArgsCount) ExtractFlags(ReadOnlySpan<string> argsSpan)
    {
        GlobalFlags gf = GlobalFlags.None;
        TargetFlags tf = TargetFlags.None;

        int cleanCount = 0;

        for (int i = 0; i < argsSpan.Length; i++)
        {
            if (argsSpan[i].StartsWith("--"))
            {
                gf |= Utils.ConvertToGFlags(argsSpan[i]);
            }
            else if (argsSpan[i].StartsWith('-'))
            {
                tf |= Utils.ConvertToTFlags(argsSpan[i]);
            }
            else
                cleanCount++;
        }

        return (gf, tf, cleanCount);
    }

    private static void CheckResult(int ok, int total)
    {
        if (ok == total)
            Display.SuccessSummary();
        else if (total > 0)
        {
            Display.ErrorSummary(ok, total);
        }
    }
}
