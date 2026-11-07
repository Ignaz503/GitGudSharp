# Performance Guidelines

Performance rules for any project in this repository, regardless of language or runtime. This document is the **single owner** of benchmark methodology, allocation budgets, regression thresholds, and profiling rules. Other documents link here instead of restating them.

Related documents: [CodingStyle.md](CodingStyle.md), [APIDesign.md](APIDesign.md), [Testing.md](Testing.md), [BuildAndCI.md](BuildAndCI.md), [Security.md](Security.md), [ErrorHandling.md](ErrorHandling.md), [CodeReview.md](CodeReview.md), [Agent.md](Agent.md).

---

## 1. Principles

1. **Performance is a first-class concern.** It is considered at design time, in every review, and in every release, not only when something is slow.
2. **Design for performance, optimize with data.** Choose efficient designs up front (data layout, algorithms, allocation behavior). Do not apply micro-optimizations without a measurement.
3. **Measure, do not guess.** Intuition about performance is often wrong. Every non-trivial optimization needs a before and after measurement.
4. **Allocations are a cost.** Minimize allocations and object creation. Prefer value types where they fit.
5. **Performance is part of the contract.** Documented performance characteristics (complexity, allocation-free paths, blocking behavior) are API (see [APIDesign.md](APIDesign.md) Section 10).
6. **Regressions are bugs.** They are caught in CI, not by users.
7. **Correctness first, then speed.** An optimization never weakens safety, validation, or error handling (see [Security.md](Security.md)).
8. **Readability still matters.** Optimized code is isolated, documented, and justified by data.

---

## 2. Design-Time Rules (always apply)

These are cheap, obvious, and do not need a profiler. They apply to all new code.

### 2.1 General
- Choose the right algorithm and data structure. Know the complexity of what you call. Avoid O(n²) on unbounded input.
- Avoid unnecessary work: repeated lookups, repeated parsing, recomputation, copies of large data.
- Do not do work on the hot path that can be done once (cache immutable results, precompute tables).
- Stream large data. Do not buffer whole inputs when a single pass works.
- Set explicit **bounds** on sizes, counts, and time (this is also a security rule, see [Security.md](Security.md)).
- Use asynchronous I/O for I/O-bound work. Never block a thread on async code. Do not use async for CPU-bound work that is short.
- Support cancellation on long operations (see [ErrorHandling.md](ErrorHandling.md) Section 6.2).
- Prefer batching over many tiny operations (process starts, P/Invoke calls, file opens, round trips).
- Keep the **public API** efficient to call: avoid forcing allocations on callers, accept spans or reusable buffers where the targets allow it, and avoid chatty designs.

### 2.2 C# specifics
- Avoid allocations in hot paths: no LINQ, closures, boxing, params arrays, or string concatenation in loops. Use simple loops and `StringBuilder`.
- Prefer `readonly struct` for small, short-lived, immutable values. Do not make large structs (over about 16 to 24 bytes) that are copied often. Pass them by `in`/`ref readonly` where the language version allows.
- Never box value types accidentally (non-generic collections, `object` parameters, interface calls on structs without constraints).
- Cache completed tasks and immutable error objects. Create `Result` errors only on failure (see [ErrorHandling.md](ErrorHandling.md)).
- Use `ConfigureAwait(false)` in library code.
- Use `ArrayPool<T>`, `Span<T>`, `Memory<T>`, and `ValueTask` only where the target supports them and the gain is measured. On .NET Framework 4.8 they need the `System.Memory` package, which is not allowed under the current dependency policy. Use `#if` for modern targets only, keeping the public surface identical (see [APIDesign.md](APIDesign.md)).
- Pre-size collections (`new List<T>(capacity)`, `StringBuilder(capacity)`).
- Use `StringComparison.Ordinal` for non-linguistic comparisons. Use `CultureInfo.InvariantCulture` for machine data.
- Avoid reflection, `dynamic`, and exceptions in hot paths. Exceptions are slow and are not used for control flow anyway.
- Avoid finalizers. Prefer `SafeHandle` and deterministic disposal.
- Avoid locks on hot paths. Prefer immutability, `Interlocked`, or partitioning. Keep lock scopes tiny.
- Be aware of the .NET Framework 4.8 runtime: older JIT, no tiered compilation, no `Span`, slower `string` APIs. Do not assume modern .NET performance there.

