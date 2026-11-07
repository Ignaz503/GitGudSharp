# Code Review Guidelines

Code review rules for any project in this repository, regardless of language or runtime. This document is the single place for review process, etiquette, and the consolidated checklists.

Related documents: [Contributing.md](Contributing.md), [GitWorkflows.md](GitWorkflows.md), [CodingStyle.md](CodingStyle.md), [APIDesign.md](APIDesign.md), [Testing.md](Testing.md), [Security.md](Security.md), [BuildAndCI.md](BuildAndCI.md), [ReleaseProcess.md](ReleaseProcess.md), [Agent.md](Agent.md).

---

## 1. Principles

1. **Review is about the code, not the person.**
2. **Goals:** correctness, security, maintainability, shared knowledge, and consistent quality. Not personal preference.
3. **Every change is reviewed by a human** before it is merged. This includes agent-generated code (see [Agent.md](Agent.md)).
4. **Tools first, humans second.** Formatters, analyzers, and CI catch style and mechanical issues. Reviewers focus on design, logic, risk, and clarity.
5. **The author owns the change; the reviewer shares responsibility** for what they approve.
6. **Small and fast beats large and thorough-but-late.**

---

## 2. Roles

| Role | Responsibility |
|------|----------------|
| Author | Prepares a small, self-reviewed, green change with a clear description. Responds to feedback promptly. |
| Reviewer | Reads the full diff, understands it, gives clear and actionable feedback, and approves or requests changes. |
| Code owner | Required reviewer for owned paths (see `CODEOWNERS` in [Contributing.md](Contributing.md) Section 9). |
| Security reviewer | Required when a security trigger applies (Section 6). |
| Maintainer | Resolves disagreements, merges when all conditions are met. |

- The author never approves their own pull request.
- A reviewer who cannot explain the change does not approve it. Ask, or pull in someone who can.

---

## 3. Author Responsibilities

Before requesting review:
- [ ] Build has zero warnings and zero errors, all tests pass, formatter and analyzers clean.
- [ ] You reviewed your own diff line by line. No debug code, commented-out code, or unrelated changes.
- [ ] The description explains **what, why, how tested, and risk** (use the template in [Contributing.md](Contributing.md) Section 8.2).
- [ ] Public API impact, security impact, and AI assistance are disclosed.
- [ ] The change is small and focused (Section 4).
- [ ] Link the issue. Add screenshots, logs, or benchmarks where relevant.

During review:
- Respond to every comment, with a change or a reasoned answer.
- Do not take feedback personally, and do not argue by repetition. Escalate per Section 9.
- Push follow-up commits during review. Do not force-push over reviewed content unless asked (it hides what changed).
- Mark conversations resolved only when addressed. Let the reviewer resolve disputed ones.
- Do not merge with unresolved blocking comments.

---

## 4. Size Limits

| Size (changed lines, excluding generated files and lock files) | Guidance |
|-----------|----------|
| Up to 200 | Ideal |
| 200 to 400 | Acceptable |
| 400 to 800 | Needs justification. Split if possible. |
| Over 800 | Split. Exceptions: mechanical changes (renames, formatting, moves) in their own pull request with no behavior change. |

- One logical change per pull request. Do not mix refactoring, formatting, and behavior changes.
- Large features: land in small steps behind a feature switch or as unused internal code first, or stack pull requests.
- Reviewers may reject a pull request for being too large to review properly. This is not a judgment on the quality of the work.
- Large pull requests are reviewed by at least two reviewers.

---

## 5. Turnaround Expectations

| Event | Target |
|-------|--------|
| First response to a review request | 1 business day |
| Complete review of a normal pull request | 2 business days |
| Author response to feedback | 2 business days |
| Hotfix or broken-build fix | Same day (within 4 working hours) |
| Security fix | Same day, by the security reviewer or a named deputy |
| Stale pull request (no activity) | Reminder after 5 days, closed after 30 days (can be reopened) |

- Reviewing has priority over starting new work. A waiting pull request blocks someone.
- Reviewers who cannot meet the target say so and hand over to another reviewer.
- Each team defines a review rota or uses automatic assignment so reviews are spread fairly.
- Long review cycles (more than 3 rounds) are a signal to talk synchronously (call or pairing) instead of writing comments.

---

## 6. Who Must Approve What

Minimum approvals before merging (all also need green CI, see [BuildAndCI.md](BuildAndCI.md) Section 11):

