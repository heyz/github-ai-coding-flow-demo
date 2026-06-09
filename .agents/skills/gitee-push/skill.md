---
name: gitee-push
description: End-to-end Gitee automation workflow that orchestrates issue creation, branch setup, spec drafting, user-reviewed implementation, code commit, push, and PR creation — all using Gitee MCP for repository interactions. Use this skill whenever the user wants to start a new feature or fix from scratch on Gitee and carry it all the way through to a pull request, or when the user says "gitee full workflow", "gitee 从 issue 到 PR", "gitee 完整流程".
---

# gitee-push

Orchestrate the complete local development workflow from Gitee issue to PR in a
single invocation, using **Gitee MCP tools** for repository-facing operations
(create issue, create PR) instead of GitHub CLI.

## Overview

This skill chains together individual skills into one cohesive workflow with
**two mandatory user-review checkpoints** — one after spec drafting, one after
implementation:

```
create-issue (Gitee MCP) → git-branch → write-product-spec → write-tech-spec
    → (user review: specs) → implement-specs → (user review: code)
    → git-commit → git-push → create-pr (Gitee MCP)
```

Each step feeds its output forward — the Gitee issue identifier informs the
branch name, the branch carries the issue ID, the specs go under
`specs/issue-<ID>/`, and the PR links back to the originating issue. The user
only needs to provide the initial feature description; the two review
checkpoints ensure the user stays in control of what gets built and committed.

**The two review checkpoints solve different problems:**

1. **Spec review** (after docs, before code): The user validates that the
   product behavior and technical approach are correct BEFORE any code is
   written. This prevents wasted implementation effort on misaligned specs.
2. **Code review** (after implementation, before commit): The user validates
   that the actual code matches the approved specs and is correct in detail.

## When to Use

- The user describes a feature, bug fix, or change they want implemented end to
  end on Gitee, from issue to PR.
- The user explicitly asks for a "full workflow", "complete flow", "gitee 从
  issue 到 PR", "gitee 完整流程", or similar.
- The user wants to start work on a new task on Gitee and carry it through to a
  reviewable PR.

Do NOT use this skill when:
- The user only wants one step (e.g., just create an issue, just commit).
  Invoke the individual skill directly instead.
- The user already has an existing Gitee issue to work from. Use
  `gitee-push-test` instead, which fetches the existing issue.
- The user already has an issue and branch set up. Use `spec-driven-implementation`
  or the relevant individual skill for the remaining steps.

## Prerequisites

- A git repository with a **Gitee** remote (`origin`).
- Gitee MCP server configured (`.mcp.json` with `gitee` server entry).
- The following skills available: `git-branch`, `write-product-spec`,
  `write-tech-spec`, `implement-specs`, `git-commit`, `git-push`.

## Repository Info

The Gitee repo is determined from the git remote:

```bash
git remote get-url origin
# e.g., https://gitee.com/heyz/ai-coding-flow.git  → owner=heyz, repo=ai-coding-flow
#       git@gitee.com:heyz/ai-coding-flow.git       → owner=heyz, repo=ai-coding-flow
```

Store these as `GITEE_OWNER` and `GITEE_REPO` for MCP tool calls.

**Gitee issue identifiers** are alphanumeric strings (e.g., `IJT82I`), NOT
numeric like GitHub. They are returned by the MCP `gitee__create_issue` tool as
the issue `number` field. Branch names should lowercase them: `IJT82I` →
`ijt82i`.

## Workflow

### Phase 1: Preparation

### Step 1: Create Issue via Gitee MCP

Create a Gitee issue using the `mcp__gitee__create_issue` MCP tool.

**Input:** The user's description of the feature, bug fix, or change.

**Action:**

1. From the user's natural-language request, derive a clear **title** and
   **body** for the issue. The title should be concise; the body should capture
   the full context, requirements, and acceptance criteria.

2. Determine the Gitee owner and repo from `git remote get-url origin`:
   - `https://gitee.com/{owner}/{repo}.git` → owner={owner}, repo={repo}
   - `git@gitee.com:{owner}/{repo}.git` → owner={owner}, repo={repo}

3. Determine the issue **type** from the user's description:
   - Bug fix / 缺陷 → `issue_type=缺陷`
   - New feature / 需求 → `issue_type=需求`
   - Task / refactor / chore → `issue_type=任务`

4. Determine **labels** from the description (e.g., `bug`, `enhancement`,
   `refactor`, `documentation`).

5. Call the `mcp__gitee__create_issue` tool with:
   - `owner`: the Gitee owner
   - `repo`: the Gitee repo name
   - `title`: issue title
   - `body`: issue body/description
   - `issue_type`: inferred from description
   - `labels`: comma-separated labels if applicable

6. **Parse the response** to capture:
   - `number` — Gitee issue identifier (e.g., `IJT82I`)
   - `html_url` — issue URL
   - `title` — issue title

**Output to capture:**
- Issue identifier (e.g., `IJT82I`)
- Issue URL (e.g., `https://gitee.com/heyz/ai-coding-flow/issues/IJT82I`)
- Issue title

