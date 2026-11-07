# Documentation Guidelines

Documentation rules for any project in this repository, regardless of language or runtime. Covers where docs live, how they are written and generated (DocFX for .NET, Doxygen for other languages), templates for README and changelog, and Architecture Decision Records (ADRs).

Related documents: [CodingStyle.md](CodingStyle.md), [APIDesign.md](APIDesign.md), [BuildAndCI.md](BuildAndCI.md), [ReleaseProcess.md](ReleaseProcess.md), [CodeReview.md](CodeReview.md), [Contributing.md](Contributing.md), [Agent.md](Agent.md).

---

## 1. Principles

1. **Docs are part of the change.** A change is not done until its documentation is updated.
2. **Docs live next to the code** and are versioned with it.
3. **Generated where possible.** API reference comes from the source, so it cannot drift.
4. **Written for the reader.** State who the document is for and what they can do after reading it.
5. **One source of truth.** Link instead of copy.
6. **Documentation builds are checked.** Broken links and doc warnings fail the build.
7. **Accurate or removed.** Outdated docs are bugs.

---

## 2. Where Documentation Lives

| Content | Location | Format | Audience |
|---------|----------|--------|----------|
| Project overview, quick start | `README.md` (root, and one per project/package) | Markdown | Everyone |
| How to contribute | `CONTRIBUTING.md` (root pointer) and `.rules/Contributing.md` | Markdown | Contributors |
| Team rules and standards | `.rules/*.md` | Markdown | Contributors, agents |
| API reference | Generated from source comments | DocFX (.NET), Doxygen (others) | Library consumers |
| Concepts, guides, tutorials | `docs/` | Markdown | Consumers |
| Architecture and design | `docs/architecture/` | Markdown + diagrams | Maintainers |
| Architecture Decision Records | `docs/adr/` | Markdown | Maintainers |
| Release history | `CHANGELOG.md` | Markdown | Consumers |
| Support and EOL dates | `SUPPORT.md` | Markdown | Consumers |
| Security policy | `SECURITY.md` (root) | Markdown | Researchers, consumers |
| Code of conduct | `CODE_OF_CONDUCT.md` (root) | Markdown | Everyone |
| License and notices | `LICENSE`, `THIRD-PARTY-NOTICES.md` | Text/Markdown | Everyone |
| Code-level explanations | Source comments | Comments | Maintainers |

### 2.1 Repository layout

```
/ README.md 
  CHANGELOG.md
  CONTRIBUTING.md
  SECURITY.md
  SUPPORT.md
  LICENSE
  .rules/                    team rules
  docs/
	docfx.json               DocFX configuration (.NET)
	Doxyfile                 Doxygen configuration (C/C++ and others)
	toc.yml                  table of contents
	index.md
  guides/                  how-to guides, tutorials
  architecture/            overview, diagrams
  adr/                     decision records
  api/                     generated output
  source (not committed, see 5.3)
  images/
```

### 2.2 Rules
- Use Markdown (CommonMark/GitHub-flavored) for all prose documents.
- File names are lowercase with hyphens in `docs/` (`getting-started.md`). Rule documents keep their PascalCase names.
- Diagrams are text-based (Mermaid, PlantUML) so they can be diffed and reviewed. Binary images need alt text.
- Do not store documentation in wikis, chat, or external tools as the only source. If used, they link to the repository and are not authoritative.
- Use relative links between documents.

---

## 3. How Documentation Is Created

### 3.1 Workflow
1. **Write** the documentation in the same pull request as the code: XML/Doxygen comments for the API, Markdown for guides, changelog entry, ADR if a design decision was made.
2. **Build locally** with the documentation tool (Section 3.3) and fix all warnings.
3. **Review** it with the code (see [CodeReview.md](CodeReview.md)). Reviewers check accuracy, clarity, and examples.
4. **CI builds the docs** on every pull request. Warnings and broken links fail the build (see [BuildAndCI.md](BuildAndCI.md)).
5. **CI publishes** the docs for each release from the release tag. Docs are versioned per MAJOR.MINOR.
6. **Update on change.** Any change to behavior, defaults, errors, or API updates the affected docs in the same pull request.

### 3.2 Tool selection

