#region  <<版本注释>>
/* ==============================================================================
// <copyright file="SysPositionService.cs" company="Shiji.BO.CS">
// Copyright (c) SJ.BO.CS. All rights reserved.
// </copyright>
* 功能描述：SysPositionService
* 创 建 者：何应芝
* 创建时间：2026/6/9 16:33:18
* ==============================================================================*/
#endregion

using Mapster;

namespace SJ.BackEnd.Template.Services;

/// <summary>
/// 岗位服务实现
/// </summary>
public class SysPositionService(IBaseRepository<SysPosition> repository) : BaseServices<SysPosition>(repository), ISysPositionService
{
    public async Task<PageModel<SysPosition>> GetPagedList(int pageIndex, int pageSize, string? keyword)
    {
        var whereExpression = ExpressionBuilder
            .Build<SysPosition>()
            .WhereIF(!string.IsNullOrWhiteSpace(keyword), u => u.Name.Contains(keyword!) || u.Code.Contains(keyword!));

        string orderByFields = "SortOrder asc, Id desc";
        return await base.QueryPagedByExpression(whereExpression, pageIndex, pageSize, orderByFields);
    }

    public async Task<SysPosition?> Create(CreatePositionRequest request)
    {
        // 名称唯一性校验
        if (await base.Exist(u => u.Name == request.Name))
            return null;

        // 编码唯一性校验
        if (await base.Exist(u => u.Code == request.Code))
            return null;

        var position = SysPosition.Create(request);

        var newId = await base.Insert(position);
        return await base.GetById(newId);
    }

    public async Task<bool> Update(long id, UpdatePositionRequest request)
    {
        // 名称唯一性校验（排除自身）
        if (await base.Exist(u => u.Name == request.Name && u.Id != id))
            return false;

        // 编码唯一性校验（排除自身）
        if (await base.Exist(u => u.Code == request.Code && u.Id != id))
            return false;

        var position = await base.GetById(id);
        if (position == null)
            return false;

        request.Adapt(position);
        position.UpdatedAt = DateTime.Now;

        return await base.Update(position);
    }

    public async Task<bool> Delete(long id)
    {
        var position = await base.GetById(id);
        if (position == null)
            return false;
        if (position.IsSystem)
            return false;

        return await base.DeleteById(id);
    }
}