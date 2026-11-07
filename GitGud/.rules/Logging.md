# Logging Guidelines

Logging rules for any project in this repository, regardless of language or runtime. This document covers log levels, structure, content, and what must never be logged. It does not prescribe specific logging libraries.

Related documents: [Security.md](Security.md), [ErrorHandling.md](ErrorHandling.md), [Performance.md](Performance.md), [CodingStyle.md](CodingStyle.md), [APIDesign.md](APIDesign.md), [Agent.md](Agent.md).

---

## 1. Principles

1. **Log for a reader.** Every message helps someone diagnose, audit, or monitor. If nobody would act on it, do not log it.
2. **Safe by default.** Logs never contain secrets or personal data (Section 6).
3. **Cheap when off.** Disabled logging costs almost nothing (Section 9).
4. **Structured.** Logs are machine-readable and searchable, with stable event names and fields.
5. **Log once, where handled.** Do not log and rethrow or return the same error at every layer (see [ErrorHandling.md](ErrorHandling.md) Section 6).
6. **Logging never changes behavior.** A logging failure must not fail the operation, throw, or block.
7. **Libraries are quiet.** A library emits diagnostics through an abstraction the host application controls. It does not decide where logs go, and it is silent by default.

---

## 2. Log Levels

| Level | Use for | Examples | Production default |
|-------|---------|----------|:------------------:|
| `Trace` | Very detailed flow, for deep debugging only | Entering a function, parsed field values (non-sensitive) | Off |
| `Debug` | Diagnostic detail useful to developers | Selected options, decisions taken, timings | Off |
| `Information` | Normal significant events | Service started, operation completed, configuration loaded | On |
| `Warning` | Unexpected but recovered or tolerated situations | Retry succeeded, fallback used, deprecated usage, approaching a limit | On |
| `Error` | An operation failed and needs attention | Unexpected failure, native call failed, `Internal` error | On |
| `Critical` | The process or a major component cannot continue | Data corruption, required resource lost | On |

Rules:
- Pick the level by **who must act**: `Error` and above means someone should look. If nobody needs to, it is not an `Error`.
- **Expected failures** (invalid input, not found, cancellation) are not `Error`. Use `Debug` or `Information`, so a consumer's mistakes do not flood the logs.
- `Cancelled` is never an error or warning.
- `Warning` must be actionable or noteworthy. A warning that is always present is noise: fix the cause or lower the level.
- Do not use `Information` inside hot paths or loops.
- Levels are configurable at runtime per component, without a rebuild.

---

## 3. Structure

### 3.1 Format
- Use **structured logging**: a constant message template plus named fields, not pre-formatted strings.
- Output format for machine consumption: JSON or key-value pairs. Human-readable console output is allowed for local development.
- Do not build messages with string concatenation or interpolation. Pass values as fields (this also avoids allocations, see Section 9).

```csharp
// Avoid
log("Cloned " + url + " in " + ms + " ms");
// Prefer (template and fields) 
log(Information, "RepositoryCloned", { durationMs: ms, depth: depth });
```

### 3.2 Standard fields

| Field | Meaning |
|-------|---------|
| `timestamp` | UTC, ISO 8601, with millisecond precision or better |
| `level` | One of the levels in Section 2 |
| `component` | Module or class emitting the event |
| `event` | Stable event name or ID (`RepositoryCloned`) |
| `message` | Short, constant, human-readable description |
| `operationId` / `correlationId` | Links all entries of one operation or request |
| `errorCode` | Stable error code when the entry reports an error (see [ErrorHandling.md](ErrorHandling.md)) |
| `durationMs` | For timed operations |
| `threadId` | Where useful for concurrency diagnosis |
| `version` | Application or library version, set once per log stream |

### 3.3 Event names and fields
- Event names are stable, PascalCase or snake_case (follow the language convention in [CodingStyle.md](CodingStyle.md)), and describe what happened, not where: `ProcessTimedOut`.
- Field names are consistent across the code base: the same concept has the same name and unit (`durationMs`, `sizeBytes`).
- Include units in field names. Use ISO 8601 for times and invariant formatting for numbers.
- Field **keys** are treated as a soft contract: dashboards and alerts depend on them. Do not rename them casually.

### 3.4 Messages
- Short, factual, in English, constant text. Start with what happened.
- One event per entry. No multi-line messages. Stack traces go into a dedicated field.
- No emojis, no decorative banners, no ALL CAPS.
- Do not rely on log text for program logic.

---

## 4. What to Log

