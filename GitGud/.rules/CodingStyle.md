# Coding Style Guidelines

Goal: code that is easy to read, easy to review, and hard to misuse. The general rules apply to every language. Language-specific sections follow the **standard conventions of that language**. When a rule here conflicts with an automated formatter (`.editorconfig`, `clang-format`), the formatter configuration wins and this document is updated.

Related documents: `APIDesign.md` (public API stability), `.github/copilot-instructions.md` (project rules).

---

## 1. General Guidelines (all languages)

### 1.1 Principles
1. **Readability over cleverness.** Code is read far more than it is written.
2. **Simple first.** Choose the simplest design that works. Optimize only with measurements, except for obvious waste (needless allocations, copies).
3. **Consistency.** Match the surrounding code and the formatter.
4. **Small units.** One function does one thing, one type has one reason to change.
5. **Make illegal states unrepresentable.** Use types, enums, and options objects instead of loose flags and magic values.

### 1.2 Complexity limits

| Metric                          | Target        | Hard limit (needs justification) |
|---------------------------------|---------------|----------------------------------|
| Function length                 | <= 30 lines   | 60 lines                         |
| Cyclomatic complexity           | <= 8          | 15                               |
| Nesting depth                   | <= 2 levels   | 3 levels                         |
| Parameters per function         | <= 4          | 6 (otherwise use an options type)|
| File length                     | <= 400 lines  | 800 lines                        |
| Type length                     | <= 300 lines  | 600 lines                        |

- Exceeding a hard limit requires a comment explaining why, or a refactor.
- Reduce complexity by extracting well-named functions, using lookup tables instead of long `if/else` or `switch` chains, and splitting types by responsibility.
- Cognitive complexity matters more than raw line count: a flat 50-line function can be better than five tangled 10-line ones.

### 1.3 Guard clauses and nesting
Handle invalid or trivial cases first and return early. Keep the main path at the lowest indentation.

Avoid:
```csharp
if (input != null) 
{	 if (input.IsValid)
	{ 
		if (!cache.Contains(input.Key))
		{ 
			Process(input);
		}
	}
}
```

Prefer:

```csharp
if (input is null)
	return;
if (!input.IsValid)
	return;
if (cache.Contains(input.Key))
	return;
Process(input);
```
Rules:
- Use guard clauses for argument checks, null/empty checks, and precondition failures.
- Use `continue` in loops to skip items instead of wrapping the body in an `if`.
- Never nest more than 3 levels. At 3, extract a function.
- Avoid `else` after `return`, `continue`, or `break`.
- Avoid deeply nested ternaries. One level is the maximum.
- Return early with an error value (see Section 1.6), not by nesting success paths.

### 1.4 Recursion
- Prefer **iteration** when the data size is unbounded or user-controlled. Stack depth is a limited resource.
- Recursion is acceptable for naturally recursive structures (trees, directory walks, parsers) with a **bounded depth**.
- Every recursive function must have:
  1. A clear base case at the top (guard clause).
  2. Guaranteed progress toward the base case.
  3. A depth limit or an explicit stack when input is untrusted.
- Do not rely on tail-call optimization. It is not guaranteed in C# and not guaranteed in C++.
- Convert deep recursion to an explicit `Stack<T>`/`std::vector` work list when depth can be large.
- Avoid mutual recursion unless it is clearly documented.
- Document the maximum expected depth in a comment.

### 1.5 Naming
- Names describe **intent**, not type or implementation: `retryCount`, not `intVal`.
- Use whole words. Abbreviations only when universal (`id`, `url`, `io`, `max`).
- Booleans read as statements: `isEmpty`, `hasChanges`, `canRetry`.
- Functions are verbs, types are nouns.
- Avoid negated names (`isNotValid`). Avoid generic names (`data`, `info`, `manager`, `helper`, `util`) without qualification.
- Name constants by meaning, not value (`MaxRetryCount`, not `Three`).

### 1.6 Error handling

