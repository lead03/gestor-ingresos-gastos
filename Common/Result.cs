namespace ControlGastos.Common;

public class Result
{
    public bool Success { get; private set; }
    public string? Error { get; private set; }

    protected Result(bool success, string? error)
    {
        Success = success;
        Error = error;
    }

    public static Result Ok() => new(true, null);
    public static Result Fail(string msg) => new(false, msg);

    public static Result<T> Ok<T>(T value) => new(value, true, null);
    public static Result<T> Fail<T>(string msg) => new(default!, false, msg);
}

public class Result<T> : Result
{
    public T Value { get; private set; }

    internal Result(T value, bool success, string? error)
        : base(success, error)
    {
        Value = value;
    }
}
