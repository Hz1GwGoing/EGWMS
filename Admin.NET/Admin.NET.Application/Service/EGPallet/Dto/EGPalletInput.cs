using Admin.NET.Core;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Admin.NET.Application;

/// <summary>
/// EGPallet基础输入参数
/// </summary>
public class EGPalletBaseInput
{
    /// <summary>
    /// 栈板编号
    /// </summary>
    public virtual string PalletNum { get; set; }

    /// <summary>
    /// 栈板名称
    /// </summary>
    public virtual string? PalletName { get; set; }

    /// <summary>
    /// 栈板规格
    /// </summary>
    public virtual string? PalletSpecs { get; set; }

    /// <summary>
    /// 有效日期
    /// </summary>
    public virtual DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public virtual int? PalletStatus { get; set; }

    /// <summary>
    /// 新增人
    /// </summary>
    public virtual string? CreateUserName { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? PalletRemake { get; set; }

}

/// <summary>
/// EGPallet分页查询输入参数
/// </summary>
public class EGPalletInput : BasePageInput
{
    /// <summary>
    /// 栈板编号
    /// </summary>
    public string PalletNum { get; set; }

    /// <summary>
    /// 栈板名称
    /// </summary>
    public string? PalletName { get; set; }

    /// <summary>
    /// 栈板规格
    /// </summary>
    public string? PalletSpecs { get; set; }

    /// <summary>
    /// 有效日期
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// 有效日期范围
    /// </summary>
    public List<DateTime?> ExpirationDateRange { get; set; }
    /// <summary>
    /// 状态
    /// </summary>
    public int? PalletStatus { get; set; }

    /// <summary>
    /// 新增人
    /// </summary>
    public string? CreateUserName { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? PalletRemake { get; set; }

}

/// <summary>
/// EGPallet增加输入参数
/// </summary>
public class AddEGPalletInput : EGPalletBaseInput
{
    /// <summary>
    /// 栈板编号
    /// </summary>
    [Required(ErrorMessage = "栈板编号不能为空")]
    public override string PalletNum { get; set; }

}

/// <summary>
/// EGPallet删除输入参数
/// </summary>
public class DeleteEGPalletInput : BaseIdInput
{
}

/// <summary>
/// EGPallet更新输入参数
/// </summary>
public class UpdateEGPalletInput : EGPalletBaseInput
{
    /// <summary>
    /// Id
    /// </summary>
    [Required(ErrorMessage = "Id不能为空")]
    public long Id { get; set; }

}

/// <summary>
/// EGPallet主键查询输入参数
/// </summary>
public class QueryByIdEGPalletInput
{

    /// <summary>
    /// 栈板编号
    /// </summary>
    public string PalletNum { get; set; }

    /// <summary>
    /// 栈板名称
    /// </summary>
    public string? PalletName { get; set; }
}