See [ErrorHandling.md](ErrorHandling.md) to get a detail on all error handling rules.

### 1.7 Functions and data
- Prefer pure functions and immutable data. Limit shared mutable state.
- No output parameters unless the language idiom demands it (`TryXxx`).
- No boolean flag parameters that switch behavior. Use two functions, an enum, or an options type.
- Prefer returning values over mutating arguments.
- Avoid long parameter lists. Group related values into a type with sensible defaults.
- Keep side effects visible in the function name.

### 1.8 Constants and magic values
- No magic numbers or strings. Name them.
- Centralize string literals and format strings in `GitGud.Constants` where practical.
- Constants live as close as possible to where they are used, unless shared.

### 1.9 Comments and documentation
- Code explains **what**, comments explain **why** (constraints, trade-offs, workarounds, links to issues).
- Delete commented-out code. Version control remembers it.
- Every public API member has documentation (summary, parameters, returns, error codes).
- `TODO` comments include an owner or issue reference: `// TODO(#123): ...`.
- Keep comments correct. An outdated comment is a bug.

### 1.10 Performance
See [Performance.md](Performance.md) for performance rules and guidelines. Performance is a first class concern for all code, not just hot paths. Avoid needless allocations, copies, and conversions. Use the right data structures for the task. Avoid unnecessary work in loops and repeated calls. But do not optimize prematurely. Measure first, then optimize the bottleneck. Document the performance characteristics of public APIs.

### 1.11 Concurrency
- Prefer immutability and message passing over locks.
- Keep lock scopes small, never call unknown code (callbacks, virtual methods) while holding a lock.
- Document thread-safety of every public type.
- Always support cancellation in long-running or async operations.

