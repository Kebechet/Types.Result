namespace Types.Result;

/// <summary>
/// Outcome of a library call that returns a value, or surfaces a native/platform exception.
/// Use <see cref="DataResult{TValue, TError}"/> when the library also has a closed set of typed errors.
/// </summary>
/// <remarks>
/// <see cref="Value"/> is only meaningful when <see cref="Result.IsSuccess"/> is <c>true</c>.
/// On failure, reference-typed values are <c>null</c> and value-typed values are <c>default(TValue)</c>.
/// Always check <see cref="Result.IsSuccess"/> before reading <see cref="Value"/>.
/// </remarks>
/// <typeparam name="TValue">The type of the produced value on success.</typeparam>
public class DataResult<TValue> : Result
{
    /// <summary>
    /// The value produced on success; <c>default(TValue)</c> on failure.
    /// </summary>
    public TValue? Value { get; init; }
}

/// <summary>
/// Outcome of a library call that returns a value, with a closed enum-typed set of failure
/// modes plus a separate channel for native/platform exceptions.
/// </summary>
/// <remarks>
/// <see cref="Value"/> is only meaningful when <see cref="Result.IsSuccess"/> is <c>true</c>.
/// On failure, reference-typed values are <c>null</c> and value-typed values are <c>default(TValue)</c>.
/// Always check <see cref="Result.IsSuccess"/> before reading <see cref="Value"/>.
/// </remarks>
/// <typeparam name="TValue">The type of the produced value on success.</typeparam>
/// <typeparam name="TError">An enum type listing the library's documented failure modes.</typeparam>
public class DataResult<TValue, TError> : Result<TError>
    where TError : struct
{
    /// <summary>
    /// The value produced on success; <c>default(TValue)</c> on failure.
    /// </summary>
    public TValue? Value { get; init; }
}
