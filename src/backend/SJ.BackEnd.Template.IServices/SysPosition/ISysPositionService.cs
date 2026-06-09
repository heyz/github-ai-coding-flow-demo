#region  <<版本注释>>
/* ==============================================================================
// <copyright file="ISysPositionService.cs" company="Shiji.BO.CS">
// Copyright (c) SJ.BO.CS. All rights reserved.
// </copyright>
* 功能描述：ISysPositionService
* 创 建 者：何应芝
* 创建时间：2026/6/9 16:33:18
* ==============================================================================*/
#endregion

namespace SJ.BackEnd.Template.IServices;

/// <summary>
/// 岗位服务接口
/// </summary>
public interface ISysPositionService : IBaseServices<SysPosition>
{
    /// <summary>
    /// 分页查询岗位列表
    /// </summary>
    Task<PageModel<SysPosition>> GetPagedList(int pageIndex, int pageSize, string? keyword);

    /// <summary>
    /// 创建岗位
    /// </summary>
    /// <returns>新岗位对象，名称/编码重复时返回 null</returns>
    Task<SysPosition?> Create(CreatePositionRequest request);

    /// <summary>
    /// 更新岗位
    /// </summary>
    /// <returns>是否更新成功</returns>
    Task<bool> Update(long id, UpdatePositionRequest request);

    /// <summary>
    /// 删除岗位
    /// </summary>
    /// <returns>是否删除成功</returns>
    Task<bool> Delete(long id);
}