#region  <<版本注释>>
/* ==============================================================================
// <copyright file="BindUserPositionRequest.cs" company="Shiji.BO.CS">
// Copyright (c) SJ.BO.CS. All rights reserved.
// </copyright>
* 功能描述：BindUserPositionRequest
* 创 建 者：何应芝
* 创建时间：2026/6/10 00:00:00
* ==============================================================================*/
#endregion

using System.ComponentModel.DataAnnotations;

namespace SJ.BackEnd.Template.Model;

/// <summary>
/// 绑定用户岗位请求 DTO
/// </summary>
public class BindUserPositionRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required(ErrorMessage = "用户ID不能为空")]
    public long UserId { get; set; }

    /// <summary>
    /// 岗位ID
    /// </summary>
    [Required(ErrorMessage = "岗位ID不能为空")]
    public long PositionId { get; set; }
}