#region  <<版本注释>>
/* ==============================================================================
// <copyright file="ISysUserPositionService.cs" company="Shiji.BO.CS">
// Copyright (c) SJ.BO.CS. All rights reserved.
// </copyright>
* 功能描述：ISysUserPositionService
* 创 建 者：何应芝
* 创建时间：2026/6/10 00:00:00
* ==============================================================================*/
#endregion

namespace SJ.BackEnd.Template.IServices;

/// <summary>
/// 用户岗位关联服务接口
/// </summary>
public interface ISysUserPositionService : IBaseServices<SysUserPosition>
{
    /// <summary>
    /// 绑定用户到岗位
    /// </summary>
    Task<bool> Bind(long userId, long positionId);

    /// <summary>
    /// 解绑用户岗位
    /// </summary>
    Task<bool> Unbind(long userId, long positionId);

    /// <summary>
    /// 查询指定用户的所有岗位
    /// </summary>
    Task<List<SysPosition>> GetPositionsByUserId(long userId);

    /// <summary>
    /// 查询指定岗位下的所有用户
    /// </summary>
    Task<List<SysUser>> GetUsersByPositionId(long positionId);
}