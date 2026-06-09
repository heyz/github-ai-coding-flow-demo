# 产品规格：实体充血模型与控制器业务逻辑剥离

## 1. 概述

当前代码库中，Controller 层包含了部分业务校验逻辑（如 SysRoleController.Delete 中检查角色是否为系统内置角色），且 Service 层直接通过 `new SysUser { ... }` 的方式手动构造实体对象，未充分利用面向对象的充血模型模式。

本规格旨在重构 Controller 和 Service 的业务逻辑分层，将业务逻辑下沉到 Service 层和 Entity 层，并在 CLAUDE.md 中记录架构规范，防止后续代码再次出现同样问题。

## 2. 问题背景

### 2.1 控制器包含业务逻辑

当前部分 Controller 中存在不应属于该层的业务校验和判断逻辑：

- **SysRoleController.Delete**：直接检查 `role.IsSystem` 并返回错误提示，该判断应属于 Service 层
- **SysPermissionController.Delete**：检查子节点是否存在并返回错误提示，该判断应属于 Service 层
- **SysRoleController.GetById**：检查 `role == null` 并返回"角色不存在"

Controller 的职责应当是：接收 HTTP 请求、参数校验、调用 Service、返回统一格式的响应。不应包含业务规则的判断。

### 2.2 Service 中手动构造实体

当前 Service 在创建实体时直接使用 `new SysUser { ... }` 或 `new SysRole { ... }` 进行属性赋值：

```csharp
var user = new SysUser
{
    Id = 0,
    Nickname = request.Nickname,
    RealName = request.RealName,
    Gender = request.Gender,
    BirthDate = request.BirthDate,
    CreatedTime = DateTime.Now
};
```

这种方式：
- 将实体构造逻辑分散在各个 Service 中，不利于复用
- 实体本身（贫血模型）只是数据容器，没有封装业务行为
- 违反 DDD 中实体自包含业务逻辑的原则

### 2.3 架构规范未明确记录

CLAUDE.md 中虽然描述了项目架构和部分编码约定，但没有明确：
- Controller 不得包含业务逻辑（仅处理 HTTP 交互）
- 实体应采用充血模型，将构造和业务行为封装在实体内部

## 3. 目标

1. **将 Controller 中的业务校验逻辑下沉到 Service 层**，使 Controller 仅负责 HTTP 请求响应
2. **引入实体充血模型实践**，将实体创建和业务行为封装到实体内部方法中
3. **更新 CLAUDE.md**，用中文记录上述两条架构规则，防止后续代码出现问题

## 4. 非目标

- 不改变当前 API 的请求/响应格式
- 不修改业务逻辑的正确性（仅仅是位置和分层调整）
- 不引入新的实体或数据库表
- 不改变项目结构或依赖关系
- 不涉及前端代码

## 5. 设计参考

无 Figma 设计稿（纯后端架构重构，无 UI 变更）。

## 6. 用户体验

本次变更对 API 消费者**完全透明**：
- 所有 API 的 URL、HTTP 方法、请求参数、响应格式保持不变
- 所有错误提示信息保持不变
- 所有业务约束和校验保持不变

变更仅影响代码内部的组织方式。

## 7. 成功标准

### 7.1 Controller 业务逻辑下沉

以下业务逻辑必须从 Controller 迁移到 Service：

| Controller | 待迁移逻辑 | 目标 |
|---|---|---|
| SysRoleController.Delete | `role.IsSystem` 系统角色判断 | SysRoleService.Delete 内处理 |
| SysRoleController.GetById | `role == null` 返回"角色不存在" | SysRoleService.GetById 内处理 |
| SysPermissionController.Delete | 子节点存在性判断 | SysPermissionService.Delete 内处理 |
| SysPermissionController.GetById | `permission == null` 返回"权限不存在" | SysPermissionService.GetById 内处理 |

### 7.2 实体充血模型

- `SysUser` 实体添加 `CreateFrom(CreateUserRequest)` 静态工厂方法，封装实体创建逻辑
- `SysRole` 实体添加 `CreateFrom(CreateRoleRequest)` 静态工厂方法，封装实体创建逻辑
- `SysUserService.Create` 和 `SysRoleService.Create` 改为调用实体的静态工厂方法

### 7.3 CLAUDE.md 规范

CLAUDE.md 新增以下规则（中文）：
1. **Controller 职责边界**：Controller 不得包含业务校验逻辑，仅处理 HTTP 参数解析、调用 Service、返回响应
2. **实体充血模型**：实体应封装业务行为，通过静态工厂方法或实例方法构造业务实体，Service 不得手动通过 `new` 赋值属性

## 8. 验证方式

- Controller 中无业务逻辑判断（如 `if (role.IsSystem)` 等），均在 Service 层处理
- 实体中存在 `CreateFrom` 等工厂/业务方法
- CLAUDE.md 包含中文架构规则
- `dotnet build` 通过
- 所有 API 请求返回结果与重构前一致

## 9. 待定事项

- `SysUserRoleController` 和 `WeatherForecastController` 是否需要同步审查和重构（待代码审查时确认）
- 是否需要为 `SysPermissionService` 添加自定义 Service 接口（当前权限管理使用了 `IBaseServices<SysPermission>` 泛型服务，但 Delete 业务逻辑在 Controller 中）
