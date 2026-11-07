# Error Handling Guidelines

Error handling rules for any project in this repository, regardless of language or runtime. This document is the **single owner** of the error model: the `Result`/`IError` pattern, error codes, and the "never reuse a code" rule. Other documents link here instead of restating it.

Related documents: [APIDesign.md](APIDesign.md), [CodingStyle.md](CodingStyle.md), [Security.md](Security.md), [Testing.md](Testing.md), [Documentation.md](Documentation.md), [Agent.md](Agent.md).

---

## 1. Principles

1. **Errors are values.** Expected failures are returned, not thrown.
2. **Never throw for expected or invalid input.** Report it through the project's `Result`/`IError` model.
3. **Error codes are public API.** They are stable, documented, and never reused.
4. **Errors carry data, not objects.** Error types never expose or retain an `Exception` object.
5. **Safe by default.** Error messages never leak secrets, stack traces, or needless internal detail (see [Security.md](Security.md) Section 10).
6. **Actionable.** An error says what happened and, where possible, what to do.
7. **Never swallow.** Every error is handled, returned, or logged with context.

---

## 2. Classification

| Kind | Examples | Handling |
|------|----------|----------|
| **Expected failure** | Invalid argument, file not found, git exit code non-zero, timeout, cancellation, unsupported platform | Return `Result` with an `IError`. |
| **Programming error (bug)** | Broken internal invariant, impossible state | Debug assertion in internal code. In public code paths, return an `Internal` error. Do not crash the consumer. |
| **Unrecoverable environment failure** | Out of memory, stack overflow | Do not try to handle. Allowed to terminate. At native boundaries, catch `bad_alloc` and return an error code. |

- Validate at the **public boundary** and return errors. Trust internal invariants afterwards.
- Cancellation is an expected outcome: return the `Cancelled` error, do not throw `OperationCanceledException` out of the public API.

---

## 3. The Result / IError Model

### 3.1 Contract
- Every public operation that can fail returns `Result` or `Result<T>`.
- A result is either **success** (with a value for `Result<T>`) or **failure** (with exactly one `IError`).
- Reading the value of a failed result, or the error of a successful one, is a misuse. The API makes it hard: provide `IsSuccess`, `TryGetValue`, and `Error` that is `null`/empty on success. Do not throw on misuse, return a default and document it.
- Results are immutable.
- Prefer a `readonly struct` or sealed class for `Result`, chosen to minimize allocations on the success path (see [CodingStyle.md](CodingStyle.md)). The choice is part of the API and cannot change later.

### 3.2 IError
An error contains **only plain data**:

| Member | Type | Meaning | Stable? |
|--------|------|---------|:-------:|
| `Code` | integer-backed enum or `int` | Unique stable identifier (Section 4) | Yes, forever |
| `Category` | enum | Group of the error (Section 4.2) | Yes |
| `Message` | string | Human-readable, safe, English description | No (may be improved) |
| `Details` | read-only key/value pairs or a typed detail object | Extra data such as the argument name, exit code, or native code | Keys: yes, values: no |
| `Inner` | `IError?` | Optional cause, as another error value (not an exception) | Yes |

