# AGENTS.md

## Role

Act as a careful local coding assistant for this repository.

Prefer small, focused changes. Keep architecture decisions visible. Do not hide broad refactorings behind automated edits.

## Repository workflow

This repository uses story branches for larger work.

Current story workflow:

```text
main
  -> story/feature-composition
       -> feature/refactor branches
```

Feature, refactor, fix, and docs branches that belong to the current story should target:

```text
story/feature-composition
```

The story branch should target `main` only when the whole story is complete.

## Branch update workflow

When asked to update the current branch:

1. Inspect the current state:
   - `git branch --show-current`
   - `git status --short`
   - `git remote -v`

2. If the working tree is not clean, stop and report the uncommitted changes.

3. Determine the base branch:
   - If the current branch starts with `story/`, use `main`.
   - If the current branch starts with `feat/`, `refactor/`, `fix/`, or `docs/`, use `story/feature-composition` when that branch exists on origin.
   - If unsure, stop and ask.

4. Update the branch:
   - `git fetch origin`
   - `git rebase origin/<base-branch>`

5. If there are conflicts, stop and report the conflicting files. Do not guess.

## Validation workflow

When asked to validate the current branch:

1. Run:
   - `dotnet build`

2. Only if build succeeds, run:
   - `dotnet test`

3. Report the exact result.

Do not claim that tests passed unless they were run in this session.

## Pull request workflow

When asked to create a pull request for the current branch:

1. Follow the branch update workflow.
2. Follow the validation workflow.
3. If validation succeeds, push the current branch.
4. Create a GitHub pull request with `gh pr create`.
5. Use an English title and Markdown body.
6. Do not merge the pull request.

Generate the PR title and body from the actual diff and commit messages.

Use this PR body structure:

```markdown
## Summary

<short summary>

## Changes

- <actual change>
- <actual change>

## Design decisions

- <relevant design decision>
- <relevant design decision>

## Testing

- `dotnet build`
- `dotnet test`
- Result: all tests passed
```

If validation was not run, use:

```markdown
## Testing

- Not run
```

If validation failed, do not create the PR unless explicitly asked.

## Hard rules

- Do not force push.
- Do not run `git reset --hard`.
- Do not delete untracked files.
- Do not amend commits unless explicitly asked.
- Do not merge pull requests.
- Do not create broad cleanup commits inside focused feature branches.
- Do not refactor unrelated code.
- Do not rename public concepts unless explicitly asked.
- Do not invent changes that are not in the diff.
- Do not claim tests passed unless they were run in this session.

## Coding rules

- Preserve existing behavior unless the task explicitly asks to change it.
- Keep endpoint handlers thin.
- Keep Application independent from API and Infrastructure.
- Keep Domain independent from Application, API, Infrastructure, EF Core, ASP.NET Core, and hosting concerns.
- Prefer explicit registration and visible composition over hidden global conventions.
- Use functional composition where it improves readability.
- Prefer guard clauses and early exits over nested control flow.
- Keep comments focused on why, not what.

## Feature composition rules

Feature modules are activated explicitly from the composition root.

Do not introduce global assembly-wide registration unless explicitly asked.

When moving registrations into feature modules:

- Keep branch scope narrow.
- Move only the registrations named by the task.
- Preserve ordering constraints.
- Do not mix persistence, security, OpenAPI, cross-cutting concerns, and product behavior in the same branch unless explicitly requested.

Current feature markers include:

- `ProductsFeature`
- `CrossCuttingConcerns`
- `DomainEventsFeature`

Expected future feature markers may include:

- `PersistenceFeature`
- `SecurityFeature`
- `OpenApiFeature`

## Pull request quality bar

Before creating a PR, verify that the PR description is factual:

- Mention only files and behavior changed in the branch.
- Keep the summary concise.
- Explain design decisions briefly.
- Include test commands only if actually run.
- Never state or imply that unrelated architecture work was completed.