| Language / component | Tool | Input | Output |
|----------------------|------|-------|--------|
| C# / .NET (.NET Framework 4.8, modern .NET) | **DocFX** | XML documentation comments (`///`), Markdown | Static website |
| C / C++ | **Doxygen** | Doxygen comments in headers, Markdown | HTML (and optionally XML/PDF) |
| Other languages | **Doxygen** if supported, otherwise the language's standard tool | Source comments | HTML |
| Guides, concepts, ADRs | DocFX (combined site) | Markdown | Static website |

- Prefer these tools. Other tools need a recorded decision (see ADRs, Section 8).
- DocFX is the primary site generator. The Doxygen output for native APIs is published beside it and linked from the DocFX table of contents, so consumers have one entry point.
- Tool versions are pinned (see [BuildAndCI.md](BuildAndCI.md) Section 6).

### 3.3 DocFX (.NET)

Setup:
- Install DocFX as a pinned local tool (`dotnet tool install docfx --version <x.y.z>` through a tool manifest `.config/dotnet-tools.json`) so every developer and CI use the same version.
- Enable XML documentation generation in the projects (`<GenerateDocumentationFile>true</GenerateDocumentationFile>`, in `Directory.Build.props`).
- Missing documentation on public members is a warning (CS1591), which fails the build (zero warnings rule).

Configuration (`docs/docfx.json`, abbreviated):
```
{ 
	"metadata": [ { 
			"src": [ { 
				 "src": "../src",
				 "files": [ "/*.csproj" ]
			} ],
			"dest": "api",
			"properties": {
				"TargetFramework": "netstandard2.0"
			} 
		} ], 
	"build": { 
		"content": [ 
			{ "files": [ "api/.yml", "api/index.md" ] }, 
			{ "files": [ "guides/.md", "architecture/.md", "adr/.md", "toc.yml", "*.md" ] } 
		], 
		"resource": [ { "files": [ "images/" ] } ], 
		"output": "_site", 
		"template": [ "default" ], 
		"warningsAsErrors": true 
	}
}
```

Commands:
```
dotnet tool restore            # restore the pinned local tools
dotnet docfx docs/docfx.json            # metadata + build
dotnet docfx docs/docfx.json --serve    # preview locally
```

Rules:
- Generate API metadata from the **public surface of the `netstandard2.0` target**, which is identical across targets (see [APIDesign.md](APIDesign.md)).
- Use `warningsAsErrors` so broken `<see cref>`, invalid XML, and unresolved links fail the build.
- Add conceptual articles and examples under `docs/guides/`. Reference API with `xref:` links so they stay valid when types move.
- Generated output (`_site/`, `docs/api/*.yml`) is **not committed**. CI builds it.

### 3.4 Doxygen (C, C++, and others)

Setup:
- Pin the Doxygen version in CI and in the developer setup instructions. Use Graphviz only if diagrams are enabled.
- Keep one `Doxyfile` under `docs/`, reviewed like code.

Required settings (excerpt):
```
PROJECT_NAME           = "<Project>"
PROJECT_NUMBER         = <version from the single source of truth>
INPUT                  = ../include ../src/native README.md 
RECURSIVE              = YES 
EXTRACT_ALL            = NO          # undocumented public items must be reported 
EXTRACT_PRIVATE        = NO 
WARN_IF_UNDOCUMENTED   = YES 
WARN_IF_DOC_ERROR      = YES 
WARN_NO_PARAMDOC       = YES 
WARN_AS_ERROR          = FAIL_ON_WARNINGS 
GENERATE_HTML          = YES 
GENERATE_LATEX         = NO 
MARKDOWN_SUPPORT       = YES 
EXCLUDE_SYMBOLS        = ::detail  # internal namespaces are not documented

```
Commands:
```
doxygen docs/Doxyfile

```

Rules:
- Document **exported (public) headers only**. Internal and `detail` code is excluded.
- `WARN_AS_ERROR` makes undocumented parameters or invalid commands fail the build.
- Output is not committed. CI builds and publishes it, together with the DocFX site.
- Link the generated native reference from the DocFX site (`docs/toc.yml`).

### 3.5 Checks in CI
- Docs build with zero warnings.
- Markdown lint and link check (relative and external links, with retries and an allow-list for flaky hosts).
- Code samples in docs compile where practical (extract them into a sample project, or reference snippets from tested sample files instead of pasting code).
- Spell check with a project dictionary (optional but recommended).
- Documentation build artifact is retained as defined in [BuildAndCI.md](BuildAndCI.md) Section 10.

