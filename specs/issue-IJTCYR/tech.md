# 技术规格：移除角色权限模块

Issue: [#IJTCYR](https://gitee.com/heyz/ai-coding-flow/issues/IJTCYR) — 移除角色权限模块代码包括测试代码

## 问题

移除整个 SysRolePermission 模块，包括实体、DTO、服务接口/实现、控制器，以及 SysPermissionService / SysRoleService 中的级联删除逻辑和对应测试代码。

## 相关文件

### 要删除的文件（5 个）

| 文件 | 位置 |
|------|------|
| 实体 | `SJ.BackEnd.Template.Model/Entities/SysRolePermission.cs` |
| DTO | `SJ.BackEnd.Template.Model/Dtos/SysRolePermission/AssignPermissionsRequest.cs` |
| 服务接口 | `SJ.BackEnd.Template.IServices/SysRolePermission/ISysRolePermissionService.cs` |
| 服务实现 | `SJ.BackEnd.Template.Services/SysRolePermission/SysRolePermissionService.cs` |
| 控制器 | `SJ.BackEnd.Template.WebAPI/Controllers/SysRolePermissionController.cs` |

同时删除空目录：`Dtos/SysRolePermission/`、`IServices/SysRolePermission/`、`Services/SysRolePermission/`

### 要修改的文件（4 个）

| 文件 | 修改内容 |
|------|----------|
| `Services/SysPermission/SysPermissionService.cs` | 第 14 行：删除 `IBaseRepository<SysRolePermission> rpRepo` 参数；第 17 行：删除 `_rpRepo` 字段；第 51-70 行：删除级联删除逻辑 |
| `Services/SysRole/SysRoleService.cs` | 第 14 行：删除 `IBaseRepository<SysRolePermission> rpRepo` 参数；第 16 行：删除 `_rpRepo` 字段；第 59-74 行：删除级联删除逻辑 |
| `Tests/Services/SysPermissionServiceTests.cs` | 第 10,16 行：删除 `_mockRpRepo`；删除 `Delete_WithRolePermissions_CascadeDeletes` 测试（99-129行）；简化 `Delete_NoChildren_ReturnsTrue`（移除 rpRepo mocks） |
| `Tests/Services/SysRoleServiceTests.cs` | 第 10,16 行：删除 `_mockRpRepo`；删除 `Delete_WithRolePermissions_CascadeDeletes` 测试（106-131行）；简化 `Delete_NonSystemRole_ReturnsTrue`（移除 rpRepo mocks） |

## 当前状态

- SysRolePermission 实体有 `[SugarTable("sys_role_permission")]` 和 `[Tenant("2")]` 注解
- SysPermissionService 和 SysRoleService 在 Delete 方法中注入 `IBaseRepository<SysRolePermission>` 来级联删除关联记录
- 测试中 Mock 了 `IBaseRepository<SysRolePermission>` 来测试级联删除行为

## 变更方案

### 步骤 1：删除模块文件
使用 `git rm` 删除 5 个文件及其目录结构。

### 步骤 2：修改 SysPermissionService

```csharp
// 前
public class SysPermissionService(IBaseRepository<SysPermission> repository, IBaseRepository<SysRolePermission> rpRepo)
    : BaseServices<SysPermission>(repository), ISysPermissionService
{
    private readonly IBaseRepository<SysRolePermission> _rpRepo = rpRepo;

    public new async Task<bool> Delete(long id)
    {
        var permission = await base.GetById(id);
        if (permission == null)
            return false;
        // 检查是否有子节点
        var children = await base.QueryByExpression(u => u.ParentId == id);
        if (children.Any())
            return false;
        // 级联删除角色-权限关联
        var rpList = await _rpRepo.QueryByExpression(rp => rp.PermissionId == id);
        if (rpList.Count > 0)
        {
            var rpIds = rpList.Select(rp => rp.Id).Cast<object>().ToArray();
            await _rpRepo.DeleteByIds(rpIds);
        }
        return await base.DeleteById(id);
    }
}

// 后
public class SysPermissionService(IBaseRepository<SysPermission> repository)
    : BaseServices<SysPermission>(repository), ISysPermissionService
{
    public async Task<bool> Delete(long id)
    {
        var permission = await base.GetById(id);
        if (permission == null)
            return false;

        // 检查是否有子节点
        var children = await base.QueryByExpression(u => u.ParentId == id);
        if (children.Any())
            return false;

        return await base.DeleteById(id);
    }
}
```

### 步骤 3：修改 SysRoleService

```csharp
// 前
public class SysRoleService(IBaseRepository<SysRole> repository, IBaseRepository<SysRolePermission> rpRepo)
    : BaseServices<SysRole>(repository), ISysRoleService
{
    private readonly IBaseRepository<SysRolePermission> _rpRepo = rpRepo;

    public new async Task<bool> Delete(long id)
    {
        var role = await base.GetById(id);
        if (role == null)
            return false;
        if (role.IsSystem)
            return false;
        // 级联删除角色-权限关联
        var rpList = await _rpRepo.QueryByExpression(rp => rp.RoleId == id);
        if (rpList.Count > 0)
        {
            var rpIds = rpList.Select(rp => rp.Id).Cast<object>().ToArray();
            await _rpRepo.DeleteByIds(rpIds);
        }
        return await base.DeleteById(id);
    }
}

// 后
public class SysRoleService(IBaseRepository<SysRole> repository)
    : BaseServices<SysRole>(repository), ISysRoleService
{
    public async Task<bool> Delete(long id)
    {
        var role = await base.GetById(id);
        if (role == null)
            return false;
        if (role.IsSystem)
            return false;

        return await base.DeleteById(id);
    }
}
```

### 步骤 4：修改测试

**SysPermissionServiceTests：**
- 删除 `_mockRpRepo` 字段和初始化
- `Delete_NoChildren_ReturnsTrue`：删除 `_mockRpRepo.Setup`（2 处）
- 删除 `Delete_WithRolePermissions_CascadeDeletes` 整个测试方法
- 构造函数改为 `new SysPermissionService(_mockRepo.Object)`

**SysRoleServiceTests：**
- 删除 `_mockRpRepo` 字段和初始化
- `Delete_NonSystemRole_ReturnsTrue`：删除 `_mockRpRepo.Setup`（2 处）
- 删除 `Delete_WithRolePermissions_CascadeDeletes` 整个测试方法
- 构造函数改为 `new SysRoleService(_mockRepo.Object)`

### 步骤 5：验证

```bash
dotnet build SH.BackEnd.Tempalte.sln    # 0 errors
dotnet test SH.BackEnd.Tempalte.sln     # all passing
grep -rn "SysRolePermission" src/       # no remaining references
```

## 风险与缓解

| 风险 | 影响 | 缓解 |
|------|------|------|
| 外部有调用者引用 SysRolePermission 控制器 API | API 消费者不可用 | 已确认这是纯后端重构，无已知外部消费者 |
| 遗漏引用导致编译失败 | 构建中断 | 步骤 5 的 `grep` 扫描确保无残留 |

## 测试策略

- 删除 `Delete_WithRolePermissions_CascadeDeletes` 测试（功能已移除）
- 简化涉及 `_mockRpRepo` 的现有测试（去掉不再需要的 mock setup）
- 所有其他测试必须不变且通过

## 后续事项

无（本次变更为完整的模块移除，无后续依赖）