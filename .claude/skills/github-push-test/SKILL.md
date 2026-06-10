---
name: github-push-test
description: Complete spec-driven DDD workflow that creates an issue, branches, writes product and tech specs, gets user spec approval, implements from specs, auto-creates Xunit tests, runs local code review, gets user code approval, then commits, pushes, and creates a PR. Use for "从 issue 到 PR 含测试和审查", "完整 DDD 流程", "spec-driven with tests".
---

# github-push-test

Full spec-first DDD development workflow from issue creation to PR, with mandatory unit tests and automated code review. Compared to `github-pull-test`, this skill creates a new issue instead of fetching an existing one.

## Flow

```
create-issue → git-branch → write-product-spec → write-tech-spec
    → 🔴 user approves specs
    → implement-specs → create-and-run-unit-tests → review-pr-local
    → 🔴 user approves code → git-commit → git-push → create-pr
```

## When to Use

- User describes a feature and wants the full cycle with tests and review
- User says "DDD 流程", "spec-driven with tests", "含测试和代码审查的完整开发流程"
- Do NOT use when: trivial one-liner, user only wants one step

## Prerequisites

- Git repo with GitHub `origin`, `gh` CLI authenticated, `python3` available
- Skills: `create-issue`, `git-branch`, `write-product-spec`, `write-tech-spec`, `implement-specs`, `review-pr-local`, `git-commit`, `git-push`, `create-pr`
- For tests: `dotnet test` + Xunit

## Steps

### Step 1: Create Issue

Invoke `create-issue` with user's description. Capture issue number, URL, title. Stop on failure.

### Step 2: Create Branch

Invoke `git-branch` with issue number + title. Infer type from description. Stop on failure.

### Step 3: Write Product Spec

Invoke `write-product-spec` → `specs/issue-<N>/product.md`.

### Step 4: Write Tech Spec

Invoke `write-tech-spec` → `specs/issue-<N>/tech.md`. Include "Testing and Validation" section for Step 7.

### Step 5: 🔴 User Approves Specs

**Mandatory checkpoint. Do NOT proceed without approval.**

Present: issue URL, spec paths, key decisions. Inform next steps:
```
✅ 批准后：
   Step 6  implement-specs       — 根据规格编写代码
   Step 7  创建并运行单元测试      — Xunit
   Step 8  review-pr-local       — 自动代码审查
   Step 9  🔴 用户审查代码        — 确认/修复
   Step 10 git-commit → 11 git-push → 12 create-pr
```
Loop on changes until approved.

### Step 6: Implement from Specs

Invoke `implement-specs` with approved specs. Keep specs in sync.

### Step 7: Create and Run Unit Tests

Check for existing `*Test*` project. If none:

```bash
dotnet new xunit -n SJ.BackEnd.Template.Tests -o src/backend/SJ.BackEnd.Template.Tests
dotnet sln SH.BackEnd.Tempalte.sln add src/backend/SJ.BackEnd.Template.Tests
dotnet add src/backend/SJ.BackEnd.Template.Tests reference \
  src/backend/SJ.BackEnd.Template.Services/SJ.BackEnd.Template.Services.csproj \
  src/backend/SJ.BackEnd.Template.WebAPI/SJ.BackEnd.Template.WebAPI.csproj
dotnet add src/backend/SJ.BackEnd.Template.Tests package Moq
```

Write tests covering acceptance criteria and edge cases. Run `dotnet test` and fix failures.

### Step 8: Run Local Code Review

Invoke `review-pr-local` → produces `review.json`. Do NOT run git/gh commands. Keep `review.json` as artifact.

### Step 9: 🔴 User Approves Code

**Mandatory checkpoint. Do NOT proceed without approval.**

Present: `git diff --stat`, test results, `review.json` findings.

Inform next steps:
```
✅ 批准后：
   Step 10 git-commit
   Step 11 git-push
   Step 12 create-pr
```
Loop fixes → re-test → re-review until approved.

### Step 10: Commit

Stage code + `review.json`, invoke `git-commit`. Format:
```
feat: 简短中文描述

- **模块** — 变更说明

Closes #<N>
```
Do NOT `--no-verify`. Stop on hook failure.

### Step 11: Push

Invoke `git-push`. Sets upstream tracking.

### Step 12: Create PR

Invoke `create-pr`. Link issue with `Closes #<N>`. Include summary, validation, test results.

## Error Handling

Any step fails → stop, report which step and state preserved, suggest recovery.

## Output Summary

```
✅ Push-Test Workflow Complete

Issue:    #<N> — <title>
Branch:   <branch-name>
Specs:    specs/issue-<N>/{product,tech}.md
Tests:    <project> — <count> passed
Review:   review.json — <count> findings
PR:       <pr-url>
```

## Example

User: `实现用户导出功能，支持导出为 Excel 和 CSV`

1. **Create Issue**: #42 "实现用户导出功能"
2. **Branch**: `feat/add-user-export-42`
3. **Specs**: `specs/issue-42/product.md` + `tech.md`
4. **🔴 User approves**
5. **Implement**: Via `implement-specs`
6. **Tests**: Xunit created, all pass ✅
7. **Review**: `review.json` with 2 minor findings
8. **🔴 User approves** after quick fix
9. **Commit + Push + PR**: `Closes #42`
