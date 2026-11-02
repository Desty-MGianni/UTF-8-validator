namespace Utf8Validator.Tools;

internal static class Readers
{
    // We manually parsed the each string into byte[].
    // if it is successful, we then create a stream
    // and run the validator on it.
    internal static (bool success, string errorMsg) CLI(
        ReadOnlySpan<string> remArgs,
        bool showCharMode
    )
    {
        string errorMsg;
        foreach (string arg in remArgs)
        {
            (bool success, byte[] data) = Utils.ParseTextToBytes(arg, showCharMode);
            if (!success)
            {
                errorMsg = $"Failed to parse string {arg}";
                return (false, errorMsg);
            }
            MemoryStream ms = new(data);

            (success, errorMsg) = Validator.Run(ms, showCharMode);
            if (!success)
            {
                return (false, errorMsg);
            }
        }
        return (true, string.Empty);
    }

    internal static (bool success, string errorMsg) Stream(bool showCharMode)
    {
        Stream stdin = Console.OpenStandardInput();
        return Validator.Run(stdin, showCharMode);
    }
}
