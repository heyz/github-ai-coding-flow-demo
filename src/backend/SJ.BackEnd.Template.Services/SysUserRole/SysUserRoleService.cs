#region  <<版本注释>>
/* ==============================================================================
// <copyright file="SysUserRoleService.cs" company="Shiji.BO.CS">
// Copyright (c) SJ.BO.CS. All rights reserved.
// </copyright>
* 功能描述：SysUserRoleService
* 创 建 者：何应芝
* 创建时间：2026/6/8 23:00:00
* ==============================================================================*/
#endregion

using SJ.BackEnd.Template.IServices;

namespace SJ.BackEnd.Template.Services;

public class SysUserRoleService(
    IBaseRepository<SysUserRole> repository,
    IBaseRepository<SysRole> roleRepository,
    IBaseRepository<SysUser> userRepository
) : BaseServices<SysUserRole>(repository), ISysUserRoleService
{
    public async Task<bool> Bind(long userId, long roleId)
    {
        // 检查用户是否存在
        var user = await userRepository.GetById(userId);
        if (user == null)
            return false;

        // 检查角色是否存在
        var role = await roleRepository.GetById(roleId);
        if (role == null)
            return false;

        // 检查是否已存在绑定关系
        if (await base.Exist(u => u.UserId == userId && u.RoleId == roleId))
            return false;

        var relation = SysUserRole.CreateRelation(userId, roleId);
        var newId = await base.Insert(relation);
        return newId > 0;
    }

    public async Task<bool> Unbind(long userId, long roleId)
    {
        var relations = await base.QueryByExpression(u => u.UserId == userId && u.RoleId == roleId);
        if (!relations.Any())
            return false;

        return await base.Delete(relations.First());
    }

    public async Task<List<SysRole>> GetRolesByUserId(long userId)
    {
        var relations = await base.QueryByExpression(u => u.UserId == userId);
        if (!relations.Any())
            return new List<SysRole>();

        var roleIds = relations.Select(r => r.RoleId).ToArray();
        return await roleRepository.GetByIds(roleIds.Cast<object>().ToArray());
    }

    public async Task<List<SysUser>> GetUsersByRoleId(long roleId)
    {
        var relations = await base.QueryByExpression(u => u.RoleId == roleId);
        if (!relations.Any())
            return new List<SysUser>();

        var userIds = relations.Select(r => r.UserId).ToArray();
        return await userRepository.GetByIds(userIds.Cast<object>().ToArray());
    }
}
