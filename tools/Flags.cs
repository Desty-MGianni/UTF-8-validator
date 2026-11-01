namespace Utf8Validator.Tools;

[Flags]
internal enum GlobalFlags
{
    None,
    Debug,
    ShowChar,
    All = Debug | ShowChar,
}
