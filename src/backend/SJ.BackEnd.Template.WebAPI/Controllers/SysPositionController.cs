#region  <<版本注释>>
/* ==============================================================================
// <copyright file="SysPositionController.cs" company="Shiji.BO.CS">
// Copyright (c) SJ.BO.CS. All rights reserved.
// </copyright>
* 功能描述：SysPositionController
* 创 建 者：何应芝
* 创建时间：2026/6/9 16:33:18
* ==============================================================================*/
#endregion

namespace SJ.BackEnd.Template.WebAPI.Controllers;

/// <summary>
/// 岗位管理控制器
/// </summary>
[ApiController]
[Route("position")]
public class SysPositionController(ISysPositionService sysPositionService) : ControllerBase
{
    /// <summary>
    /// 分页查询岗位列表
    /// </summary>
    [HttpGet("list")]
    public async Task<ApiResponse<PageModel<SysPosition>>> GetList([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? keyword = null)
    {
        var result = await sysPositionService.GetPagedList(pageIndex, pageSize, keyword);
        return ApiResponse<PageModel<SysPosition>>.Success("查询成功", result);
    }

    /// <summary>
    /// 根据 ID 获取岗位详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ApiResponse<SysPosition>> GetById(long id)
    {
        var position = await sysPositionService.GetById(id);
        return ApiResponse<SysPosition>.Success("获取成功", position);
    }

    /// <summary>
    /// 创建岗位
    /// </summary>
    [HttpPost]
    public async Task<ApiResponse<SysPosition>> Create([FromBody] CreatePositionRequest request)
    {
        var result = await sysPositionService.Create(request);
        if (result == null)
            return ApiResponse<SysPosition>.Fail("岗位名称或编码已存在");
        return ApiResponse<SysPosition>.Success("创建成功", result);
    }

    /// <summary>
    /// 更新岗位
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ApiResponse<bool>> Update(long id, [FromBody] UpdatePositionRequest request)
    {
        var result = await sysPositionService.Update(id, request);
        if (!result)
            return ApiResponse<bool>.Fail("岗位不存在或名称已存在");
        return ApiResponse<bool>.Success("更新成功", true);
    }

    /// <summary>
    /// 删除岗位
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ApiResponse<bool>> Delete(long id)
    {
        var result = await sysPositionService.Delete(id);
        if (!result)
            return ApiResponse<bool>.Fail("删除失败，岗位不存在或为系统内置岗位");
        return ApiResponse<bool>.Success("删除成功", true);
    }
}