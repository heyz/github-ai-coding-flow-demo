# 移除权限业务 — 技术规格

## 概述

删除系统中所有与 `SysPermission` 相关的文件及引用。该实体是一个纯 CRUD 数据实体，不涉及权限认证/授权基础设施，移除后不影响其他业务模块。

## 影响范围分析

### 依赖分析

- `SysPermission` 不涉及其它业务的实体引用（无外键引用、无 RolePermission/UserPermission 等关联实体）
- 其他 Service/Controller 不引用 `SysPermissionService` 或 `ISysPermissionService`
- Autofac 通过程序集扫描自动注册，无需手动注销注册项
- `SysPermission` 在 `InitTables` 中仅用于 CodeFirst 表自动创建

### 改动清单

#### 删除文件（7 个）

```bash
# 实体
src/backend/SJ.BackEnd.Template.Model/Entities/SysPermission.cs

# DTO 目录
src/backend/SJ.BackEnd.Template.Model/Dtos/SysPermission/CreatePermissionRequest.cs
src/backend/SJ.BackEnd.Template.Model/Dtos/SysPermission/UpdatePermissionRequest.cs

# Service 接口 + 实现
src/backend/SJ.BackEnd.Template.IServices/SysPermission/ISysPermissionService.cs
src/backend/SJ.BackEnd.Template.Services/SysPermission/SysPermissionService.cs

# 控制器
src/backend/SJ.BackEnd.Template.WebAPI/Controllers/SysPermissionController.cs

# 测试
src/backend/SJ.BackEnd.Template.Tests/Services/SysPermissionServiceTests.cs
```

#### 修改文件（1 个）

**`src/backend/SJ.BackEnd.Template.WebAPI/Program.cs`**

```diff
- db.CodeFirst.InitTables<SysRole, SysUserRole, SysPermission, SysPosition>();
+ db.CodeFirst.InitTables<SysRole, SysUserRole, SysPosition>();
```

#### 删除空目录（3 个）

```bash
src/backend/SJ.BackEnd.Template.Model/Dtos/SysPermission/
src/backend/SJ.BackEnd.Template.IServices/SysPermission/
src/backend/SJ.BackEnd.Template.Services/SysPermission/
```

## 风险与缓解

| 风险 | 缓解措施 |
|------|----------|
| 遗漏其他文件中对 `SysPermission` 的引用 | 删除后执行 `dotnet build` 验证编译无错误 |
| DTO 目录空文件夹残留 | 手动清理空目录 |
| 测试项目中引用权限测试命名空间 | 直接删除整个测试文件 |

## 执行步骤

1. 一次性删除所有 7 个权限相关文件
2. 修改 `Program.cs` 移除 `SysPermission` 泛型参数
3. 删除 3 个空目录
4. 执行 `dotnet build` 验证编译
5. 执行 `dotnet test` 验证原有测试通过

## Testing and Validation

### 编译验证

```bash
cd src/backend
dotnet build SH.BackEnd.Tempalte.sln
```
预期结果：0 errors

### 单元测试验证

```bash
cd src/backend
dotnet test SH.BackEnd.Tempalte.sln
```
预期结果：权限测试文件已被删除，其余测试全部通过