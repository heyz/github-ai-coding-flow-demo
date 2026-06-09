# 技术规格：仓储层查询方法优化

> Issue: [#IJT82I](https://gitee.com/heyz/ai-coding-flow/issues/IJT82I)
> 产品规格: [product.md](./product.md)

## 1. 问题

`IBaseRepository<TEntity>` / `BaseRepository<TEntity>` / `IBaseServices<TEntity>` / `BaseServices<TEntity>` 四层之间存在方法冗余、命名不一致的问题。需要在不改变数据库交互行为的前提下，统一 API 命名。

## 2. 相关代码

| 文件 | 角色 | 变更类型 |
|------|------|----------|
| `SJ.BackEnd.Template.IRepository/BASE/IBaseRepository.cs` | 仓储接口定义 | 方法合并、重命名 |
| `SJ.BackEnd.Template.Repository/BASE/BaseRepository.cs` | 仓储实现 | 方法合并、重命名、实现调整 |
| `SJ.BackEnd.Template.IServices/BASE/IBaseServices.cs` | 服务接口定义 | 方法合并、重命名 |
| `SJ.BackEnd.Template.Services/BaseService.cs` | 服务实现 | 方法合并、重命名 |
| `SJ.BackEnd.Template.Services/SysUser/SysUserService.cs` | 调用方 | 更新方法调用 |
| `SJ.BackEnd.Template.Services/SysRole/SysRoleService.cs` | 调用方 | 更新方法调用 |
| `SJ.BackEnd.Template.Services/SysPermission/SysPermissionService.cs` | 调用方 | 更新方法调用 |
| `SJ.BackEnd.Template.Services/SysUserRole/SysUserRoleService.cs` | 调用方 | 更新方法调用 |
| `SJ.BackEnd.Template.Tests/Services/SysPermissionServiceTests.cs` | 单元测试 | 更新 Mock 方法名 |
| `SJ.BackEnd.Template.Tests/Services/SysRoleServiceTests.cs` | 单元测试 | 更新 Mock 方法名 |
| `SJ.BackEnd.Template.Tests/Services/SysUserRoleServiceTests.cs` | 单元测试 | 更新 Mock 方法名 |
| `SJ.BackEnd.Template.Tests/Services/SysUserServiceTests.cs` | 单元测试 | 可能需更新 |

## 3. 当前状态

### 3.1 方法清单（IBaseRepository 查询方法）

```
QueryBySqlWhere(string where)                               → QueryByWhere
QueryByExpression(Expression where)                          → 合并到 QueryByExpression(where, orderByFields?, orderByExpression?, isAsc?)
QueryByExpressionOrdered(Expression, string)                 → 合并
QueryByExpressionOrdered(Expression, Expression, bool)       → 合并
Select<TResult>(Expression)                                  → 保留
SelectByCondition<TResult>(Expression, Expression, string)   → Select<TResult>(Expression, Expression, string)
QueryByWhereOrdered(string, string)                          → 保留
QueryTopNByExpression(Expression, int, string)               → 保留
QueryTopNByWhere(string, int, string)                        → 保留
QueryByRawSql(string, SugarParameter[])                       → 保留
QueryPagedByExpression(Expression, int, int, string)         → 保留
QueryPagedByWhere(string, int, int, string)                  → 保留
GetPagedListByExpression(Expression, int, int, string)       → QueryPagedByExpression (重命名，注意与上面不同——此方法返回PageModel)
```

### 3.2 当前调用方使用情况

所有调用方仅使用 `QueryByExpression` 和 `GetPagedListByExpression` 两个查询方法：

- `SysUserService`: `QueryByExpression`, `GetPagedListByExpression`
- `SysRoleService`: `QueryByExpression`, `GetPagedListByExpression`
- `SysPermissionService`: `QueryByExpression`
- `SysUserRoleService`: `QueryByExpression`

没有调用方使用 `QueryByExpressionOrdered`、`Select`、`SelectByCondition`、`QueryByWhereOrdered`、`QueryTopNBy*`、`QueryPagedBy*`、`QueryBySqlWhere`。

## 4. 变更方案

### 4.1 方法合并：QueryByExpression

**IBaseRepository 变更：**

```csharp
// 删除 3 个方法：
// Task<List<TEntity>> QueryByExpression(Expression<Func<TEntity, bool>> whereExpression);
// Task<List<TEntity>> QueryByExpressionOrdered(Expression<Func<TEntity, bool>> whereExpression, string orderByFields);
// Task<List<TEntity>> QueryByExpressionOrdered(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, object>> orderByExpression, bool isAsc = true);

// 替换为 1 个方法：
Task<List<TEntity>> QueryByExpression(
    Expression<Func<TEntity, bool>> whereExpression,
    string orderByFields = null,
    Expression<Func<TEntity, object>> orderByExpression = null,
    bool isAsc = true);
```

**BaseRepository 实现：**

```csharp
public async Task<List<TEntity>> QueryByExpression(
    Expression<Func<TEntity, bool>> whereExpression,
    string orderByFields = null,
    Expression<Func<TEntity, object>> orderByExpression = null,
    bool isAsc = true)
{
    var query = _db.Queryable<TEntity>()
        .WhereIF(whereExpression != null, whereExpression);

    if (!string.IsNullOrEmpty(orderByFields))
    {
        query = query.OrderBy(orderByFields);
    }
    else if (orderByExpression != null)
    {
        query = query.OrderBy(orderByExpression, isAsc ? OrderByType.Asc : OrderByType.Desc);
    }

    return await query.ToListAsync();
}
```

### 4.2 重命名

| 原方法名 | 新方法名 | 涉及层次 |
|----------|----------|----------|
| `QueryBySqlWhere` | `QueryByWhere` | IBaseRepository, BaseRepository, IBaseServices, BaseServices |
| `GetPagedListByExpression` | `QueryPagedByExpression` | IBaseRepository, BaseRepository, IBaseServices, BaseServices |
| `SelectByCondition` | `Select`（重载） | IBaseRepository, BaseRepository, IBaseServices, BaseServices |

**`GetPagedListByExpression` → `QueryPagedByExpression` 注意：** 与现有的 `QueryPagedByExpression`（返回 `List<TEntity>`，页码从 0 开始）不同，此方法返回 `PageModel<TEntity>`（页码从 1 开始）。两个方法名冲突，需要将原有的 `QueryPagedByExpression` 重命名为 `QueryPagedListByExpression` 以区分。

修订后的分页方法命名：

| 原方法名 | 新方法名 | 返回类型 | 页码起始 |
|----------|----------|----------|----------|
| `QueryPagedByExpression` | `QueryPagedListByExpression` | `List<TEntity>` | 0 |
| `QueryPagedByWhere` | `QueryPagedListByWhere` | `List<TEntity>` | 0 |
| `GetPagedListByExpression` | `QueryPagedByExpression` | `PageModel<TEntity>` | 1 |

这样命名约定为：
- `QueryPagedList*` → 返回 `List<TEntity>`（纯数据列表）
- `QueryPaged*` → 返回 `PageModel<TEntity>`（含分页信息）

### 4.3 Select 方法合并

将 `Select`（无过滤）和 `SelectByCondition`（带过滤排序）合并：

```csharp
// 合并后
Task<List<TResult>> Select<TResult>(
    Expression<Func<TEntity, TResult>> expression,
    Expression<Func<TEntity, bool>> whereExpression = null,
    string orderByFields = null);
```

实现：
```csharp
public async Task<List<TResult>> Select<TResult>(
    Expression<Func<TEntity, TResult>> expression,
    Expression<Func<TEntity, bool>> whereExpression = null,
    string orderByFields = null)
{
    return await _db.Queryable<TEntity>()
        .WhereIF(whereExpression != null, whereExpression)
        .OrderByIF(!string.IsNullOrEmpty(orderByFields), orderByFields)
        .Select(expression)
        .ToListAsync();
}
```

### 4.4 链式调用顺序统一

所有 BaseRepository 方法实现中，链式调用顺序统一为：

```
Queryable → WhereIF → OrderByIF → Select/Take/ToPageList/ToList
```

当前 `SelectByCondition` 的实现顺序为 `OrderBy → Where → Select`，需改为 `Where → OrderBy → Select`。

## 5. 端到端流程

本任务为纯内部重构，无用户可见交互流程。变更流程如下：

```
1. 修改 IBaseRepository.cs → 合并方法、重命名
2. 修改 BaseRepository.cs  → 实现合并后的方法、重命名、统一链式顺序
3. 修改 IBaseServices.cs  → 合并方法、重命名（无补齐）
4. 修改 BaseServices.cs   → 同步接口变更（无补齐）
5. 更新调用方             → SysUserService, SysRoleService, SysPermissionService, SysUserRoleService
6. 更新单元测试           → Mock Setup 方法名同步更新
7. dotnet build           → 验证编译通过
8. dotnet test            → 验证测试通过
```

### 4.5 链式调用顺序统一说明

所有 `SelectByCondition`（合并后的 `Select`）及 `QueryPagedByWhere` 等方法中，链式调用顺序统一为：
```
Queryable → WhereIF → OrderByIF → Select/Take/ToPageList/ToList
```

当前 `SelectByCondition` 的实现顺序为 `OrderBy → Where → Select`，需改为 `Where → OrderBy → Select`。

## 6. 风险与缓解

| 风险 | 影响 | 缓解措施 |
|------|------|----------|
| 遗漏调用方 | 编译失败 | 全局 grep 搜索旧方法名，确保零遗漏 |
| `QueryPagedByExpression` 名冲突 | 编译歧义 | 原先两个方法语义不同（List vs PageModel），通过 `QueryPagedListByExpression` vs `QueryPagedByExpression` 区分 |
| 破坏现有测试 | CI 失败 | 仅在测试中更新 Mock Setup 方法名，不改变测试逻辑 |
| 合并后的 `QueryByExpression` 可选参数过多 | 调用方困惑 | 3 个可选参数仍可接受；参数命名明确（`orderByFields` / `orderByExpression`），IDE 智能提示可导航 |

## 7. 测试与验证

### 7.1 编译验证

```bash
dotnet build src/backend/SH.BackEnd.Tempalte.sln
```

### 7.2 单元测试

更新 Mock 设置中的方法名后运行：

```bash
dotnet test src/backend/SJ.BackEnd.Template.Tests --no-restore
```

### 7.3 残留引用检查

```bash
# 确认无旧方法名残留
grep -rn "QueryBySqlWhere\|GetPagedListByExpression\|SelectByCondition\|QueryByExpressionOrdered" src/backend/ --include="*.cs"
```

### 7.4 需要新增的测试

- `QueryByExpression` 三种排序模式（无排序、字符串排序、Lambda 排序）的行为测试
- `Select` 方法无过滤 + 带过滤两种模式的测试

## 8. 后续工作

- `IBaseServices` 中 `Select` 和 `QueryTwoTableJoinPaged`/`QueryTwoTableJoinGroupedPaged` 方法从未被调用方使用——可考虑后续移除或添加使用示例
- `QueryByWhereOrdered` 和 `QueryTopNByWhere` 类似于 `QueryByExpression` + `Take`，后续可考虑统一参数化
