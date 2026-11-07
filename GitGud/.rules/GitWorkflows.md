# Git Workflows

How we work with Git in this repository.

- **Within a team:** Gitflow workflow (as described by Atlassian).
- **Across teams and external contributors:** Forking workflow.

Related documents: `Agent.md` (agents never commit, push, or merge), `APIDesign.md` (versioning), `CodingStyle.md`.

---

## 1. Overview

| Situation                                   | Workflow          | Who pushes where                                        |
|---------------------------------------------|-------------------|---------------------------------------------------------|
| Developer in the owning team                | Gitflow           | Branches directly in the team repository                |
| Developer from another team or outside      | Forking           | Own fork, then pull request to the upstream repository  |

Both workflows share the same rules for commits, reviews, and releases (Sections 5 to 8).

---

## 2. Gitflow (within one team)

### 2.1 Branches

| Branch      | Purpose                                         | Branches from | Merges into              | Lifetime  |
|-------------|-------------------------------------------------|---------------|--------------------------|-----------|
| `main`      | Released, production-ready code. Every commit is a release, tagged `vMAJOR.MINOR.PATCH`. | n/a | n/a               | permanent |
| `develop`   | Integration branch for the next release.        | `main`        | `main` (via release)     | permanent |
| `feature/*` | One feature or change.                          | `develop`     | `develop`                | short     |
| `release/*` | Stabilization of a release (version bump, docs, fixes only). | `develop` | `main` and `develop` | short     |
| `hotfix/*`  | Urgent fix of a released version.               | `main`        | `main` and `develop`     | very short|

```
gitGraph commit id: "v1.0.0" tag: "v1.0.0" 
	branch develop
	checkout develop commit id: "dev"
	branch feature/clone-options
	checkout feature/clone-options commit id: "feat"
	checkout develop
	merge feature/clone-options 
	branch release/1.1.0 
	checkout release/1.1.0 commit id: "bump 1.1.0"
	checkout main 
	merge release/1.1.0 tag: "v1.1.0"
	checkout develop
	merge release/1.1.0
```

### 2.2 Naming
- `feature/<issue-id>-<short-description>`, e.g. `feature/123-clone-options`
- `release/<MAJOR.MINOR.PATCH>`, e.g. `release/1.1.0`
- `hotfix/<MAJOR.MINOR.PATCH>`, e.g. `hotfix/1.0.1`
- Branch names must be **lowercase ASCII only**: letters `a-z`, digits, hyphens (`-`), and a single `/` after the prefix. No uppercase letters, spaces, underscores, dots, or non-ASCII characters.
- Reason: branch names are case-sensitive on Linux/macOS but not on Windows. Names differing only by case cause collisions and confusing checkouts.
- Pattern: `^(feature|release|hotfix)/[a-z0-9]+(-[a-z0-9]+)*$` (release and hotfix versions use dots, for example `release/1.1.0`, so allow `.` only there: `^(release|hotfix)/\d+\.\d+\.\d+$`).
- Enforce with a server-side branch name rule or a CI check. Pull requests from non-conforming branches are rejected.

### 2.3 Feature branches
1. Create from the latest `develop`.
2. Keep them small and short-lived (days, not weeks). Split large features.
3. Rebase onto `develop` regularly to stay current (only on your own unshared branch).
4. Open a pull request into `develop`. Merge only after review and green checks.
5. Delete the branch after merging.

### 2.4 Release branches
1. Create `release/x.y.z` from `develop` when the feature set is complete.
2. Only version bumps, changelog, documentation, and bug fixes go in. No new features.
3. Choose the version according to `APIDesign.md` (SemVer): breaking changes require MAJOR.
4. Verify that the API baseline and `CHANGELOG.md` are updated, and run the acceptance tests.
5. Merge into `main` with a merge commit, tag `vx.y.z`, then merge back into `develop`.
6. Delete the release branch.

### 2.5 Hotfix branches
1. Create from `main` (the tagged release that has the bug).
2. Fix with a regression test. PATCH bump only, with no API changes.
3. Merge into `main` and tag, then merge into `develop` (or the current `release/*` branch, if one exists).
4. Delete the hotfix branch.

### 2.6 Protected branches
- `main` and `develop` are protected: no direct pushes, no force pushes, no deletion.
- Pull requests require at least one human approval (two for public API, interop, or security-sensitive changes), passing CI, and resolved comments.
- The author cannot approve their own pull request.

---

## 3. Forking Workflow (across teams and external contributors)

Used when the contributor does not have write access to the upstream repository.

### 3.1 Setup
1. Fork the upstream repository to your own account or team namespace.
2. Clone **your fork**: `git clone <fork-url>`
3. Add the upstream remote:

```
git remote add upstream <upstream-url> git remote -v
```
- `origin` = your fork (you push here).
- `upstream` = the official repository (you only fetch from here).

### 3.2 Contribution flow
1. Sync your fork's `develop`:
```
git fetch upstream
git checkout develop
git merge --ff-only upstream/develop
git push origin develop
```

