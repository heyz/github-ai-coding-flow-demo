# 用户岗位模块 — 技术规格

## 概述

参照现有 `SysUserRole`（用户角色）模式，实现 `SysUserPosition`（用户岗位）模块。

## 影响范围

### 新增文件（8 个）

| # | 文件 | 说明 |
|---|------|------|
| 1 | `Model/Entities/SysUserPosition.cs` | 用户岗位关联实体 |
| 2 | `Model/Dtos/SysUserPosition/BindUserPositionRequest.cs` | 绑定岗位 DTO |
| 3 | `Model/Dtos/SysUserPosition/UnbindUserPositionRequest.cs` | 解绑岗位 DTO |
| 4 | `IServices/SysUserPosition/ISysUserPositionService.cs` | Service 接口 |
| 5 | `Services/SysUserPosition/SysUserPositionService.cs` | Service 实现 |
| 6 | `WebAPI/Controllers/SysUserPositionController.cs` | 控制器 |
| 7 | `Tests/Services/SysUserPositionServiceTests.cs` | 单元测试 |
| 8 | `Tests/Services/SysPositionServiceWithUserTests.cs` | SysPosition 删除保护测试 |

### 修改文件（2 个）

| # | 文件 | 变更内容 |
|---|------|----------|
| 1 | `Services/SysPosition/SysPositionService.cs` | Delete 方法增加用户关联检查 |
| 2 | `WebAPI/Program.cs` | 在 InitTables 中追加 SysUserPosition |

## 详细设计

### 1. 实体：SysUserPosition

参照 `SysUserRole`，位于 `Model/Entities/SysUserPosition.cs`：

```csharp
[SugarTable("sys_user_position")]
[Tenant("2")]
public class SysUserPosition
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }
    public long UserId { get; set; }
    public long PositionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public static SysUserPosition CreateRelation(long userId, long positionId)
    {
        return new SysUserPosition { UserId = userId, PositionId = positionId, CreatedAt = DateTime.Now };
    }
}
```

### 2. DTOs

参照 `BindUserRoleRequest` / `UnbindUserRoleRequest` 模式。

### 3. Service 接口：ISysUserPositionService

```csharp
public interface ISysUserPositionService : IBaseServices<SysUserPosition>
{
    Task<bool> Bind(long userId, long positionId);
    Task<bool> Unbind(long userId, long positionId);
    Task<List<SysPosition>> GetPositionsByUserId(long userId);
    Task<List<SysUser>> GetUsersByPositionId(long positionId);
}
```

### 4. Service 实现：SysUserPositionService

核心逻辑：
- **Bind**：校验用户和岗位存在性 → 校验是否已绑定 → 创建关联
- **Unbind**：查询关联记录 → 删除
- **GetPositionsByUserId**：查询关联 → 按 PositionId 批量查询岗位
- **GetUsersByPositionId**：查询关联 → 按 UserId 批量查询用户

### 5. SysPositionService.Delete 修改

在删除岗位前增加用户关联检查：

```csharp
public async Task<bool> Delete(long id)
{
    var position = await base.GetById(id);
    if (position == null)
        return false;
    if (position.IsSystem)
        return false;

    // 检查是否有用户关联
    var userPositions = await base.QueryByExpression(up => up.PositionId == id);
    if (userPositions.Any())
        return false;

    return await base.DeleteById(id);
}
```

> 注意：此处需要注入 `IBaseRepository<SysUserPosition>` 到 `SysPositionService`。

### 6. 注册 CodeFirst

在 `Program.cs` 中追加 `SysUserPosition` 到 `InitTables` 泛型参数列表。

## Testing and Validation

### 单元测试：SysUserPositionService

| 测试用例 | 描述 |
|----------|------|
| Bind_Success | 正常绑定，返回 true |
| Bind_UserNotExists | 用户不存在，返回 false |
| Bind_PositionNotExists | 岗位不存在，返回 false |
| Bind_AlreadyBound | 已绑定，返回 false |
| Unbind_Success | 正常解绑，返回 true |
| Unbind_NotFound | 绑定关系不存在，返回 false |
| GetPositionsByUserId_Success | 查询用户岗位列表 |
| GetUsersByPositionId_Success | 查询岗位用户列表 |

### SysPositionService.Delete 新增规则测试

| 测试用例 | 描述 |
|----------|------|
| Delete_HasUsers_ReturnsFalse | 岗位有关联用户，删除失败 |
| Delete_NoUsers_ReturnsTrue | 岗位无关联用户，删除成功 |

### 编译验证

```bash
cd src/backend
dotnet build SH.BackEnd.Tempalte.sln
```
预期结果：0 errors

### 测试执行

```bash
cd src/backend
dotnet test SH.BackEnd.Tempalte.sln
```
预期结果：所有测试通过