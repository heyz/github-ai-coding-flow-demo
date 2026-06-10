#region  <<版本注释>>
/* ==============================================================================
// <copyright file="SysUserPositionService.cs" company="Shiji.BO.CS">
// Copyright (c) SJ.BO.CS. All rights reserved.
// </copyright>
* 功能描述：SysUserPositionService
* 创 建 者：何应芝
* 创建时间：2026/6/10 00:00:00
* ==============================================================================*/
#endregion

namespace SJ.BackEnd.Template.Services;

/// <summary>
/// 用户岗位关联服务实现
/// </summary>
public class SysUserPositionService(
    IBaseRepository<SysUserPosition> repository,
    IBaseRepository<SysPosition> positionRepository,
    IBaseRepository<SysUser> userRepository
) : BaseServices<SysUserPosition>(repository), ISysUserPositionService
{
    public async Task<bool> Bind(long userId, long positionId)
    {
        // 检查用户是否存在
        var user = await userRepository.GetById(userId);
        if (user == null)
            return false;

        // 检查岗位是否存在
        var position = await positionRepository.GetById(positionId);
        if (position == null)
            return false;

        // 检查是否已存在绑定关系
        if (await base.Exist(u => u.UserId == userId && u.PositionId == positionId))
            return false;

        var relation = SysUserPosition.CreateRelation(userId, positionId);
        var newId = await base.Insert(relation);
        return newId > 0;
    }

    public async Task<bool> Unbind(long userId, long positionId)
    {
        var relations = await base.QueryByExpression(u => u.UserId == userId && u.PositionId == positionId);
        if (!relations.Any())
            return false;

        return await base.Delete(relations.First());
    }

    public async Task<List<SysPosition>> GetPositionsByUserId(long userId)
    {
        var relations = await base.QueryByExpression(u => u.UserId == userId);
        if (!relations.Any())
            return new List<SysPosition>();

        var positionIds = relations.Select(r => r.PositionId).ToArray();
        return await positionRepository.GetByIds(positionIds.Cast<object>().ToArray());
    }

    public async Task<List<SysUser>> GetUsersByPositionId(long positionId)
    {
        var relations = await base.QueryByExpression(u => u.PositionId == positionId);
        if (!relations.Any())
            return new List<SysUser>();

        var userIds = relations.Select(r => r.UserId).ToArray();
        return await userRepository.GetByIds(userIds.Cast<object>().ToArray());
    }
}