If issue creation fails or the user declines, stop the workflow.

### Step 2: Create Branch

Invoke the `git-branch` skill, passing the issue identifier from Step 1.

Infer the branch type from the issue type used in Step 1:
- 缺陷 / bug → `fix/`
- 需求 / feature → `feat/`
- 任务 with refactoring keywords → `refactor/`

The branch will be named `<type>/<short-desc>-<issueID>` following the
`git-branch` skill's naming convention. **Lowercase the issue ID** in the
branch name: `IJT82I` → `ijt82i`.

**Input:** Issue identifier and title from Step 1.

**Output to capture:**
- Branch name (e.g., `feat/add-user-export-ijt82i`)
- Current branch confirmed

If branch creation fails, stop the workflow.

### Phase 2: Specs (docs only, no code yet)

### Step 3: Write Product Spec

Invoke the `write-product-spec` skill.

Produce the file at `specs/issue-<ID>/product.md`. This describes **what** the
feature does from a user/external perspective — behavior, goals, non-goals,
acceptance criteria, edge cases. No implementation details.

For small, low-risk changes where specs would be unnecessary overhead, this step
may be skipped — but you **MUST** explicitly tell the user:
1. That this step is being skipped.
2. **Why** it qualifies as "small, low-risk" — state the specific reasons (e.g.,
   single-method addition, few call sites, no architectural impact, clear scope).
3. What the implementation plan is (which files, what changes).

Do NOT silently skip — the user must see the rationale and plan before
proceeding.

### Step 4: Write Tech Spec

Invoke the `write-tech-spec` skill when the feature warrants it (cross-cutting,
architectural decisions, multi-module changes).

Produce the file at `specs/issue-<ID>/tech.md`. This describes **how** to build
it — relevant files, implementation plan, data flow, risks, test strategy. Must
be grounded in actual codebase patterns.

For small features or pure UI changes where a tech spec adds no value, this step
may be skipped — but you **MUST** explicitly tell the user:
1. That this step is being skipped.
2. **Why** a tech spec adds no value — state the specific reasons (e.g., no
   architectural decisions, trivial implementation, existing patterns to follow).

Do NOT silently skip — the user must see the rationale before proceeding.

### Phase 3: Spec Review Checkpoint (🔴 MUST WAIT)

### Step 5: User Review of Specs

**This is a mandatory checkpoint. Do NOT proceed to Step 6 without user
confirmation.**

1. Present a summary to the user:
   - Issue: `#<ID> — <title>` with URL
   - Product spec: `specs/issue-<ID>/product.md` (or "skipped" with reason)
   - Tech spec: `specs/issue-<ID>/tech.md` (or "skipped" with reason)
   - Key architectural decisions captured in the tech spec

2. Ask the user to review the specs:
   - Does the product spec describe the right behavior?
   - Does the tech spec propose a sound implementation approach?

   **Also inform the user what happens after approval:**
   ```
   ✅ 批准后执行：
      Step 6  implement-specs     — 根据规格编写代码
      Step 7  🔴 再次审查代码      — 确认代码正确
      Step 8  git-commit          — 提交代码
      Step 9  git-push            — 推送到远程
      Step 10 create-pr (Gitee MCP) — 创建 Pull Request
   ```

3. Wait for explicit user approval before continuing.

   If the user requests spec changes:
   - Update the spec files accordingly
   - Re-present for approval
   - Repeat until approved

   If the user says "approved" or "ok" or "go ahead" or equivalent: proceed to
   Step 6.

The purpose of this checkpoint is to avoid wasting effort on code that implements
the wrong behavior or the wrong architecture. Specs are cheap to revise; code is
not.

### Phase 4: Implementation

### Step 6: Implement from Specs

Invoke the `implement-specs` skill with the approved product spec and tech spec.

This step reads the approved specs and writes the actual code. The implementation
may update the specs if discoveries during coding change the approach — that's
normal and expected. What matters is that the user approved the direction in
Step 5.

**Output:** Working code on the branch, with specs possibly updated to reflect
implementation realities.

### Phase 5: Code Review Checkpoint (🔴 MUST WAIT)

### Step 7: User Review of Code

**This is a mandatory checkpoint. Do NOT proceed to Step 8 without user
confirmation.**

1. Present a summary to the user:
   - Issue: `#<ID> — <title>` with URL
   - Branch: `<branch-name>`
   - Specs: list the spec file paths
   - Files changed: summary (`git diff --stat`)
   - Key implementation highlights: briefly describe what was implemented

2. If a `review.json` was generated (e.g., via manual review), present its
   findings to the user.

3. Ask the user to review:
   - Does the code match the approved specs?
   - Are there any issues with the implementation?

   **Also inform the user what happens after approval:**
   ```
   ✅ 批准后执行：
      Step 8  git-commit   — 提交代码
      Step 9  git-push     — 推送到远程
      Step 10 create-pr    — 创建 Pull Request (Gitee MCP)
   ```

