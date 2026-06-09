# 技术规格：岗位管理模块

> **Issue:** [#IJTDYU](https://gitee.com/heyz/ai-coding-flow/issues/IJTDYU)
> **产品规格:** [product.md](./product.md)

## 1. 问题

系统缺少"岗位"（Position）管理功能，无法对组织内的岗位进行维护。需要新增一个完整的 CRUD 模块，遵循项目现有的层级架构和编码规范。

## 2. 相关代码

### 2.1 参考模板（SysRole 模块）

| 文件 | 作用 |
|------|------|
| `SJ.BackEnd.Template.Model/Entities/SysRole.cs` | 角色实体（岗位实体的对照模板） |
| `SJ.BackEnd.Template.Model/Dtos/SysRole/CreateRoleRequest.cs` | 角色创建请求 DTO |
| `SJ.BackEnd.Template.Model/Dtos/SysRole/UpdateRoleRequest.cs` | 角色更新请求 DTO |
| `SJ.BackEnd.Template.IServices/SysRole/ISysRoleService.cs` | 角色服务接口 |
| `SJ.BackEnd.Template.Services/SysRole/SysRoleService.cs` | 角色服务实现 |
| `SJ.BackEnd.Template.WebAPI/Controllers/SysRoleController.cs` | 角色控制器 |

### 2.2 基础框架层（无需改动）

| 文件 | 作用 |
|------|------|
| `SJ.BackEnd.Template.IRepository/BASE/IBaseRepository.cs` | 基础仓储接口 |
| `SJ.BackEnd.Template.Repository/BASE/BaseRepository.cs` | 基础仓储实现 |
| `SJ.BackEnd.Template.IServices/BASE/IBaseServices.cs` | 基础服务接口 |
| `SJ.BackEnd.Template.Services/BaseService.cs` | 基础服务实现 |
| `SJ.BackEnd.Template.Services/ServicesAutofacModule.cs` | Autofac 自动注册（无需修改） |

## 3. 当前状态

当前系统中不存在任何岗位（Position）相关的代码。实体、DTO、Service、Controller 各层均需新建。

## 4. 变更方案

### 4.1 新增实体：SysPosition

**文件：** `SJ.BackEnd.Template.Model/Entities/SysPosition.cs`

```csharp
[SugarTable("sys_position")]
[Tenant("2")]
public class SysPosition
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
    public bool IsSystem { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public static SysPosition CreateFrom(CreatePositionRequest request) { ... }
}
```

- 表名 `sys_position`，与 `sys_user`、`sys_permission` 等命名一致
- 使用 `Tenant("2")`，与业务模块（SysUser、SysRole、SysPermission）同库
- 自动增长主键 `Id`

### 4.2 新增 DTO

**CreatePositionRequest.cs** — `SJ.BackEnd.Template.Model/Dtos/SysPosition/`
```csharp
public class CreatePositionRequest
{
    [Required(ErrorMessage = "岗位名称不能为空")]
    [StringLength(50, ErrorMessage = "岗位名称长度不能超过{1}个字符")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "岗位编码不能为空")]
    [StringLength(50, ErrorMessage = "岗位编码长度不能超过{1}个字符")]
    public string Code { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "描述长度不能超过{1}个字符")]
    public string? Description { get; set; }

    public int SortOrder { get; set; } = 0;
}
```

**UpdatePositionRequest.cs** — `SJ.BackEnd.Template.Model/Dtos/SysPosition/`

与 Create 相同字段结构（不含 Id）。

### 4.3 新增服务接口

**文件：** `SJ.BackEnd.Template.IServices/SysPosition/ISysPositionService.cs`

```csharp
public interface ISysPositionService : IBaseServices<SysPosition>
{
    Task<PageModel<SysPosition>> GetPagedList(int pageIndex, int pageSize, string? keyword);
    Task<SysPosition?> Create(CreatePositionRequest request);
    Task<bool> Update(long id, UpdatePositionRequest request);
    Task<bool> Delete(long id);
}
```

### 4.4 新增服务实现

**文件：** `SJ.BackEnd.Template.Services/SysPosition/SysPositionService.cs`

```csharp
public class SysPositionService(IBaseRepository<SysPosition> repository)
    : BaseServices<SysPosition>(repository), ISysPositionService
{
    // GetPagedList — 按 SortOrder ASC, Id DESC 排序，keyword 模糊匹配 Name/Code
    // Create — 检查 Name/Code 唯一性，调用 SysPosition.CreateFrom()
    // Update — 检查唯一性（排除自身），加载实体更新字段
    // Delete — 检查存在性和 IsSystem 标记
}
```

业务规则：
- **创建：** 名称/编码重复 → 返回 null（Controller 层返回 Fail 消息）
- **更新：** 名称/编码被其他岗位占用 → 返回 false；岗位不存在 → 返回 false
- **删除：** 系统内置岗位（`IsSystem = true`）不可删除 → 返回 false

### 4.5 新增控制器

**文件：** `SJ.BackEnd.Template.WebAPI/Controllers/SysPositionController.cs`

```csharp
[ApiController]
[Route("position")]
public class SysPositionController(ISysPositionService sysPositionService) : ControllerBase
{
    [HttpGet("list")]     → 分页查询
    [HttpGet("{id}")]     → 获取详情
    [HttpPost]             → 创建
    [HttpPut("{id}")]      → 更新
    [HttpDelete("{id}")]   → 删除
}
```

- 路由 `position`，与现有 `role`、`user-role`、`permission` 风格一致
- Controller 不含业务逻辑，仅负责参数解析和统一响应包装

### 4.6 Autofac 注册

无需修改 `ServicesAutofacModule.cs`，因为是按照约定自动注册：
```csharp
builder.RegisterAssemblyTypes(assembly)
    .Where(t => t.Name.EndsWith("Service"))
    .AsImplementedInterfaces()
    .InstancePerLifetimeScope();
```

`SysPositionService` 实现 `ISysPositionService`，Autofac 会自动发现并注册。

## 5. 端到端流程

用户请求岗位列表 → HTTP GET /position/list → SysPositionController.GetList → ISysPositionService.GetPagedList → BaseServices.QueryPagedByExpression → BaseRepository.QueryPagedByExpression → SqlSugar 查询数据库 → 逐层返回 → ApiResponse<PageModel<SysPosition>> 序列化为 JSON

## 6. 风险与缓解

| 风险 | 影响 | 缓解措施 |
|------|------|----------|
| 表名 `sys_position` 与已有表冲突 | 编译通过但运行时报错 | 查询现有表名清单确认无冲突 |
| 唯一性约束缺少数据库索引 | 并发场景可能重复插入 | Repository 层可添加唯一索引创建脚本（本次不覆盖，依赖 Service 层的业务校验） |

## 7. 测试与验证

### 7.1 单元测试

**文件：** `SJ.BackEnd.Template.Tests/Services/SysPositionServiceTests.cs`

测试用例覆盖：

| 用例 | 验证点 |
|------|--------|
| `Create_ShouldReturnEntity_WhenDataValid` | 正常创建返回岗位实体 |
| `Create_ShouldReturnNull_WhenNameExists` | 名称重复返回 null |
| `Create_ShouldReturnNull_WhenCodeExists` | 编码重复返回 null |
| `Update_ShouldReturnTrue_WhenDataValid` | 正常更新返回 true |
| `Update_ShouldReturnFalse_WhenNameExistsByOther` | 更新时名称被其他岗位占用 |
| `Update_ShouldReturnFalse_WhenNotExists` | 更新不存在的岗位 |
| `Delete_ShouldReturnTrue_WhenNotSystem` | 删除非系统岗位 |
| `Delete_ShouldReturnFalse_WhenIsSystem` | 删除系统内置岗位 |
| `Delete_ShouldReturnFalse_WhenNotExists` | 删除不存在的岗位 |
| `GetPagedList_ShouldFilterByKeyword` | 关键词搜索按 Name/Code 过滤 |
| `GetPagedList_ShouldReturnAll_WhenKeywordEmpty` | 空关键词返回全部 |

### 7.2 编译验证

```bash
dotnet build src/backend/SH.BackEnd.Tempalte.sln
```

### 7.3 测试运行

```bash
dotnet test src/backend/SJ.BackEnd.Template.Tests --no-restore
```

## 8. 后续工作

- 岗位与用户的关联关系（如设置用户的岗位）可作为独立模块后续实现
- 考虑在数据库层面为 `Name` 和 `Code` 添加唯一索引