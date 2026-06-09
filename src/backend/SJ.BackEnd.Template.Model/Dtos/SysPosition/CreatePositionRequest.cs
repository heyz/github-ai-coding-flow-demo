#region  <<版本注释>>
/* ==============================================================================
// <copyright file="CreatePositionRequest.cs" company="Shiji.BO.CS">
// Copyright (c) SJ.BO.CS. All rights reserved.
// </copyright>
* 功能描述：CreatePositionRequest
* 创 建 者：何应芝
* 创建时间：2026/6/9 16:33:18
* ==============================================================================*/
#endregion

using System.ComponentModel.DataAnnotations;

namespace SJ.BackEnd.Template.Model;

/// <summary>
/// 创建岗位请求 DTO
/// </summary>
public class CreatePositionRequest
{
    /// <summary>
    /// 岗位名称
    /// </summary>
    [Required(ErrorMessage = "岗位名称不能为空")]
    [StringLength(50, ErrorMessage = "岗位名称长度不能超过{1}个字符")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 岗位编码
    /// </summary>
    [Required(ErrorMessage = "岗位编码不能为空")]
    [StringLength(50, ErrorMessage = "岗位编码长度不能超过{1}个字符")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    [StringLength(200, ErrorMessage = "描述长度不能超过{1}个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 排序序号
    /// </summary>
    public int SortOrder { get; set; } = 0;
}