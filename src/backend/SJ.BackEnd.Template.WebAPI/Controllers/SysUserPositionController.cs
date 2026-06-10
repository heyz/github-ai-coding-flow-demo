#region  <<版本注释>>
/* ==============================================================================
// <copyright file="SysUserPositionController.cs" company="Shiji.BO.CS">
// Copyright (c) SJ.BO.CS. All rights reserved.
// </copyright>
* 功能描述：SysUserPositionController
* 创 建 者：何应芝
* 创建时间：2026/6/10 00:00:00
* ==============================================================================*/
#endregion

namespace SJ.BackEnd.Template.WebAPI.Controllers;

/// <summary>
/// 用户岗位关联控制器
/// </summary>
[ApiController]
[Route("user-position")]
public class SysUserPositionController(ISysUserPositionService sysUserPositionService) : ControllerBase
{
    /// <summary>
    /// 绑定用户到岗位
    /// </summary>
    [HttpPost("bind")]
    public async Task<ApiResponse<bool>> Bind([FromBody] BindUserPositionRequest request)
    {
        var result = await sysUserPositionService.Bind(request.UserId, request.PositionId);
        if (!result)
            return ApiResponse<bool>.Fail("绑定失败，用户/岗位不存在或已绑定");
        return ApiResponse<bool>.Success("绑定成功", true);
    }

    /// <summary>
    /// 解绑用户岗位
    /// </summary>
    [HttpPost("unbind")]
    public async Task<ApiResponse<bool>> Unbind([FromBody] UnbindUserPositionRequest request)
    {
        var result = await sysUserPositionService.Unbind(request.UserId, request.PositionId);
        if (!result)
            return ApiResponse<bool>.Fail("解绑失败，绑定关系不存在");
        return ApiResponse<bool>.Success("解绑成功", true);
    }

    /// <summary>
    /// 查询指定用户的所有岗位
    /// </summary>
    [HttpGet("user/{userId}/positions")]
    public async Task<ApiResponse<List<SysPosition>>> GetPositionsByUserId(long userId)
    {
        var positions = await sysUserPositionService.GetPositionsByUserId(userId);
        return ApiResponse<List<SysPosition>>.Success("查询成功", positions);
    }

    /// <summary>
    /// 查询指定岗位下的所有用户
    /// </summary>
    [HttpGet("position/{positionId}/users")]
    public async Task<ApiResponse<List<SysUser>>> GetUsersByPositionId(long positionId)
    {
        var users = await sysUserPositionService.GetUsersByPositionId(positionId);
        return ApiResponse<List<SysUser>>.Success("查询成功", users);
    }
}