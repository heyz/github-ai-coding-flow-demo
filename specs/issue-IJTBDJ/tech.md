# 技术规格：性能优化 — GetFirst + Service 继承

## 变更文件

| 文件 | 操作 | 说明 |
|------|------|------|
| `IBaseRepository.cs` | 修改 | 新增 `GetFirst` 接口 |
| `BaseRepository.cs` | 修改 | 实现 `GetFirst`（`FirstAsync`） |
| `IBaseServices.cs` | 修改 | 新增 `GetFirst` 接口 |
| `BaseService.cs` | 修改 | 实现 `GetFirst` 委托 |
| `ISysRolePermissionService.cs` | 修改 | 继承 `IBaseServices<SysRolePermission>` |
| `SysRolePermissionService.cs` | 修改 | 继承 `BaseServices<SysRolePermission>`，RemovePermission 改用 GetFirst |
| `CLAUDE.md` | 修改 | 添加 Service 继承 BaseService 规范 |

## GetFirst 实现

```csharp
// IBaseRepository
Task<TEntity> GetFirst(Expression<Func<TEntity, bool>> whereExpression);

// BaseRepository
public async Task<TEntity> GetFirst(Expression<Func<TEntity, bool>> whereExpression)
{
    return await _db.Queryable<TEntity>().First(whereExpression).FirstAsync();
}
```

SqlSugar 的 `FirstAsync` 生成 `SELECT TOP 1 ...`，只返回一条数据。

## Service 继承变更

```csharp
// 前
public interface ISysRolePermissionService { ... }
public class SysRolePermissionService : ISysRolePermissionService { ... }

// 后
public interface ISysRolePermissionService : IBaseServices<SysRolePermission> { ... }
public class SysRolePermissionService : BaseServices<SysRolePermission>, ISysRolePermissionService { ... }
```

由于构造函数参数从 4 变为 3（rpRepo 由基类提供），可使用 Primary Constructor：
```csharp
public class SysRolePermissionService(
    IBaseRepository<SysRolePermission> rpRepo,
    IBaseRepository<SysRole> roleRepo,
    IBaseRepository<SysPermission> permRepo,
    IUnitOfWorkManage uow)
    : BaseServices<SysRolePermission>(rpRepo), ISysRolePermissionService
```

RemovePermission 中 `_rpRepo.QueryByExpression(...)` + `list[0]` 改为 `await base.GetFirst(...)`。

## CLAUDE.md 补充

在 "Base Services / Repository" 节增加：自定义 Service 必须继承 `BaseServices<TEntity>`，接口必须继承 `IBaseServices<TEntity>`。

## 测试策略

更新 `SysRolePermissionService` 构造函数调用，适配新继承结构。
