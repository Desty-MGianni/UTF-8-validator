namespace Utf8Validator.Tools;

[Flags]
internal enum GlobalFlags
{
    // Use of bit shift for an easier implementation
    // of future flags if needed.
    None = 0,
    Invalid = 1 << 0,
    Debug = 1 << 1,
    ShowChar = 1 << 2,
}

internal enum TargetFlags
{
    None,
    Invalid,
    File,
    Directory,
    Text,
}