### 3.6 Publishing
- Build from the release tag, so the docs match the released version.
- Publish to a static host (for example GitHub Pages or an internal web server). Keep each MAJOR.MINOR version available (`/v1.2/`) and a `/latest/` alias for the newest stable release.
- Pre-release or `develop` docs are published to a separate `/next/` location, clearly marked as unstable.
- Documentation for unsupported versions stays available with an end-of-life notice (see [ReleaseProcess.md](ReleaseProcess.md) Section 13).
- Do not publish internal or confidential documentation to public sites.

---

## 4. Code Documentation Standards

### 4.1 General
- Every **public** type and member is documented: summary, parameters, return value, error behavior, and thread safety where relevant.
- Describe **behavior and contract**, not implementation. State preconditions, defaults, units, ranges, ownership, and the error codes returned.
- First sentence is a short summary. It is shown in lists and IntelliSense.
- Document the **why** for non-obvious internal code. Do not restate the code.
- Documentation of public API is part of the API contract (see [APIDesign.md](APIDesign.md)). Behavior described there cannot change in a MINOR or PATCH.
- Mark deprecations in the docs with the replacement and the removal version.
- Include a short example for non-trivial APIs.
- No commented-out code. No `TODO` without an issue reference.

### 4.2 C# XML documentation
```xml
/// <summary>
/// Clones a repository into a new directory.
/// </summary> 
/// <param name="url">The source URL. Must use an allowed scheme (see remarks).</param>
/// <param name="path">The target directory. Must not exist or must be empty.</param>
/// <param name="options">Optional settings. <see langword="null"/> uses <see cref="CloneOptions"/> defaults.</param>
/// <param name="cancellationToken">Token to cancel the operation.</param>
/// <returns>
/// A successful <see cref="Result"/>, or an error with one of the codes listed in the remarks.
/// </returns>
/// <remarks>
/// <para>Thread safety: instance members are not thread safe.</para>
/// <para>Error codes: <c>InvalidArgument</c>, <c>PathExists</c>, <c>Cancelled</c>.</para>
/// </remarks>
/// <example>
/// <code>
/// var result = await client.CloneAsync(url, path);
/// </code>
/// </example>
public Task<Result> CloneAsync(string url, string path, CloneOptions? options = null, CancellationToken cancellationToken = default)
```

Rules:
- Use `<summary>`, `<param>`, `<returns>`, `<remarks>`, `<example>`, `<exception>` (only for exceptions that are really thrown), and `<see cref>`/`<seealso>` for links.
- Use `<inheritdoc/>` for overrides and interface implementations. Add to it only when behavior differs.
- Properties: "Gets or sets ..." and state the default value.
- Booleans: "`true` if ...; otherwise `false`."
- Validate XML with the compiler: `CS1591` (missing) and `CS1570`/`CS1572`/`CS1573` (malformed or mismatched) are build errors.
- Use `[Obsolete]` together with a `<remarks>` note naming the replacement.

### 4.3 C and C++ Doxygen comments
```
/**
*	@brief Clones a repository into a new directory.
*	@param[in]  url      UTF-8, NUL-terminated source URL. Must not be NULL.
*	@param[in]  path     UTF-8, NUL-terminated target directory. Must not be NULL.
*	@param[in]  options  Optional settings, may be NULL for defaults.
*	                 `struct_size` must be set.
*	@return GG_OK on success, otherwise an error code (see @ref gg_result).
*	@note Thread safety: may be called concurrently on different paths.
*	@note Ownership: the caller keeps ownership of all arguments.
*	@warning Never throws. All errors are returned as codes.
*	@code
*	gg_clone(url, path, NULL);
*	@endcode / GG_API gg_result gg_clone(const char url, const char* path, const gg_clone_options* options);
```

Rules:
- Use `/** ... */` with `@`-commands consistently (`@brief`, `@param[in/out]`, `@return`, `@note`, `@warning`, `@code`, `@deprecated`, `@since`).
- Document **ownership** (who allocates and frees), **nullability**, **encoding**, **thread safety**, and **error codes** for every exported function.
- Use `@since 1.2` for additions and `@deprecated` for deprecations (with the replacement and removal version).
- Document struct fields, including reserved fields ("must be zero").
- Place documentation in the header, not in the `.cpp` file.

