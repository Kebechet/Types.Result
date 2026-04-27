namespace Types.Result;

/// <summary>
/// Outcome of a library call that either succeeded or threw a native/platform exception.
/// Use <see cref="Result{TError}"/> when the library also has a closed set of typed errors.
/// </summary>
public class Result
{
    /// <summary>
    /// True when the call produced no exception.
    /// </summary>
    public virtual bool IsSuccess => ErrorException is null;

    /// <summary>
    /// True when the call failed.
    /// </summary>
    public bool IsError => !IsSuccess;

    /// <summary>
    /// The native/platform exception that surfaced from the underlying call, or null on success.
    /// </summary>
    public Exception? ErrorException { get; init; } = null;
}

/// <summary>
/// Outcome of a library call with a closed, enum-typed set of failure modes plus a separate
/// channel for native/platform exceptions. Consumers can <c>switch</c> on <see cref="Error"/>
/// to handle the documented failures exhaustively, and inspect <see cref="Result.ErrorException"/>
/// for unexpected lower-layer faults.
/// </summary>
/// <typeparam name="TError">An enum type listing the library's documented failure modes.</typeparam>
public class Result<TError> : Result
    where TError : struct
{
    /// <inheritdoc />
    public override bool IsSuccess => Error is null && ErrorException is null;

    /// <summary>
    /// The typed failure mode, or null when the call succeeded or surfaced as an exception.
    /// </summary>
    public TError? Error { get; init; } = null;
}
