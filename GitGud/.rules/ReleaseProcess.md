# Release Process

Release rules for any project in this repository, regardless of language or runtime. Covers versioning decisions, changelogs, packaging, NuGet feeds (public and internal), Windows code signing, support policy, and rollback.

Related documents: [APIDesign.md](APIDesign.md), [BuildAndCI.md](BuildAndCI.md), [GitWorkflows.md](GitWorkflows.md), [Security.md](Security.md), [Testing.md](Testing.md), [Agent.md](Agent.md).

---

## 1. Principles

1. **Releases are boring.** The process is scripted, repeatable, and rehearsed.
2. **Built by CI only.** Release artifacts never come from a developer machine.
3. **Immutable.** A published version is never changed, replaced, or deleted. Publish a new version instead.
4. **Human-approved.** A human triggers and approves every release (see [Agent.md](Agent.md)).
5. **Traceable.** Every artifact maps to a tag, a commit, a build, and a changelog entry.
6. **Verified before and after publishing.**

---

## 2. Roles

| Role | Responsibility |
|------|----------------|
| Release manager | Owns the release, runs the checklist, gives the final go |
| Maintainer | Reviews and merges release changes |
| Security reviewer | Signs off on security-relevant changes and signing configuration |
| Key custodian | Controls access to signing keys and publishing credentials |

- The same person should not be both the author of the release changes and the only approver.
- Each release has a named release manager.

---

## 3. Versioning Decisions

Versions follow SemVer as defined in [APIDesign.md](APIDesign.md) Section 1.

| Change since last release | Bump |
|---------------------------|------|
| Any breaking public API, ABI, behavior, or serialization change | MAJOR |
| New backward-compatible functionality, deprecations | MINOR |
| Bug fixes and security fixes without API change | PATCH |

Process:
1. Review the diff of the **API baseline** and the changelog "Removed/Changed" entries. The highest-impact change decides the bump.
2. If the API-contract tests report a break, the release is MAJOR or the change is reverted.
3. Record the decision and reasoning in the release pull request.
4. Pre-releases: `-alpha.N`, `-beta.N`, `-rc.N`. They carry no stability guarantee and are not marked as the latest stable version.
5. Version source: one source of truth (see [BuildAndCI.md](BuildAndCI.md) Section 7).
6. .NET Framework: `AssemblyVersion` changes only on MAJOR. `AssemblyFileVersion` and the informational version carry the full version.
7. Native libraries: embed the MAJOR version in the file name when the ABI breaks, and expose a version function.

---

## 4. Changelog