4. Wait for explicit user approval before continuing to commit.

   If the user requests code changes:
   - Make the requested changes
   - Re-present the summary for approval
   - Repeat until approved

   If the user says "approved" or "ok" or "go ahead" or equivalent: proceed to
   Step 8.

This checkpoint exists because commits and pushes are hard to undo, and the user
must validate the implementation before it enters git history.

### Phase 6: Ship

### Step 8: Commit

Stage all code files plus `review.json`, then invoke the `git-commit` skill.

**Commit format (参考 PR #15 风格):**

```text
feat: 简短中文描述

- **模块** — 变更说明
- **模块** — 变更说明

Closes #<ID>
```

- Use English `type(scope):` prefix (e.g., `feat:`, `fix:`, `refactor:`)
- Use Chinese summary and body
- Include `Closes #<ID>` or `Fixes #<ID>` (using the Gitee issue identifier)
- List each Change as a bullet with bold module name

```bash
git add src/...  # code files
git add review.json
```

**Output to capture:**
- Commit hash
- Whether hooks/checks ran

If commit hooks fail, stop and report the failure. Do not use `--no-verify`.

### Step 9: Push

Invoke the `git-push` skill.

**Input:** The committed branch from Step 8.

The push will set upstream tracking on first push (`git push -u origin
<branch>`).

**Output to capture:**
- Remote branch
- Pushed commit hash

If push is rejected, follow the `git-push` skill's rejection handling (fetch,
inspect divergence, ask before rebasing or force-with-lease).

### Step 10: Create PR via Gitee MCP

Create a Pull Request using the `mcp__gitee__create_pull` MCP tool. This
replaces the `create-pr` skill (which depends on `gh` CLI for GitHub).

**Input:** The pushed branch, issue identifier from Step 1.

**Action:**

1. Determine `owner` and `repo` from the git remote (same as Step 1).

2. Determine the base branch (typically `master` or the repo's default branch):

   ```bash
   git symbolic-ref refs/remotes/origin/HEAD | sed 's|.*/||'
   ```
   (Fallback: `master` if the command fails.)

3. Build the PR **title** from the issue title or the main commit subject.

4. Build the PR **body** summarizing the changes and validation performed, with
   a link to the issue using `Closes #<ID>` (Gitee recognizes this syntax).

5. Call `mcp__gitee__create_pull` with:
   - `owner`: Gitee owner
   - `repo`: Gitee repo name
   - `title`: PR title
   - `head`: the source branch (e.g., `feat/add-user-export-ijt82i`)
   - `base`: the target branch (e.g., `master`)
   - `body`: PR body with changes summary and `Closes #<ID>`
   - `labels`: same labels as the issue

   If the branch is not yet ready for review, add `"draft": true`.

6. **Parse the response** to capture:
   - PR number
   - PR URL (`html_url`)
   - PR title

7. If the API call fails (auth, permissions, rate limit), report the exact
   title, body, base, and head needed for manual creation at:
   `https://gitee.com/${owner}/${repo}/pulls`

**Output to capture:**
- PR URL
- Base branch, title, PR number

Report the final result to the user with a complete summary.

## Error Handling

If any step fails:

1. **Stop the workflow.** Do not silently continue to the next step.
2. **Report the failure clearly** — which step failed, what went wrong, and
   what output was captured.
3. **Preserve state.** Any successfully completed steps remain in effect
   (e.g., if the issue was created but the branch failed, the issue still
   exists).
4. **Suggest recovery.** Tell the user how to fix the issue or resume
   manually using individual skills.

## Output Summary

After all steps complete successfully, present this summary:

```
✅ Gitee Full Workflow Complete

Issue:    #<ID> — <title>
URL:      <issue-url>
Branch:   <branch-name>
Specs:    specs/issue-<ID>/product.md (and tech.md if created)
Commit:   <hash>
PR:       <pr-url>

Next steps:
- Watch CI checks on the PR
- Respond to review comments
- Keep the PR current by rebasing on the base branch as needed
```

## Example

User says:
> 实现用户导出功能，支持导出为 Excel 和 CSV 格式

Workflow execution:
1. **Gitee MCP create_issue**: Creates issue on Gitee (e.g., #IJD82K "实现用户导出功能")
2. **git-branch**: Creates branch `feat/add-user-export-ijd82k`
3. **write-product-spec**: Writes product spec to `specs/issue-IJD82K/product.md`
4. **write-tech-spec**: Writes tech spec to `specs/issue-IJD82K/tech.md`
5. **🔴 Spec Review**: User reviews specs, approves the plan
6. **implement-specs**: Implements the feature based on approved specs
7. **🔴 Code Review**: User reviews the code, approves the implementation
8. **git-commit**: Commits with message `feat(export): add user export to Excel and CSV Refs #IJD82K`
9. **git-push**: Pushes to `origin/feat/add-user-export-ijd82k`
10. **Gitee MCP create_pull**: Creates PR linking to `Closes #IJD82K`
