/*
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
 */

namespace Utf8Validator.Tools;

static class Validator
{
    internal static (bool success, string errorMsg) Run(Stream stream, bool showCharMode)
    {
        string errorMsg = string.Empty;
        int bytesRead = 0;
        long totalPosition = 0;

        // 2kb window of the stream.
        Span<byte> buffer = stackalloc byte[2048];

        // Temp array to store the char being validated
        Span<byte> ch = stackalloc byte[4];

        // Read the stream block by block (of size 2048)
        while ((bytesRead = stream.Read(buffer)) > 0)
        {
            ReadOnlySpan<byte> currentChunk = buffer[..bytesRead];
            int i = 0;

            while (i < currentChunk.Length)
            {
                byte firstByte = currentChunk[i];
                long currentBytePosition = totalPosition + i;

                if (IsASCIICharacter(firstByte))
                {
                    //reset byte counter (start another character)
                    ch[0] = firstByte;
                    i++;
                    if (showCharMode)
                        Display.Character(ch[..1]);
                    continue;
                }

                // Here, the byte is not an ASCII character -> multi-byte character.
                // We verify if the first byte is valid
                if (!IsFirstByteValid(firstByte))
                {
                    errorMsg =
                        $"The first byte of the UTF-8 character is invalid: {firstByte} -> {firstByte:X}. [byte nº{currentBytePosition}]";
                    return (false, errorMsg);
                }

                int cByteNeeded = ContinuationByteNeeded(firstByte);

                // If we reach EOF or the charcters is split
                // between 2 chunk when reading the stream
                if (i + cByteNeeded >= currentChunk.Length)
                {
                    errorMsg =
                        $"EOF or block boundary reached before multi-byte character was fully consumed. [byte nº{currentBytePosition}]";
                    return (false, errorMsg);
                }

                // We extract the complete character from our working Span
                int totalCharLength = 1 + cByteNeeded;
                ReadOnlySpan<byte> fullCharSpan = currentChunk.Slice(i, totalCharLength);

                // Continuous bytes Validation
                for (int j = 1; j <= cByteNeeded; j++)
                {
                    byte b = fullCharSpan[j];
                    currentBytePosition++;

                    if (!IsContinuationByte(b))
                    {
                        errorMsg =
                            $"Expected continuation byte, but got: {b} -> {b:X}. [byte nº{currentBytePosition}]";
                        return (false, errorMsg);
                    }

                    // Overlong rule validation (only the 1st continuous byte)
                    if (j == 1 && IsOverlong(b, cByteNeeded))
                    {
                        errorMsg =
                            $"Overlong Encoding detected: {b} -> {b:X}. [byte nº{currentBytePosition}]";
                        return (false, errorMsg);
                    }
                }

                // UTF-16 surrogate pars validation
                // (only when characters is made of 3 bytes)
                if (cByteNeeded == 2 && IsUTF16Surrogate(fullCharSpan))
                {
                    errorMsg =
                        $"UTF-16 surrogate pairs detected: {fullCharSpan[0]:X}{fullCharSpan[0]:X}";
                    return (false, errorMsg);
                }

                // Here, the character is valid UTF-8.
                // If the user want, we can display it to the console.
                if (showCharMode)
                {
                    Display.Character(fullCharSpan);
                }
                // We advance the pointer of the full treated character
                i += totalCharLength;
            }
            // Update the global stream counter.
            totalPosition += bytesRead;
        }
        return (true, string.Empty);
    }

    // Raw Bytes for when we hande CLI / stdin arguments
    private static int ContinuationByteNeeded(byte b)
    {
        return b switch
        {
            >= 0xC2 and <= 0xDF => 1,
            >= 0xE0 and <= 0xEF => 2,
            >= 0xF0 and <= 0xF4 => 3,
            _ => 0,
        };
    }

    private static bool IsOverlong(byte b, int nbCByte)
    {
        // For 1 continuous byte,
        // the overlong is handled by ContinuationByteNeeded.
        return nbCByte switch
        {
            1 => false,
            2 => b < 0x90,
            3 => b < 0x90,
            _ => false,
        };
    }

    private static bool IsUTF16Surrogate(ReadOnlySpan<byte> b) =>
        b[0] == 0xED && b[1] >= 0xA0 && b[1] <= 0xBF;

    private static bool IsContinuationByte(byte b) => b >= 0x80 && b <= 0xBF;

    private static bool IsASCIICharacter(byte b) => b <= 0x7F;

    private static bool IsFirstByteValid(byte b)
    {
        if (IsContinuationByte(b))
            return false;

        if (b == 0xC0 || b == 0xC1)
            return false;

        if (b >= 0xF5)
            return false;

        return true;
    }
}