2. Create a topic branch from `develop`, with the same naming as Gitflow (`feature/...`):
```
git checkout -b feature/123-clone-options develop
```
3. Commit your work and push the branch to **your fork**:
```
git push -u origin feature/123-clone-options
```
4. Open a pull request from your fork's branch to the upstream `develop` branch.
5. Address review comments by pushing more commits to the same branch.
6. After the merge, delete the branch and sync your fork again.

### 3.3 Rules
- Pull requests target upstream `develop`. Only maintainers create `release/*` and `hotfix/*` branches.
- Exception: for a critical fix to a released version, the contributor opens an issue first. A maintainer decides on a hotfix and the target branch.
- Keep your branch up to date with upstream `develop` (rebase on your own branch, or merge if shared).
- Do not push to `upstream` directly, and do not change upstream protected branches.
- Maintainers do not push to contributors' fork branches without permission.
- Contributors follow the same coding, API, and testing rules as the owning team.
- Larger changes (public API, new dependencies, new components) start with an issue or design discussion before code is written.
- Topic branches in forks follow the same lowercase naming rules as Section 2.2.

---

## 4. Choosing the Workflow

```
flowchart TD A["Do you have write access to the upstream repository?"] 
	-->|"Yes, member of the owning team"| B["Gitflow: branch in the team repository"] A 
	-->|"No, other team or external"| C["Forking workflow: fork, branch, pull request"]
```
---

## 5. Commits

- One logical change per commit. The build and tests should pass at every commit where practical.
- Commit message format:
```
<type>(<scope>): <short summary, imperative, max 72 characters>
<body: what and why, wrapped at 100 characters>
Refs: #123
```
- Types: `feat`, `fix`, `docs`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`.
- Mark breaking changes with `BREAKING CHANGE:` in the body (only allowed in MAJOR release work, see `APIDesign.md`).
- Mention AI assistance where team policy requires it (see `Agent.md`). The human who commits is the author of record.
- No secrets, credentials, large binaries, or generated build output in commits. Keep `.gitignore` up to date.
- Never rewrite history on shared branches (`develop`, `main`, `release/*`, or any pushed branch others use).

---

## 6. Pull Requests

- Small and focused, with one logical change.
- Description includes: what and why, linked issue, testing done, public API impact, and any agent assistance.
- Requirements before merging:
  - [ ] Build passes with zero warnings and zero errors.
  - [ ] All tests pass (unit, integration, functional, API-contract).
  - [ ] Human review of the complete diff completed (see `Agent.md`).
  - [ ] `CHANGELOG.md` and the API baseline updated where relevant.
  - [ ] Branch is up to date with its target.
- Merge strategy:

| Merge                       | Strategy                                             |
|-----------------------------|------------------------------------------------------|
| `feature/*` into `develop`  | Squash merge (clean history), or rebase merge for well-structured commits |
| `release/*` into `main`     | Merge commit (`--no-ff`)                             |
| `hotfix/*` into `main`      | Merge commit (`--no-ff`)                             |
| `main` or hotfix into `develop` | Merge commit (`--no-ff`)                         |

---

## 7. Releases and Tags

- Releases are tagged on `main` only: annotated, signed where possible, named `vMAJOR.MINOR.PATCH` (`v2.1.0`).
- Pre-releases use SemVer suffixes (`v2.1.0-beta.1`) and are tagged on the `release/*` branch.
- Tags are immutable. Never move or delete a published tag. Publish a new version instead.
- Version numbers follow `APIDesign.md` Section 1.
- Only a human tags and publishes (see `Agent.md`).

---

## 8. Agents and Automation

- Agents never commit, push, merge, tag, or publish. A human does this after review (see `Agent.md`).
- Agents work in the working tree or on a feature branch, never on `main` or `develop`.
- Bots may only open draft pull requests, and use the forking or feature-branch flow like any other contributor.

---
## 9. Common Commands

### Start a feature (team member)

```
git checkout develop git pull --ff-only git checkout -b feature/123-clone-options

```
### Update a feature branch (own, unshared branch)
```
git fetch origin git rebase origin/develop
```
### Finish a release (maintainer)
```
git checkout main
git merge --no-ff release/1.1.0
git tag -a v1.1.0 -m "Release 1.1.0"
git checkout develop
git merge --no-ff release/1.1.0
git push origin main develop --follow-tags
git branch -d release/1.1.0
```
### Start a hotfix (maintainer)
```
git checkout main git checkout -b hotfix/1.0.1
```
- Prefer `git pull --ff-only` to avoid accidental merge commits.
- Prefer `git push --force-with-lease` over `--force`, and only on your own unshared branches.

---
## 10. Review Checklist

- [ ] Correct workflow for the contributor (Gitflow in-team, fork across teams)?
- [ ] Branch created from the right base ?
- [ ] Branch name is lowercase and matches the naming pattern?
- [ ] Pull request targets the right branch (`develop`, never `main` directly, except `release/*` and `hotfix/*` by maintainers)?
- [ ] Commit messages follow the format and reference an issue?
- [ ] No history rewritten on shared branches?
- [ ] No secrets or build output committed?
- [ ] Version, tag, changelog, and API baseline match the change type (SemVer)?
- [ ] Human review completed, and agent involvement disclosed?