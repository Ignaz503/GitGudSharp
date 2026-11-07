# Testing Guidelines

Testing rules for any project in this repository, regardless of language or runtime. Language-specific notes are listed where relevant (C#, C/C++, interop).

Related documents: [APIDesign.md](APIDesign.md), [CodingStyle.md](CodingStyle.md), [Security.md](Security.md), [Agent.md](Agent.md), [GitWorkflows.md](GitWorkflows.md).

---

## 1. Principles

1. **Tests are the safety net for change.** Code without tests is code nobody dares to change.
2. **Test behavior, not implementation.** Tests survive refactoring.
3. **Fast feedback.** The cheapest test that can catch a bug is the right one.
4. **Deterministic.** The same code gives the same result, every run, on every machine.
5. **Tests are code.** They follow [CodingStyle.md](CodingStyle.md) and are reviewed like production code.
6. **A failing test is a stop signal.** Never merge on red. Never disable a test to get green.

---

## 2. Test Levels

Tests are organized as a pyramid: many fast, narrow tests at the bottom, few slow, broad tests at the top.
```
        /\
       /  \      Acceptance   (few, slowest)
      /----\
     /      \    Functional
    /--------\
   /          \  Integration
  /------------\
 /              \ Unit       (many, fastest)
/________________\
```


| Level       | Scope                                                        | Protects against                          | Speed  | Runs                     |
|-------------|--------------------------------------------------------------|-------------------------------------------|--------|--------------------------|
| Unit        | One type or function, in isolation                           | Logic regressions, error-handling changes | ms     | Every build / PR         |
| Integration | Several components plus real external systems (processes, file system, network services, databases, native libraries) | Wiring, configuration, and interop breaks | s      | Every PR                 |
| Functional  | Public API used as a consumer would, feature by feature      | Behavioral and signature breaks           | s      | Every PR                 |
| Acceptance  | End-to-end user scenarios against packaged artifacts         | Release-blocking regressions              | s to min | Nightly / before release |

Additional cross-cutting test types: API-contract (Section 8), performance (Section 10), security (Section 11), and fuzz tests (Section 12).

### 2.1 Choosing the level
- Logic with many branches: unit test.
- Code that talks to something outside the process: integration test.
- A documented feature or error code: functional test.
- A user story or release criterion: acceptance test.
- Push tests down the pyramid. If a bug is found by an acceptance test, add a unit test that would have caught it.

---

## 3. General Rules

- Tests target the **public API**. Test internals only through public behavior, or with unit tests where logic is complex.
- Every public member has at least one test. Every bug fix starts with a failing regression test.
- Each test checks **one behavior** and has a clear failure message.
- Tests are independent: no shared mutable state, no reliance on execution order, safe to run in parallel.
- Assert on the documented error contract (error codes, result types, exceptions). The error contract is part of the API.
- Run tests on every supported runtime, platform, and architecture.
- Keep test-only dependencies out of shipped artifacts and keep them minimal. New test dependencies follow the dependency policy in [Security.md](Security.md).
- Builds that run tests complete with zero warnings and zero errors.
- Do not test third-party code or the language runtime. Test your use of it.

---

## 4. Structure and Naming

### 4.1 Arrange, Act, Assert
Every test has three visible parts, separated by blank lines or comments:
```
// Arrange
var options = new CloneOptions();
// Act
var result = Validate(options);
// Assert
Assert.IsTrue(result.IsSuccess);
```

- One Act per test.
- No logic (loops, conditionals) in tests. Use parameterized (data-driven) tests instead.

### 4.2 Naming
- Pattern: `Method_Scenario_ExpectedResult`, for example `Clone_EmptyUrl_ReturnsInvalidArgumentError`.
- C++ equivalent in snake_case: `clone_empty_url_returns_invalid_argument`.
- The name must explain a failure without reading the body.
- Test classes mirror the type under test: `RepositoryReaderTests`.

### 4.3 Test project layout
```
tests/
    <Project>.UnitTests/
    <Project>.IntegrationTests/
    <Project>.FunctionalTests/
    <Project>.AcceptanceTests/
    <Project>.ApiContractTests/
    <Project>.PerformanceTests/
    <Project>.Fuzz/
    native/
        <Project>.Native.Tests/    (tests of the native/C ABI, if applicable)
```


- Folder structure inside a test project mirrors the production code.
- Shared helpers and fakes live in a `TestUtilities` project, never in production code.

### 4.4 Categories and filtering
- Tag tests by level and by cost (`Unit`, `Integration`, `Slow`, `RequiresNetwork`, `Quarantine`).
- Developers can run `Unit` on every save and the rest before pushing.

---

## 5. Unit Tests

- Scope: parsing, validation, defaults, error mapping, and pure logic.
- Isolate external dependencies behind internal abstractions so they are never executed.
- Prefer simple hand-written fakes over heavy mocking where practical. Use mocks only to verify an interaction that is the actual requirement.
- Verify that every options/configuration type has its documented default values. A changed default is a behavior break.
- Verify that invalid input produces the documented error behavior.
- Cover boundaries: null, empty, minimum, maximum, one past the limit, Unicode, very long input.
- Keep them fast (target under about 100 ms each) and safe to run in parallel.
- No file system, network, clock, or environment access. Inject abstractions for time, randomness, and IO.

---

## 6. Integration Tests

- Scope: the library working with real external systems: file system, child processes, databases, network services, or native libraries.
- Use throwaway, isolated environments (temp directories, random ports, in-memory or containerized databases). Do not depend on the developer's or machine's global configuration.
- Pin and document the minimum supported versions of external tools. Test that a missing or too-old dependency is reported through the documented error contract.
- Interop: test version handshakes, struct size and layout handling, string marshalling (including non-ASCII), and release of native handles.
- Test every shipped architecture (for example x86 and x64).
- Clean up all resources, also on failure and cancellation.
- Tag them with a category so they can be filtered.

---

## 7. Functional and Acceptance Tests

### 7.1 Functional tests
- Scope: black-box tests of one feature through the public API only.
- Written from the consumer's perspective, with no access to internals.
- Cover the success path, every documented error, cancellation and timeouts, and option combinations.
- **Compatibility tests**: keep tests that use the API the old way (obsolete members, old option sets). They must pass until the next MAJOR release.
- Suppress deprecation warnings only locally, scoped to those tests, so the build stays warning-free.

### 7.2 Acceptance tests
- Scope: realistic end-to-end user scenarios with explicit pass criteria, each tied to a requirement or user story.
- Run against the **packaged** artifacts (packages, binaries), not project references.
- Run on a clean machine or CI image for each supported runtime and platform.
- Include an **upgrade scenario**: a consumer built against the previous MINOR/PATCH release runs unchanged against the new binary without recompiling. This verifies binary compatibility.
- Run before every release. A failing acceptance test blocks the release.

---

## 8. API-Contract Tests

- Compare the public surface against a stored baseline. Any unreviewed difference fails the build.
- Use reflection or equivalent tooling to assert rules such as: required types are sealed or final, enum values are unchanged, and no public member exposes third-party types.
- Native: assert exported symbols and the size and layout of exported structs against the baseline. Layout changes require a MAJOR bump.
- Serialization: keep golden-file tests for persisted or wire formats.
- See [APIDesign.md](APIDesign.md) for what counts as a breaking change.

---

## 9. Test Data and Test Doubles

### 9.1 Test data
- Create data inside the test, or with small builders and factories with sensible defaults. The test shows only what matters to it.
- Keep fixtures small. Avoid large shared fixtures that couple tests.
- Golden files and sample inputs are stored in the repository, reviewed, and kept small. Large binaries are not committed.
- Use realistic data including edge cases: non-ASCII text, long paths, empty values, different line endings, and different cultures.
- Never use production data, real personal data, or real credentials. Test secrets are fake and clearly marked (see [Security.md](Security.md)).
- Random data uses a **fixed, logged seed** so failures can be reproduced.

### 9.2 Test doubles

| Type  | Use                                                 |
|-------|-----------------------------------------------------|
| Stub  | Returns canned answers                              |
| Fake  | Working lightweight implementation (in-memory store) |
| Mock  | Verifies that an interaction happened               |
| Spy   | Records calls for later assertion                   |

- Prefer fakes and stubs. Over-mocking couples tests to implementation.
- Do not mock types you do not own. Wrap them in an abstraction you own, and fake that.
- Never mock the unit under test.

### 9.3 Time, randomness, and environment
- Inject clocks, random sources, and environment access. No `Sleep` for synchronization. Wait for a condition with a timeout.
- Tests must not depend on the current date, time zone, culture, machine name, or user.

---

## 10. Coverage and Quality Gates

### 10.1 Coverage expectations

| Scope                                         | Minimum line coverage | Branch coverage |
|-----------------------------------------------|-----------------------|-----------------|
| New and changed code (per pull request)       | 90%                   | 80%             |
| Overall project                               | 80%                   | 70%             |
| Public API, parsers, validation, security code| 95%                   | 90%             |

- Values are starting points. The team may raise them, and lowering them needs a recorded decision.
- Coverage is a **floor, not a goal**. 100% coverage does not mean correct. A test with no meaningful assertion does not count.
- Coverage must not drop in a pull request. CI enforces the gates.
- Exclusions (generated code, trivial glue) are explicit, justified, and reviewed.
- Consider mutation testing for critical logic to measure whether tests detect changes.

### 10.2 Quality gates (pull request)
- [ ] All tests pass on every supported runtime.
- [ ] Coverage gates met.
- [ ] No tests skipped, disabled, or newly quarantined without an issue reference.
- [ ] Zero build warnings and zero errors.
- [ ] API-contract baseline reviewed.

---

## 11. Performance Tests

See [Performance.md](Performance.md) for performance rules and guidelines.

---

## 12. Security Tests and Fuzzing

### 12.1 Security tests
See [Security.md](Security.md). At minimum, test malicious input, injection attempts, path traversal, oversized input, and that secrets are not leaked in errors or logs.

### 12.2 Fuzzing
Fuzz everything that parses or consumes untrusted data: parsers, deserializers, protocol handlers, and native entry points.

- Use a coverage-guided fuzzer where available (libFuzzer, AFL++, SharpFuzz, or the toolchain's equivalent), or a simple randomized harness otherwise.
- Provide a **seed corpus** of valid and edge-case inputs, committed to the repository.
- The fuzz target takes bytes and must never crash, hang, leak, or use unbounded memory. The only allowed outcome for bad input is a clean error result.
- Native code: build fuzz targets with AddressSanitizer and UndefinedBehaviorSanitizer.
- Run a short fuzz session in CI on every pull request (smoke), and long sessions nightly or continuously.
- Every crash or hang found is minimized, fixed, and added to the corpus as a **regression test**.
- Property-based tests are a good complement: for example "parse then serialize returns the original".

### 12.3 Native code testing
- Run native tests under AddressSanitizer, UndefinedBehaviorSanitizer, and (where supported) ThreadSanitizer or Valgrind.
- Test all exported functions with invalid arguments: null pointers, wrong `struct_size`, zero and huge sizes.
- Test x86 and x64 (and other shipped architectures). Struct sizes and calling conventions differ.
- Check for leaks: every allocation test has a matching release check.

---

## 13. Flaky Tests Policy

A flaky test passes and fails without a code change. It destroys trust in the test suite.

1. **Do not just rerun.** A flaky test is a bug, in the test or in the code (races, time, order dependence, shared state).
2. **Report it.** Create an issue with the test name, failure logs, and frequency.
3. **Quarantine within one day.** Tag it `Quarantine` so it no longer blocks merges, and keep running it in a separate job so data is collected.
4. **Fix or delete within 14 days.** A test that cannot be made reliable is rewritten at a lower level or removed with a recorded decision.
5. **No blind retries.** Automatic retry is allowed only as a diagnostic that reports the test as flaky, never as a way to hide failures.
6. **Track** the number of quarantined tests. The target is zero. A growing count is reported to the team.

Common causes and fixes:

| Cause                          | Fix                                                    |
|--------------------------------|--------------------------------------------------------|
| Timing and `Sleep`             | Wait for a condition with a timeout, inject a clock    |
| Shared state or test order     | Isolate state per test, avoid statics                  |
| Parallelism races              | Unique resources per test (temp dirs, ports)           |
| External services              | Use fakes or local containers, never the public network |
| Environment differences        | Pin versions, set locale and culture explicitly        |
| Unseeded randomness            | Fixed, logged seed                                     |

---

## 14. Test Pipeline

| Stage        | Runs                                                              |
|--------------|-------------------------------------------------------------------|
| Local        | Unit tests on every change, relevant integration tests before push |
| Pull request | Unit, Integration, Functional, API-contract, security tests, fuzz smoke, coverage gates |
| Nightly      | All of the above, plus Acceptance on all targets, long fuzz sessions, performance tests, sanitizers |
| Release      | Acceptance against the packaged artifacts, then publish           |

- Test results and coverage are published as CI artifacts.
- A failing test on `main`/`develop` is fixed or reverted immediately (see [GitWorkflows.md](GitWorkflows.md)).

---

## 15. Agents and AI-Generated Tests

See [Agent.md](Agent.md). Testing-specific points:
- A human reviews generated tests as carefully as production code.
- Agents may write tests that only confirm their own code. Reviewers check that tests assert the **required** behavior, not the current behavior.
- Agents must not delete, weaken, skip, or disable tests, assertions, or coverage gates to make a build pass.
- Agents run the relevant tests after a change and report honestly what was run and what was not.

---

## 16. Review Checklist

- [ ] Right level in the pyramid (pushed as low as possible)?
- [ ] Names follow the pattern and explain failures?
- [ ] One behavior per test, Arrange/Act/Assert, no logic in tests?
- [ ] Deterministic: no time, network, order, or global state dependence?
- [ ] Defaults, boundaries, and every documented error code covered?
- [ ] Regression test added for each bug fix?
- [ ] Compatibility tests for obsolete and old call patterns still pass?
- [ ] API-contract baseline updated and reviewed?
- [ ] Security tests and fuzz targets added for untrusted input?
- [ ] Performance budgets checked for hot paths?
- [ ] Coverage gates met, and no test skipped or disabled without an issue?
- [ ] Test data is fake, small, and free of secrets?
- [ ] Runs on all supported runtimes and architectures with zero warnings?

---

## 17. Adding a New Language

Add a section covering:
1. **Test framework and runner**, and how it is run locally and in CI.
2. **Naming and layout** conventions of that language.
3. **Mocking and fakes** approach (without adding unapproved dependencies).
4. **Coverage, sanitizer, and fuzzing tools.**
5. **Benchmark tool.**
6. **Interop testing notes** with the existing languages.

Then add language-specific items to the checklist in Section 16.