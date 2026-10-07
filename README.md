# Xylocopadream.EasyResu

Lightweight result types to return a **success**, **warning** or **error** status instead of throwing exceptions.

- `Resu`: outcome without value.
- `Resu<T>`: outcome that may carry a value — a payload on success, or a detailed code (e.g. an enum) on warning/error.
- `IResu`: common read-only view to handle any outcome generically (logging, user notification).

`Resu` and `Resu<T>` are deliberately unrelated: a `Resu<T>` can never silently be passed where a `Resu` is expected.

## Usage

```csharp
using Xylocopadream.EasyResu;

Resu Save()
    => canSave ? Resu.Ok() : Resu.Error("Disk is full");

// Payload on success, no value on error
Resu<User> Load(int id)
    => repository.Find(id) is { } user ? Resu<User>.Ok(user) : Resu<User>.Error("Not found");

// Detailed code on warning/error: attach it explicitly with WarningWith / ErrorWith
Resu<LoginOutcome> Login()
    => offline
        ? Resu<LoginOutcome>.ErrorWith(LoginOutcome.NetworkError, "Server unreachable")
        : Resu<LoginOutcome>.Ok(LoginOutcome.Success);

var result = Load(42);
if (result.TryGetValue(out var user))   // true only when a value was explicitly given
{
    Console.WriteLine(user.Name);
}
else
{
    Console.WriteLine(result.GetMessage);
}
```

A value is present only when given to `Ok`, `WarningWith` or `ErrorWith`: `Error(...)` on a `Resu<SomeEnum>` never exposes `default(SomeEnum)` as a value.

## License

MIT
