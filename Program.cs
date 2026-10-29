/*
 * Copyright (c) 2026 Gianni Mucci
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
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
 * This program will stop reading a file when an error is detected,
 * showing the byte value and the position
 *
 */

int total = 0;
int ok = 0;

//TODO: Implement argument detection logic to handle entire folders or raw bytes transmitted from the console.

foreach (string filename in args)
{
    total++;
    if (!File.Exists(filename))
    {
        DisplayError(filename, $"File does not exist in the current directory");
        continue;
    }

    Console.WriteLine($"Reading {filename}...");
    (bool success, string errorMsg) = Validator(filename);
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

if (ok == total && total > 0)
    DisplaySuccessSummary();
else
    DisplayErrorSummary(ok, total);

static (bool, string) Validator(string filename)
{
    int position = 0;
    byte b;
    int currentByte,
        nextByte;
    string msg = string.Empty;

    using FileStream fs = new(filename, FileMode.Open);
    while (true)
    {
        currentByte = fs.ReadByte();
        position++;
        if (IsEOF(currentByte))
            break;

        b = (byte)currentByte;
        if (IsASCIICharacter(b))
        {
            //reset byte counter (start another character)
            continue;
        }

        // Here,the byte is not an ASCII character -> multi-byte character.
        // We verify if the first byte is valid

        if (!IsFirstByteValid(b))
        {
            msg =
                $"The first byte of the UTF-8 character is invalid: {b} -> {b:X}. [byte nº{position}]";
            return (false, msg);
        }

        int nbrBytesNeeded = ContinuationByteNeeded(b);

        for (int i = 0; i < nbrBytesNeeded; i++)
        {
            nextByte = fs.ReadByte();
            position++;
            b = (byte)nextByte;

            if (IsEOF(nextByte))
            {
                msg = "EOF reached when a multi-byte character has not been consumed.";
                return (false, msg);
            }

            if (!IsContinuationByte(b))
            {
                msg =
                    $"Expected continuation byte at byte, but got: {b} -> {b:X}. [byte nº{position}]";
                return (false, msg);
            }
            // We verify the value of the 2nd octet to validate the overlong rule in UTF-8.
            if (i == 0 && !IsOverlong(b, nbrBytesNeeded))
            {
                msg = $"Overlong Encoding detected: {b} -> {b:X}. [byte nº{position}]";
                return (false, msg);
            }
        }
    }
    return (true, msg);
}

static void DisplayError(string filename, string msg)
{
    Console.Write("\u001b[31m[error]\u001b[0m");
    Console.Write($"[{filename}] {msg}");
    Console.WriteLine();
}

static void DisplaySuccessByFile(string filename)
{
    Console.Write("\u001b[32m[success]\u001b[0m");
    Console.Write($"[{filename}] All the tests are successful");
    Console.WriteLine();
}

static void DisplaySuccessSummary()
{
    Console.Write("\u001b[32mAll test passed successfuly\u001b[0m");
    Console.WriteLine();
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

static bool IsOverlong(byte b, int nbByte)
{
    return nbByte switch
    {
        2 => b >= 0xA0,
        3 => b >= 0x90,
        4 => b <= 0x8F,
        _ => false,
    };
}

static bool IsEOF(int b) => b < 0;

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
