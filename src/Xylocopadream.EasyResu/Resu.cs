using System.Diagnostics.CodeAnalysis;

namespace Xylocopadream.EasyResu;

/// <summary>
/// Severity of an operation outcome.
/// </summary>
public enum ResultStatus
{
    Success,
    Warning,
    Error
}

/// <summary>
/// Read-only view shared by <see cref="Resu"/> and <see cref="Resu{T}"/>,
/// to handle any outcome generically (logging, user notification...).
/// <para>Resu and Resu&lt;T&gt; are deliberately unrelated types: a Resu&lt;T&gt; can never be passed where a Resu is expected.</para>
/// </summary>
public interface IResu
{
    ResultStatus Status { get; }
    string? Message { get; }
    Exception? Exception { get; }

    bool IsSuccess { get; }
    bool IsWarning { get; }
    bool IsError { get; }

    /// <summary>
    /// Message, or the exception message, or an empty string.
    /// </summary>
    string GetMessage { get; }
}

/// <summary>
/// Outcome of an operation that returns no value.
/// </summary>
public sealed record Resu : IResu
{
    public ResultStatus Status { get; }
    public string? Message { get; }
    public Exception? Exception { get; }

    private Resu(ResultStatus status, string? message, Exception? exception)
    {
        Status = status;
        Message = message;
        Exception = exception;
    }

    public static Resu Ok()
        => new(ResultStatus.Success, null, null);

    public static Resu Warning(string? message = null, Exception? ex = null)
        => new(ResultStatus.Warning, message, ex);

    public static Resu Error(string? message = null, Exception? ex = null)
        => new(ResultStatus.Error, message, ex);

    public bool IsSuccess => Status == ResultStatus.Success;
    public bool IsWarning => Status == ResultStatus.Warning;
    public bool IsError => Status == ResultStatus.Error;

    public string GetMessage =>
        Message ?? Exception?.Message ?? string.Empty;
}

/// <summary>
/// Outcome of an operation that may carry a value.
/// <para>The value is optional whatever the status: a payload on success, or a detailed code
/// (e.g. an enum) on warning/error. It is only present when explicitly given:
/// use <c>Ok</c>, <c>WarningWith</c> or <c>ErrorWith</c> to attach one, never <c>default</c>.</para>
/// </summary>
public sealed record Resu<T> : IResu
{
    public ResultStatus Status { get; }
    public string? Message { get; }
    public Exception? Exception { get; }

    /// <summary>
    /// True only when a value was explicitly given.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue { get; }

    /// <summary>
    /// The value when <see cref="HasValue"/> is true; <c>default</c> otherwise.
    /// Prefer <see cref="TryGetValue"/>.
    /// </summary>
    public T? Value { get; }

    private Resu(ResultStatus status, bool hasValue, T? value, string? message, Exception? exception)
    {
        if (hasValue && value is null)
        {
            throw new ArgumentNullException(nameof(value), "Use the overload without value instead of passing null.");
        }

        Status = status;
        HasValue = hasValue;
        Value = value;
        Message = message;
        Exception = exception;
    }

    public static Resu<T> Ok(T value)
        => new(ResultStatus.Success, true, value, null, null);

    /// <summary>Warning without value.</summary>
    public static Resu<T> Warning(string? message = null, Exception? ex = null)
        => new(ResultStatus.Warning, false, default, message, ex);

    /// <summary>Warning carrying a value (payload or detailed code).</summary>
    public static Resu<T> WarningWith(T value, string? message = null, Exception? ex = null)
        => new(ResultStatus.Warning, true, value, message, ex);

    /// <summary>Error without value.</summary>
    public static Resu<T> Error(string? message = null, Exception? ex = null)
        => new(ResultStatus.Error, false, default, message, ex);

    /// <summary>Error carrying a value (payload or detailed code).</summary>
    public static Resu<T> ErrorWith(T value, string? message = null, Exception? ex = null)
        => new(ResultStatus.Error, true, value, message, ex);

    public bool IsSuccess => Status == ResultStatus.Success;
    public bool IsWarning => Status == ResultStatus.Warning;
    public bool IsError => Status == ResultStatus.Error;

    public string GetMessage =>
        Message ?? Exception?.Message ?? string.Empty;

    /// <summary>
    /// Gets the value if one was explicitly given, whatever the status.
    /// </summary>
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = Value;
        return HasValue;
    }
}
