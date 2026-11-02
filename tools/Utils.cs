namespace Utf8Validator.Tools;

internal static class Utils
{
    // This method isn't called with value argument set to None.
    internal static bool CheckFlagValue(GlobalFlags gf, GlobalFlags target) => (gf & target) != 0;

    internal static byte? ConvertCharToByte(char c) =>
        c switch
        {
            '0' => 0x0,
            '1' => 0x1,
            '2' => 0x2,
            '3' => 0x3,
            '4' => 0x4,
            '5' => 0x5,
            '6' => 0x6,
            '7' => 0x7,
            '8' => 0x8,
            '9' => 0x9,
            'a' or 'A' => 0xA,
            'b' or 'B' => 0xB,
            'c' or 'C' => 0xC,
            'd' or 'D' => 0xD,
            'e' or 'E' => 0xE,
            'f' or 'F' => 0xF,
            _ => null,
        };

    // this method parse string from printf style (\xFF) into raw bytes.
    // In the actual implementation, the program first go through all of
    internal static (bool success, byte[] result) ParseTextToBytes(string text, bool showCharMode)
    {
        // If this test fails, it means the user
        // uses wrong syntax.
        if (text.Length % 4 != 0)
            return (false, []);

        int bufferLength = text.Length / 4;
        byte[] resultBuffer = new byte[bufferLength];
        int byteCount = 0;

        // We analyse the text 4 characters at the time.
        for (int i = 0; i < text.Length; i += 4)
        {
            // If the chunk doesn't start with \x, it fails
            if (text[i] != '\\' || text[i + 1] != 'x')
                return (false, []);

            // At this point, the syntax skeletion is valid, ew now verify
            // if the users parsed "values" between 0-9 and a-f or A-F.
            byte? result1 = ConvertCharToByte(text[i + 2]);
            byte? result2 = ConvertCharToByte(text[i + 3]);
            if (result1 is null || result2 is null)
                return (false, resultBuffer[..byteCount]);

            // We construct the correct value
            resultBuffer[byteCount] = (byte)(result1.Value << 4 | result2.Value);
            byteCount++;
        }

        if (showCharMode)
            Display.Character(resultBuffer.AsSpan());

        return (true, resultBuffer[..byteCount]);
    }

    internal static GlobalFlags ConvertToGFlags(string arg) =>
        arg switch
        {
            "--debug" => GlobalFlags.Debug,
            "--show" => GlobalFlags.ShowChar,
            _ => GlobalFlags.Invalid,
        };

    internal static TargetFlags ConvertToTFlags(string arg) =>
        arg switch
        {
            "-d" => TargetFlags.Directory,
            "-f" => TargetFlags.File,
            "-t" => TargetFlags.Text,
            _ => TargetFlags.Invalid, // Captures -h or --help
        };
}