### 1.12 Formatting
- Indentation: 4 spaces, no tabs (C#), 4 spaces (C++).
- One statement per line. One declaration per line.
- Always use braces for control statements, even single-line bodies.
- Line length: 120 characters maximum.
- Files end with a newline, use UTF-8 (no BOM preferred), and consistent line endings (`.gitattributes`).
- Avoid trailing whitespace.
- Imports/includes at the top, sorted, with no unused entries.

### 1.13 Build hygiene
- A build is successful only with **zero warnings and zero errors**.
- Do not suppress warnings globally. Suppress locally with a justification.
- Remove unused code, variables, and parameters.

### 1.14 Reviews
- Small, focused changes. One logical change per pull request.
- Reviewers check: correctness, clarity, complexity limits, tests, public API impact (`APIDesign.md`).
- Style issues that a formatter can fix are not discussed in review. Run the formatter.

---
## 2. C# Guidelines

Targets: .NET Framework 4.8 and modern .NET. Language version must be compatible with both (use `LangVersion` explicitly, avoid features that need runtime support missing in .NET Framework 4.8 such as default interface methods, `Index`/`Range` without polyfills).

### 2.1 Naming

| Element                               | Convention              | Example                      |
|---------------------------------------|-------------------------|------------------------------|
| Namespace                             | PascalCase              | `GitGud.Core`                |
| Class, struct, record, enum, delegate | PascalCase              | `RepositoryReader`           |
| Interface                             | `I` + PascalCase        | `ICommandRunner`             |
| Method, property, event               | PascalCase              | `GetStatusAsync`             |
| Public/protected field (avoid)        | PascalCase              | `MaxCount`                   |
| Constant (`const`, `static readonly`) | PascalCase              | `DefaultTimeout`             |
| Private/internal field                | `_camelCase`            | `_commandRunner`             |
| Parameter, local variable             | camelCase               | `repositoryPath`             |
| Generic type parameter                | `T` or `TPascalCase`    | `TResult`                    |
| Async method                          | suffix `Async`          | `CloneAsync`                 |
| Enum (non-flags)                      | singular                | `FileStatus`                 |
| Enum (flags)                          | plural                  | `FileAttributes`             |

- Acronyms of two letters are uppercase (`IO`), longer ones are PascalCase (`Url`, `Json`).
- Do not use Hungarian notation or type prefixes.
- Test names: `Method_Scenario_ExpectedResult`.

### 2.2 Layout and structure
- File-scoped namespaces are not available on older language versions. Use block namespaces unless `LangVersion` allows file-scoped ones.
- One top-level type per file. The file is named after the type.
- Member order: constants, static fields, fields, constructors, properties, public methods, internal methods, private methods, nested types.
- Allman braces (opening brace on its own line).
- `using` directives outside the namespace, `System` namespaces first, then others alphabetically.

### 2.3 Language usage
- Use `var` when the type is obvious from the right side (`new`, cast, explicit method name). Use the explicit type otherwise.
- Use `readonly` for fields that are not reassigned. Prefer `readonly struct` for small immutable value types.
- Mark classes `sealed` unless designed for inheritance.
- Default visibility: the smallest possible (`private`, then `internal`). Public is an API promise.
- Use properties, never public fields.
- Use `nameof` instead of strings for member names.
- Use string interpolation for readability. Avoid it in hot paths if it allocates needlessly.
- Prefer `is null` / `is not null` where the language version supports it, otherwise `== null`.
- Use expression-bodied members only for trivial, single-expression members.
- Prefer `foreach` over `for` unless the index is needed.
- Avoid `dynamic`, and avoid reflection in hot paths.
- Avoid LINQ in hot paths. Use simple loops to avoid allocations.
- Use `StringBuilder` for repeated concatenation. Use `string.Create`/`ArrayPool<T>`/`Span<T>` only where the target supports them (guard with `#if`, or use the `System.Memory` polyfill only if allowed by the dependency rules.
- Use `ConfigureAwait(false)` in library code.
- Pass `CancellationToken` as the last parameter, named `cancellationToken`.
- Do not use `async void` except for event handlers. Never block with `.Result` or `.Wait()`.
- Return `Task.FromResult`/cached completed tasks instead of allocating needlessly. Use `ValueTask` only where it is available and justified.
- Dispose `IDisposable` objects with `using`. Implement the full dispose pattern only when a finalizer is needed. Prefer `SafeHandle`.
- Nullable reference types: enable where the language version supports it. Annotate public APIs accurately.

### 2.4 Error handling (C#)
- Public methods return `Result`/`Result<T>`. They do not throw for invalid input or expected failures.
- Use `TryXxx` patterns only for hot, allocation-sensitive paths where `Result` would allocate.
- Catch only exceptions you can handle and only at boundaries (I/O, process, interop). Convert to the library's error data. Do not store the `Exception` object.
- Never use exceptions for control flow.
- Do not catch `Exception` without a reason documented in a comment.
- Document potential `Exception` that are not handled and propagate to the caller.

### 2.5 Documentation (C#)
- XML documentation on every public and protected member: `<summary>`, `<param>`, `<returns>`, and `<remarks>` for error codes and thread safety.
- Use `<inheritdoc/>` for overrides and implementations.

### 2.6 Tooling
- A repository `.editorconfig` defines formatting and analyzer severities.
- Enable .NET analyzers (built-in) and treat warnings as errors in CI.
- Format with `dotnet format` or Visual Studio **Format Document** before committing.

### 2.7 Example

```csharp
public sealed class StatusReader
{
    private readonly ICommandRunner _runner;

    public StatusReader(ICommandRunner runner)
        => _runner = runner;
    

    public async Task<Result<StatusInfo>> ReadAsync(string repositoryPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(repositoryPath))
            return Result<StatusInfo>.Fail(Errors.InvalidPath);
        

        if (cancellationToken.IsCancellationRequested)
            return Result<StatusInfo>.Fail(Errors.Cancelled);
        

        Result<string> output = await _runner.RunAsync(repositoryPath, Constants.StatusArguments, cancellationToken)
            .ConfigureAwait(false);
        if (!output.IsSuccess)
            return Result<StatusInfo>.Fail(output.Error);
        
        return StatusParser.Parse(output.Value);
    }
}

```
---

## 3. C++ Guidelines

Standard: C++17 as the baseline (check the toolchain; use the highest standard all targets support). The stable boundary is a **C ABI** (see `APIDesign.md`). Internal code may use modern C++.

### 3.1 Naming
The convention follows the C++ standard library style: **snake_case**.

| Element                          | Convention                  | Example                      |
|----------------------------------|-----------------------------|------------------------------|
| Namespace                        | snake_case                  | `gitgud::detail`             |
| Class, struct, enum, alias       | snake_case                  | `command_runner`             |
| Function, method                 | snake_case                  | `read_status`                |
| Variable, parameter              | snake_case                  | `repository_path`            |
| Private/protected data member    | snake_case + trailing `_`   | `runner_`                    |
| Public struct data member        | snake_case                  | `struct_size`                |
| Constant, `constexpr`            | `k_` + snake_case, or UPPER | `k_max_retries`              |
| Enumerator (`enum class`)        | snake_case                  | `file_status::modified`      |
| Template parameter               | PascalCase                  | `TValue`                     |
| Macro                            | `UPPER_SNAKE_CASE`, prefixed| `GG_API`, `GG_DEPRECATED`    |
| Exported C function              | `prefix_snake_case`         | `gg_clone_ex`                |
| Exported C type                  | `prefix_snake_case`         | `gg_clone_options`           |
| File                             | snake_case                  | `command_runner.cpp`         |

- Pick one constant style (`k_` prefix or UPPER) and use it everywhere.
- Exported symbols always carry the project prefix (`gg_`, `GG_`).
- Headers use `.h` for C-compatible headers and `.hpp` for C++-only headers. Source files use `.cpp`.

### 3.2 Layout and structure
- One class per header/source pair where practical.
- Use `#pragma once` (supported by all target compilers) or include guards for C headers (`GITGUD_CLONE_H`).
- Include order: matching header, project headers, third-party headers, standard library headers. Each group sorted.
- Include what you use. Prefer forward declarations in headers to reduce coupling.
- Keep implementation in `.cpp` files. Headers hold declarations, templates, and trivial inline functions.
- Put internal declarations in `detail` namespaces or private headers.
- Allman braces to match the C# code, or the Linux/LLVM style, but pick one in `.clang-format` and keep it.

### 3.3 Language usage
> The rules below apply to **internal code**. Exported (C ABI) declarations follow Section 3.5 and `APIDesign.md`: no `std::` types, no references, no templates, only fixed-width types, pointers, and opaque handles.

- Follow **RAII**. Every resource is owned by an object (`std::unique_ptr`, `std::vector`, custom handle types). No manual `new`/`delete`, `malloc`/`free` outside the C ABI layer.
- Prefer `std::unique_ptr` for ownership. Use `std::shared_ptr` only for genuinely shared ownership.
- Use references for non-null parameters, pointers for nullable or optional ones, `std::optional` for optional values.
- Use `const` and `constexpr` liberally. Mark single-argument constructors `explicit`.
- Use `enum class`, never unscoped enums, except in C headers.
- Use `nullptr`, never `NULL` or `0`.
- Use `auto` when the type is obvious or noisy, explicit types otherwise.
- Pass cheap types by value, larger types by `const&`. Use `std::string_view` and `std::span` (C++20, or a local equivalent) for non-owning views.
- Use move semantics to avoid copies. Follow the Rule of Zero. If a special member is needed, follow the Rule of Five.
- Mark overrides with `override`, and final classes with `final`.
- Use `[[nodiscard]]` on functions whose result must be checked (error codes, results).
- Use `static_assert` to check struct sizes and layout of exported types.
- Avoid macros except for export, platform, and versioning. Prefer `constexpr` and templates.
- Avoid global mutable state and static initialization order dependencies.
- Use `noexcept` where a function truly cannot throw.
- NEVER use C-style casts. Use `static_cast`, `reinterpret_cast` (rarely), and `const_cast` (almost never).
- Avoid raw arrays and pointer arithmetic in internal code. Use `std::array`, `std::vector`, and spans. Raw pointers + length are permitted only in the exported C ABI layer, and should be converted to a span/view immediately on entry.
- Reserve capacity and avoid needless allocations in hot paths.

### 3.4 Error handling (C++)
- **No exceptions cross the C ABI boundary.** Catch everything at the exported function and convert to a `gg_result` code.
- Internally, prefer error values (`result<T>`, `std::optional`, error codes) in line with the project rule of not throwing. Exceptions from the standard library (`std::bad_alloc`) are caught at the boundary.
- Never use `catch (...)` without converting to an error code.
- Exported functions validate arguments (null pointers, `struct_size`) and return an error code.

### 3.5 Exported (C ABI) rules
- Use fixed-width integer types (`int32_t`, `uint64_t`). No `bool`, `long`, `wchar_t` in the ABI.
- Use opaque handles, and a `struct_size` first field in option structs.
- Every exported function has `GG_API` and an explicit calling convention.
- Document ownership: who allocates, who frees.

### 3.6 Documentation (C++)
- Doxygen-style comments (`///` or `/** */`) on every exported declaration: `@brief`, `@param`, `@return`, ownership, thread safety.

### 3.7 Tooling
- `.clang-format` in the repository root defines formatting. Run it before committing.
- `clang-tidy` (or MSVC code analysis) with warnings as errors in CI. Enable `/W4` (MSVC) or `-Wall -Wextra -Wpedantic` (Clang/GCC).
- Run sanitizers (AddressSanitizer, UndefinedBehaviorSanitizer) in CI where supported.

### 3.8 Example
```cpp

// status_reader.hpp
#pragma once
#include <string_view>
namespace gitgud 
{ 
    class status_reader 
    { 
    public: 
        explicit status_reader(command_runner& runner) noexcept 
            : runner_(runner)
            { }
    [[nodiscard]] result<status_info> read(std::string_view repository_path) const;

    private:
        command_runner& runner_;
    };
}
```
```cpp

// status_reader.cpp 
#include "status_reader.hpp"
namespace gitgud 
{ 
    result<status_info> status_reader::read(std::string_view repository_path) const
    { 
        if (repository_path.empty())
            return result<status_info>::fail(error_code::invalid_path);
        
        auto output = runner_.run(repository_path, k_status_arguments);
        if (!output)
            return result<status_info>::fail(output.error());
        
        return parse_status(output.value());
    }
}

```

---

## 4. Review Checklist

- [ ] Functions within complexity limits (length, cyclomatic complexity, parameters)?
- [ ] Nesting depth <= 3, guard clauses used for early exits?
- [ ] Recursion bounded and documented, or replaced with iteration?
- [ ] Names follow the language convention and express intent?
- [ ] No magic strings or numbers (centralized where practical)?
- [ ] No thrown exceptions for expected failures, errors returned as values?
- [ ] Resources owned and released (`using`/`SafeHandle`/RAII)?
- [ ] No needless allocations or copies in hot paths?
- [ ] Async code supports cancellation and does not block?
- [ ] Public members documented, and API impact checked against `APIDesign.md`?
- [ ] Formatter and analyzers run, build has zero warnings and zero errors?

---

## 5. Adding a New Language

Add a section `## N. <Language> Guidelines` covering:
1. **Targets and language version.**
2. **Naming** (follow the language's standard convention, as a table).
3. **Layout and structure** (files, imports, member order, brace style).
4. **Language usage** (idioms, ownership/resources, async, null handling).
5. **Error handling** (consistent with the general rule of returning errors as values).
6. **Documentation** format.
7. **Tooling** (formatter, linter, analyzers, CI flags).
8. **A short example** demonstrating the rules.

Then add language-specific items to the review checklist in Section 4.