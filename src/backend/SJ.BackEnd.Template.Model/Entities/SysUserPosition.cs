#region  <<版本注释>>
/* ==============================================================================
// <copyright file="SysUserPosition.cs" company="Shiji.BO.CS">
// Copyright (c) SJ.BO.CS. All rights reserved.
// </copyright>
* 功能描述：SysUserPosition
* 创 建 者：何应芝
* 创建时间：2026/6/10 00:00:00
* ==============================================================================*/
#endregion

using SqlSugar;

namespace SJ.BackEnd.Template.Model;

/// <summary>
/// 用户岗位关联实体
/// </summary>
[SugarTable("sys_user_position")]
[Tenant("2")]
public class SysUserPosition
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public long UserId { get; set; }

    public long PositionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 创建用户岗位关联
    /// </summary>
    public static SysUserPosition CreateRelation(long userId, long positionId)
    {
        return new SysUserPosition
        {
            UserId = userId,
            PositionId = positionId,
            CreatedAt = DateTime.Now
        };
    }
}