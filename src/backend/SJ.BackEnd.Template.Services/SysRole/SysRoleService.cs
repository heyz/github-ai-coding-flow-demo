#region  <<版本注释>>
/* ==============================================================================
// <copyright file="SysRoleService.cs" company="Shiji.BO.CS">
// Copyright (c) SJ.BO.CS. All rights reserved.
// </copyright>
* 功能描述：SysRoleService
* 创 建 者：何应芝
* 创建时间：2026/6/8 22:00:00
* ==============================================================================*/
#endregion

namespace SJ.BackEnd.Template.Services;

public class SysRoleService(IBaseRepository<SysRole> repository) : BaseServices<SysRole>(repository), ISysRoleService
{
    public async Task<PageModel<SysRole>> GetPagedList(int pageIndex, int pageSize, string? keyword)
    {
        var whereExpression = ExpressionBuilder
            .Build<SysRole>()
            .WhereIF(!string.IsNullOrWhiteSpace(keyword),u => u.Name.Contains(keyword!) || u.Code.Contains(keyword!));

        string orderByFields = "SortOrder asc, Id desc";
        return await base.QueryPagedByExpression(whereExpression, pageIndex, pageSize, orderByFields);
    }

    public async Task<SysRole?> Create(CreateRoleRequest request)
    {
        // 名称唯一性校验
        if (await base.Exist(u => u.Name == request.Name))
            return null;

        var role = SysRole.CreateFrom(request);

        var newId = await base.Insert(role);
        return await base.GetById(newId);
    }

    public async Task<bool> Update(long id, UpdateRoleRequest request)
    {
        // 名称唯一性校验（排除自身）
        if (await base.Exist(u => u.Name == request.Name && u.Id != id))
            return false;

        var role = await base.GetById(id);
        if (role == null)
            return false;

        role.Name = request.Name;
        role.Code = request.Code;
        role.Description = request.Description ?? string.Empty;
        role.SortOrder = request.SortOrder;
        role.UpdatedAt = DateTime.Now;

        return await base.Update(role);
    }

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