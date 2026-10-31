/*
 * Copyright (c) 2026 Gianni Mucci
 *
 * This source code is licensed under the MIT license found in the
 * README.md file in the root directory of this source tree.
 */

// Personnal reproduction in C# of the UTF-8 validator shown by Dave Eddy from the YouTube channel You Suck At Programming.

/* EXPLANATION OF THE UTF-8 STANDARD
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
*/

/* BEHAVIOUR OF THE PROGRAM
* ---------------------------------
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

int total = 0;
int ok = 0;

switch (args[0])
{
    case "-d":
        (ok, total) = HandleDirectory(args[1..]);
        break;
    case "-f":
        (ok, total) = HandleFiles(args[1..]);
        break;
    case "-t":
        (ok, total) = HandleText(args[1..]);
        break;
    case "-s":
    // TODO: Add 2nd argument logic for `--debug` flag
    case "--debug":
    default:
        DisplayUsage();
        break;
}

//TODO: toggle for when a -s flag is present
if (ok == total && total > 0)
    DisplaySuccessSummary();
else
    DisplayErrorSummary(ok, total);

static (int ok, int total) HandleDirectory(string[] args)
{
    int ok = 0;
    int total = 0;
    if (args.Length == 0)
    {
        DisplayError("Directory", $"No arguments given for directories. Use -h to show the usage");
        return (0, 0);
    }
    foreach (string dir in args)
    {
        if (!Directory.Exists(dir))
        {
            DisplayError(dir, $"Directory {dir} is not found");
            total++;
        }
        string[] Files = Directory.GetFiles(dir);
        (int currentOk, int currentTotal) = HandleFiles(Files);
        ok += currentOk;
        total += currentTotal;
    }
    return (ok, total);
}

static (int ok, int total) HandleFiles(string[] filePath)
{
    int total = 0;
    int ok = 0;
    foreach (string filename in filePath)
    {
        total++;
        if (!File.Exists(filename))
        {
            DisplayError(filename, $"File does not exist in the current directory");
            continue;
        }

        DisplayInfo($"Reading {filename}");

        Stream data = new FileStream(filename, FileMode.Open);
        (bool success, string errorMsg) = Validator(data);
        if (success)
        {
            DisplaySuccessByFile(filename);
            ok++;
        }
        else
        {
            DisplayError(filename, errorMsg);
        }
    }

    return (ok, total);
}

static (int ok, int total) HandleText(string[] args)
{
    // C# only permets a string[] for the CLI arguments input.
    // This method bypass the C# logic and permit 2 use cases :
    //    - with argument after the flag -> uses manual parsing
    //    - without argument after the flag -> uses stdin ??????

    int ok = 0;
    int total = 0;

    // If the user type bytes as CLI Arguments,
    // we manually parse each character in the string
    // into a byte following the printf command syntax
    //
    // (\xFA become real 0xFA byte)
    if (args.Length >= 1)
    {
        total = 1;
        byte[] output = ParseTextToBytes(args[0]);
        Stream outputStream = new MemoryStream(output);
        (bool success, string errorMsg) = Validator(outputStream);
        if (success)
        {
            DisplaySuccessByFile("Raw bytes");
            ok++;
        }
        else
        {
            DisplayError("Raw bytes", errorMsg);
        }
    }
    else
    {
        total++;
    }
    // No other argument in args : we prompt the user via stdin
    return (ok, total);
}

static byte[] ParseTextToBytes(string text)
{
    static int ZeroToNineDecimal(char c)
    {
        return c switch
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
            _ => throw new ArgumentException(
                $"Unexpected behaviour inside {nameof(ZeroToNineDecimal)}"
            ),
        };
    }
    static int AtoFinDecimal(char c)
    {
        return c switch
        {
            'A' or 'a' => 10,
            'B' or 'b' => 11,
            'C' or 'c' => 12,
            'D' or 'd' => 13,
            'E' or 'e' => 14,
            'F' or 'f' => 15,
            _ => throw new ArgumentException(
                $"Unexpected behaviour inside {nameof(AtoFinDecimal)}"
            ),
        };
    }

    static byte? ConvertCharToByte(char c)
    {
        return c switch
        {
            >= '0' and <= '9' => (byte)ZeroToNineDecimal(c),
            >= 'A' and <= 'F' => (byte)AtoFinDecimal(c),
            _ => null,
        };
    }

    /*
     * for a result of 0xAF in hex :
     * e1 = A
     * e0 = F
     */
    byte? e0 = null,
        e1 = null;
    byte[] buffer = new byte[text.Length];
    int idx = 0;

    foreach (char c in text)
    {
        if (c == '\\' || c == 'x')
            continue;

        if (e1 is null)
        {
            e1 = ConvertCharToByte(c);
            if (e1 is null)
                DisplayError(
                    "Raw bytes",
                    $"Invalid character detected when parsing text to byte: {c}"
                );
            else
                continue;
        }

        e0 ??= ConvertCharToByte(c);
        if (e0 is null)
            DisplayError("Raw bytes", $"Invalid character detected when parsing text to byte: {c}");

        byte value = (byte)(e1 * 16 + e0)!;
        buffer[idx] = value;
        idx++;

        e0 = null;
        e1 = null;

        //TODO: debug and show detection here to display each byte
        Console.Write($"{value:X2} ");
    }
    Console.Write("\n");
    return buffer;
}

