# 移除权限业务 — 产品规格

## 背景

当前系统中存在 `SysPermission`（权限）相关的完整 CRUD 业务代码，包含实体、DTO、Service、Controller 以及对应的单元测试。

## 目标

彻底移除系统中所有与权限（Permission）相关的代码，保留项目其他功能不受影响。

## 移除范围

### 删除的文件（7 个）

| # | 文件 | 类型 |
|---|------|------|
| 1 | `SJ.BackEnd.Template.Model/Entities/SysPermission.cs` | 实体 |
| 2 | `SJ.BackEnd.Template.Model/Dtos/SysPermission/CreatePermissionRequest.cs` | 创建 DTO |
| 3 | `SJ.BackEnd.Template.Model/Dtos/SysPermission/UpdatePermissionRequest.cs` | 更新 DTO |
| 4 | `SJ.BackEnd.Template.IServices/SysPermission/ISysPermissionService.cs` | Service 接口 |
| 5 | `SJ.BackEnd.Template.Services/SysPermission/SysPermissionService.cs` | Service 实现 |
| 6 | `SJ.BackEnd.Template.WebAPI/Controllers/SysPermissionController.cs` | 控制器 |
| 7 | `SJ.BackEnd.Template.Tests/Services/SysPermissionServiceTests.cs` | 单元测试 |

### 修改的文件（1 个）

| # | 文件 | 变更内容 |
|---|------|----------|
| 1 | `SJ.BackEnd.Template.WebAPI/Program.cs` | 从 `InitTables` 泛型参数中移除 `SysPermission` |

## 排除范围

- 不修改项目中其他实体的逻辑（角色、岗位、用户等）
- 不修改公共基础设施代码（BaseServices、BaseRepository、Autofac 模块等）
- 不引入新的依赖

## 验收标准

1. 上述 7 个权限相关文件被彻底删除
2. `Program.cs` 中不再引用 `SysPermission`
3. 项目编译通过，无残留引用错误
4. 已有单元测试除被删除的权限测试外全部通过