Log:
- **Lifecycle:** start, stop, configuration summary (redacted), version.
- **Significant operations:** start and outcome of operations that matter, with duration and result code.
- **Errors** at the point where they are handled: code, category, operation, redacted details, and the exception type and stack trace for unexpected failures (see Section 7).
- **Warnings:** retries, fallbacks, degraded modes, limits reached, use of deprecated features.
- **Security-relevant events:** policy denials, rejected unsafe input, authentication failures, permission changes (Section 8).
- **Boundary events:** calls to external systems (process, network, native) with outcome and duration, without sensitive content.
- **Diagnostics for support:** enough context (IDs, sizes, counts, codes) to reproduce a problem.

Do not log:
- Every function call, every loop iteration, or large payloads.
- Data the system already records elsewhere (avoid duplication).
- Normal, high-volume success paths above `Debug`.

---

## 5. Context and Correlation

- Every operation that spans several steps, components, or threads has a **correlation ID** carried through all its log entries.
- Prefer scoped context (set once, attached automatically) over repeating fields on every call.
- Propagate the ID across process and native boundaries where the interface allows it.
- Use random identifiers for correlation. Never derive an ID from personal data.
- Include enough context in each entry to be understood alone: what, which resource (by safe ID), and the outcome.

---

## 6. What Must Never Be Logged

Applies at **every level**, including `Debug` and `Trace`, in every environment.

### 6.1 Secrets and credentials
- Passwords, passphrases, API keys, tokens, session IDs, cookies, authorization headers.
- Private keys, certificates with private material, signing keys, connection strings with credentials.
- URLs containing credentials (`https://user:token@host`), or query strings with tokens.
- Environment variables that may hold secrets (log names only, if at all).
- Command lines and request bodies that may contain any of the above (see [Security.md](Security.md) Section 4).

### 6.2 Personal and sensitive data
- Names, email addresses, phone numbers, postal addresses, government or financial identifiers, health data, precise location, IP addresses (unless required and approved).
- **File paths that contain personal data**, such as user profile directories (`C:\Users\<name>\...`) or home directories. Log a redacted or relative form, a hash, or a safe identifier instead.
- Free-form user content (documents, messages, commit messages, source code) unless required for the purpose and approved.
- Anything covered by privacy law (GDPR and similar) or by contracts, without a documented legal basis.

### 6.3 Other prohibited content
- Raw, unbounded external input or output (process output, network payloads, file contents).
- Internal security details that help an attacker: stack traces in consumer-visible errors, internal addresses, detailed configuration of security controls.
- Content that enables **log injection**: see Section 7.

### 6.4 When in doubt
- Do not log it. Log a safe identifier, a length, a count, or a hash instead.
- If an exception to this section is truly needed, get a security review (see [Security.md](Security.md) Section 14) and record the decision.

---

## 7. Safe Logging Practices

### 7.1 Redaction
- Redact **centrally**, not at each call site. Provide a single redaction facility that all logging passes through.
- Use allow-lists of loggable fields where possible rather than deny-lists of forbidden ones.
- Replace sensitive values with a fixed marker (`***`) that does not reveal length or content. Keep non-sensitive parts if useful (`https://***@host/repo`).
- Mark sensitive types and fields so they cannot be logged accidentally (a wrapper type that prints as `***`).
- Hash only when correlation is needed, with a keyed hash. Plain hashes of low-entropy data (emails, usernames) can be reversed.

### 7.2 Log injection
- Treat all external values as untrusted. Newlines, control characters, and delimiters can forge or hide log entries.
- Structured fields prevent most injection. For text output, encode or strip `\r`, `\n`, and other control characters.
- **Truncate** values to a maximum length and mark the truncation.

### 7.3 Exceptions
- Log exceptions only at the point where they are handled and converted to an error (see [ErrorHandling.md](ErrorHandling.md) Section 7).
- Log the exception type, a safe message, and the stack trace in a dedicated field. Exception messages may contain paths or values: pass them through redaction.
- Logs may keep diagnostic data that the returned error does not. The reverse (secrets in returned errors) is not allowed.

### 7.4 Failures of logging
- Logging code must not throw. Catch and drop internally if needed.
- Logging must not block the operation. Use buffered or asynchronous output with a bounded queue and a defined overflow policy (drop lower levels first, never block).
- Never log from within a lock, a finalizer, or a signal/callback context where it can deadlock.

---

## 8. Audit and Security Logging