### 4.4 Other languages
Use the language's standard doc comment format, generated with Doxygen where it is supported. Follow the same content rules as Section 4.1.

---

## 5. Prose Documentation Standards

### 5.1 Style
- Clear, short sentences. Active voice. Present tense.
- One idea per paragraph. Use headings, lists, and tables to structure.
- Define terms on first use, or link to a glossary.
- Use consistent terminology. The same thing has one name.
- Code samples are complete, correct, minimal, and tested. State the expected output where useful.
- Specify language on all code blocks.
- Avoid time-relative words ("recently", "new") and marketing language.
- Do not document internal-only details in consumer documentation.
- Examples never contain real secrets, real personal data, or internal hostnames.

### 5.2 Document types (use the right one)

| Type | Purpose | Example |
|------|---------|---------|
| Tutorial | Learn by doing, step by step | "Your first clone" |
| How-to guide | Solve a specific task | "Clone with a depth limit" |
| Reference | Complete, factual description | Generated API reference, error code list |
| Explanation | Why it works this way | Architecture overview, design rationale |

Do not mix types in one document.

### 5.3 Header block for rule and design documents
Each document begins with a title, a one-paragraph scope, and a link to related documents. Rule documents in `.rules/` additionally carry an owner and a last-reviewed date in the repository index (`.rules/README.md`).

---

## 6. README Template

Every repository, and every package or project that is published, has a `README.md`.
```
# <Project name>
<One or two sentences: what it is and who it is for.>

link LICENSE
## Features
-	<Key capability>
-	<Key capability>
## Supported platforms

| Runtime | Versions |
|------|---------|
| .NET Framework | 4.8 |
| .NET | <supported versions> |
| Native | <OS and architectures> |

## Installation
dotnet add package <PackageId>

## Quick start
// Smallest complete example that works

## Documentation

Follows Semantic Versioning. See SUPPORT.md for supported versions.

## Contributing
See CONTRIBUTING.md. Report vulnerabilities as described in SECURITY.md.

## License
<License name>. See LICENSE.
```


Rules:
- The quick start must work when copied. It is tested (compiled in CI where practical).
- NuGet package READMEs avoid relative links that break on nuget.org. Use absolute URLs there.
- Keep it short. Detailed content goes to `docs/`.

---

## 7. Changelog

- File: `CHANGELOG.md`, following Keep a Changelog.
- The format, sections, and release rules are defined in [ReleaseProcess.md](ReleaseProcess.md) Section 4. This document only requires:
  - Every user-visible change has an entry in the pull request that makes it, under `[Unreleased]`.
  - Entries are written for users and name the impact and migration.
  - Docs and changelog agree on deprecations and removal versions.

Template:
```
# Changelog
All notable changes to this project are documented here. The format follows Keep a Changelog, and the project follows Semantic Versioning.
[Unreleased]
## Added
## Changed
## Deprecated
## Removed
## Fixed
## Security
[1.0.0] - YYYY-MM-DD
## Added
-	Initial release.
[Unreleased]: <compare-url>/v1.0.0...HEAD
[1.0.0]: <compare-url>/releases/tag/v1.0.0
```

---

## 8. Architecture Decision Records (ADRs)

An ADR records **one significant decision**, its context, and its consequences, so future maintainers understand why.

### 8.1 When to write one
Write an ADR when a decision:
- affects the public API, ABI, or serialized formats,
- adds or removes a dependency, language, target, or architecture,
- chooses between significant alternatives (design, tools, protocols),
- changes build, CI, signing, or release structure,
- accepts a security risk or deviates from a rule document,
- would be expensive to reverse.

Not for small, local, easily reversed choices.

### 8.2 Rules
- Location: `docs/adr/`. File name: `NNNN-short-title.md` (zero-padded sequence, lowercase, hyphens), for example `0007-use-docfx-for-dotnet-docs.md`.
- One decision per ADR. Keep it to one or two pages.
- ADRs are **immutable once accepted**. To change a decision, write a new ADR that **supersedes** the old one, and update the old one's status line only.
- Created in a pull request and reviewed like code. The decision is accepted when the pull request merges.
- Keep an index: `docs/adr/README.md` (a list of ADRs with status).
- Link ADRs from code comments, rule documents, and pull requests where they explain a non-obvious choice.