static (bool success, string msg) Validator(Stream data)
{
    int position = 0;
    int currentByte,
        nextByte;
    string msg = string.Empty;

    while (true)
    {
        currentByte = data.ReadByte();
        byte firstByte = (byte)currentByte;
        position++;

        if (IsEOF(currentByte))
            break;

        if (IsASCIICharacter(firstByte))
        {
            //reset byte counter (start another character)
            continue;
        }

        // Here,the byte is not an ASCII character -> multi-byte character.
        // We verify if the first byte is valid

        if (!IsFirstByteValid(firstByte))
        {
            msg =
                $"The first byte of the UTF-8 character is invalid: {firstByte} -> {firstByte:X}. [byte nº{position}]";
            return (false, msg);
        }

        int CByteNeeded = ContinuationByteNeeded(firstByte);

        byte[] ch = new byte[4];
        ch[0] = (byte)currentByte;

        // Start at 1 as the ch[0] is already populated at this point.
        for (int i = 1; i <= CByteNeeded; i++)
        {
            nextByte = data.ReadByte();
            position++;

            if (IsEOF(nextByte))
            {
                msg = "EOF reached when a multi-byte character has not been completely consumed.";
                return (false, msg);
            }

            byte b = (byte)nextByte;

            if (!IsContinuationByte(b))
            {
                msg =
                    $"Expected continuation byte at byte, but got: {b} -> {b:X}. [byte nº{position}]";
                return (false, msg);
            }

            // We verify the value of the 2nd octet
            // to validate the overlong rule in UTF-8.
            // (we do it on the first iteration of the loop)
            if (i == 1 && IsOverlong(b, CByteNeeded))
            {
                msg = $"Overlong Encoding detected: {b} -> {b:X}. [byte nº{position}]";
                return (false, msg);
            }

            // Once all the check are valid, we "push" the byte
            // into the array for the surrogate check.
            ch[i] = b;
        }

        // The UTF-16 surrogates uses 3 octets and are between
        // 0xED 0xA0 0x80 and 0xED 0xBF-lxBF
        if (CByteNeeded == 2 && IsUTF16Surrogate(ch))
        {
            msg = $"UTF-16 surrogate pairs detected: {ch[0]:X}{ch[1]:X}";
            return (false, msg);
        }
    }
    data.Close();
    return (true, msg);
}

static void DisplayUsage() { }

static void DisplayInfo(string msg)
{
    Console.Write("\u001b[34m[INFO]\u001b[0m ");
    Console.Write(msg + "\n");
}

// Here, section is either the name of the file/directory or the string
// Raw Bytes for when we hande CLI / stdin arguments
static void DisplayError(string section, string msg)
{
    Console.Write("\u001b[31m[ERROR]\u001b[0m");
    Console.Write($"[{section}] {msg}\n");
}

static void DisplaySuccessByFile(string section)
{
    Console.Write("\u001b[32m[OK]\u001b[0m");
    Console.Write($"[{section}] All the tests are successful\n");
}

static void DisplaySuccessSummary()
{
    Console.Write("\u001b[32mAll test passed successfuly\u001b[0m\n");
}

static void DisplayErrorSummary(int ok, int total)
{
    Console.WriteLine();
    Console.Write(ok);
    Console.Write("\u001b[33m/\u001b[0m");
    Console.Write(total);
    Console.Write($" \u001b[33mtests has been successful\u001b[0m\n");
}

static int ContinuationByteNeeded(byte b)
{
    return b switch
    {
        >= 0xC2 and <= 0xDF => 1,
        >= 0xE0 and <= 0xEF => 2,
        >= 0xF0 and <= 0xF4 => 3,
        _ => 0,
    };
}

static bool IsOverlong(byte b, int nbCByte)
{
    return nbCByte switch
    {
        1 => b < 0xA0,
        2 => b < 0x90,
        3 => b > 0x8F,
        _ => false,
    };
}

static bool IsEOF(int b) => b < 0;

static bool IsUTF16Surrogate(byte[] b) => b[0] == 0xED && b[1] >= 0xA0 && b[1] <= 0xBF;

static bool IsContinuationByte(byte b) => b >= 0x80 && b <= 0xBF;
static bool IsASCIICharacter(byte b) => b <= 0x7F;
static bool IsFirstByteValid(byte b)
{
    if (IsContinuationByte(b))
        return false;

    if (b == 0xC0 || b == 0xC1)
        return false;

    if (b >= 0xF5)
        return false;

    return true;
}
