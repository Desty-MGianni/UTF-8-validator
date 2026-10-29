// Personnal reproduction in C# of the UTF-8 validator shown by Dave Eddy from the YouTube channel You Suck At Programming.

// To codify any characters from humankind, we uses UNICODE code points,
// which we restranscript in computer language with the
// UTF-8, UTF-16 or UTF-32 standards.
//
// UTF-8 can uses UP TO 4 octets, detailed like this :
//
// the first octet (or the "leftmost" octet of the series will determine
// the number of continuation bytes necessary.
//
//
// When we encode U+10FFFF in UTF-8 (the largest codepoint available
// for the standard), the first octet will be 11110100 -> 0xF4.
//
// On top of that, the Values 0xC0 and 0xC1 are not permitted
// by the UTF-8 standard.
// Any other value for the first octet is invalid UTF-8.
//
// The continuation bytes are all the same, following this convention :
//     10XXXXXX
//
// TODO: implement overlong error (uses too much space than needed for a character -> defined by the UNICODE stardard)

int total = 0;
int ok = 0;

foreach (string filename in args)
{
    Console.WriteLine($"Reading {filename}...");
    total++;
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
    byte b;
    int currentByte,
        nextByte;

    if (!File.Exists(filename))
        return (false, $"File {filename} does not exist in the current directory");
    using FileStream fs = new(filename, FileMode.Open);
    {
        while (true)
        {
            currentByte = fs.ReadByte();
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
                return (false, $"The first byte of the UTF-8 character is invalid: {b} -> {b:X}");

            int nbrBytesNeeded = ContinuationByteNeeded(b);

            for (int i = 0; i < nbrBytesNeeded; i++)
            {
                nextByte = fs.ReadByte();

                if (IsEOF(nextByte))
                    return (false, "The multi-byte character has been truncated");
                b = (byte)nextByte;

                if (!IsContinuationByte(b))
                    return (false, $"Expected continuation byte, but got: {b} -> {b:X}");
            }
        }
        return (true, "");
    }
}

static void DisplayError(string filename, string msg)
{
    ConsoleColor originalColor = Console.ForegroundColor;

    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write("[error] ");

    Console.ForegroundColor = originalColor;
    Console.Write($"[{filename}] {msg}");
    Console.WriteLine();
}

static void DisplaySuccessByFile(string filename)
{
    ConsoleColor originalColor = Console.ForegroundColor;

    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("[success] ");

    Console.ForegroundColor = originalColor;
    Console.Write($"[{filename}] All the tests are successful");
    Console.WriteLine();
}

static void DisplaySuccessSummary()
{
    ConsoleColor originalColor = Console.ForegroundColor;

    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("All test passed successfuly");
    Console.ForegroundColor = originalColor;
    Console.WriteLine();
}

static void DisplayErrorSummary(int ok, int total)
{
    ConsoleColor originalColor = Console.ForegroundColor;
    Console.WriteLine();
    Console.Write(ok);

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("/");

    Console.ForegroundColor = originalColor;
    Console.Write(total);

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write($" tests has been successful\n");

    Console.ForegroundColor = originalColor;
}
static bool IsEOF(int b) => b < 0;

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
