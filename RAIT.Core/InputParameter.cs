namespace RAIT.Core;

internal class InputParameter
{
    public InputParameter()
    {
    }

    public object? Value { get; init; }
    public string Name { get; init; } = null!;
    public bool Used { get; set; }
    public bool IsQuery { get; init; }
    public bool IsForm { get; set; }
    public Type? Type { get; set; }
}