### 8.3 Statuses

| Status | Meaning |
|--------|---------|
| Proposed | Under discussion in a pull request |
| Accepted | Decided, in effect |
| Rejected | Considered and declined (keep for history) |
| Deprecated | No longer relevant |
| Superseded by NNNN | Replaced by a later ADR |

### 8.4 Template
```
# NNNN. <Short title of the decision>
-	Status: Proposed | Accepted | Rejected | Deprecated | Superseded by NNNN
-	Date: YYYY-MM-DD
-	Deciders: <names or teams>
-	Related: <issues, pull requests, other ADRs, rule documents>
## Context
<The problem, the forces at play, constraints (compatibility with .NET Framework 4.8, zero-warnings rule, dependency policy, etc.). Facts, not opinions.>
## Options considered
1.	<Option A>: <summary>. Pros: ... Cons: ...
2.	<Option B>: <summary>. Pros: ... Cons: ...
3.	<Do nothing>: <consequences>.
## Decision
<The chosen option, stated in active voice: "We will ...", and the main reasons.>
## Consequences
-	Positive: <benefits>
-	Negative: <costs, risks, trade-offs accepted>
-	Follow-up: <tasks, migration, documentation, owner>
## Security and compatibility impact
<Public API/ABI impact, security considerations, migration steps. "None" if none.>
```

### 8.5 Good ADR habits
- State the problem before the solution.
- Record the alternatives you rejected and why. This is the most valuable part.
- Be honest about the downsides.
- Include the date and deciders, and revisit when the context changes.

---

## 9. Architecture Documentation

- `docs/architecture/overview.md` describes: purpose, main components, dependency direction, trust boundaries (see [Security.md](Security.md)), public vs internal surface, and how the managed and native parts interact.
- Use Mermaid diagrams for component, sequence, and flow views. Keep each diagram focused.
- Keep the architecture documents short, current, and linked to ADRs for the reasons behind the structure.
- Review them when a pull request changes component boundaries.

---

## 10. Maintenance and Ownership

- Each document has an owner (a team in `CODEOWNERS`).
- Documentation is reviewed as part of normal code review (see [CodeReview.md](CodeReview.md)): accuracy, clarity, examples, links.
- Review all guides and the architecture overview **before each MAJOR or MINOR release** and fix what is outdated.
- Broken-link and doc-build failures are treated as build failures and fixed immediately.
- Remove or archive documentation for removed features. Keep docs for supported versions only, plus archived EOL versions.
- Issues labeled `documentation` are tracked like any other work.

---

## 11. Agents and AI-Generated Documentation

See [Agent.md](Agent.md).
- Generated documentation is reviewed by a human, as carefully as code.
- Check that described behavior, parameters, defaults, error codes, and examples are real. Agents can invent APIs and behavior.
- Do not paste secrets, internal data, or confidential designs into external tools to generate docs.
- Agents must not silently rewrite existing documentation beyond the task's scope.

---

## 12. Review Checklist

- [ ] Public types and members documented (summary, parameters, returns, errors, thread safety)?
- [ ] Documentation describes behavior and contract, and matches the code?
- [ ] Examples compile or are tested, and contain no secrets?
- [ ] Docs build with DocFX (.NET) or Doxygen (native) with zero warnings?
- [ ] Links, cross-references, and diagrams are valid?
- [ ] Guides and README updated for changed behavior?
- [ ] `CHANGELOG.md` `[Unreleased]` updated for user-visible changes?
- [ ] Deprecations documented with replacement and removal version?
- [ ] ADR written for significant decisions, with alternatives and consequences?
- [ ] Generated output not committed, and published from the release tag?
- [ ] Owner assigned (CODEOWNERS), and AI assistance reviewed?

---

## 13. Adding a New Language

Add a section covering:
1. **Doc comment format** of the language, with an example.
2. **Generator tool** (prefer Doxygen if it supports the language), pinned version, and the commands.
3. **Strict settings** so missing or malformed documentation fails the build.
4. **Where the generated output is published** and how it is linked from the main site.

Then add its items to the checklist in Section 12.