### 2.3 C and C++ specifics
- Follow RAII and avoid hidden allocations. Reserve vector capacity. Prefer `std::string_view` and `std::span` for views, `std::array` for fixed sizes.
- Pass by `const&` or view for large types. Use move semantics. Avoid copies of containers.
- Prefer contiguous memory and cache-friendly layouts (arrays of structs or structs of arrays, chosen by access pattern). Avoid pointer chasing in hot loops.
- Avoid `std::shared_ptr` on hot paths (atomic reference counts). Avoid virtual calls in tight loops when a template or flat design works.
- Avoid unnecessary dynamic allocation: use small-buffer optimization, arenas, or preallocated buffers where measured.
- Mark `noexcept`, `constexpr`, and `[[nodiscard]]` where correct, since they help the optimizer and callers.
- The C ABI boundary has a cost. Design exports to take batches and sizes, and avoid fine-grained calls (see Section 2.4).
- Do not trade away safety for speed (bounds checks, validation at the boundary). Optimize behind the safe interface.

### 2.4 Interop costs
- Every P/Invoke transition has a fixed cost (tens of nanoseconds, plus marshalling). Do not call native code per element in a tight loop. Batch.
- Prefer blittable types (`int`, `long`, `IntPtr`, blittable structs) so no marshalling copy is needed. Avoid implicit string marshalling in hot paths. Marshal once and reuse the buffer.
- Avoid `SetLastError = true` unless the error is needed (it adds cost).
- Pin memory only for the duration of the call. Prefer pooled or preallocated native buffers for repeated calls.
- Avoid crossing the boundary with callbacks on hot paths.
- Measure the managed-to-native round trip with a benchmark before deciding on a design (Section 4).

### 2.5 Child processes and I/O
- Starting a process is expensive (milliseconds). Batch operations into one invocation, use long-lived or batch modes of the tool where available, and avoid process-per-item designs.
- Stream stdout and stderr, with output size limits.
- Use buffered, asynchronous I/O. Avoid many small reads and writes.
- Avoid unnecessary file system scans and repeated stat calls. Cache with a clear invalidation rule.

---

## 3. When to Measure and Profile

### 3.1 Triggers
Measure (benchmark and/or profile) when:

- [ ] You write or change a **hot path**: code called per item, per request, per file, per line, or in a loop over unbounded input.
- [ ] You make a change **for performance reasons**. The measurement is the justification.
- [ ] A change touches code with an existing **benchmark or budget** (Section 6).
- [ ] You add or change a **public API with performance characteristics** (throughput, latency, allocation behavior, or complexity).
- [ ] You add a parser, serializer, or a process, interop, or I/O boundary.
- [ ] You choose between two designs where performance is a deciding factor.
- [ ] A user, CI, or monitoring reports slowness, high memory, or a regression.
- [ ] You upgrade a runtime, compiler, optimizer flag, or dependency.
- [ ] Before each MAJOR or MINOR release (full benchmark run compared with the previous release).

### 3.2 What to use