Rules:
- **No `Exception` property, field, or captured reference.** Convert the exception to the library's own data (code, message, selected details) at the catch site, then let it go.
- Consumers branch on `Code` (and `Category`). They must **never** parse `Message`. Document this.
- `Message` is not localized in the library (localization is the application's job, keyed by `Code`).
- `IError` implementations are sealed and immutable. New members are added through new interfaces or properties, following [APIDesign.md](APIDesign.md).
- Do not put secrets, credentials, full command lines, or raw process output into `Message` or `Details` (see [Security.md](Security.md)). Redact first.

### 3.3 Example (C#)

```csharp
public enum ErrorCode 
{ 
	None = 0,
	InvalidArgument = 1001,
	PathNotFound = 2001,
	ProcessTimeout = 3001, 
	Cancelled = 9001,
	Internal = 9999,
}

public interface IError 
{ 
	ErrorCode Code { get; }
	ErrorCategory Category { get; }
	string Message { get; }
	IReadOnlyDictionary<string, string> Details { get; }
	IError? Inner { get; }
}

public Result<StatusInfo,TError> Read<TError>(string path) 
{ 
	if (string.IsNullOrEmpty(path)) 
	{ 
		return Result<StatusInfo,TError>.Fail(Errors.InvalidArgument(nameof(path)));
	}
	// ...
}
```

### 3.4 Example (C ABI)
The C boundary returns a numeric `gg_result` and exposes details through a function. No exceptions, no C++ types cross it.

```cpp
typedef int32_t gg_result;            /* 0 = success, non-zero = error code */
GG_API gg_result gg_get_last_error(char* buffer, size_t capacity, size_t* required); 
GG_API const char* gg_error_name(gg_result code);   /* static string, never freed */
```

- The numeric values are **identical** to the managed `ErrorCode` values. One table, one meaning (Section 4).
- Last-error storage is **thread-local**, and the function documents when it is overwritten.

---

## 4. Error Codes

### 4.1 Rules (the stable-code contract)
1. **A code, once released, is never changed, renamed, renumbered, or reused.** Not even after the feature that raised it is removed.
2. **Retired codes stay reserved.** Mark them as `[Obsolete]`/reserved in the registry (Section 4.4) and never assign the number again.
3. **New codes are additive** and ship in a MINOR release. Adding a code is a non-breaking change, but consumers must handle unknown codes (Section 4.3).
4. **A code means one thing.** Do not broaden or narrow its meaning in a MINOR or PATCH release. A change of meaning is a breaking change (MAJOR).
5. **Numbers are explicit.** Never rely on auto-numbering or declaration order.
6. **Names may be improved only by adding a new name for the same number** while keeping the old one as obsolete. Prefer not to.
7. **Managed and native values are identical.** The registry is the single source of truth, and a test verifies that both match.
8. **Which code is returned for a given failure is part of the contract.** Changing it is a behavior change (see [APIDesign.md](APIDesign.md) Section 10).

### 4.2 Ranges

| Range | Category | Examples |
|-------|----------|----------|
| `0` | Success (`None`) | |
| `1000-1999` | Argument and validation | InvalidArgument, ValueOutOfRange, PathInvalid |
| `2000-2999` | File system and resources | PathNotFound, AccessDenied, DiskFull |
| `3000-3999` | Process and external tools | ToolNotFound, ToolTooOld, ProcessTimeout, ToolFailed |
| `4000-4999` | Parsing and data format | UnexpectedOutput, OutputTooLarge, VersionUnsupported |
| `5000-5999` | Network and remote | RemoteUnreachable, AuthenticationFailed |
| `6000-6999` | State and operation | InvalidState, AlreadyExists, Conflict |
| `7000-7999` | Interop and native | NativeLibraryNotFound, NativeVersionMismatch, NativeCallFailed |
| `8000-8999` | Security and policy | PolicyDenied, UnsafeInputRejected |
| `9000-9899` | General | Cancelled, NotSupported, Timeout |
| `9900-9999` | Internal and unknown | Internal (`9999`) |
| `10000+` | Reserved for future categories | |
| Negative values | **Reserved.** Not used by this project | |

- Pick the lowest free number in the matching range. Leave gaps by category, not within a category.
- Components that add their own codes get a documented block inside the appropriate range, or a new range above 10000 recorded in an ADR (see [Documentation.md](Documentation.md) Section 8).
- Wrapped native or OS codes are **not** exposed as library codes. They are carried in `Details` (for example `native_code`, `win32_error`, `exit_code`) and mapped to a library code.

### 4.3 Consumer guidance
- Always handle unknown codes with a default branch, or fall back on the **category** (`Code / 1000`).
- Treat `Internal` (`9999`) as "report a bug".
- Do not exhaustively `switch` on `ErrorCode` without a default.
- Document, for each public method, which codes it can return (see Section 9).

### 4.4 The registry
- One file is the source of truth: `docs/errors.md` (human-readable table), kept in sync with the enum and native header by a test or code generator.
- Each entry has: number, name, category, description, when it is returned, introduced in version, status (Active, Deprecated, Reserved), and the recommended consumer action.
```

|Code|Name|Category|Since|Status|Description|
|----|----|--------|-----|------|-----------|
|1001|InvalidArgument|Argument|1.0.0|Active|An argument is null, empty, malformed, or out of range. Details["argument"] names it.|
|2001|PathNotFound|FileSystem|1.0.0|Active|The path does not exist.|
|3002|ToolTooOld|Process|1.1.0|Active|The external tool is older than the minimum supported version.|
|3005|(reserved)|Process|1.2.0|Retired in 2.0.0|Do not reuse.|
```

- Adding, deprecating, or retiring a code requires an update to the registry, the changelog (under **Added** or **Deprecated**), and the tests, in the same pull request.

### 4.5 Required tests
- **Uniqueness:** no two names share a number.
- **Baseline:** every released code still exists with the same number, name, and category (compare with a stored baseline, like the API baseline in [APIDesign.md](APIDesign.md)). A removed or changed entry fails the build.
- **Range:** each code lies in its category range.
- **Parity:** managed and native codes are identical.
- **Registry:** every code is documented in `docs/errors.md`.
- **Contract tests:** for each public method, the documented failure produces the documented code (see [Testing.md](Testing.md)).

---

## 5. Creating Errors

- Use central factory methods (`Errors.InvalidArgument(name)`), not ad-hoc construction at each site. One place defines the code, category, and message format.
- Message text and format strings are kept in constants (see [CodingStyle.md](CodingStyle.md), `GitGud.Constants` for this repository), not inline.
- Messages are short, factual, in English, and start with the problem: "Path does not exist." Add the hint where useful: "Check that the repository was cloned."
- Put variable data in `Details`, not only in the message text, so callers can use it without parsing.
- Do not allocate on the success path. Create the error object only on failure. Cache immutable errors that carry no variable data.
- Do not include values the caller supplied unless they are safe. Redact or truncate (see [Security.md](Security.md) Section 10).

---

## 6. Handling and Propagating Errors

### 6.1 Rules
- **Check every result.** Ignoring a result is a bug. Mark result-returning functions so the compiler warns (`[[nodiscard]]` in C++, an analyzer rule in C#). Warnings fail the build.
- **Return early** on failure using guard clauses (see [CodingStyle.md](CodingStyle.md)).
- **Propagate** an error as is when the caller can add nothing. Wrap with context when it can:
```csharp
Result<string> output = await _runner.RunAsync(args, cancellationToken).ConfigureAwait(false);
if (!output.IsSuccess)
	return Result<StatusInfo>.Fail(output.Error);          // propagate unchanged 
```

- When wrapping, create a new error with the **appropriate code for this layer** and keep the lower-level error as `Inner`. Do not change the code of an error you only pass through.
- **Do not log and return** the same error at every layer. Log once, at the boundary that decides what happens next.
- Do not use errors for control flow when a boolean or `Try` pattern is clearer. Use `TryXxx` only on hot paths where creating a `Result` would allocate (see [CodingStyle.md](CodingStyle.md)).
- **Cleanup always runs** (`using`, `finally`, RAII), also on error paths.
- Partial failure: define and document whether an operation is atomic, rolled back, or may leave partial state. Return a specific error if state is left behind.

### 6.2 Cancellation and timeouts
- Every long-running or async operation accepts a `CancellationToken` as the last parameter.
- Cancellation returns the `Cancelled` code. A timeout returns the `Timeout` or `ProcessTimeout` code. They are different outcomes, and the caller can tell them apart.
- Cancellation must release resources, and kill child processes (see [Security.md](Security.md) Section 4).

### 6.3 Async
- Errors travel in the `Result`, not in a faulted `Task`. A public async method does not complete with an exception for expected failures.
- Do not use `async void` except for event handlers.

---

## 7. Exceptions

Exceptions are for **bugs and unrecoverable conditions**, and for catching failures of code you do not control. They are not part of the public contract.

### 7.1 Rules
- **Public API does not throw** for invalid input or expected failures. Guard against it and return an error.
- Catch exceptions **only at boundaries** where foreign code runs: I/O, process execution, interop, user callbacks, and third-party libraries.
- Catch the **narrowest** type that you can handle. A broad catch (`catch (Exception)`, `catch (...)`) is allowed only at a boundary, with a comment that explains why, and it **must convert to an `IError`** (usually `Internal` or the matching category).
- Convert at the catch site: map to a library code, copy a safe message and selected details, then discard the exception. **Do not store it.**
- Never use exceptions for control flow.
- Never catch and ignore. An empty catch block is not allowed.
- Do not rethrow with `throw ex` (it destroys the stack). If a rethrow is truly needed in internal code, use `throw;`.
- Fatal exceptions (out of memory, stack overflow, thread abort) are not caught to continue running, except at the native boundary where they are converted to an error code before returning.
- Diagnostics: the stack trace and exception details go to the **log** (with secrets redacted), not to the consumer-visible error.

### 7.2 Example (boundary conversion)
```csharp
try 
{
	// call foreign code: file system, process, native interop
} catch (IOException ex)   // narrowest handled type
{
    return Result.Fail(Errors.FromIo(ex.HResult));   // copy data, drop the exception
} catch (UnauthorizedAccessException) {
    return Result.Fail(Errors.AccessDenied());
}
```

### 7.3 Internal code
- Internal code follows the same model where practical. Internal helpers that cannot fail return plain values.
- Violated invariants use debug assertions. In release builds, a violated invariant on a public path returns `Internal`, with the details logged.
- A guard on internal-only arguments may use assertions instead of `Result`.

---

## 8. Interop and the C ABI

Complements [APIDesign.md](APIDesign.md) Sections 8 and 9.

- **No exception crosses the C ABI, a P/Invoke boundary, or a callback.** Catch everything at the exported function and convert to a `gg_result`.
- Exported functions validate every argument (null pointers, `struct_size`, sizes) and return `InvalidArgument` instead of crashing.
- Managed wrapper: after each P/Invoke call, check the return code and map it to a library `IError`. Carry the native number in `Details`.
- Windows API: capture the last error immediately (`SetLastError = true`, `Marshal.GetLastWin32Error()`) and map it. Store it as `win32_error` in `Details`.
- Callbacks into managed code catch all exceptions and convert them to a return code. Callbacks from native code into the library do the same.
- An unknown native code maps to `NativeCallFailed` with the number in `Details`, never to a silent success.
- Error text returned across the ABI follows the ownership rules (caller buffer with a two-call pattern, or static strings).

---

## 9. Documenting Errors

- Every public method documents the codes it can return (see [Documentation.md](Documentation.md) Section 4): in `<remarks>` for XML docs, `@return` for Doxygen.
- The registry `docs/errors.md` is the reference. Per-method documentation links to it.
- Documented codes are part of the contract. Returning a **new** code from an existing method for a situation that previously returned a different one is a behavior change (see Section 4.1, rule 8).
- Describe the recommended consumer action for each code (retry, fix input, report a bug).

---

## 10. Logging and Diagnostics

See [Logging.md](Logging.md) to get a detail on all logging rules.

---

## 11. Evolving the Error Model

| Change | Version bump |
|--------|--------------|
| Add a new code | MINOR |
| Add a new `Details` key | MINOR |
| Improve a `Message` text | PATCH or MINOR (consumers must not parse messages) |
| Return a different code for an existing situation | MAJOR |
| Change the meaning of a code | MAJOR |
| Remove or renumber a code | **Never** (a reserved number stays reserved) |
| Add a member to `IError` | New interface or MAJOR (see [APIDesign.md](APIDesign.md)) |
| Change the `Result` type kind (class/struct) | MAJOR |

- Retiring a code: stop raising it, mark it deprecated in the registry and changelog, keep the number reserved forever.
- Security-related validation may get stricter in a PATCH only when it fixes a vulnerability, and it is documented in the changelog under **Security**.

---

## 12. Agents and AI-Generated Code

See [Agent.md](Agent.md).
- Agents must not invent, renumber, reuse, or remove error codes. New codes need human approval and a registry update.
- Agents must not replace `Result` returns with exceptions, or add catch blocks that swallow errors, to make a build or test pass.
- Reviewers check every generated `catch` block and every new code.

---

## 13. Review Checklist

- [ ] Public methods return `Result`/`Result<T>` and do not throw for expected failures?
- [ ] Errors hold plain data only, with no `Exception` object kept anywhere?
- [ ] Each error has a stable code in the correct range, created through a factory?
- [ ] No existing code changed, renumbered, reused, or removed?
- [ ] New code added to the registry, baseline, changelog, native header, and tests?
- [ ] Messages are safe: no secrets, paths with personal data, command lines, or raw output?
- [ ] Every result is checked, and the error path cleans up resources?
- [ ] Catch blocks are narrow, at boundaries, convert to `IError`, and never swallow?
- [ ] Cancellation and timeouts return the correct distinct codes?
- [ ] No exceptions or C++ types cross the C ABI, P/Invoke, or callbacks?
- [ ] Possible error codes documented for each public method?
- [ ] Tests cover each documented failure and its code?

---

## 14. Adding a New Language

Add a section covering:
1. **How `Result`/`IError` is represented** in that language (type names, success/failure access).
2. **Mapping** to the shared code registry (identical numbers).
3. **How foreign exceptions or errors are converted** at boundaries.
4. **Compiler or analyzer support** for unchecked results (`nodiscard` equivalent).
5. **Tests** for code parity with the registry.

Then add its items to the checklist in Section 13.