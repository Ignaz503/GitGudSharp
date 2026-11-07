# API Design Guidelines

Goal: a **stable public API** with minimal breaking changes. This document is language-agnostic at the top and has language-specific sections (C#, C++, Interop) that can be extended with new languages.

Supported targets: .NET Framework 4.8 and modern .NET (via `netstandard2.0` or multi-targeting), C++ (native), and C#/C++ interop.

---

## 1. Semantic Versioning (SemVer)

Version format: `MAJOR.MINOR.PATCH` (e.g. `2.4.1`), optionally with a pre-release suffix (`-beta.1`) and build metadata (`+build.5`).

| Part  | Increment when...                                              | Consumer impact                    |
|-------|----------------------------------------------------------------|------------------------------------|
| MAJOR | Incompatible (breaking) public API change                      | Code changes may be required       |
| MINOR | Backward-compatible functionality added                        | Safe upgrade                       |
| PATCH | Backward-compatible bug fix, no API surface change             | Safe upgrade                       |

Rules:
- Once a version is released, its contents never change. Publish a new version instead.
- `0.y.z` is initial development; anything may change. Move to `1.0.0` once the API is used in production.
- Deprecating functionality is a **MINOR** change. Removing it is a **MAJOR** change.
- Pre-release versions (`-alpha`, `-beta`, `-rc`) carry no stability guarantees.
- Bug fixes that consumers may depend on are still PATCH, but document behavior changes in the changelog.
- Maintain a `CHANGELOG.md` with sections: Added, Changed, Deprecated, Removed, Fixed.

---

## 2. What Counts as the Public API

Everything a consumer can compile or link against or depend on at runtime:
- Public/protected types, members, constants, enum values, namespaces.
- Binary layout of exported structs and native ABI.
- Documented behavior, error codes, and thrown/returned error types.
- Assembly identity (name, strong-name key) and package ID.
- Serialized formats and wire protocols.

Anything not intended for consumers must be `internal` (C#) or in an `internal`/`detail` namespace or private header (C++). **Default to the smallest visibility.** Public is a promise.

---

## 3. Breaking vs. Non-Breaking Changes

### Non-breaking (MINOR/PATCH)
- Adding new types, new methods, new optional members.
- Adding a new overload (beware of ambiguity, see below).
- Adding a new property to an options class with a sensible default.
- Marking a member `[Obsolete]` (warning only).
- Relaxing preconditions, strengthening postconditions.

### Breaking (MAJOR only)
- Removing or renaming public types/members/namespaces.
- Changing a parameter type, count, order, or return type.
- Changing a parameter's default value semantics.
- Adding abstract members or interface members to existing public types (see C# section).
- Adding enum values when consumers may switch exhaustively (document as non-exhaustive up front).
- Changing a type from `class` to `struct` or vice versa.
- Sealing an unsealed class; making a virtual member non-virtual.
- Changing the layout, size, or field order of exported structs.
- Changing exception/error behavior consumers rely on.
- Tightening preconditions or nullability contracts.

---

## 4. Core Design Principles

1. **Additive evolution only** within a major version.
2. **Options objects over long parameter lists.** Do not grow method signatures.
3. **Sensible defaults** for every option so new fields never force caller changes.
4. **Seal by default**; open for extension only deliberately.
5. **Prefer composition and interfaces you own** over exposing inheritance.
6. **Don't expose third-party types** in the public API; wrap them.
7. **Errors are values**: use the project's `Result`/`TError` model rather than throwing. Error types must not expose or retain underlying `Exception` objects; convert to the library's own error data.
8. **Review every public addition** as if it must live for years.

---

## 5. Evolving Method Signatures

### Problem
Adding a parameter to an existing method breaks source and binary compatibility.

### Rule
Public methods take a small, stable set of **required** arguments, plus an **options object** for everything else.

```csharp
// v1.0
public sealed class CloneOptions 
{ 
	public bool Recursive { get; set; } = false; 
}
public Task<Result> CloneAsync(string url, string path, CloneOptions? options = null);

// v1.3: new option, no signature change
public sealed class CloneOptions 
{
	public bool Recursive { get; set; } = false;
	public int? Depth { get; set; } // default: null = full history 
	public bool SingleBranch { get; set; }     // default: false
}
```

### If a new field is *required* later
Do not add a required parameter. Instead:
1. Add it as an options property with a sensible default that preserves old behavior.
2. If no sensible default exists, add a **new overload/method** and obsolete the old one.
3. Validate in the new path and report problems via `Result`/`TError`, not exceptions.

### Overload pitfalls
- Don't mix optional parameters and overloads for the same method; ambiguity arises.
- **C# optional parameters are baked into the caller at compile time.** Changing a default value does not affect already-compiled callers, and adding an optional parameter is a binary break. Prefer options objects, or overloads, in public API.
- Never reorder parameters.

---

## 6. Obsoleting API

### Process
1. **Introduce the replacement first** (MINOR release).
2. **Mark the old API obsolete** with a message naming the replacement and the removal version (same or next MINOR).
3. **Keep it working** and tested for at least one full MAJOR cycle (minimum: all releases until the next MAJOR).
4. **Escalate** to error (`error: true`) only in the MAJOR before removal if needed.
5. **Remove** only in a MAJOR release; record in the changelog.

### C#

```csharp
[Obsolete("Use CloneAsync(string, string, CloneOptions) instead. Will be removed in 3.0.", false)]
public Task<Result> CloneAsync(string url, string path, bool recursive);
```

- Use a diagnostic ID where supported (`DiagnosticId = "GG0001"`, .NET 5+ only; guard with conditional compilation for .NET Framework 4.8).
- Use `[EditorBrowsable(EditorBrowsableState.Never)]` for hiding legacy members from IntelliSense (keeps them binary compatible).
- Obsolete messages must stay consistent: `Use X instead. Will be removed in N.0.`
- Keep message strings in `Constants` where practical.


### C++
```cpp
[[deprecated("Use gg_clone_ex instead. Will be removed in 3.0.")]] 
GG_API int gg_clone(const char* url, const char* path);
```
- For C headers, use a macro (`GG_DEPRECATED(msg)`) mapping to `__declspec(deprecated)` / `__attribute__((deprecated))`.

### Documentation

Every deprecation is listed in `CHANGELOG.md` under **Deprecated**, with the replacement and planned removal version.

---

## 7. C# Specific Guidelines

### Targeting
- Multi-target `netstandard2.0` (covers .NET Framework 4.8 and modern .NET) plus optionally a modern TFM for performance features.
- Keep the public surface identical across targets. Use `#if` only inside implementation.

### Types
- Mark classes `sealed` unless inheritance is a designed feature.
- Prefer `readonly struct` for small immutable value types; never change a type's kind later.
- Options/config types: `sealed class` with settable properties and defaults (allows adding members later). Use `record`-style only where the target supports it.
- Don't expose public fields; use properties.
- Never add members to a public interface in a MINOR. Alternatives:
  - Introduce a new interface (`IFooV2 : IFoo`) or a separate capability interface.
  - Use abstract base classes with virtual members when extension is expected.
  - Default interface methods are **not available on .NET Framework 4.8**; do not rely on them.
- Enums: assign explicit values, never reorder or reuse values, include documentation that new values may be added. Do not rename members.
- Use `IReadOnlyList<T>`/`IReadOnlyCollection<T>` for returned collections, not concrete types, so internals can change.
- Return `Task`/`ValueTask` consistently; do not change sync to async in place.
- Async methods: provide an optional `CancellationToken` as a trailing parameter from the first release to avoid a later signature change.

### Tooling
- Enable `Microsoft.CodeAnalysis.PublicApiAnalyzers`-style tracking via `PublicAPI.Shipped.txt` / `PublicAPI.Unshipped.txt`. If the analyzer package cannot be added , keep a manually maintained API baseline and diff it in review.
- Optionally run API compatibility checks via the SDK's built-in package validation (`EnablePackageValidation`) when the build supports it.
- Builds must have **zero warnings and zero errors**. Treat warnings as errors in CI.
- Use `[assembly: AssemblyVersion]` that changes only on MAJOR; `AssemblyFileVersion` and `AssemblyInformationalVersion` carry the full version. This reduces binding redirect issues on .NET Framework.

### Error model

See [ErrorHandling.md](ErrorHandling.md) to get a detail on all error handling rules.

---

## 8. C++ Specific Guidelines

- Export a **C ABI** (`extern "C"`) for the stable boundary; keep C++ classes internal.
- Do not expose STL types, exceptions, or templates **in exported signatures or exported struct layouts**. Internal code behind the C ABI may use them freely. Convert at the boundary (`std::string_view` ↔ `const char*` + length, `std::unique_ptr` ↔ opaque handle).
- Use opaque handles (`typedef struct gg_repo* gg_repo_t;`) so layout can change.
- **Versioned/sized structs**: first field is `uint32_t struct_size`, so fields can be appended safely.

```cpp
typedef struct gg_clone_options 
{
	uint32_t struct_size;   // set to sizeof(gg_clone_options)
	uint32_t flags;         // reserved, must be 0 initially 
	int32_t  depth;         // 0 = full history
	void*    reserved[4];   // reserved for future use, must be NULL
} gg_clone_options;
GG_API gg_result gg_clone(const char* url, const char* path, const gg_clone_options* options /* nullable */);
```
- Provide an initializer (`GG_CLONE_OPTIONS_INIT`) that sets defaults and `struct_size`.
- Only append fields; never reorder or resize existing ones. Code reads new fields only if `struct_size` covers them.
- Use fixed-width integer types (`int32_t`, `uint64_t`), not `int`/`long`/`bool` across the ABI.
- Never throw across the boundary; return error codes (`gg_result`) and an error-detail function.
- Export functions with a stable calling convention (`__cdecl`) and a `GG_API` macro.
- Memory ownership: whoever allocates frees. Provide `gg_free_*` functions.
- Never remove or change exported function signatures within a MAJOR; add `_ex` or versioned names (`gg_clone2`) instead.
- Provide `gg_get_version()` returning MAJOR/MINOR/PATCH for runtime checks.
- Embed the MAJOR version in the library filename when ABI breaks (`gitgud2.dll`).

---

## 9. C# ↔ C/C++ Interop Guidelines

### 9.1 Rules
- Use P/Invoke over a **C ABI** only. Do not use C++/CLI in the stable surface (not supported on all .NET targets).
- Keep P/Invoke declarations `internal`; expose only managed wrappers so native changes never leak into the public API.
- Mirror native structs with `[StructLayout(LayoutKind.Sequential)]` and set `struct_size`/`sType` fields in the wrapper, never in caller code.
- Use fixed-width types: `int`/`uint` = 32-bit, `long`/`ulong` = 64-bit **only** in C# terms. Map C `long` carefully (32-bit on Windows, 64-bit on Linux/macOS). Prefer `int32_t`/`int64_t` in native headers.
- Use `IntPtr`/`UIntPtr` for `size_t`, `void*`, and handles. Never use `int` for pointers.
- Do not rely on implicit `bool` marshalling. Use `byte` or `int` and convert explicitly.
- Marshal strings explicitly. Prefer UTF-8 via `byte[]` for cross-platform behavior. Specify `CharSet` explicitly if marshalling is used.
- Use `CallingConvention.Cdecl` explicitly. Windows APIs (`kernel32`, `user32`) use `StdCall` (the default).
- Use `SafeHandle` for every native handle that must be released.
- Never throw across the boundary. Native returns error codes; the wrapper converts them to the library's error model.
- Check native/managed version compatibility at load time and report a mismatch as an error.
- Ship native binaries per architecture (`runtimes/win-x64/native`, `win-x86`). On .NET Framework 4.8, load by explicit path (`LoadLibrary`) since `NativeLibrary` is unavailable.
- Pin or copy managed memory for the duration of the call only. Never let native code keep a pointer into managed memory after the call returns.
- Callbacks: keep the delegate alive (store in a field) for as long as native code can call it. Never let exceptions escape a callback.

### 9.2 Layering

Public managed API   (stable, versioned, returns Result) | Managed wrapper      (SafeHandle, marshalling, error mapping) | internal NativeMethods (P/Invoke, mirrors the C header exactly) | Native library / OS  (C ABI)

### 9.3 Example: OS syscall (Win32 API)

Prefer the "last error" pattern and convert to an error value instead of throwing.

```csharp
internal static class NativeMethods 
{
	private const string Kernel32 = "kernel32.dll";

	[DllImport(Kernel32, SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "CreateFileW")]
	internal static extern SafeFileHandle CreateFile(
		string lpFileName,
		uint dwDesiredAccess,
		uint dwShareMode,
		IntPtr lpSecurityAttributes,
		uint dwCreationDisposition,
		uint dwFlagsAndAttributes,
		IntPtr hTemplateFile);

	[DllImport(Kernel32, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool GetFileSizeEx(SafeFileHandle hFile, out long lpFileSize);

}

public static class FileInfoReader 
{ 
	public static Result<long> GetSize(string path)
	{ 
		using (SafeFileHandle handle = NativeMethods.CreateFile( 
			path, 0x80000000 /GENERIC_READ/, 1 /FILE_SHARE_READ/, IntPtr.Zero,
			3 /OPEN_EXISTING/, 0x80 /FILE_ATTRIBUTE_NORMAL/, IntPtr.Zero))
		{
			if (handle.IsInvalid)
				return Result<long>.Fail(NativeError.FromWin32(Marshal.GetLastWin32Error()));
			if (!NativeMethods.GetFileSizeEx(handle, out long size))
				return Result<long>.Fail(NativeError.FromWin32(Marshal.GetLastWin32Error()));
			return Result<long>.Ok(size); 
		}
	}
}

```

Notes:
- `SetLastError = true` is required for `Marshal.GetLastWin32Error()` to be reliable.
- `SafeFileHandle` (from `Microsoft.Win32.SafeHandles`) releases the handle automatically.
- `NativeError` copies the numeric code and message into the library's own error data. It does not store an `Exception`.
- Keep OS-specific code behind an interface or `#if`/runtime check, and return a "not supported" error on other platforms.

### 9.4 Example: Linux/macOS syscall (libc)

```csharp
internal static class LibC 
{
	private const string Lib = "libc";
	[DllImport(Lib, SetLastError = true, EntryPoint = "getpid")]
	internal static extern int GetPid();

	[DllImport(Lib, SetLastError = true, EntryPoint = "write")]
	internal static extern IntPtr Write(int fd, byte[] buffer, UIntPtr count); // ssize_t write(int, const void*, size_t)
}

```

- `size_t` maps to `UIntPtr`, `ssize_t` to `IntPtr`.
- Select the platform at runtime (`RuntimeInformation.IsOSPlatform`, available in .NET Framework 4.7.1+).

### 9.5 Example: Vulkan-style API (sType, pNext, handles, two-call pattern)

Vulkan is a good model of a stable C ABI: every struct begins with `sType` and `pNext`, so fields can be appended and extensions chained without breaking callers.

#### Native header (excerpt):

```cpp
typedef struct VkApplicationInfo 
{ 
	uint32_t    sType;              // VK_STRUCTURE_TYPE_APPLICATION_INFO
	const void* pNext;              // extension chain, NULL if unused
	const char* pApplicationName;   // UTF-8, nullable
	uint32_t    applicationVersion;
	const char* pEngineName;
	uint32_t    engineVersion;
	uint32_t    apiVersion;
} VkApplicationInfo;

typedef struct VkInstanceCreateInfo
{ 
	uint32_t                 sType;
	const void*              pNext;
	uint32_t                 flags;
	const VkApplicationInfo* pApplicationInfo;
	uint32_t                 enabledLayerCount;
	const char* const*       ppEnabledLayerNames;
	uint32_t                 enabledExtensionCount;
	const char* const*       ppEnabledExtensionNames;
} VkInstanceCreateInfo;

VkResult vkCreateInstance(const VkInstanceCreateInfo* pCreateInfo, const void* pAllocator, VkInstance* pInstance);
void     vkDestroyInstance(VkInstance instance, const void* pAllocator);

```
#### Managed mirror (internal):

```csharp
[StructLayout(LayoutKind.Sequential)]
internal struct VkApplicationInfo
{ 
	public uint sType;                 // 0 = VK_STRUCTURE_TYPE_APPLICATION_INFO
	public IntPtr pNext;
	public IntPtr pApplicationName;// UTF-8, allocated by the wrapper
	public uint applicationVersion;
	public IntPtr pEngineName;
	public uint engineVersion;
	public uint apiVersion;
}
[StructLayout(LayoutKind.Sequential)] 
internal struct VkInstanceCreateInfo 
{ 
	public uint sType;                 // 1 = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO
	public IntPtr pNext;
	public uint flags;
	public IntPtr pApplicationInfo;
	public uint enabledLayerCount;
	public IntPtr ppEnabledLayerNames;
	public uint enabledExtensionCount;
	public IntPtr ppEnabledExtensionNames;
}

internal static class Vk 
{ 
	private const string Lib = "vulkan-1.dll"; // libvulkan.so.1 on Linux
	[DllImport(Lib, CallingConvention = CallingConvention.Winapi)]
	internal static extern int vkCreateInstance(ref VkInstanceCreateInfo createInfo, IntPtr allocator, out IntPtr instance);

	[DllImport(Lib, CallingConvention = CallingConvention.Winapi)]
	internal static extern void vkDestroyInstance(IntPtr instance, IntPtr allocator);
}
```
Public wrapper: an options class with defaults, a `SafeHandle`, and no native types exposed.

```csharp
public sealed class InstanceOptions
{
	public string ApplicationName { get; set; } = "App"; 
	public uint ApplicationVersion { get; set; } = 1;
	public uint ApiVersion { get; set; } = (1u << 22) | (0u << 12); // Vulkan 1.0 
	// Future options are added here with defaults. No signature changes.
}

internal sealed class InstanceHandle : SafeHandle
{ 
	public InstanceHandle() : base(IntPtr.Zero, true) { } 
	public override bool IsInvalid => handle == IntPtr.Zero;

	protected override bool ReleaseHandle()
	{
		Vk.vkDestroyInstance(handle, IntPtr.Zero);
		return true;
	}
}

public sealed class GraphicsInstance : IDisposable 
{
	private readonly InstanceHandle _handle; 
	private GraphicsInstance(InstanceHandle handle)
	{ 
		_handle = handle;
	}
	public static Result<GraphicsInstance> Create(InstanceOptions? options = null)
	{
		options = options ?? new InstanceOptions();
		IntPtr namePtr = IntPtr.Zero, appInfoPtr = IntPtr.Zero;
		try
		{
			namePtr = Utf8.Alloc(options.ApplicationName);   // helper: Marshal.AllocHGlobal + UTF-8 bytes + NUL
			var app = new VkApplicationInfo
			{
				sType = 0,
				pApplicationName = namePtr,
				applicationVersion = options.ApplicationVersion,
				apiVersion = options.ApiVersion,
			};
			appInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VkApplicationInfo)));
			Marshal.StructureToPtr(app, appInfoPtr, false);

			var info = new VkInstanceCreateInfo { sType = 1, pApplicationInfo = appInfoPtr };
			int rc = Vk.vkCreateInstance(ref info, IntPtr.Zero, out IntPtr raw);
			if (rc != 0)
				return Result<GraphicsInstance>.Fail(NativeError.FromCode("vk", rc));

			var handle = new InstanceHandle();
			Marshal.InitHandle(handle, raw);   // see note below
			return Result<GraphicsInstance>.Ok(new GraphicsInstance(handle));
		}
		finally
		{
			if (appInfoPtr != IntPtr.Zero) Marshal.FreeHGlobal(appInfoPtr);
			if (namePtr != IntPtr.Zero) Marshal.FreeHGlobal(namePtr);
		}
	}

	public void Dispose()
	{
		_handle.Dispose();
	}
}
```

Note: `Marshal.InitHandle` does not exist in .NET Framework 4.8. Use a `SetHandle` helper exposed by `InstanceHandle` (`internal void Init(IntPtr raw) => SetHandle(raw);`) instead.

Lessons from this design:
- `sType`/`pNext` (or `struct_size`) allow adding fields later. The wrapper fills them in.
- Native structs stay `internal`. The public API exposes `InstanceOptions` with defaults.
- Every allocation has a matching `finally` release, and every native handle is wrapped in a `SafeHandle`.

### 9.6 Example: two-call buffer pattern (enumerate)

Many C APIs return a count first, then fill a caller-allocated buffer (for example `vkEnumeratePhysicalDevices`).

```csharp
[DllImport(Lib)]
internal static extern int vkEnumeratePhysicalDevices(IntPtr instance, ref uint count, IntPtr[] devices);
internal static Result<IReadOnlyList<IntPtr>> EnumerateDevices(IntPtr instance)
{ 
	uint count = 0;
	int rc = Vk.vkEnumeratePhysicalDevices(instance, ref count, null);   // 1st call: query size 
	if (rc != 0) 
		return Result<IReadOnlyList<IntPtr>>.Fail(NativeError.FromCode("vk", rc));
	var devices = new IntPtr[count];
	rc = Vk.vkEnumeratePhysicalDevices(instance, ref count, devices);    // 2nd call: fill
	if (rc != 0) 
		return Result<IReadOnlyList<IntPtr>>.Fail(NativeError.FromCode("vk", rc));
	return Result<IReadOnlyList<IntPtr>>.Ok(devices);
}

```
- Handle the case where the count changes between calls (retry or truncate, and document which).
- Return `IReadOnlyList<T>` publicly so the implementation can change.

### 9.7 Example: callbacks

```csharp
[UnmanagedFunctionPointer(CallingConvention.Cdecl)] 
internal delegate void LogCallback(int level, IntPtr utf8Message, IntPtr userData);
public sealed class NativeLogger : IDisposable 
{
	private readonly LogCallback _callback;   // field keeps the delegate alive (prevents GC crash)
	public NativeLogger(Action<int, string> sink)
	{
		_callback = (level, msg, _) =>
		{
			try { sink(level, Utf8.ToString(msg)); }
			catch { /* never let exceptions cross into native code */ }
		};
		NativeMethods.gg_set_log_callback(_callback, IntPtr.Zero);
	}

	public void Dispose() 
	{ 
		NativeMethods.gg_set_log_callback(null, IntPtr.Zero); 
	}
}
```
- Unregister the callback before the delegate can be collected.
- Document the thread the callback runs on.

### 9.8 Example: sized struct for a project-owned native library

```cpp
typedef struct gg_clone_options 
{
	uint32_t struct_size;   // set to sizeof(gg_clone_options) 
	uint32_t flags;         // must be 0
	int32_t  depth;         // 0 = full history
	void*    reserved[4];   // must be NULL
} gg_clone_options;
```
```csharp
[StructLayout(LayoutKind.Sequential)]
internal struct GgCloneOptions 
{ 
	public uint struct_size;
	public uint flags;
	public int depth;
	public IntPtr reserved0, reserved1, reserved2, reserved3;

	public static GgCloneOptions Create(int depth)
	{
		return new GgCloneOptions { struct_size = (uint)Marshal.SizeOf(typeof(GgCloneOptions)), depth = depth };
	}
}
```
- Newer managed code with an older native library: the native side reads only the fields covered by `struct_size`.
- Older managed code with a newer native library: appended fields default to zero.

### 9.9 Loading native libraries on .NET Framework 4.8

```csharp
internal static class NativeLoader 
{ 
	[DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
	private static extern IntPtr LoadLibraryW(string path);

	internal static bool TryLoad(string baseDir, out int win32Error)
	{
		string arch = Environment.Is64BitProcess ? "win-x64" : "win-x86";
		string path = Path.Combine(baseDir, "runtimes", arch, "native", "gitgud.dll");
		IntPtr h = LoadLibraryW(path);
		win32Error = h == IntPtr.Zero ? Marshal.GetLastWin32Error() : 0;
		return h != IntPtr.Zero;
	}
}

```
- Call this once before the first P/Invoke. On modern .NET, prefer `NativeLibrary.SetDllImportResolver` behind `#if`.

### 9.10 Interop pitfalls checklist

- [ ] Struct layout, field order, and packing match the header on every architecture.
- [ ] `size_t`, `long`, and pointers are mapped to `UIntPtr`/`IntPtr`/`int`/`long` correctly.
- [ ] Strings are encoded and freed on the correct side (whoever allocates frees).
- [ ] No managed pointer or `GCHandle` outlives the call unless pinned deliberately.
- [ ] Delegates passed to native code are rooted.
- [ ] `SetLastError = true` is set where `GetLastWin32Error` is used.
- [ ] Every native handle is wrapped in a `SafeHandle`.
- [ ] No exceptions escape callbacks, and no exceptions are thrown to public callers.
- [ ] x86 and x64 are both tested (calling conventions and struct sizes differ).
---

## 10. Behavioral Compatibility

- Document and test behavior, not just signatures.
- Keep serialization/output formats stable; add fields, never repurpose them.
- Don't change thread-safety guarantees, ordering, or performance contracts without a MAJOR.
- Keep default values stable; a changed default is a behavior break.

---

## 11. Testing Strategy

Testing rules live in [Testing.md](Testing.md). API stability is enforced by the API-contract tests (Section 8) and compatibility tests (Section 7.1).

## 12. Review Checklist (before merging any public change)

- [ ] Is the member really required to be public?
- [ ] Is the change additive only?
- [ ] No parameters added/removed/reordered on existing members?
- [ ] New optional inputs go in an options object with safe defaults?
- [ ] New API has `CancellationToken` (async) and returns `Result`?
- [ ] Replaced API marked obsolete with a replacement, version, and changelog entry?
- [ ] API baseline file updated?
- [ ] Builds on .NET Framework 4.8 and modern .NET with zero warnings?
- [ ] Native ABI structs are versioned/size-prefixed and unchanged in layout?
- [ ] Version bump matches the change type (MAJOR/MINOR/PATCH)?
- [ ] Unit tests cover new logic, defaults, and error codes?
- [ ] Integration/functional tests cover the new public behavior on both runtimes?
- [ ] Compatibility tests for obsolete or old call patterns still pass?
- [ ] API-contract tests pass and the baseline diff was reviewed?
- [ ] Acceptance scenarios updated for user-visible changes?
- [ ] Docs and changelog updated?

---

## 12. Adding a New Language

To extend this guide, add a section `## N. <Language> Specific Guidelines` covering:
1. **Targets and compatibility** (runtime/toolchain versions).
2. **Visibility rules** (how to keep things internal).
3. **Options pattern** (how to add parameters without breaking).
4. **Deprecation syntax**.
5. **Binary/ABI concerns** (if applicable).
6. **Tooling** for API diffing and baseline checks.
7. **Interop notes** with the existing languages.
8. **Testing strategy** for unit, integration, functional, and acceptance tests.

Then add any language-specific items to the review checklist in section 12.