- Security-relevant events go to a **separate category** or stream so they can be retained and protected separately.
- Record: who or what (by safe ID), what action, which resource (safe ID), outcome, time, and correlation ID.
- Audit records are **append-only** and protected from modification. Access is restricted.
- Do not log the sensitive data involved in the event, only the fact and the identifiers.
- Examples: authentication failure, authorization denial, rejected unsafe input, policy violation, signing or release actions, configuration changes.
- Review security logging when a change touches a trigger in [Security.md](Security.md) Section 14.

---

## 9. Performance

See [Performance.md](Performance.md).
- Check whether a level is enabled **before** computing expensive arguments or formatting.
- Use constant templates and typed fields so disabled calls do not allocate or format strings.
- No logging inside tight loops or hot paths above `Trace`. Aggregate instead (count, then log one summary).
- Keep entries small. Bound the size of every field.
- Rate-limit repetitive messages (log the first, then a count after an interval) so a failure loop cannot flood storage or hide other events.
- Logging output is asynchronous and bounded where possible (Section 7.4).

---

## 10. Libraries and Public APIs

- A library does **not** configure global logging, write to files, console, or the event log on its own, and does not add output by default.
- It exposes an **optional diagnostics hook** that the host application can connect to its own logging. The hook is off by default.
- The diagnostics surface is public API: event names, levels, and field keys are documented and follow the stability rules in [APIDesign.md](APIDesign.md). Adding events is additive. Renaming or repurposing them is a behavior change.
- The same redaction and content rules apply to everything a library emits.
- Do not require the consumer to adopt a specific logging framework.
- Native code reports diagnostics through a registered callback with a level and a UTF-8 message. Callbacks never throw, and the library keeps no pointer to the message after the call (see [APIDesign.md](APIDesign.md) Section 9).

---

## 11. Configuration, Storage, and Retention

- Log levels and outputs are **configuration**, not code. Changing the level needs no rebuild.
- Default levels: `Information` and above in production, `Debug` and above in development. `Trace` only for targeted, time-limited diagnosis.
- Raising the level in production for an investigation is temporary and recorded. Turn it back afterwards.
- Logs are stored with restricted access, encrypted at rest and in transit where they leave the machine.
- Retention is limited and defined per category (for example 30 days for general logs, longer for audit logs as required). Delete when no longer needed.
- Rotate and cap log size so logs cannot fill the disk.
- Logs are timestamped in UTC with synchronized clocks.
- Test and CI logs follow the same rules: no secrets, and secret masking enabled (see [BuildAndCI.md](BuildAndCI.md) Section 13).

---

## 12. Testing Logging

See [Testing.md](Testing.md).
- Test that **secrets and personal data are redacted**: feed known fake secrets and paths and assert they never appear in the output.
- Test that logging failures do not fail operations.
- Test that disabled levels do not evaluate expensive arguments or allocate on hot paths.
- Test the stability of event names and fields that are documented as part of the diagnostics surface.
- Test that control characters in input cannot forge log entries.

---

## 13. Agents and AI-Generated Code

See [Agent.md](Agent.md).
- Generated code is checked for logging of secrets, full paths, command lines, raw input, or whole objects.
- Agents do not add logging of sensitive data to debug a problem, and do not paste logs containing secrets or personal data into prompts or external tools.
- Agents must not remove redaction, lower levels of security events, or silence errors to make a test pass.
- Reviewers check every new log statement for level, content, and cost.

---

## 14. Review Checklist

- [ ] Each log entry has a purpose and the correct level?
- [ ] Expected failures and cancellation are not logged as errors?
- [ ] Constant message templates with named fields, no string-built messages?
- [ ] Standard fields, correlation ID, and stable event names used?
- [ ] No secrets, credentials, tokens, or credential-bearing URLs at any level?
- [ ] No personal data or paths containing personal data?
- [ ] No raw unbounded external input or output?
- [ ] Values truncated and control characters neutralized (no log injection)?
- [ ] Redaction handled centrally, not per call site?
- [ ] Logged once where the error is handled, not at every layer?
- [ ] Cheap when disabled: no allocation or formatting, no logging in hot loops?
- [ ] Logging cannot throw, block, or change behavior?
- [ ] Library output off by default, through a host-controlled hook?
- [ ] Security events go to the audit stream, without sensitive content?
- [ ] Tests cover redaction and logging failure behavior?

---

## 15. Adding a New Language

Add a section covering:
1. **How the diagnostics hook or logging abstraction is exposed** in that language.
2. **Mapping of levels** to the levels in Section 2.
3. **Redaction and structured fields** support.
4. **Tests** for redaction and for disabled-level cost.

Then add its items to the checklist in Section 14.