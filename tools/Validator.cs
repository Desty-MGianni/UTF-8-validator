namespace Utf8Validator.tools;

static class Validator
{
    internal static (bool success, string errorMsg) Run(Stream s)
    {
        int position = 0;
        int currentByte,
            nextByte;
        string msg = string.Empty;

        while (true)
        {
            currentByte = s.ReadByte();
            position++;

            if (IsEOF(currentByte))
                break;

            byte firstByte = (byte)currentByte;

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
                nextByte = s.ReadByte();
                position++;

                if (IsEOF(nextByte))
                {
                    msg =
                        "EOF reached when a multi-byte character has not been completely consumed.";
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
        s.Close();
        return (true, msg);
    }

    // Here, section is either the name of the file/directory or the string
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
        return nbCByte switch
        {
            1 => b < 0xA0,
            2 => b < 0x90,
            3 => b > 0x8F,
            _ => false,
        };
    }

    private static bool IsEOF(int b) => b < 0;

    private static bool IsUTF16Surrogate(byte[] b) => b[0] == 0xED && b[1] >= 0xA0 && b[1] <= 0xBF;

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