File: `CHANGELOG.md` in the repository root, following [Keep a Changelog](https://keepachangelog.com) conventions.
```
[1.2.0] - 2026-11-06
Added
•	CloneOptions.Depth for shallow clones. (#123)
Changed
•	Default timeout is now 60 seconds. (#130)
Deprecated
•	Clone(string, string, bool): use Clone(string, string, CloneOptions). Removal planned in 3.0.
Removed
Fixed
•	Crash when the path contains non-ASCII characters. (#141)
Security
•	Fixed argument injection in ... (CVE-XXXX-YYYY)
```


Rules:
- Sections: Added, Changed, Deprecated, Removed, Fixed, Security.
- Written for **users**, not developers. State the impact and the migration.
- Every user-visible change has an entry in the pull request that makes it.
- An `[Unreleased]` section collects entries between releases.
- Link versions to tag comparisons. Dates use ISO 8601.
- Security entries do not contain exploit details before users can update.

---

## 5. Release Checklist

### 5.1 Preparation (release branch)
- [ ] Release manager named, target date and version decided (Section 3).
- [ ] `release/x.y.z` created from `develop` (see [GitWorkflows.md](GitWorkflows.md)).
- [ ] Feature freeze: only fixes, docs, and version changes.
- [ ] `CHANGELOG.md` finalized, `[Unreleased]` moved to the new version.
- [ ] API baseline updated and reviewed. Deprecation messages name the correct removal version.
- [ ] Version number set in the single source of truth.
- [ ] Documentation and migration notes updated.
- [ ] Dependency, vulnerability, and license audit is clean. SBOM generated (see [Security.md](Security.md)).
- [ ] No open blocker issues. No skipped or quarantined tests without a recorded decision.

### 5.2 Verification
- [ ] CI pipeline green on the release branch, with **zero warnings and zero errors**.
- [ ] All supported targets built in Release: .NET Framework 4.8, modern .NET, all native architectures.
- [ ] Acceptance tests pass against the **packaged** artifacts, including the upgrade scenario (see [Testing.md](Testing.md)).
- [ ] Fuzz, performance, and sanitizer runs show no regressions.
- [ ] Reproducibility check: a second build produces identical hashes.
- [ ] Security review for security-relevant changes complete.

### 5.3 Publishing
- [ ] Release pull request approved by a human (two for MAJOR).
- [ ] Merge to `main`, create the annotated tag `vx.y.z` (see [GitWorkflows.md](GitWorkflows.md)).
- [ ] CI release stage builds from the tag, signs (Sections 9 and 10), and publishes after the approval gate.
- [ ] Packages pushed to the correct feeds (Section 7).
- [ ] Hashes, signatures, SBOM, and symbols published.
- [ ] Merge the release branch back into `develop`. Delete the release branch.

### 5.4 After release
- [ ] Install and smoke test from the real feed on a clean machine, for each supported runtime.
- [ ] Verify signatures (Section 10.4) and hashes.
- [ ] Publish release notes (taken from the changelog) and announce.
- [ ] Monitor issues and telemetry for 48 hours. Rollback criteria in Section 12.
- [ ] Close the milestone and record lessons learned.

---

## 6. Packaging

### 6.1 General
- Artifacts are built in **Release** configuration from a clean checkout of the tag.
- Every package contains: binaries, symbols (or a pointer to them), license, readme, third-party notices, and the version.
- No secrets, test files, build output, absolute paths, or personal data in packages (scan before publishing).
- Packages are deterministic and use source link.

### 6.2 NuGet packages (.nupkg)
- Package ID is a stable part of the API. Never rename it. Use a unique prefix owned by the team and reserve it on public feeds.
- Required metadata: `Id`, `Version`, `Authors`, `Description`, `License` (SPDX expression), `ProjectUrl`, `RepositoryUrl` and commit, `Readme`, `Copyright`, `PackageTags`.
- **Target frameworks:** `netstandard2.0` (covers .NET Framework 4.8 and modern .NET) and optional modern TFMs. The public surface is identical across them (see [APIDesign.md](APIDesign.md)).
- Dependencies use a minimum version range (`[1.2.0, )`) and no floating versions. Keep the dependency count minimal.
- Symbols: publish `.snupkg` (portable PDB) to the symbol server, or embed symbols for internal packages.
- Enable package validation (`EnablePackageValidation`, with the previous version as the baseline) to detect breaking changes.
- Native assets are placed under `runtimes/<rid>/native/` (for example `runtimes/win-x64/native`, `runtimes/win-x86/native`).
- On .NET Framework 4.8, add a `.targets` file in `build/` that copies native files to the output, since `runtimes/` is not resolved automatically.
- Mark packages as **development dependency** only for analyzers and build tools.
- Pre-release packages use SemVer suffixes. Stable and pre-release packages share the same ID.

### 6.3 Native artifacts (DLL, LIB, headers)
- Ship per architecture: x86, x64, and ARM64 where supported.
- Ship headers (`include/`) matching the binary, import libraries (`.lib`), and PDBs.
- ABI rules as in [APIDesign.md](APIDesign.md) Sections 8 and 9: file name carries the MAJOR version when the ABI breaks.
- Build with the hardening flags listed in [Security.md](Security.md) Section 7.
- Binaries carry version resources (file version, product version, company, copyright).

### 6.4 Other formats
- Installers (MSI, MSIX), zip archives, and container images follow the same rules: built by CI, signed, hashed, and listed in the release notes.

---

## 7. NuGet Feeds: Public and Internal

### 7.1 Where packages go

| Package type | Feed | Examples |
|--------------|------|----------|
| Public stable release | **Public** (nuget.org) | Release `1.2.0` of a public library |
| Public pre-release | Public feed (pre-release version) or internal feed first | `1.3.0-beta.1` |
| Internal-only library | **Internal** (Azure Artifacts, GitHub Packages, ProGet, Artifactory, or a file share for small teams) | Company-specific code |
| CI / pull request builds | Internal **CI feed** with short retention | `1.3.0-ci.57` |
| Nightly builds | Internal feed, nightly view | `1.3.0-nightly.20261106` |
| Symbols | Public symbol server or internal symbol feed | `.snupkg` |

Decision rule: **Is the code meant for external consumers and free of internal or confidential content?**
- Yes: publish to the public feed, after the checks in Section 7.2.
- No: internal feed only. Never publish internal packages to a public feed.

### 7.2 Public feed (nuget.org) rules
- **Irreversible:** a public version can be unlisted but never truly deleted. Treat every push as permanent.
- Prerequisites before pushing: license reviewed, no confidential or internal references (names, URLs, paths), no secrets, signed package (Section 8), SBOM available.
- Reserve the package ID prefix and enable two-factor authentication on the owning account. Prefer an organization account, not a personal one.
- Use a **scoped API key**: limited to specific packages, push only, short expiry (rotate at least yearly, immediately on any suspicion).
- The key is stored in the CI secret store and is available **only** to the release job after the approval gate.
- Humans never run `nuget push` for a release by hand.
- Pre-releases go to the public feed only when external feedback is wanted.
- Deprecate (not delete) a bad version: mark it deprecated with the reason and the recommended version, and **unlist** it. Security-affected versions are marked as vulnerable via an advisory.

### 7.3 Internal feed rules
- One feed per trust level. Suggested views: `ci` (short-lived), `prerelease`, `release`. Promote packages between views, **do not rebuild** them.
- Packages are immutable on the feed (no overwrite). Configure the feed to reject re-pushing an existing version.
- Access: read for all developers and CI. Write only for the CI identity. Delete/admin for maintainers.
- Retention: CI packages 30 days, nightly 90 days, pre-release until the stable release ships, **release packages permanent**.
- Internal and public packages with the same ID are forbidden. Reserve the ID on the public feed to prevent **dependency confusion** attacks.
- Use package source mapping so that each package ID resolves from exactly one intended feed.

```xml
<!-- nuget.config -->
<configuration>
	<packageSources> 
		<clear />
		<add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
		<add key="internal" value="https://feeds.example.com/internal/v3/index.json" />
	</packageSources>
	<packageSourceMapping>
		<packageSource key="internal">
			<package pattern="Company." />
		</packageSource>
		<packageSource key="nuget.org">
			<package pattern="" />
		</packageSource>
	</packageSourceMapping>
</configuration>
```

### 7.4 Consuming packages
- Only approved sources are listed in `nuget.config`, with `<clear />` first.
- Restore in locked mode with lock files in CI (see [BuildAndCI.md](BuildAndCI.md) Section 6).
- New dependencies require approval (see [Security.md](Security.md)).
- Credentials for internal feeds come from the credential provider or CI secrets, never from files in the repository.

### 7.5 Versioning on feeds
- Release versions: `MAJOR.MINOR.PATCH`.
- CI builds: `MAJOR.MINOR.PATCH-ci.<build>` (sorts below the release).
- Never reuse a version number across different content, on any feed.

### 7.6 Local testing of packages
- Test a package before publishing by installing it from a local folder feed into a sample consumer project, for each supported runtime.

---

## 8. Package and Artifact Signing (NuGet and general)

- **Sign NuGet packages** with a code signing certificate (`dotnet nuget sign` with a timestamp). nuget.org requires an RFC 3161 timestamp and accepts only certificates it has registered for the owner.
- Internal feeds: sign as well, and configure consumers to require signed packages (`signatureValidationMode`/`trustedSigners`) where practical.
- Sign **assemblies and native binaries before packing**, so the package contains signed files. Then sign the package.
- Publish SHA-256 hashes for all release artifacts. A signature file (or attestation/provenance) is attached to the release.
- Verify signatures after signing and after publishing (Section 10.4).

---

## 9. Windows Code Signing: Policy

### 9.1 What to sign

| Artifact | Sign | Method |
|----------|------|--------|
| `.dll`, `.exe` (managed and native) | Yes | Authenticode, with timestamp |
| `.msi`, `.msix`, `.appx` | Yes | Authenticode, with timestamp |
| PowerShell scripts (`.ps1`) shipped to users | Yes | Authenticode |
| Drivers (`.sys`, `.cat`) | Yes | Per Microsoft hardware program requirements |
| `.nupkg` | Yes | NuGet signing (Section 8) |
| Test assemblies, CI-only builds | No (or a test certificate) | Never with the production key |

### 9.2 Certificates
- Use a certificate from a trusted public CA for public releases. Use an internal CA only for internal software on managed machines.
- **EV (Extended Validation) or hardware-protected keys** are required. Industry rules require private keys to be stored on FIPS-validated hardware (HSM, hardware token) or in a cloud key service.
- Preferred: a **cloud signing service** (for example Azure Trusted Signing / Artifact Signing, Azure Key Vault with HSM, or a CA-provided cloud signing) so the key never leaves the service and no one can export it.
- **Never** store a private key (`.pfx`/`.p12`) in the repository, on a developer machine, or as a plain CI secret file.
- Separate certificates (or key identities) for **production** and **test/internal** signing. The test certificate is never trusted on user machines.
- Record: certificate subject, thumbprint, issuer, expiry, owner, and storage location in a secure inventory.

### 9.3 Key custody and access
- Key custodian(s) named (at least two, for continuity).
- Only the CI **release job** can use the signing identity, after the approval gate (Section 11). Use workload identity (OIDC) where available, not long-lived secrets.
- No signing from pull request builds, forks, or unreviewed branches.
- Signing requests are logged and the logs retained.
- Agents never access signing keys or credentials (see [Agent.md](Agent.md)).
- Offboarding: remove access immediately when a custodian leaves.

### 9.4 Certificate lifecycle
- Track expiry. Alert **90, 60, and 30 days** before. Plan renewal early. Do not let a certificate expire without a replacement in place.
- **Timestamp every signature** (RFC 3161, SHA-256). A timestamp keeps a signature valid after the certificate expires. Use at least two timestamp servers for fallback.
- Use SHA-256 or stronger for the file digest and the timestamp digest. Do not use SHA-1.
- On suspected key compromise: follow Section 9.5.
- Renewal can change the certificate subject or reputation (SmartScreen). Announce and test with a pre-release.

### 9.5 Compromise response
1. Stop all signing and revoke access immediately.
2. Ask the CA to **revoke** the certificate.
3. Determine the exposure window: which artifacts were signed in it.
4. Issue a new certificate and re-sign and re-release affected versions as new PATCH versions.
5. Notify users and publish an advisory (see [Security.md](Security.md) Section 16).
6. Record the cause and update these rules.

---

## 10. Windows Code Signing: Procedure

### 10.1 Order of operations in the release job
1. Build all targets in Release from the tag.
2. **Sign** every `.dll` and `.exe` (managed and native) with Authenticode plus timestamp.
3. Verify the signatures.
4. **Pack**: create the `.nupkg`, installers, and archives from the signed binaries.
5. **Sign** the installers and the `.nupkg`.
6. Verify again, compute hashes, generate the SBOM.
7. Pause for the **approval gate**.
8. Publish.

Never modify a file after it has been signed. Any change invalidates the signature.

### 10.2 Example commands

#### Authenticode (local tool with a hardware/cloud-backed certificate)
```
signtool sign /fd SHA256 /tr http://timestamp.example.com /td SHA256 /sha1 <thumbprint> /d "ProductName" /du https://example.com path\to\file.dll
```


#### Cloud signing via a dedicated tool 
(example: AzureSignTool or the Trusted Signing client)
**Use workload identity. Never pass a key or password on the command line.**
#### NuGet package signing
```
dotnet nuget sign package.nupkg --certificate-fingerprint <sha256-fingerprint> --timestamper http://timestamp.example.com
```

- Pass secrets through the CI secret store or identity federation, never as literal arguments (arguments are visible in process listings).
- Replace the timestamp URLs with the servers of your CA.

### 10.3 Sign everything shipped
- Include native DLLs of all architectures (x86, x64, ARM64) and third-party binaries you redistribute (if their license allows re-signing, otherwise keep their original signature).
- Check for unsigned files in the package before publishing: a CI step lists every PE file in the package and fails on any unsigned one.

### 10.4 Verification
```
signtool verify /pa /all /v path\to\file.dll Get-AuthenticodeSignature path\to\file.dll | Format-List dotnet nuget verify package.nupkg --all
```

Verify: signature valid, certificate chain trusted, timestamp present, expected subject and thumbprint, and SHA-256 digest algorithm.
- Run the verification again on the artifact **downloaded from the feed** after publishing.
- Test on a clean Windows machine: SmartScreen and Defender behavior, installer prompts show the correct publisher.

### 10.5 .NET Framework 4.8 notes
- Authenticode signing is independent of strong-name signing. **Strong-name signing is for identity only, not security** (see [Security.md](Security.md)). If used, the strong-name key is a separate key, and public/private handling follows the same custody rules (delay-signing where practical).
- Keep `AssemblyVersion` stable across MINOR and PATCH to avoid binding redirect issues (Section 3).

---

## 11. Approval Gates and Release Automation

- The release pipeline is triggered **only** by a signed tag on `main`.
- The pipeline has a **manual approval step** before signing and publishing, performed by the release manager plus one more maintainer (two approvers for MAJOR).
- Protected environment: signing and publishing secrets exist only in the `release` environment, restricted to `main` tags and named approvers.
- The pipeline is idempotent: if a step fails midway, rerunning must not publish a different artifact under the same version. If a package was already pushed, **bump the version**.
- All actions are logged and retained (who approved, what was signed, which hashes).
- Agents and bots may prepare the release pull request as a draft but never approve, tag, sign, or publish (see [Agent.md](Agent.md)).

---

## 12. Rollback and Recovery

Published versions cannot be removed or changed, so rollback means **moving users forward or back safely**.

| Situation | Action |
|-----------|--------|
| Bug found, no data/security risk | Fix in `hotfix/*`, release a PATCH. Mark the bad version as deprecated with the recommended version. |
| Serious defect (crash, data loss, regression) | Unlist the bad version immediately, release a PATCH with the fix (or re-release the previous code as a new PATCH). Announce. |
| Security vulnerability | Follow [Security.md](Security.md) Section 16: private fix, PATCH release, advisory, mark affected versions vulnerable. |
| Wrong content in a package (secret, confidential file) | Unlist, **revoke and rotate any exposed secret first**, request removal from the feed owner if required, release a corrected version, publish an advisory. |
| Signing problem (wrong or expired certificate) | Re-sign and release as a new PATCH. See Section 9.5 for compromise. |
| Broken release pipeline midway | Stop. Do not republish the same version with different content. Fix the pipeline and bump the version if anything was published. |

Rollback triggers (monitor for 48 hours after release): crash rate increase, failing consumer upgrades, security report, or a regression found in acceptance tests.

Rules:
- **Never** delete or move a tag, and never force-push `main`. Use a new tag.
- Rollback decisions are made by the release manager, and recorded in the changelog and an incident note.
- Consumers pin a previous version in their own project to roll back. Document this in the release notes.
- After any rollback, add a test or check that would have caught the problem.

---

## 13. Support and End-of-Life Policy

| Stage | Meaning |
|-------|---------|
| **Current** | Latest MAJOR.MINOR. Gets features, bug fixes, and security fixes |
| **Maintenance** | Previous MINOR within the current MAJOR: bug fixes for critical issues and security fixes |
| **Security-only** | Previous MAJOR for **12 months** after the next MAJOR ships: security fixes only |
| **End of life (EOL)** | No fixes. Announced in advance |

- Within a MAJOR, users are expected to upgrade to the latest MINOR. MINOR and PATCH upgrades are safe (see [APIDesign.md](APIDesign.md)).
- Each release states its support end date in the release notes and in a `SUPPORT.md` table.
- EOL is announced at least **6 months** in advance, in the changelog, release notes, and package deprecation message.
- Deprecated APIs live for at least one full MAJOR cycle before removal (see [APIDesign.md](APIDesign.md) Section 6).
- Dropping support for a runtime, OS, or architecture (for example .NET Framework 4.8) is a **MAJOR** change, announced one MAJOR earlier.
- Security fixes for supported lines are backported using `hotfix/*` branches from the matching tag (see [GitWorkflows.md](GitWorkflows.md)). Long-lived `support/x.y` branches may be created by maintainers for this.
- At EOL, mark the packages as deprecated on the feed with a pointer to the supported version. Do not delete them.

---

## 14. Documentation and Communication

- Release notes: generated from the changelog, with highlights, upgrade steps, deprecations, known issues, and the support dates.
- Breaking changes (MAJOR) include a **migration guide** with before/after code.
- Announce through the agreed channels (repository releases page, mailing list, internal chat).
- Keep an archive of all release notes and the release checklist results for each version.

---

## 15. Review Checklist

- [ ] Version bump matches the change type (SemVer), and the decision is recorded?
- [ ] Changelog complete, API baseline reviewed, deprecations correct?
- [ ] Build from the tag with zero warnings and zero errors, on all targets?
- [ ] Acceptance tests (incl. upgrade scenario) pass on the packaged artifacts?
- [ ] Packages contain only intended files, correct metadata, symbols, native assets in `runtimes/`?
- [ ] Correct feed chosen (public vs internal), source mapping and ID reservation in place?
- [ ] No secrets, internal references, or confidential content in artifacts?
- [ ] All binaries and packages signed with the production identity, timestamped, SHA-256, and verified?
- [ ] No unsigned PE files in the package?
- [ ] Signing identity and publishing keys used only by the approved release job?
- [ ] Approval gate completed by the required humans?
- [ ] Hashes, signatures, SBOM, and symbols published and retained?
- [ ] Post-release install and signature verification done on a clean machine?
- [ ] Support dates and EOL announcements updated?
- [ ] Rollback plan known, and monitoring in place for 48 hours?

---

## 16. Adding a New Language or Package Type

Add a section covering:
1. **Package format and registry** (public and internal), and where artifacts are placed.
2. **Metadata and versioning** requirements of that ecosystem.
3. **Signing** mechanism and key custody.
4. **Verification** commands.
5. **Rollback** options the registry allows (unlist, yank, deprecate).
6. **Dependency confusion protections** for that ecosystem.

Then add the items to the checklist in Section 15.