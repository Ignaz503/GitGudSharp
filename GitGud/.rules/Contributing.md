# Contributing Guide

How to contribute to any project in this repository, whether you are a team member or an external contributor. This is the entry point: it explains how to get started and links to the detailed rules.

---

## 1. Welcome

- Be respectful and constructive. Follow the `CODE_OF_CONDUCT.md` in the repository root.
- Questions are welcome. Open a discussion or an issue before starting large work.
- Security problems are **not** reported in public issues. See [Security.md](Security.md) Section 16.

---

## 2. Map of the Rule Documents

| Document | Read it when... |
|----------|-----------------|
| [GitWorkflows.md](GitWorkflows.md) | You create a branch, commit, or open a pull request |
| [CodingStyle.md](CodingStyle.md) | You write or review code (C#, C++) |
| [APIDesign.md](APIDesign.md) | You add or change anything public, including native exports |
| [Testing.md](Testing.md) | You add features, fix bugs, or touch tests |
| [BuildAndCI.md](BuildAndCI.md) | The build or pipeline fails, or you change build settings |
| [Security.md](Security.md) | You handle input, files, processes, native code, secrets, or dependencies |
| [ReleaseProcess.md](ReleaseProcess.md) | You prepare or support a release |
| [Agent.md](Agent.md) | You use AI assistants or automation |

Every contributor must read at least: this guide, `GitWorkflows.md`, `CodingStyle.md`, and the sections of the other documents that apply to the change.

---

## 3. Onboarding Steps

### 3.1 Prerequisites
- Git (current version) and a Git client or IDE.
- Visual Studio (current version) with the .NET desktop and Desktop development with C++ workloads, or the equivalent SDKs and compilers.
- The SDK and toolchain versions pinned in `global.json` and the build files. Do not use different ones.
- Targets: .NET Framework 4.8 and modern .NET. Native code: x86 and x64.

### 3.2 First-time setup
1. **Team member:** clone the team repository. **Other teams and external contributors:** fork it first and clone your fork (see [GitWorkflows.md](GitWorkflows.md) Section 3).
2. Add the `upstream` remote if you cloned a fork.
3. Configure your identity: `git config user.name` and `git config user.email`.
4. Enable the repository hooks and formatters, if provided (pre-commit format check, secret scan).
5. Restore, build, and run the tests once (Section 4 and 5) to confirm your setup works before changing anything.
6. Read the open issues labeled `good first issue` to pick a starting task.

### 3.3 Your first change
1. Find or open an issue and say you are working on it.
2. Create a branch from `develop` (see Section 6).
3. Make a small, focused change with tests.
4. Build with zero warnings, run the tests, run the formatter.
5. Open a pull request using the template (Section 8).

---

## 4. How to Build

Use the provided scripts. CI runs the same commands (see [BuildAndCI.md](BuildAndCI.md) Section 15).

Rules:
- A build is successful only with **zero warnings and zero errors**. Fix warnings, do not suppress them (see [BuildAndCI.md](BuildAndCI.md) Section 2).
- Do not change warning, analyzer, or formatter settings in a feature pull request.
- Do not add new dependencies without approval (see [Security.md](Security.md) Section 11).

---

## 5. How to Run Tests

Or in Visual Studio: **Test > Test Explorer > Run All Tests**.

- Run the unit tests often, and the integration tests relevant to your change before you push.
- Every bug fix starts with a failing test. Every new public member has tests.
- Tests must pass on all supported runtimes and architectures.
- Do not skip, disable, or weaken a test to get green.
- Details: [Testing.md](Testing.md).

---

## 6. Branches and Commits

- Team members use Gitflow, other contributors use forks. See [GitWorkflows.md](GitWorkflows.md).
- Branch names are **lowercase**: `feature/123-short-description`.
- Branch from `develop`. Never commit directly to `main` or `develop`.
- Commit message format:
```
<type>(<scope>): <summary in imperative, max 72 characters>
<what and why>
Refs: #123
```

- Types: `feat`, `fix`, `docs`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`.
- One logical change per commit and per pull request. Do not mix refactoring with behavior changes.
- Never commit secrets, build output, or large binaries.

---

## 7. Code Expectations (summary)

- Follow [CodingStyle.md](CodingStyle.md): small functions, guard clauses, low nesting, clear names, no magic strings.
- Public API changes are additive. Use options objects and the obsolete process (see [APIDesign.md](APIDesign.md)).
- Return errors as values where the project's error model applies. Do not throw for expected failures.
- Minimize allocations, support cancellation, avoid blocking on async code.
- Update documentation, the changelog (`[Unreleased]`), and the API baseline where relevant.

---

## 8. Pull Requests

### 8.1 Before opening
- [ ] Builds with zero warnings and zero errors.
- [ ] Formatter run, no analyzer findings.
- [ ] Tests added or updated, and all pass.
- [ ] Documentation and changelog updated.
- [ ] Branch is up to date with `develop`.
- [ ] No secrets, personal data, or unrelated changes.
- [ ] You reviewed your own diff completely.

### 8.2 The template
Save this as `.github/pull_request_template.md`:

```
# Summary
<!-- What does this change do and why? Link the issue: Closes #123 -->
## Type of change
-	[ ] Bug fix
-	[ ] New feature (backward compatible)
-	[ ] Breaking change (MAJOR release work only)
-	[ ] Refactoring (no behavior change)
-	[ ] Documentation, build, or CI
## Public API impact
-	[ ] No public API change
-	[ ] Additive public API change (baseline updated)
-	[ ] Deprecation added (replacement, removal version, and changelog entry present)
-	[ ] Breaking change (approved, MAJOR)
## Testing
<!-- What did you run? Which runtimes and architectures? -->
-	[ ] Unit tests
-	[ ] Integration / functional tests
-	[ ] Regression test added for bug fixes
-	[ ] Security tests for untrusted input (if applicable)
## Checklist
-	[ ] Build has zero warnings and zero errors
-	[ ] Formatter and analyzers pass
-	[ ] Follows CodingStyle.md and APIDesign.md
-	[ ] CHANGELOG.md updated
-	[ ] Documentation updated
-	[ ] No new dependencies (or approved: link)
-	[ ] No secrets or personal data
## Security review needed?
<!-- Check Security.md Section 14 triggers. -->
-	[ ] No
-	[ ] Yes: process execution, paths, native/interop, parsing, credentials, dependencies, crypto, build/CI
## AI assistance
-	[ ] No AI tools used
-	[ ] AI tools used (tool and scope):
-	[ ] I reviewed and understand all AI-generated code in this pull request
```


### 8.3 Review process
- Pull requests target `develop`. Release and hotfix branches are for maintainers.
- At least **one human approval**. Two for public API, interop, and security-sensitive changes. The author cannot approve their own pull request.
- Code owners are requested automatically (Section 9).
- CI must be green. Do not merge on red.
- Respond to comments with commits or an explanation. Resolve conversations when done.
- Keep pull requests small (target under about 400 changed lines). Split larger ones.
- Reviewers answer within **2 business days**. Ping politely after that.
- Merge strategy is defined in [GitWorkflows.md](GitWorkflows.md) Section 6.

### 8.4 AI-assisted contributions
- Allowed, but you are fully responsible for the result. All code must be reviewed by a human before it is committed.
- Disclose the use in the pull request template.
- Agents never commit, push, or merge (see [Agent.md](Agent.md)).

---

## 9. CODEOWNERS

Save this as `.github/CODEOWNERS`. Later matching patterns take precedence. Replace the teams with your own.
```

# Default owners for everything

- @org/maintainers

# Public API and compatibility

- /src/**/PublicAPI.*.txt         @org/api-owners 
- /.rules/APIDesign.md            @org/api-owners

# Security-sensitive areas

- /.rules/Security.md             @org/security 
- /src//Interop/                  @org/security @org/native 
- /src//Process/                  @org/security 
- /native/**                      @org/native

# Build, CI, release, signing

- /.github/workflows/**           @org/build-owners
- /.github/CODEOWNERS             @org/maintainers
- /nuget.config                   @org/build-owners @org/security 
- /.rules/BuildAndCI.md           @org/build-owners 
- /.rules/ReleaseProcess.md       @org/release-managers @org/security

# Rules and process documents

- /.rules/                        @org/maintainers 
- /CONTRIBUTING.md                @org/maintainers
```

- Enable **Require review from Code Owners** in the branch protection rules (see [BuildAndCI.md](BuildAndCI.md) Section 11).
- Use teams, not individuals, so reviews continue when people are away.
- Keep the file valid. A broken line silently disables ownership for that path. GitHub shows errors in the file view.
- Changes to `CODEOWNERS` need approval from the maintainers.

---

## 10. Issues

- Search for existing issues first.
- **Bug report:** version, runtime (.NET Framework 4.8 or modern .NET), OS, architecture, steps to reproduce, expected and actual result, logs (with secrets removed).
- **Feature request:** the problem to solve first, then the proposed solution and alternatives. Public API changes get a design discussion before code.
- Use issue templates in `.github/ISSUE_TEMPLATE/`.
- Labels: `bug`, `feature`, `good first issue`, `help wanted`, `breaking`, `security` (private only), `needs-triage`.

---

## 11. Design Discussions and Large Changes

Open an issue or design proposal first when the change:
- adds or changes public API, native exports, or serialized formats,
- adds a dependency, target, or architecture,
- changes build, CI, signing, or release configuration,
- touches security-sensitive areas (see [Security.md](Security.md) Section 14),
- is larger than about 400 lines.

Record decisions in an Architecture Decision Record (`docs/adr/NNNN-title.md`) when they affect the design long-term.

---

## 12. Rule Changes

The documents in `.rules/` are reviewed like code.
1. Open an issue or pull request describing the problem and the proposed change.
2. Code owners and at least one maintainer review it.
3. Keep the documents consistent: update links and related sections in other files.
4. Announce the change to the team.

When a rule conflicts with a tool configuration (`.editorconfig`, `clang-format`), fix the conflict. Do not ignore either one.

---

## 13. Legal

- Contributions are licensed under the project license (see `LICENSE`).
- Only submit code you wrote or are allowed to submit. Do not copy code with incompatible licenses (see [Security.md](Security.md) Section 11).
- If required, sign the Contributor License Agreement or add a `Signed-off-by` line (Developer Certificate of Origin, `git commit -s`).

---

## 14. Getting Help

- Build or test problems: check [BuildAndCI.md](BuildAndCI.md) and [Testing.md](Testing.md), then ask in the team channel or open an issue.
- Rule questions: ask the code owners of the relevant document.
- Security concerns: private channel only (see [Security.md](Security.md) Section 16).

---

## 15. Checklist for New Contributors

- [ ] Read this guide, `GitWorkflows.md`, and `CodingStyle.md`.
- [ ] Set up the repository (fork or clone, `upstream` remote, identity).
- [ ] Built and ran the tests successfully before changing code.
- [ ] Picked an issue and announced it.
- [ ] Created a correctly named lowercase branch from `develop`.
- [ ] Opened a pull request using the template and passed all checks.