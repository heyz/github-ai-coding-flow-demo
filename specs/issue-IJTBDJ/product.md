# 产品规格：性能优化 — GetFirst + Service 继承

## 概述

两个独立的小优化：ISysRolePermissionService 继承标准化，以及新增 GetFirst 方法优化单条查询。

## 目标 1：Service 继承标准化

`ISysRolePermissionService` 继承 `IBaseServices<SysRolePermission>`，`SysRolePermissionService` 继承 `BaseServices<SysRolePermission>`，使其与项目中其他 Service 保持一致。

## 目标 2：新增 GetFirst 方法

在 Repository 基类新增 `GetFirst` 方法，使用 `TOP 1` 查询替代 `QueryByExpression`（拉取全部行），用于只需判断存在或取首条记录的场景。

## 验收标准

- [ ] `ISysRolePermissionService` 继承 `IBaseServices<SysRolePermission>`
- [ ] `SysRolePermissionService` 继承 `BaseServices<SysRolePermission>`
- [ ] `IBaseRepository` 新增 `GetFirst(Expression)` 方法
- [ ] `BaseRepository` 实现 `GetFirst` 使用 SqlSugar `FirstAsync`
- [ ] `SysRolePermissionService.RemovePermission` 使用 `GetFirst` 替代 `QueryByExpression`
- [ ] 写入 CLAUDE.md 作为 Service 实现规范
