using Admin.NET.Core;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Admin.NET.Application;

/// <summary>
/// EGRelocation基础输入参数
/// </summary>
public class EGRelocationBaseInput
{
    /// <summary>
    /// 移库编号
    /// </summary>
    public virtual string RelocatioNum { get; set; }

    /// <summary>
    /// 移库类型
    /// </summary>
    public virtual int? RelocationType { get; set; }

    /// <summary>
    /// 移库数量
    /// </summary>
    public virtual int? RelocationCount { get; set; }

    /// <summary>
    /// 移库人
    /// </summary>
    public virtual string? RelocationUser { get; set; }

    /// <summary>
    /// 移库时间
    /// </summary>
    public virtual DateTime? RelocationTime { get; set; }

    /// <summary>
    /// 仓库编号
    /// </summary>
    public virtual string? WHNum { get; set; }

    /// <summary>
    /// 栈板编号
    /// </summary>
    public virtual string? PalletNum { get; set; }

    /// <summary>
    /// 料箱编号
    /// </summary>
    public virtual string? WorkBinNum { get; set; }

    /// <summary>
    /// 移库备注
    /// </summary>
    public virtual string? RelocationRemake { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    public virtual string? MaterielNum { get; set; }

}

/// <summary>
/// EGRelocation分页查询输入参数
/// </summary>
public class EGRelocationInput : BasePageInput
{
    /// <summary>
    /// 移库编号
    /// </summary>
    public string RelocatioNum { get; set; }

    /// <summary>
    /// 移库类型
    /// </summary>
    public int? RelocationType { get; set; }

    /// <summary>
    /// 原库位
    /// </summary>
    public string OldStorage { get; set; }

    /// <summary>
    /// 新库位
    /// </summary>
    public string NewStorage { get; set; }


    /// <summary>
    /// 移库人
    /// </summary>
    public string? RelocationUser { get; set; }

    /// <summary>
    /// 移库时间
    /// </summary>
    public DateTime? RelocationTime { get; set; }

    /// <summary>
    /// 移库时间范围
    /// </summary>
    public List<DateTime?> RelocationTimeRange { get; set; }

    /// <summary>
    /// 仓库编号
    /// </summary>
    public string? WHNum { get; set; }

    /// <summary>
    /// 料箱编号
    /// </summary>
    public string? WorkBinNum { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    public string? MaterielNum { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public virtual DateTime? CreateTime { get; set; }

}

/// <summary>
/// EGRelocation增加输入参数
/// </summary>
public class AddEGRelocationInput : EGRelocationBaseInput
{
    /// <summary>
    /// 移库编号
    /// </summary>
    [Required(ErrorMessage = "移库编号不能为空")]
    public override string RelocatioNum { get; set; }

}

/// <summary>
/// EGRelocation删除输入参数
/// </summary>
public class DeleteEGRelocationInput : BaseIdInput
{
}

/// <summary>
/// EGRelocation更新输入参数
/// </summary>
public class UpdateEGRelocationInput : EGRelocationBaseInput
{
    /// <summary>
    /// Id
    /// </summary>
    [Required(ErrorMessage = "Id不能为空")]
    public long Id { get; set; }

}

/// <summary>
/// EGRelocation主键查询输入参数
/// </summary>
public class QueryByIdEGRelocationInput
{

    /// <summary>
    /// 移库编号
    /// </summary>
    public virtual string RelocatioNum { get; set; }
}
