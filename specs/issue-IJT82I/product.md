# 产品规格：仓储层查询方法优化

> Issue: [#IJT82I](https://gitee.com/heyz/ai-coding-flow/issues/IJT82I) — 仓储优化

## 1. 概述

对 `IBaseRepository<TEntity>` 和 `IBaseServices<TEntity>` 中的查询方法进行命名统一、签名简化和冗余消除。本任务为纯内部重构，不改变数据库交互行为，所有调用方仅需调整方法名和参数传递方式。

## 2. 问题

当前仓储查询 API 存在三个问题：

1. **方法粒度冗余**：`QueryByExpression`（无条件查询）和 `QueryByExpressionOrdered`（带排序查询）是两个独立方法，但无排序的查询只是排序参数为空的特例，二者可合并为一个方法，通过可选参数消除冗余。

2. **命名不一致**：查询方法中既有 `Query*` 前缀（如 `QueryByExpression`、`QueryPagedByExpression`），又有 `Select*` 前缀（如 `Select`、`SelectByCondition`），还有 `Get*` 前缀（如 `GetPagedListByExpression`）。调用方需要在不同前缀之间切换，缺乏统一约定。

3. **未声明的隐性问题**：
   - `SelectByCondition` 返回投影类型 `TResult`，功能上等价于「投影 + 条件 + 排序」，命名未体现其投影查询的本质
   - `GetPagedListByExpression` 是唯一使用 `Get` 前缀的查询方法，与其他 `Query` 前缀不一致
   - `QueryBySqlWhere` 使用 "SqlWhere" 命名，而同类方法 `QueryByWhereOrdered` 仅用 "Where"

## 3. 目标

### 3.1 方法合并

- `QueryByExpression` 与两个 `QueryByExpressionOrdered` 重载合并为一个方法，通过可选参数支持无排序、字符串排序、Lambda 表达式排序三种场景
- 合并后调用方无需判断用哪个方法，统一传参即可

### 3.2 命名统一

- 所有返回 `List<TEntity>` 的过滤查询方法统一使用 `Query*` 前缀
- 所有返回 `TResult` 投影的方法统一使用 `Select*` 前缀
- 所有返回 `PageModel<T>` 的分页方法统一使用 `QueryPaged*` 前缀（不再使用 `Get*`）
- `QueryBySqlWhere` → `QueryByWhere`（对齐 `QueryByWhereOrdered`）
- `GetPagedListByExpression` → `QueryPagedByExpression`（对齐命名约定）

### 3.3 其他优化

- `QueryTopNByExpression` / `QueryTopNByWhere` 保持独立方法（语义特殊：Take + 排序，业务含义明确）
- 统一所有方法实现中的条件判断顺序（Where → OrderBy 链式调用顺序一致）
- 统一空值检查风格（字符串用 `!string.IsNullOrEmpty`，对象用 `!= null`）

## 4. 非目标

- **不改变** 方法实际执行的 SQL 或数据库交互逻辑
- **不修改** 多表联查方法的签名（`QueryThreeTableJoin`、`QueryTwoTableJoinPaged`、`QueryTwoTableJoinGroupedPaged` 保持不变）
- **不修改** 增删改方法的签名（Insert / Update / Delete 系列）
- **不引入** 新的查询模式或 Fluent API

## 5. Figma / 设计参考

无（纯后端 API 重构，无 UI 变更）

## 6. 开发者体验（API 契约）

### 6.1 合并后的查询方法

合并前（3 个方法）：
```csharp
Task<List<TEntity>> QueryByExpression(Expression<Func<TEntity, bool>> whereExpression);
Task<List<TEntity>> QueryByExpressionOrdered(Expression<Func<TEntity, bool>> whereExpression, string orderByFields);
Task<List<TEntity>> QueryByExpressionOrdered(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, object>> orderByExpression, bool isAsc = true);
```

合并后（1 个方法）：
```csharp
Task<List<TEntity>> QueryByExpression(
    Expression<Func<TEntity, bool>> whereExpression,
    string orderByFields = null,
    Expression<Func<TEntity, object>> orderByExpression = null,
    bool isAsc = true);
```

调用示例：
```csharp
// 无排序（原 QueryByExpression）
await QueryByExpression(u => u.Name == "test");

// 字符串排序（原 QueryByExpressionOrdered）
await QueryByExpression(u => u.Name == "test", orderByFields: "age desc");

// Lambda 排序（原 QueryByExpressionOrdered 重载）
await QueryByExpression(u => u.Name == "test", orderByExpression: u => u.Age, isAsc: false);
```

### 6.2 重命名对照表

| 原名 | 新名 | 说明 |
|------|------|------|
| `QueryBySqlWhere` | `QueryByWhere` | 去掉 "Sql"，对齐其他 Where 方法 |
| `GetPagedListByExpression` | `QueryPagedByExpression` | Get → Query，对齐命名约定 |
| `SelectByCondition` | `Select`（重载） | 与无参 `Select` 合并为同一方法的重载 |

### 6.3 不变的方法

以下方法保持名称和签名不变：
- `GetById` / `GetById` (缓存) / `GetByIds` — 按主键查询，使用 `Get` 前缀语义合理
- `GetAll` — 返回全部，`Get*` 前缀语义合理
- `QueryTopNByExpression` / `QueryTopNByWhere` — TopN 语义独立，保持
- `QueryPagedByWhere` — 已符合命名约定
- `QueryByWhereOrdered` — 已符合命名约定
- 所有 Insert / Update / Delete 方法
- 所有多表联查方法

## 7. 成功标准

1. `QueryByExpression` + `QueryByExpressionOrdered` (2 个重载) 合并为 1 个 `QueryByExpression` 方法，所有现有调用方通过编译
2. 重命名方法的新名称在 `IBaseRepository`、`BaseRepository`、`IBaseServices`、`BaseServices` 四层保持一致
3. 所有现有调用方（`SysUserService`、`SysRoleService`、`SysPermissionService`、`SysUserRoleService`）使用新方法名通过编译
5. 所有现有单元测试使用新方法名通过
6. 所有方法实现的链式调用顺序统一为 `Where → OrderBy → Select/Take/ToPageList`

## 8. 验证方式

- **编译检查**：`dotnet build` 整个解决方案零错误
- **单元测试**：`dotnet test` 所有现有测试通过（仅更新 Mock 设置的方法名）
- **代码审查**：检查四个文件命名一致性（`IBaseRepository.cs`、`BaseRepository.cs`、`IBaseServices.cs`、`BaseServices.cs`）
- **调用方扫描**：`grep` 验证无残留旧方法名引用

## 9. 待解决问题

无。本规格基于充分的代码分析，所有变更点已明确。