| Change | Approvals | Required reviewers |
|--------|-----------|--------------------|
| Normal code change | 1 human | Any qualified reviewer, plus code owner for owned paths |
| Public API addition (additive) | 2 | API owner |
| Deprecation, or any API/ABI/behavior break | 2 | API owner, plus maintainer approval and MAJOR planning (see [APIDesign.md](APIDesign.md)) |
| Native code, P/Invoke, interop | 2 | Native owner and security reviewer |
| Security trigger (see [Security.md](Security.md) Section 14) | 2 | Security reviewer |
| New or updated dependency | 2 | Maintainer and security reviewer (license and vulnerability review) |
| Build, CI, signing, release, `nuget.config` | 2 | Build owners (and release manager or security for signing/feeds) |
| `.rules/` documents and `CODEOWNERS` | 2 | Maintainers and the document's code owner |
| Release pull request | 2 (MAJOR: 2 plus release manager) | Release manager (see [ReleaseProcess.md](ReleaseProcess.md)) |
| Documentation or comments only | 1 | Any maintainer or code owner |
| Agent-generated change in the high-risk areas above | Same as above, **line-by-line** review | Preferably a different second reviewer (see [Agent.md](Agent.md)) |

- Approvals are dismissed when new commits change reviewed content, where branch protection supports it.
- Reviewers must have the competence for the area. If nobody qualified is available, the change waits.
- Emergency exceptions: only for production incidents, approved by a maintainer, with a follow-up review within 2 business days.

---

## 7. What Blocks a Merge

A pull request **must not merge** when any of these is true:

- Build has any warning or error, or any required check fails.
- A test fails, is newly skipped, disabled, or weakened without a recorded decision.
- Coverage or API-contract gates fail (see [Testing.md](Testing.md)).
- Required approvals or code-owner reviews are missing.
- Unresolved blocking comments exist.
- The branch is not up to date with its target.
- A secret, credential, personal data, or build output is committed.
- A breaking public change is not in MAJOR release work (see [APIDesign.md](APIDesign.md)).
- A new warning suppression, analyzer disablement, or setting change lacks approval (see [BuildAndCI.md](BuildAndCI.md) Section 2).
- A new dependency or license is not approved.
- A security trigger applies and no security review exists.
- The description is missing, or AI assistance is undisclosed.
- The author or reviewer cannot explain a part of the change.

Blocking is for issues in the categories above and for real defects. Do not block on personal style (Section 8).

---

## 8. Reviewer Guidance

### 8.1 What to look for (in priority order)
1. **Correctness:** does it do what the issue asks, including edge cases, errors, cancellation, and concurrency?
2. **Design:** right place, right abstraction, no needless complexity. Does it fit the architecture?
3. **Security and safety:** input validation, resource handling, interop, secrets.
4. **Public API and compatibility:** additive, documented, obsolete process followed.
5. **Tests:** meaningful, test required behavior not just current behavior, cover failures and boundaries.
6. **Performance:** needless allocations, blocking calls, algorithmic cost in hot paths.
7. **Readability and maintainability:** names, size, nesting, comments explain why.
8. **Docs and changelog.**

Review the **tests and the description** too, not only the production code.

### 8.2 How to review
- Read the description and linked issue first, then the tests, then the code.
- Review the whole diff. For large changes, also check out the branch and run it.
- Ask questions instead of making accusations: "What happens if the path is empty?"
- Explain **why**, and suggest an alternative when you can.
- Distinguish severity with prefixes:

| Prefix | Meaning | Blocks? |
|--------|---------|:-------:|
| `blocker:` | Defect, risk, or rule violation that must be fixed | Yes |
| `issue:` | Should be fixed, discuss if you disagree | Usually |
| `suggestion:` | Improvement the author may take or decline | No |
| `nit:` | Minor, optional (a formatter should handle most) | No |
| `question:` | Needs an answer to understand | Until answered |
| `praise:` | Something done well | No |

- Do not discuss what a formatter or analyzer can fix. Fix the tool configuration instead.
- Do not request unrelated changes ("while you are here"). Open a follow-up issue.
- Approve when the change improves the code base and meets the rules, even if you would have written it differently. Perfection is not required, consistency and safety are.
- Approve only what you reviewed. Say which parts you did not review.

### 8.3 Etiquette
- Be kind, specific, and professional. Critique the code, never the person.
- Assume good intent and limited context.
- Use "we" and questions rather than "you" and commands.
- Avoid sarcasm, absolutes ("always", "never") when giving opinions, and rhetorical questions.
- Acknowledge good work.
- Keep disagreements technical and bring data (benchmarks, rules, documentation).
- Follow `CODE_OF_CONDUCT.md`. Report unacceptable behavior to the maintainers.
- New contributors and external forks receive extra patience and clear explanations. Point to the relevant rule document.

---

## 9. Disagreements and Escalation

1. Discuss in the pull request, with reasons and evidence.
2. If unresolved after two rounds, talk synchronously.
3. If still unresolved, a maintainer or the code owner of the area decides. The decision is recorded in the pull request.
4. If a rule itself is the problem, open a rule-change proposal (see [Contributing.md](Contributing.md) Section 12) instead of working around it.

The decision must follow the rule documents. Opinions do not override them.

---

## 10. Reviewing Agent-Generated Code

See [Agent.md](Agent.md) Section 3. Additional reviewer duties:
- Confirm every referenced API, type, and package exists and is available on .NET Framework 4.8 and modern .NET.
- Look for invented behavior, weakened validation, removed checks, deleted or softened tests, and scope creep.
- Check that tests assert required behavior, not whatever the generated code happens to do.
- Verify the author understands the code. "The agent wrote it" is not an answer to a review question.
- Check the pull request discloses AI assistance and the human author takes responsibility.

