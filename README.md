[!["Buy Me A Coffee"](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/kebechet)

# Types.Result
[![NuGet Version](https://img.shields.io/nuget/v/Kebechet.Types.Result)](https://www.nuget.org/packages/Kebechet.Types.Result/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Kebechet.Types.Result)](https://www.nuget.org/packages/Kebechet.Types.Result/)
[![Twitter](https://img.shields.io/twitter/url/https/twitter.com/samuel_sidor.svg?style=social&label=Follow%20samuel_sidor)](https://x.com/samuel_sidor)

A minimal `Result` / `Result<TError>` for library authors who want to publish a closed, exhaustively-checkable contract of failure modes - with a separate channel for native/platform exceptions. No combinators, no extensibility hooks - the library defines its failure surface, the consumer reacts.

## Why this exists

Most Result libraries (FluentResults, ErrorOr, Ardalis.Result) are designed for application-level error pipelines: open polymorphic errors, combinators (`Map` / `Bind` / `Match`), reasons / metadata chains. That's the wrong shape for a *library boundary* type, where the calls:

- have a fixed, documented set of failure modes (good fit for an enum),
- can also throw raw platform/SDK exceptions that the library can't fully classify,
- are translated by the consumer at the boundary, not pipelined deeper.

This package occupies that niche.

## Install

```bash
dotnet add package Kebechet.Types.Result
```

## Types in this package

| Type | When to use |
|---|---|
| `Result` | Library call that returns no value; only failure is "it threw." |
| `Result<TError>` | Library call that returns no value; closed enum of documented failures + native exception channel. |
| `DataResult<TValue>` | Library call that returns a value; only failure is "it threw." |
| `DataResult<TValue, TError>` | Library call that returns a value; closed enum of documented failures + native exception channel. |

The non-generic `Result` types and the `DataResult` types can also be subclassed to give per-operation return types named docs (e.g. `WriteHealthDataResult : Result<WriteError>` carrying its own `RecordIds` property). Use the generic `DataResult` directly for simple cases; subclass when you want a self-documenting return type.

## Usage

### Exception-only outcome

When the call has only one failure shape - "it threw":

```csharp
using Types.Result;

public class ConnectResult : Result
{
    public string? SessionId { get; init; }
}

public ConnectResult Connect()
{
    try
    {
        var session = _native.OpenSession();
        return new ConnectResult { SessionId = session.Id };
    }
    catch (Exception ex)
    {
        return new ConnectResult { ErrorException = ex };
    }
}
```

### Typed enum errors plus exception channel

When the library has a closed set of documented failure modes and *also* needs to surface raw platform exceptions:

```csharp
using Types.Result;

public enum WriteError
{
    PermissionDenied,
    SdkUnavailable,
    QuotaExceeded
}

public class WriteResult : Result<WriteError>
{
    public IReadOnlyList<string> RecordIds { get; init; } = [];
}

public WriteResult Write(IList<Record> items)
{
    if (!_hasPermission)
    {
        return new WriteResult { Error = WriteError.PermissionDenied };
    }

    try
    {
        var ids = _native.Insert(items);
        return new WriteResult { RecordIds = ids };
    }
    catch (Exception ex)
    {
        return new WriteResult { ErrorException = ex };
    }
}
```

Consumer side:

```csharp
var result = library.Write(items);

if (result.IsSuccess)
{
    Persist(result.RecordIds);
    return;
}

if (result.Error is { } error)
{
    var message = error switch
    {
        WriteError.PermissionDenied => "Grant permission and retry.",
        WriteError.SdkUnavailable   => "SDK not installed.",
        WriteError.QuotaExceeded    => "Try again later."
    };

    ShowError(message);
    return;
}

LogPlatformException(result.ErrorException!);
```

The `switch` expression over a `WriteError` enum is exhaustively checked by the compiler (warning `CS8509` - promote to error in your csproj for hard enforcement).

### Generic value carrier (no per-operation subclass)

For simple cases where a per-operation type would be overkill:

```csharp
using Types.Result;

public DataResult<int> CountSessions()
{
    try
    {
        return new DataResult<int> { Value = _native.CountSessions() };
    }
    catch (Exception ex)
    {
        return new DataResult<int> { ErrorException = ex };
    }
}

public DataResult<UserProfile, ProfileError> GetProfile(string id)
{
    if (!_hasPermission)
    {
        return new DataResult<UserProfile, ProfileError> { Error = ProfileError.Forbidden };
    }

    try
    {
        var profile = _native.LoadProfile(id);
        return new DataResult<UserProfile, ProfileError> { Value = profile };
    }
    catch (Exception ex)
    {
        return new DataResult<UserProfile, ProfileError> { ErrorException = ex };
    }
}
```

Reading the value:

```csharp
var result = service.GetProfile("abc");

if (!result.IsSuccess)
{
    HandleFailure(result);
    return;
}

var profile = result.Value!;  // safe to dereference once IsSuccess is checked
```

`Value` is only meaningful when `IsSuccess` is `true`. On failure, reference-typed values are `null` and value-typed values are `default(TValue)` - always check `IsSuccess` first.

## Design notes

| | This package | FluentResults / ErrorOr |
|---|---|---|
| Errors are a closed set | yes - enum | no - open polymorphic objects |
| Compiler-checked exhaustive matching | yes | no |
| Per-error data fields | no - enum is just an int | yes - subclass `Error` |
| Native exception channel | yes - first-class peer | no - collapsed into a generic error |
| Combinators (`Map`, `Bind`, `Match`) | no | yes |
| Value carrier | per-operation subclass | `Result<T>` |
| Dependencies | none | varies |

If you need rich per-error metadata, error chaining, or pipeline composition, reach for FluentResults or ErrorOr. If you're publishing a library that wants a closed failure contract and a place to put raw platform exceptions, this is the smaller, sharper tool.

## License

[MIT](LICENSE)
