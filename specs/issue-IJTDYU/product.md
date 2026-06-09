# 岗位模块 — 产品规格

> **Issue:** [#IJTDYU](https://gitee.com/heyz/ai-coding-flow/issues/IJTDYU)
> **标签:** `feature`
> **类型:** 任务

## 1. 概述

新增"岗位管理"模块，支持对组织内的岗位（职位/职务）进行集中管理。岗位是组织架构中的基本单元，标识一个员工在组织中的职能位置（如"软件工程师"、"项目经理"、"技术总监"）。

## 2. 问题

当前系统缺少对"岗位"的管理功能。在实际业务中，岗位是一个核心基础数据维度，用于标识员工的职能角色。缺少岗位管理功能会导致后续员工信息的结构化程度受限。

## 3. 目标

- 提供岗位的基础 CRUD 能力
- 支持分页查询和关键词检索
- 与现有 SysRole（角色）、SysUser（用户）模块的风格和接口规范保持一致

## 4. 非目标

- 不涉及岗位与用户的关联关系管理（后续可通过用户扩展字段实现）
- 不涉及岗位与角色的关联关系管理（后续可通过单独模块实现）
- 不涉及岗位的树形层级结构（扁平化管理）
- 不涉及前端 UI

## 5. Figma / 设计参考

无 Figma 设计稿。

## 6. 用户体验（API 行为）

### 6.1 实体定义

岗位实体包含以下字段：

| 字段 | 类型 | 说明 | 约束 |
|------|------|------|------|
| Id | long | 主键 | 自增，系统生成 |
| Name | string | 岗位名称 | 必填，最长 50 字符，唯一 |
| Code | string | 岗位编码 | 必填，最长 50 字符，唯一 |
| Description | string? | 岗位描述 | 可选，最长 200 字符 |
| SortOrder | int | 排序序号 | 默认 0 |
| IsSystem | bool | 是否系统内置 | 默认 false，系统内置岗位不可删除 |
| CreatedAt | DateTime | 创建时间 | 系统自动填充 |
| UpdatedAt | DateTime | 更新时间 | 系统自动填充 |

### 6.2 API 端点

#### GET `position/list` — 分页查询岗位列表

**参数：**
- `pageIndex` (int, query, 默认 1)
- `pageSize` (int, query, 默认 10)
- `keyword` (string?, query, 可选) — 模糊匹配 Name 和 Code

**响应：** `ApiResponse<PageModel<SysPosition>>`

- 返回分页数据，按 `SortOrder ASC, Id DESC` 排序
- `keyword` 为空时返回全部岗位

#### GET `position/{id}` — 获取岗位详情

**参数：**
- `id` (long, route)

**响应：** `ApiResponse<SysPosition>`

- 岗位存在时返回实体数据
- 岗位不存在时返回 `success: false, msg: "岗位不存在"`（由服务层行为决定，统一由 Controller 的 Fail 处理）

#### POST `position` — 创建岗位

**请求体：** `CreatePositionRequest`
- `Name` (string) — 必填，最长 50
- `Code` (string) — 必填，最长 50
- `Description` (string?, 可选) — 最长 200
- `SortOrder` (int, 默认 0)

**响应：** `ApiResponse<SysPosition>`

- 创建成功返回岗位实体
- 岗位名称或编码重复时返回 `success: false, msg: "岗位名称或编码已存在"`

#### PUT `position/{id}` — 更新岗位

**请求体：** `UpdatePositionRequest`
- `Name` (string) — 必填，最长 50
- `Code` (string) — 必填，最长 50
- `Description` (string?, 可选) — 最长 200
- `SortOrder` (int, 默认 0)

**响应：** `ApiResponse<bool>`

- 更新成功返回 `true`
- 岗位不存在或名称/编码被其他岗位占用时返回 `success: false, msg: "岗位不存在或名称已存在"`

#### DELETE `position/{id}` — 删除岗位

**参数：**
- `id` (long, route)

**响应：** `ApiResponse<bool>`

- 删除成功返回 `true`
- 岗位不存在或为系统内置岗位时返回 `success: false, msg: "删除失败，岗位不存在或为系统内置岗位"`

### 6.3 实体行为

- 岗位名称和编码在系统内唯一
- 系统内置岗位（`IsSystem = true`）不可删除
- 创建/更新时自动填充/更新 `CreatedAt`、`UpdatedAt`

## 7. 成功标准

- [x] 能通过 API 创建岗位，名称/编码重复时明确拒绝
- [x] 能通过 API 更新岗位，更新时检查唯一性排除自身
- [x] 能通过 API 删除岗位，系统内置岗位拒绝删除
- [x] 能通过 API 按分页查询岗位列表，支持关键词模糊搜索
- [x] 能通过 API 根据 ID 获取单个岗位详情
- [x] 岗位名称和编码满足唯一性约束
- [x] 排序字段 `SortOrder` 影响列表返回顺序

## 8. 验证方式

- 通过 `dotnet test` 运行单元测试覆盖 CRUD 各路径
- 通过 curl/Postman 手动验证 API 响应格式和状态码

## 9. 开放问题

无。