---

## 11. Review Checklists

The checklists from the individual rule documents are consolidated here. Reviewers use the relevant parts. Authors use them before requesting review.

### 11.1 General
- [ ] Does the change do what was asked, and nothing more?
- [ ] Description clear, issue linked, and the change small and focused?
- [ ] Zero warnings and zero errors, all required checks green?
- [ ] No secrets, personal data, build output, or unrelated edits?
- [ ] Docs, changelog (`[Unreleased]`), and comments updated?

### 11.2 Design and code style ([CodingStyle.md](CodingStyle.md))
- [ ] Functions within complexity limits (length, cyclomatic complexity, parameters)?
- [ ] Nesting depth 3 or less, guard clauses used for early exit?
- [ ] Recursion bounded and documented, or replaced with iteration?
- [ ] Names follow the language convention and express intent?
- [ ] No magic numbers or strings?
- [ ] Errors returned as values for expected failures, no exceptions for control flow?
- [ ] Resources owned and released (`using`, `SafeHandle`, RAII)?
- [ ] No needless allocations or copies in hot paths?
- [ ] Async code supports cancellation and does not block?
- [ ] Public members documented?

### 11.3 Public API ([APIDesign.md](APIDesign.md))
- [ ] Does the member need to be public?
- [ ] Change is additive only? No parameters added, removed, or reordered?
- [ ] New optional inputs in an options object with safe defaults?
- [ ] Replaced API marked obsolete with replacement, removal version, and changelog entry?
- [ ] API baseline updated and the diff reviewed?
- [ ] Native structs versioned or size-prefixed and unchanged in layout?
- [ ] Version bump matches the change type?
- [ ] No third-party types exposed, and `std::` types (or other implementation types) not exposed in exported signatures?

### 11.4 Tests ([Testing.md](Testing.md))
- [ ] Right level in the test pyramid?
- [ ] Names follow the pattern, one behavior per test, Arrange/Act/Assert?
- [ ] Deterministic (no time, network, order, or global state dependence)?
- [ ] Defaults, boundaries, and every documented error covered?
- [ ] Regression test for each bug fix?
- [ ] Compatibility tests for obsolete and old call patterns still pass?
- [ ] Security tests and fuzz targets for untrusted input?
- [ ] Coverage gates met, nothing skipped or disabled without an issue?
- [ ] Tests assert required behavior rather than current behavior?

### 11.5 Security ([Security.md](Security.md))
- [ ] Inputs validated with allow-lists and explicit limits?
- [ ] No shell, arguments passed as a list, untrusted values cannot become options or configuration?
- [ ] Paths canonicalized and checked for containment?
- [ ] No secrets in code, logs, errors, command lines, or tests?
- [ ] Timeouts, cancellation, and output size limits in place?
- [ ] Parsers bounded, malformed input returns errors?
- [ ] Native sizes checked for overflow, handles released, no dangling pointers?
- [ ] No new dependency, or approved with license and vulnerability review?
- [ ] Safe defaults, unsafe behavior opt-in and documented?

### 11.6 Build, CI, and release ([BuildAndCI.md](BuildAndCI.md), [ReleaseProcess.md](ReleaseProcess.md))
- [ ] No new suppressions, or each is narrow, justified, and references an issue?
- [ ] Warning, analyzer, and formatter settings unchanged, or approved by code owners?
- [ ] Tool, SDK, and dependency versions pinned, lock files updated?
- [ ] Builds on every supported runtime and architecture?
- [ ] Packaging, feeds, and signing configuration unchanged, or approved by build owners and security?

### 11.7 Git ([GitWorkflows.md](GitWorkflows.md))
- [ ] Branch name lowercase and matches the naming pattern, created from the right base?
- [ ] Pull request targets the right branch?
- [ ] Commit messages follow the format and reference an issue?
- [ ] No history rewritten on shared branches?

### 11.8 Agent involvement ([Agent.md](Agent.md))
- [ ] AI assistance disclosed in the pull request?
- [ ] All referenced APIs, types, and packages are real and available?
- [ ] No removed or weakened tests, checks, or error handling?
- [ ] The human author can explain and take responsibility for all of it?

---

## 12. Metrics and Improvement

Track, and review quarterly as a team (to improve the process, never to rank individuals):
- Time to first review and time to merge.
- Pull request size distribution.
- Number of review rounds.
- Defects found after merge that review should have caught.
- Rate of reverts of merged changes.

Update this document when the data shows a rule is not working. Use post-incident findings the same way (see [Agent.md](Agent.md) Section 10 and [Security.md](Security.md) Section 16.3).

---

## 13. Adding to This Document

When a new language, component, or rule document is added:
1. Add its checklist items to Section 11.
2. Add its approval requirements to Section 6.
3. Add any new merge blockers to Section 7.