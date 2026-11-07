# Build and CI Guidelines

Build and continuous integration rules for any project in this repository, regardless of language or runtime. Language-specific settings are listed where relevant (C#, C/C++, interop).

Related documents: [Testing.md](Testing.md), [Security.md](Security.md), [CodingStyle.md](CodingStyle.md), [APIDesign.md](APIDesign.md), [GitWorkflows.md](GitWorkflows.md), [Agent.md](Agent.md).

---

## 1. Principles

1. **Stability first.** `main` and `develop` are always buildable and releasable. A broken build is the team's top priority.
2. **Zero tolerance for warnings.** A build is successful only with **zero warnings and zero errors**.
3. **Reproducible.** The same commit produces the same output on any machine.
4. **One way to build.** CI and developers run the same commands. No hidden steps.
5. **Fail fast and loud.** Cheap checks run first. Failures are clear and actionable.
6. **Everything as code.** Build and CI configuration is versioned, reviewed, and tested like source code.
7. **Never bypass.** No merging on red, no disabling checks to get green.

---

## 2. Warnings and Errors Policy

This is the strictest rule in this document.

### 2.1 Rules
- **Warnings are errors** in every configuration (Debug and Release), locally and in CI.
- A build with any warning is a **failed build**. This includes compiler, analyzer, linker, packaging, and restore warnings (for example NuGet).
- Warnings are fixed, not hidden. Suppression is the last resort.
- **No global suppressions.** No project-wide `NoWarn`, `-Wno-*`, or `/wd` entries without a recorded decision.
- Local suppression (`#pragma warning disable`, `[SuppressMessage]`, `// NOLINT`) must be:
  - as narrow as possible (one line or one member),
  - paired with a comment stating the reason and an issue reference,
  - restored immediately after the affected code.
- Suppressions are reviewed like code. A pull request that adds one needs explicit reviewer approval.
- Count and track suppressions. The trend must not grow.
- Never lower a warning level, disable an analyzer, or remove "treat warnings as errors" to make a build pass.
- New compiler or analyzer versions that add warnings are handled in a dedicated pull request that fixes all of them. Pin tool versions so this happens deliberately (Section 6).

### 2.2 Settings by language

| Language | Required settings |
|----------|-------------------|
| C#       | `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`, `<WarningLevel>` highest, `<Nullable>enable</Nullable>` where the language version supports it, `<AnalysisLevel>` latest, `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>`, `<TreatWarningsAsErrors>` also for NuGet (`<WarningsAsErrors>NU1605;NU1701;NU1903</WarningsAsErrors>` or all `nullable`) |
| C++ (MSVC) | `/W4 /WX /permissive-`, `/sdl`, linker `/WX` |
| C++ (Clang/GCC) | `-Wall -Wextra -Wpedantic -Werror`, plus `-Wconversion -Wshadow` where practical |
| All      | Static analysis (Section 5) with findings treated as errors |

- Put these settings in shared files (`Directory.Build.props`, `Directory.Build.targets`, a shared CMake or `.props` file) so no project can drift.
- Projects cannot override shared warning settings.

### 2.3 Enforcement
- CI runs a clean build and fails on any warning or error.
- The CI log is scanned for the text `warning` as a second line of defense, in case a tool prints warnings without setting an exit code.
- Pull requests cannot merge with a failed build.

---

## 3. Build Configurations

| Configuration | Purpose | Optimization | Debug info | Asserts |
|---------------|---------|--------------|------------|---------|
| Debug         | Local development and debugging | Off | Full | On |
| Release       | Shipping artifacts and benchmarks | On | Portable/PDB, separate | Off (checks that protect safety remain) |
| (Optional) Sanitize/Coverage | Sanitizers, coverage, fuzzing (see [Testing.md](Testing.md)) | Moderate | Full | On |

- Both Debug and Release must build and pass tests in CI. Release is what ships.
- Shipping artifacts are built **only** by CI, in Release.
- Every supported target (runtime, platform, architecture) is built in CI. For this repository that includes .NET Framework 4.8, modern .NET, and every shipped native architecture (for example x86 and x64).
- Public surfaces must be identical across targets (see [APIDesign.md](APIDesign.md)).
- The build never depends on the developer's machine state (global tools, environment variables, installed SDK differences). Pin the SDK/toolchain (for example `global.json`) and fail with a clear message when it is missing.

---

## 4. Formatting and Style Enforcement

- Formatting and style are enforced by tools, not by reviewers.
- Required configuration files in the repository root:

| Language | File | Tool |
|----------|------|------|
| All      | `.editorconfig` | Editor and CLI formatters |
| All      | `.gitattributes` | Line endings and binary detection |
| C#       | `.editorconfig` (with analyzer severities) | `dotnet format` |
| C/C++    | `.clang-format`, `.clang-tidy` | `clang-format`, `clang-tidy` |

- CI runs the formatters in **verify mode** (no changes written) and fails if a file would change:
  - C#: `dotnet format --verify-no-changes`
  - C/C++: `clang-format --dry-run --Werror`
- Developers run the formatter before committing. A pre-commit hook or IDE "format on save" is recommended, but CI is the authority.
- Analyzer severities for rules the team cares about are set to `error` in `.editorconfig`.
- Changes to these configuration files need approval from the code owners, because they change the rules for everybody.

---

## 5. Static Analysis and Quality Checks

Run in CI on every pull request. All findings fail the build unless explicitly suppressed under Section 2.

| Check | Examples |
|-------|----------|
| Compiler analyzers | .NET analyzers (latest, all categories relevant), MSVC `/analyze` |
| Linters | `clang-tidy`, language-appropriate linters |
| Security analysis | Rules from [Security.md](Security.md), CodeQL or equivalent |
| Secret scanning | Fails on any detected secret |
| Dependency audit | Known-vulnerability check (for example `NuGetAudit`), license check |
| Public API check | API baseline diff and API-contract tests (see [APIDesign.md](APIDesign.md)) |
| Spelling/docs | Optional: broken links in Markdown, XML/Doxygen warnings |
| Complexity | Complexity limits from [CodingStyle.md](CodingStyle.md) via analyzer rules where available |

- Do not add a check without a documented way to fix or suppress its findings.
- New checks start as a warning in a dedicated pull request, all findings are fixed, and then the check is raised to an error.

---

## 6. Reproducibility and Dependencies

- **Pin everything:** SDK and compiler versions, CI runner images, CI actions (by version or commit hash), build tools, and package versions. No floating versions (`*`, `latest`).
- Use lock files and restore in locked mode in CI (for example `RestoreLockedMode`) so dependency changes are explicit.
- Use only approved package sources. No new dependencies without approval (see [Security.md](Security.md), [Agent.md](Agent.md)).
- Deterministic builds: `Deterministic` and `ContinuousIntegrationBuild` for C#, `/Brepro` and fixed timestamps for C++, source link, no embedded absolute paths where avoidable.
- Build from a **clean checkout** on **ephemeral** runners. No state from earlier runs except explicit caches (Section 9).
- Builds do not need network access after restore. Network access during build steps is limited and logged.
- Verify reproducibility periodically: build the same commit twice and compare the hashes of the outputs.
- Upgrades of tools and dependencies are made in dedicated pull requests, with all fallout fixed in the same change.

---

## 7. Versioning in the Build

- The version comes from one source of truth (a version file or Git tag) and follows SemVer (see [APIDesign.md](APIDesign.md)).
- The build stamps the version into assemblies, native binaries, and packages. For .NET Framework binding stability, `AssemblyVersion` changes only on MAJOR.
- Pre-release builds carry a suffix and the build number (`2.1.0-beta.1+build.57`).
- Release versions are produced only from a tag on `main` (see [GitWorkflows.md](GitWorkflows.md)).
- Tags are never moved. A failed release uses a new version.

---

## 8. CI Pipeline Stages

Order stages from cheapest to most expensive. A failed stage stops later stages.

| # | Stage | Contents | Blocks merge |
|---|-------|----------|:------------:|
| 1 | Validate | Format verify, secret scan, config/lint checks, commit message and branch name checks | Yes |
| 2 | Build | Restore (locked), build all targets and configurations, zero warnings | Yes |
| 3 | Static analysis | Analyzers, security analysis, dependency and license audit | Yes |
| 4 | Unit tests | Per target, with coverage | Yes |
| 5 | Integration, functional, API-contract tests | Per target and architecture | Yes |
| 6 | Package | Build packages and native artifacts in Release, generate SBOM | Yes (on release branches and PRs touching packaging) |
| 7 | Extended (nightly) | Acceptance tests on packed artifacts, long fuzzing, performance tests, sanitizers, reproducibility check | Reports, and blocks release |
| 8 | Release | Sign, publish, tag verification, publish hashes | Human-approved |

- Test details are defined in [Testing.md](Testing.md).
- Every stage has a time limit. A stage that exceeds it fails.
- The total time for the pull request pipeline should stay under **15 minutes**. Split and parallelize when it grows. Slow pipelines get bypassed.
- Jobs are independent and parallel where possible, with a matrix over target, configuration, and architecture.
- Failures print the failing command, the first error, and a link to the relevant document.

---

## 9. Caching

- Cache only **inputs that are safe to reuse**: restored packages, toolchains, compiler caches (for example ccache).
- Cache keys include the lock file or tool version hash. A changed dependency invalidates the cache.
- **Never cache build outputs** that are shipped, or test results.
- Release builds run without caches, or verify that the cached content matches.
- Caches are not shared with pull requests from forks, and they never contain secrets.
- If a build only passes with a cache, or only fails with it, treat it as a bug. A clean (no cache) build is run nightly to detect this.

---

## 10. Artifacts and Retention

| Artifact | Retention |
|----------|-----------|
| Release packages, symbols, SBOM, hashes, signatures | Permanent (never deleted) |
| Pre-release / nightly packages | 90 days |
| Pull request build output | 7 days |
| Test results, coverage, and logs | 30 days (90 days for `main` and `develop`) |
| Fuzz corpora and crash reproducers | Permanent in the repository or artifact store |
| Performance results (history) | 1 year |

- Retention values are starting points. The team may change them.
- Publish: test results, coverage reports, analyzer output, and logs on every run, including failures.
- Release artifacts are produced by CI only, from a tag, and are never replaced. Publish SHA-256 hashes and signatures.
- Keep symbol files (PDB, dSYM) for every release for crash analysis.
- Artifacts never contain secrets or user data. Scan them before publishing.

---

## 11. Branch Protection and Merge Rules

Applies to `main`, `develop`, and `release/*` (see [GitWorkflows.md](GitWorkflows.md)).

Required settings:
- [ ] No direct pushes, no force pushes, no deletion.
- [ ] Pull request required, with at least **1 human approval** (2 for public API, interop, security-sensitive changes).
- [ ] Author cannot approve their own pull request. Stale approvals are dismissed on new commits.
- [ ] All required status checks pass (Stages 1 to 5 of Section 8).
- [ ] Branch must be **up to date** with the target before merging.
- [ ] All review conversations resolved.
- [ ] Code owner review required for owned paths (`CODEOWNERS`).
- [ ] Linear history or a defined merge strategy (see [GitWorkflows.md](GitWorkflows.md)).
- [ ] Rules apply to administrators too. No bypass lists except a documented emergency procedure.
- [ ] Signed commits or tags where the team requires them.
- [ ] Changes to `.rules/`, CI files, build settings, and `CODEOWNERS` require code-owner approval.

Merge queue (recommended): validates the exact merge result before it lands, so `main` and `develop` never break because of a combination of two individually green pull requests.

---

## 12. Keeping the Build Stable

### 12.1 Broken build policy
- A red build on `main` or `develop` is fixed or **reverted within 1 hour** of detection. Reverting is always acceptable.
- No new merges into a red branch except the fix.
- The person who merged the breaking change is responsible for the fix, or reverting it.
- Record the cause. Add a check that would have caught it.

### 12.2 Flaky builds
- Flaky tests follow [Testing.md](Testing.md) Section 13. Flaky builds (infrastructure, network, runner problems) are reported and fixed the same way.
- No blind automatic reruns that hide failures. A rerun is allowed once, and the failure is logged as flaky.
- Track pipeline reliability (success rate, mean time to fix). Target is above 98% excluding genuine failures.

### 12.3 Pipeline changes
- CI changes are made in pull requests like any other change and tested on a branch first.
- Roll out risky pipeline changes gradually. Keep the previous version available for rollback.
- The build can run **locally** with a single documented command, so CI failures can be reproduced.

---

## 13. Security of the Pipeline

See [Security.md](Security.md) Section 12 for the full rules. Key points:
- Least-privilege tokens, scoped per job, short-lived.
- Secrets are never exposed to pull requests from forks and never printed to logs (masking is enabled).
- Pin third-party actions and tools by hash.
- Signing keys are held in a secure store and used only by the release stage, after human approval.
- Pull request workflows from forks run with read-only permissions and no secrets.
- Pipeline definitions are protected by code owners.

---

## 14. Agents and Automation

See [Agent.md](Agent.md).
- Agents and bots never commit, push, merge, tag, or publish. A human does.
- Agents must not edit CI, build, signing, or warning settings without human approval.
- Agents must not suppress warnings, disable analyzers or tests, or lower thresholds to make a build pass.
- Agents must run the build and report whether it finished with zero warnings and zero errors, and report honestly if it was not run.

---

## 15. Local Developer Workflow

Before pushing, run locally (the same commands as CI):
1. Restore and build (all configurations you changed) with zero warnings.
2. Format check or run the formatter.
3. Unit tests, and the integration tests relevant to the change.
4. Static analysis, if available locally.

Provide wrapper scripts (`build.ps1`, `build.sh`, or a `Makefile`) with the standard targets: `restore`, `build`, `test`, `format`, `analyze`, `package`. CI calls the same scripts.

---

## 16. Review Checklist

- [ ] Build completes with zero warnings and zero errors in all configurations and targets?
- [ ] No new suppressions, or each one is narrow, justified, and references an issue?
- [ ] Warning, analyzer, and formatting settings unchanged, or the change is approved by code owners?
- [ ] Tool, SDK, and dependency versions pinned, and lock files updated?
- [ ] Pipeline stages ordered cheapest first, with time limits, and still under the time budget?
- [ ] Caches keyed correctly, and no outputs cached?
- [ ] Artifacts, retention, hashes, and symbols handled as defined?
- [ ] Branch protection and required checks unchanged?
- [ ] Build reproducible and runnable locally with one command?
- [ ] No secrets in configuration, logs, or artifacts?

---

## 17. Adding a New Language or Platform

Add a section covering:
1. **Toolchain and version pinning.**
2. **Strict warning flags** (the "warnings as errors" setting) and analyzers.
3. **Formatter and linter** configuration and the verify-mode command.
4. **Build configurations** and reproducibility settings.
5. **Test, coverage, sanitizer, and fuzz integration** (see [Testing.md](Testing.md)).
6. **Packaging and signing.**

Then add the new targets to the CI matrix, and the language-specific items to the checklist in Section 16.