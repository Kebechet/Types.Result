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
