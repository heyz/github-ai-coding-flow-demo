---
name: github-push
description: End-to-end GitHub workflow that creates an issue, branches, optionally writes specs, implements with user approval, then commits, pushes, and creates a PR. Use when the user says "full workflow", "从 issue 到 PR", "完整流程", or describes a task spanning the full lifecycle.
---

# github-push

End-to-end local development workflow from issue creation to PR, with two user review checkpoints. No unit tests or automated code review.

## Flow

```
create-issue → git-branch → [if substantial: write-product-spec → write-tech-spec]
    → 🔴 user approves specs (or informed of skip)
    → implement-specs
    → 🔴 user reviews code
    → git-commit → git-push → create-pr
```

Compared to `github-push-test`: removes unit tests and automated code review. Specs can be skipped for small/low-risk changes.

## When to Use

- User describes a feature and wants it carried through to a PR
- User says "full workflow", "从 issue 到 PR", "完整流程"
- Do NOT use when: user only wants one step, or wants tests/review (use `github-push-test`)

## Prerequisites

- Git repo with GitHub `origin`, `gh` CLI authenticated
- Skills: `create-issue`, `git-branch`, `write-product-spec`, `write-tech-spec`, `implement-specs`, `git-commit`, `git-push`, `create-pr`

## Steps

### Step 1: Create Issue

Invoke `create-issue` with user's description. Capture issue number, URL, title. Stop on failure.

### Step 2: Create Branch

Invoke `git-branch` with issue number + title. Infer type from description. Stop on failure.

### Step 3: Write Product Spec

Invoke `write-product-spec` → `specs/issue-<N>/product.md`.

**小范围低风险跳过策略：** 单文件修改、简单重构等，**不创建规格文档**。明确告知用户后直接进入 Step 6。

### Step 4: Write Tech Spec

Invoke `write-tech-spec` → `specs/issue-<N>/tech.md`. 小范围低风险同 Step 3 跳过。

### Step 5: 🔴 User Approves Specs

**Mandatory checkpoint. Do NOT proceed without approval.**

Present: issue URL, spec paths (或 "已跳过"), key decisions. Inform next steps:
```
✅ 批准后：
   Step 6  implement-specs  — 根据规格编写代码
   Step 7  🔴 用户审查代码   — 确认代码正确
   Step 8  git-commit → 9 git-push → 10 create-pr
```
Loop on changes until approved.

### Step 6: Implement from Specs

Invoke `implement-specs` with approved specs. If specs skipped, implement from issue description.

### Step 7: 🔴 User Reviews Code

**Mandatory checkpoint. Do NOT proceed without approval.**

Present: `git diff --stat`, key implementation highlights. Inform next steps:
```
✅ 批准后：
   Step 8  git-commit
   Step 9  git-push
   Step 10 create-pr
```
Loop fixes until approved.

### Step 8: Commit

Stage code files, invoke `git-commit`. Format:
```
feat: 简短中文描述

- **模块** — 变更说明

Closes #<N>
```
Do NOT `--no-verify`. Stop on hook failure.

### Step 9: Push

Invoke `git-push`. Sets upstream tracking.

### Step 10: Create PR

Invoke `create-pr`. Link issue with `Closes #<N>`. Include summary.

## Error Handling

Any step fails → stop, report what failed and state preserved, suggest recovery.

## Output Summary

```
✅ Push Workflow Complete

Issue:    #<N> — <title>
URL:      <issue-url>
Branch:   <branch-name>
Specs:    specs/issue-<N>/{product,tech}.md (或 "已跳过")
PR:       <pr-url>
```

## Example

User: `实现用户导出功能，支持导出为 Excel 和 CSV`

1. **Create Issue**: #42 "实现用户导出功能"
2. **Branch**: `feat/add-user-export-42`
3. **Specs**: `specs/issue-42/product.md` + `tech.md`
4. **🔴 User approves**
5. **Implement**: Via `implement-specs`
6. **🔴 User approves code**
7. **Commit + Push + PR**: `Closes #42`