| Question | Tool type | Examples |
|----------|-----------|----------|
| How fast is this small piece of code? | **Micro-benchmark** | BenchmarkDotNet (C#), Google Benchmark (C++) |
| Where is the time or memory going in a real run? | **Profiler** (sampling first) | Visual Studio Profiler (CPU Usage, .NET Object Allocation, Memory Usage), PerfView, `dotnet-trace`, `dotnet-counters`, `dotnet-gcdump`; native: Visual Studio Profiler, Windows Performance Analyzer (WPA/ETW), `perf`, VTune |
| Is there a memory leak or growth? | **Memory profiler, soak test** | Visual Studio Memory Usage, `dotnet-gcdump`, heap snapshots, sanitizer leak checks |
| Does it scale under load? | **Load or stress test** | Custom harness, scenario tests (Section 8) |
| Is it contended or blocked? | **Concurrency profiler** | Visual Studio Concurrency Visualizer, ETW, thread dumps |

### 3.2.1 Order of work
1. **Reproduce** the problem with a repeatable scenario or benchmark.
2. **Profile** (sampling) to find where the time or allocations really go. Do not guess.
3. **Fix the biggest cost first.** Check whether an algorithm or design change beats a micro-optimization.
4. **Re-measure.** Keep the change only if the numbers improve and the code stays correct and acceptable.
5. **Add a benchmark or budget** so the improvement stays (Section 6).
6. Record the result in the pull request (before/after numbers, environment).

AI assistants and agents can help with this: they can create benchmarks, run profilers, and analyze results (see [Agent.md](Agent.md)). A human reviews every conclusion and every optimization.

---

## 4. Benchmark Tools

### 4.1 C#: BenchmarkDotNet (required)
- **BenchmarkDotNet** is the standard tool for C# micro-benchmarks. Do not use `Stopwatch` loops in unit tests for performance claims.
- It handles warm-up, JIT, iteration counts, outlier removal, statistics, and the memory diagnoser (allocations per operation).
- **Dependency policy:** BenchmarkDotNet is a **test-only/tooling dependency**. It lives only in the benchmark project, is never referenced by shipped projects, and never appears in shipped packages. It is an explicit exception to the "no new packages" rule and requires an approved ADR and a pinned version (see [Security.md](Security.md) Section 11, [Documentation.md](Documentation.md) Section 8). Without this approval, it must not be added.
- Supported runtimes: run on **.NET Framework 4.8** and the modern .NET targets. Use multiple jobs so the same benchmark runs on each.

Required configuration:
- Release build, no debugger attached.
- `MemoryDiagnoser` enabled (allocations and GC counts are always reported).
- Fixed, pinned BenchmarkDotNet version.
- Explicit `[Params]` for input sizes (small, typical, large, boundary).
- Warm-up and iteration defaults unless a measured reason exists to change them.
- Separate process per benchmark (default), so JIT and GC state do not leak.

Example:
```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net48)] 
[SimpleJob(RuntimeMoniker.Net80)] 
public class StatusParserBenchmarks
{ 
	private byte[] _input = Array.Empty<byte>();

	[Params(10, 1_000, 100_000)]
	public int LineCount { get; set; }

	[GlobalSetup]
	public void Setup() => _input = TestData.CreateStatusOutput(LineCount);

	[Benchmark(Baseline = true)]
	public Result<StatusInfo> Parse() => StatusParser.Parse(_input);
}

```

Rules:
- Benchmarks live in `tests/<Project>.PerformanceTests/` (see [Testing.md](Testing.md) Section 4.3).
- Benchmark the **public API** or an `InternalsVisibleTo` hot path. State which in the class comment.
- Return or consume the result so the compiler cannot remove the work.
- Do setup in `[GlobalSetup]`/`[IterationSetup]`, not inside the measured method.
- Never use random data without a fixed seed.
- Include a baseline (`Baseline = true`) to compare against.
- Export machine-readable results (JSON/CSV) for CI comparison (Section 7).

### 4.2 C and C++: Google Benchmark (recommended)
Native benchmarking has no single standard. Use **Google Benchmark** (`benchmark` library) unless an ADR chooses otherwise.

| Tool | Notes | Recommendation |
|------|-------|----------------|
| **Google Benchmark** | Widely used, CMake and vcpkg friendly, statistics, repetitions, JSON output, custom counters, `DoNotOptimize`/`ClobberMemory`, comparison tooling (`compare.py`). | **Default choice** |
| nanobench | Single header, fast to set up, good statistics and low overhead. | Acceptable for small projects |
| Catch2 `BENCHMARK` | Convenient if Catch2 is already the test framework. Fewer features. | Acceptable if Catch2 is used |
| Hand-written timing | Error-prone (no warm-up, no statistics). | Not allowed for claims |

- It is a **test-only dependency**. It lives in the benchmark target only, never in the shipped library. Pin its version and get approval through an ADR (see [Security.md](Security.md) Section 11).
- Build in Release with the same optimization flags as the shipped build, but never with sanitizers (they distort numbers).
- Use `benchmark::DoNotOptimize` and `benchmark::ClobberMemory` so the optimizer does not delete the work.
- Use `->Repetitions(n)` with aggregates (mean, median, stddev), and `->ComputeStatistics` where useful. Compare medians, not single runs.
- Report items or bytes per second (`SetItemsProcessed`, `SetBytesProcessed`) and custom counters (allocations if you track them with a counting allocator).
- Pin the CPU frequency policy where possible, and disable turbo or power-saving for reference runs (Section 5).
- For allocation counts, use a counting allocator or a tool such as heap profiling (Visual Studio native memory profiler, `heaptrack`, Valgrind `massif`).

### 4.3 Interop benchmarks
- Benchmark the **managed wrapper end to end** (public call to native and back) with BenchmarkDotNet. Compare against the native cost measured by Google Benchmark to isolate the transition and marshalling overhead.
- Cover: transition cost, string marshalling (ASCII and non-ASCII, short and long), struct passing, batch size vs per-item calls.

---

## 5. Benchmark Methodology

### 5.1 Environment
- Use **Release** builds, optimizations on, no debugger, no profiler attached, no sanitizers, no coverage instrumentation.
- Use a **dedicated, stable machine** (or runner pool) for reference runs. Shared, noisy CI runners are used only for relative comparison (Section 7).
- Record the environment with every result: CPU model, core count, OS version, runtime and compiler versions, power plan, commit hash.
- Close other workloads. Disable power saving. Prefer fixed CPU frequency where the platform allows.
- Anti-virus and indexing can distort file and process benchmarks. Exclude the benchmark directory on reference machines, and document it.
- Test **all shipped runtimes and architectures** that matter for performance (.NET Framework 4.8 and modern .NET, x86 and x64).

### 5.2 Design of a good benchmark
1. **One question per benchmark.** Name it for what it measures: `Parse_1000Lines`.
2. **Realistic inputs** (sizes and shapes from real use), plus boundaries: empty, small, typical, large, worst case. Include non-ASCII and long values for text.
3. **Isolate the measured code.** Exclude setup, I/O, and logging unless they are the subject.
4. **Prevent dead-code elimination.** Consume the result.
5. **Warm up**, then measure many iterations. Report the median and spread, not a single best time.
6. **Deterministic inputs.** Fixed seeds, no network, no wall-clock dependence.
7. **Compare against a baseline** (previous commit, previous release, or an alternative implementation).
8. **Measure allocations and GC** as well as time (`MemoryDiagnoser`, counting allocators).
9. **Test both cold and warm** where start-up matters (first call, JIT, cache effects). Separate benchmarks for start-up.
10. **Do not trust a single run.** Run again. Treat differences within the noise band as no difference.

### 5.3 Metrics to report

| Metric | Why |
|--------|-----|
| Time per operation (median, p95 where relevant) | Latency |
| Throughput (items/s, MB/s) | Capacity |
| Allocated bytes per operation, allocation count | GC pressure, hidden cost |
| GC collections (Gen0/1/2) | Pauses |
| Peak working set / memory | Footprint |
| Thread and handle counts | Leaks and resource use |
| Start-up time | First-use cost |
| Binary size (native, shipped assemblies) | Distribution cost |

### 5.4 Statistics
- Compare **medians** with confidence intervals (BenchmarkDotNet shows error and standard deviation).
- A difference counts only if it is outside the measured noise and reproducible on a second run.
- Avoid averaging across different input sizes. Report each size.
- Be careful with ratios on very small times (sub-nanosecond results usually mean the work was optimized away).

### 5.4.1 Common mistakes
- Benchmarking Debug builds.
- Measuring with the debugger or profiler attached.
- Forgetting to consume the result.
- Setup inside the measured method.
- A single run on a noisy machine.
- Micro-benchmarks of code that is not on any hot path.
- Optimizing for the benchmark and not the real scenario. Validate with a scenario test (Section 8).

---

## 6. Performance Budgets

A **budget** is a documented, tested limit on a measurable property. Budgets turn "fast" into something CI can check.

### 6.1 Allocation budgets
Allocations are deterministic and not noisy, so they are the most reliable budgets. They are enforced strictly.

| Path | Budget |
|------|--------|
| Success path of validation, parsing, and result creation on hot paths | **0 bytes allocated** per operation where practical |
| Error path | Allowed to allocate (one error object), documented |
| Per-item processing in a loop | No per-item allocation (use pooling, reuse, or value types) |
| Public API call | Allocation count documented for performance-sensitive methods |
| Interop call (blittable arguments) | 0 bytes allocated per call |
| Long-running operation | Memory is bounded and does not grow with time (no leaks) |

- Every hot-path benchmark records bytes allocated per operation. A budget of **0** or a stated number is asserted.
- An increase in allocated bytes per operation fails CI unless approved (Section 7).
- Allocation tests can also run as unit-style tests (measure allocated bytes around a call after warm-up) for critical zero-allocation paths.

### 6.2 Time budgets
- Define budgets for **user-visible operations** and **hot paths** relative to a **reference machine** (for example "parse 1 MB of output in under 50 ms", "validate a path in under 1 µs").
- Budgets are recorded in the benchmark class comment or in `docs/performance.md`, together with the reference machine.
- Absolute time budgets are enforced only on the **dedicated reference runner** (Section 7). On shared runners, only relative comparison and allocation budgets are enforced.
- Set budgets with headroom (typically 20 to 30% above the current measured value) so noise does not cause failures but real regressions do.

### 6.3 Resource budgets
- Peak memory for typical and large inputs.
- Thread count, handle count, and process count per operation.
- Binary size of shipped native libraries and assemblies (alert at a growth of more than 5% per release).
- Start-up cost (first call latency).

### 6.4 Documenting budgets
- Each budget states: what is measured, input, budget value, reference machine, owner, and the benchmark that enforces it.
- Changing a budget needs a reason in the pull request, and approval of the code owner (see [CodeReview.md](CodeReview.md)).
- Public performance claims (complexity, "does not allocate", "non-blocking") are part of the API documentation and stay true (see [Documentation.md](Documentation.md) Section 4).

---

## 7. Regression Detection in CI

### 7.1 What runs where

| Stage | Runs | Gate |
|-------|------|------|
| Local / pull request | Allocation checks for zero-allocation paths, quick benchmark subset (short job) **relative to the base branch** on the same runner | Allocation increase fails. Time regression above the PR threshold warns or fails. |
| Nightly (reference runner) | Full benchmark suite, all runtimes, with history | Threshold regressions open an issue and block the next release until resolved. |
| Before release | Full suite vs the previous release | Regression above the release threshold blocks the release unless approved. |

Pipeline integration: see [BuildAndCI.md](BuildAndCI.md) Section 8 and [Testing.md](Testing.md) Section 14. Performance tests are separate from functional tests and are not part of the fast unit run.

### 7.2 Thresholds

| Metric | Warn | Fail |
|--------|------|------|
| Allocated bytes per operation (managed) | Any increase | Any increase on a zero-allocation path, or above 5% elsewhere |
| Time per operation (relative, same machine) | above 5% slower | above 10% slower |
| Throughput | above 5% lower | above 10% lower |
| Peak memory | above 5% higher | above 10% higher |
| Shipped binary size | above 3% larger | above 5% larger |
| Start-up time | above 10% slower | above 20% slower |

- These are starting values. Tune them using observed noise. Thresholds must be above the measured noise of the runner.
- **Compare like with like:** the same machine, the same job, base commit vs head commit in the same CI run, so machine drift cancels out.
- Re-run once automatically on a suspected regression. Fail only if the second run confirms it. Log it as noisy otherwise (see the flaky policy in [Testing.md](Testing.md) Section 13).
- Improvements above the threshold are reported too, and the baseline is updated through a reviewed change.

### 7.3 Baselines and history
- Store the **baseline results** (JSON) as CI artifacts or in a performance branch/store, per runtime and machine class. Never commit large result files into the main code base.
- Keep the history for at least one year (see [BuildAndCI.md](BuildAndCI.md) Section 10) so trends are visible.
- Update the baseline only in a reviewed pull request or by the release process.
- Publish a short summary table in the pull request (benchmark, base, head, change, allocation change).

### 7.4 Handling a regression
1. Confirm it (rerun, run locally on the reference machine).
2. Find the cause (profile, bisect the commits).
3. Fix it, or revert the change.
4. If the regression is the accepted cost of a needed feature or fix, record it with an ADR or in the pull request, get approval from the code owner, and update the budget.
5. Add a benchmark if there was none.

---

## 8. Load, Stress, Soak, and Scenario Tests

Micro-benchmarks do not catch everything. Add scenario-level tests for operations that matter.

| Test | Question | Typical check |
|------|----------|---------------|
| Scenario benchmark | How fast is a real end-to-end operation? | Time budget for a representative workload (for example a large repository) |
| Load | Does it hold up under expected concurrent use? | Throughput, latency percentiles |
| Stress | What happens beyond expected limits? | Graceful errors (limit codes), no crash, bounded memory |
| Soak (long run) | Does it leak or degrade over hours? | Memory, handle, and thread counts stay flat |
| Scalability | How does cost grow with input size? | Confirm the documented complexity (linear stays linear) |
| Start-up | How long until the first result? | Cold start budget |
| Concurrency | Is it thread safe and does it scale? | No contention collapse, no deadlock |

- Run them nightly or before release, on a stable machine (see [Testing.md](Testing.md) Section 11).
- Leak detection: run the operation many times and assert that memory, handles, and threads return to the baseline. For native code, use leak sanitizers and allocation tracking in the same run.
- Test with realistic data volumes and hostile data (see [Security.md](Security.md): oversized input must produce a bounded error, not unbounded memory or CPU use).

---

## 9. Optimization Rules

When a measurement justifies optimizing:
1. Start from the **biggest cost** in the profile.
2. Prefer in this order: **algorithm and design**, **avoiding work**, **reducing allocations and copies**, **batching**, **data layout**, **caching**, and only then micro-level tuning.
3. Keep optimizations **isolated** behind a clear interface, so the rest of the code stays simple.
4. Keep the simple version available (a reference implementation in tests) and test that both produce **identical results**, including on fuzz and edge inputs (see [Testing.md](Testing.md) Section 12).
5. Comment why: link to the benchmark, the numbers, and the ADR if it is a significant trade-off.
6. Do not trade **correctness, safety, security checks, or the public API** for speed (see [Security.md](Security.md), [APIDesign.md](APIDesign.md)). Unsafe code and `stackalloc`, manual memory management, and intrinsics need security review, bounds reasoning, and a documented reason.
7. Avoid platform-specific tricks unless all supported targets have a fallback with the same behavior. The public surface and results stay identical across targets.
8. Re-measure on **all supported runtimes** (what is faster on modern .NET can be slower on .NET Framework 4.8, and the reverse).
9. Remove an optimization if the measurement no longer supports it after a runtime or compiler change.

Public API performance changes:
- Making an operation **faster** is not breaking.
- Making it **slower**, more allocating, or changing blocking or thread-safety behavior is a behavior change that may need a MAJOR bump if documented as a contract (see [APIDesign.md](APIDesign.md) Section 10).

---

## 10. Build and Compiler Settings for Performance

| Language | Settings |
|----------|----------|
| C# | Release configuration, `<Optimize>true</Optimize>`, `<DebugType>portable</DebugType>`, `<Deterministic>true</Deterministic>`, `<ServerGarbageCollector>` and `<ConcurrentGarbageCollection>` left to the host application (a library does not set them), consider ReadyToRun or NGEN only in application projects. |
| C++ (MSVC) | `/O2`, `/GL` with `/LTCG`, `/Gy`, `/Gw`, `/arch` only to a baseline all supported CPUs share, no `/fp:fast` unless numeric behavior is approved. |
| C++ (Clang/GCC) | `-O2` or `-O3`, `-flto`, no `-march=native` in shipped builds, no `-ffast-math` unless approved. Keep `-fvisibility=hidden` (smaller, faster loading). |

- Do not use unsafe optimization flags that change behavior (fast-math, removed bounds checks) in shipped builds without an ADR.
- Hardening flags required by [Security.md](Security.md) Section 7 (`/GS`, CFG, stack protector, FORTIFY) stay enabled. Their cost is accepted and measured (Section 5), not removed.
- Profile-guided optimization (PGO) may be used for native releases if the training workload is representative and the build stays reproducible (see [BuildAndCI.md](BuildAndCI.md) Section 6).
- Benchmarks and shipped binaries use the same flags (except sanitizers and coverage, which are never used for timing).

---

## 11. Observability in Production Code

- Libraries expose cheap diagnostics hooks (`EventSource`/ETW events on .NET, trace points in native code) that are **off by default** and cost almost nothing when disabled.
- Do not add logging or tracing in hot paths that allocates or formats strings when disabled. Check the enabled flag first.
- Counters (operation counts, durations, bytes processed) can be exposed for consumers to monitor (`dotnet-counters`, `EventCounter`).
- Never record secrets or personal data in diagnostics (see [Security.md](Security.md) Section 10).

---

## 12. Review Expectations

Reviewers treat performance as a first-class review item (see [CodeReview.md](CodeReview.md) Section 8.1):
- Is this code on a hot path? Does it allocate? Does it copy? Does it block?
- Is the complexity appropriate for unbounded input?
- Are there per-item process starts, P/Invoke calls, or file operations that can be batched?
- Are claims backed by measurements, and are the numbers in the pull request?
- Do benchmarks follow the methodology, with realistic data and a baseline?
- Are budgets updated and still met?
- Does the optimization hurt readability, safety, or other runtimes?

---

## 13. Agents and AI-Generated Code

See [Agent.md](Agent.md).
- Agents must not claim a performance improvement without a measurement. Reported numbers come from a real run, with the environment stated. Agents never invent benchmark results.
- Agents may write benchmarks and run profilers. A human reviews methodology (Release build, realistic data, consumed results) and conclusions.
- Agents must not remove, weaken, or skip benchmarks, budgets, or regression gates, or change thresholds to make CI pass.
- Agents must not introduce unsafe code, pooling, or caching tricks without a measurement, a documented reason, and human approval.
- Generated code is checked for hidden costs: LINQ in hot paths, string concatenation in loops, needless copies and allocations.

---

## 14. Review Checklist

- [ ] Efficient design chosen up front: algorithm, data structures, batching, bounded sizes?
- [ ] No needless allocations, copies, boxing, LINQ, or string building in hot paths?
- [ ] No per-item process starts, P/Invoke calls, or I/O round trips where batching is possible?
- [ ] Async I/O used for I/O-bound work, no blocking on async code, cancellation supported?
- [ ] Performance claim backed by a before/after measurement (Release, realistic data, environment stated)?
- [ ] Benchmark added or updated for new or changed hot paths (BenchmarkDotNet or Google Benchmark)?
- [ ] Benchmarks follow the methodology: warm-up, consumed results, fixed seeds, baseline, allocations reported?
- [ ] Allocation and time budgets defined, documented, and met?
- [ ] No regression above the thresholds, or the regression is approved and recorded?
- [ ] Measured on all supported runtimes and architectures (.NET Framework 4.8, modern .NET, x86, x64)?
- [ ] Optimization is isolated, documented with a link to the data, and tested against a reference implementation?
- [ ] No loss of correctness, safety, security checks, or public API stability?
- [ ] Load, soak, or scenario tests updated for long-running or concurrent behavior?
- [ ] Benchmark and tooling dependencies are test-only, pinned, and approved?

---

## 15. Adding a New Language

Add a section covering:
1. **Benchmark tool** (standard tool of the ecosystem, pinned, test-only, approved by ADR).
2. **Allocation and memory measurement** tooling.
3. **Profilers** for CPU, memory, and concurrency.
4. **Build flags** for optimized, reproducible builds.
5. **Language-specific design rules** (hidden allocations, copies, boxing, GC or ownership costs).
6. **CI integration** (how results are exported, compared, and gated).

Then add its items to the checklist in Section 14.