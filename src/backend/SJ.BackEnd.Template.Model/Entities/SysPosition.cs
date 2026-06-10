#region  <<版本注释>>
/* ==============================================================================
// <copyright file="SysPosition.cs" company="Shiji.BO.CS">
// Copyright (c) SJ.BO.CS. All rights reserved.
// </copyright>
* 功能描述：SysPosition
* 创 建 者：何应芝
* 创建时间：2026/6/9 16:33:18
* ==============================================================================*/
#endregion

using SqlSugar;

namespace SJ.BackEnd.Template.Model;

/// <summary>
/// 岗位实体
/// </summary>
[SugarTable("sys_position")]
[Tenant("2")]
public class SysPosition
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 岗位名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 岗位编码
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 岗位描述
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// 排序序号
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// 是否系统内置
    /// </summary>
    public bool IsSystem { get; set; } = false;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 从创建请求构造岗位实体
    /// </summary>
    public static SysPosition Create(CreatePositionRequest request)
    {
        return new SysPosition
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description ?? string.Empty,
            SortOrder = request.SortOrder,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
    }
}