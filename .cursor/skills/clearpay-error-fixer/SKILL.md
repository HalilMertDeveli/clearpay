---
name: clearpay-error-fixer
description: >-
  Collects every ClearPay failure (dotnet build/test, GitHub Actions, Docker),
  searches Stack Overflow and Reddit for each distinct error, applies a repo-safe
  fix, then re-tests. Use when CI is red, build/test fails, or the user asks to
  fix all errors from community examples.
---

# ClearPay error fixer

1. Collect **every** current failure: `dotnet build`, `dotnet test`, `gh run view` / `--log-failed`, `docker info` / `compose ps` (do not kill compose), linter if relevant. `git fetch` + cooperate: do not overwrite another agent’s good TASK/OWN patch; do not commit `.env`.
2. For each distinct error message, search:
   - Stack Overflow: `site:stackoverflow.com <exact error>`
   - Reddit: `site:reddit.com <exact error>`
   - Official docs if SO is stale (.NET 8, Azure, Docker Desktop Windows)
3. Apply the fix that matches **this** repo: Clean/Onion, .NET 8, no `UPDATE Balance`, no secrets in git, no LED repo. TARTISMA before `src/` or OWN rewrites; HANDOFF append only. Cite the SO/Reddit/doc URL in that note.
4. Re-run tests. If CI was red, commit the fix and push (never force). Prefer Release/`bin` that is not locked by a running `ClearPay.Web` (MSB3027) instead of killing another agent’s process.
5. Return: error list, thread URLs, what changed, test/CI status, user-only blocks (Docker GUI, `az login`, accounts).
