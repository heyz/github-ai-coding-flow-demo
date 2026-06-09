# 技术规格：角色关联权限模块

## 架构决策

- **实体设计**：新建 `SysRolePermission` 关联实体（充血模型），遵循项目现有 `[SugarTable]` + `[Tenant]` 约定
- **接口层级**：`ISysRolePermissionService` → `SysRolePermissionService` → `IBaseRepository<SysRolePermission>`，遵循项目分层架构
- **权限分配策略**：采用"全量替换"模式 —— 每次分配时先删除旧关联再批量插入新关联
- **级联删除**：在 Service 层实现，角色/权限删除时主动清理关联表

## 涉及文件

| 文件 | 操作 | 说明 |
|------|------|------|
| `Model/Entities/SysRolePermission.cs` | 新建 | 关联实体 |
| `Model/Dtos/SysRolePermission/AssignPermissionsRequest.cs` | 新建 | 分配权限请求 DTO |
| `IServices/SysRolePermission/ISysRolePermissionService.cs` | 新建 | 服务接口 |
| `Services/SysRolePermission/SysRolePermissionService.cs` | 新建 | 服务实现 |
| `WebAPI/Controllers/SysRolePermissionController.cs` | 新建 | API 控制器 |
| `Services/SysRole/SysRoleService.cs` | 修改 | 删除角色时级联删除关联 |
| `Services/SysPermission/SysPermissionService.cs` | 修改 | 删除权限时级联删除关联 |

## 实体设计

```csharp
[SugarTable("sys_role_permission")]
[Tenant("2")]
public class SysRolePermission
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }
    public long RoleId { get; set; }
    public long PermissionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
```

## API 设计

| 方法 | 路由 | 说明 |
|------|------|------|
| POST | `role/{roleId}/permissions` | 为角色批量分配权限（全量替换） |
| GET | `role/{roleId}/permissions` | 获取角色的权限列表 |
| DELETE | `role/{roleId}/permissions/{permissionId}` | 移除角色的某个权限关联 |

## 数据流

```
POST role/{roleId}/permissions
  → Controller: 解析 roleId + permissionIds
  → Service: 验证角色存在 + 权限存在
  → Service: BeginTran → 删除旧关联 → 批量插入新关联 → CommitTran
  → 返回更新后的权限列表
```

## 依赖关系

- `SysRolePermissionService` 依赖 `IBaseRepository<SysRolePermission>` + `IBaseServices<SysRole>` + `IBaseServices<SysPermission>`
- `SysRoleService.Delete` 增加对 `IBaseRepository<SysRolePermission>` 的依赖
- `SysPermissionService.Delete` 增加对 `IBaseRepository<SysRolePermission>` 的依赖

## 测试策略

### 单元测试 (`SysRolePermissionServiceTests`)

| 测试用例 | 覆盖 |
|---------|------|
| `AssignPermissions_ValidRoleAndPermissions_DeletesOldAndInsertsNew` | AC1 正常流程 |
| `AssignPermissions_EmptyList_ClearsAllPermissions` | 边缘情况 |
| `AssignPermissions_RoleNotFound_ReturnsFalse` | 边缘情况 |
| `GetPermissionsByRole_ExistingRole_ReturnsPermissions` | AC2 |
| `GetPermissionsByRole_NonExistingRole_ReturnsEmpty` | 边缘情况 |
| `RemovePermission_ValidIds_RemovesAssociation` | AC3 |
| `RemovePermission_DuplicateCall_ReturnsFalse` | AC6 |

### 集成验证

- `dotnet build` 通过
- `dotnet test` 全部通过

## 风险

| 风险 | 缓解措施 |
|------|---------|
| 并发分配导致数据不一致 | 使用事务（BeginTran/CommitTran）包裹删除+插入 |
| 级联删除遗漏 | 单元测试覆盖 AC4/AC5 |
