# 产品规格：移除角色权限模块

## 概述

移除整个角色-权限关联模块（SysRolePermission），包括实体、DTO、服务接口、服务实现、控制器及相关级联删除逻辑和测试代码。

## 问题

角色权限关联模块（SysRolePermission）当前实现了角色与权限之间的多对多关联管理功能，包括批量分配权限、查询角色权限、移除角色权限关联、级联删除等。该模块需要被完全移除，对应代码和测试也需要一并清理。

## 目标

- 删除 SysRolePermission 实体、DTO、服务接口、服务实现、控制器
- 从 SysPermissionService 和 SysRoleService 中移除级联删除 SysRolePermission 的逻辑
- 删除或更新相关的测试代码
- 编译通过（0 错误）
- 剩余测试全部通过

## 非目标

- 不修改其他模块的 API 和行为
- SysPermission 和 SysRole 的 Delete 方法仍保留（但不做级联删除）
- 数据库表结构不在此次变更范围内

## Figma / 设计参考

无（纯后端代码移除，无 UI 变更）

## 用户场景

无用户可见变化。这是一个纯后端代码清理任务：
- 移除 3 个 API 端点（分配权限、查询角色权限、移除权限关联）
- 删除角色/权限时不再做级联删除关联记录

## 验收标准

| # | 验收条件 | 验证方式 |
|---|---------|---------|
| AC1 | 5 个模块文件（实体、DTO、接口、服务、控制器）被删除 | 文件不存在 |
| AC2 | SysPermissionService 不再引用 SysRolePermission | 编译检查 |
| AC3 | SysRoleService 不再引用 SysRolePermission | 编译检查 |
| AC4 | 编译通过，0 错误 | `dotnet build` |
| AC5 | 剩余测试全部通过 | `dotnet test` |
| AC6 | 无残留的 SysRolePermission 引用 | grep 扫描 |

## 验证方式

- **编译检查**：`dotnet build` 整个解决方案零错误
- **单元测试**：`dotnet test` 所有测试通过
- **引用扫描**：`grep -rn "SysRolePermission" src/` 确认无残留引用