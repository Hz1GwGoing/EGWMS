using Admin.NET.Core;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Admin.NET.Application;

/// <summary>
/// EGOutBound基础输入参数
/// </summary>
public class EGOutBoundBaseInput
{
    /// <summary>
    /// 出库编号
    /// </summary>
    public virtual string OutboundNum { get; set; }

    /// <summary>
    /// 出库类型
    /// </summary>
    public virtual int? OutboundType { get; set; }

    /// <summary>
    /// 出库数量
    /// </summary>
    public virtual int? OutboundCount { get; set; }

    /// <summary>
    /// 出库人
    /// </summary>
    public virtual string? OutboundUser { get; set; }

    /// <summary>
    /// 出库时间
    /// </summary>
    public virtual DateTime? OutboundTime { get; set; }

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
    /// 出库备注
    /// </summary>
    public virtual string? OutboundRemake { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    public virtual string? MaterielNum { get; set; }

}

/// <summary>
/// EGOutBound分页查询输入参数
/// </summary>
public class EGOutBoundInput : BasePageInput
{
    /// <summary>
    /// 出库编号
    /// </summary>
    public string OutboundNum { get; set; }

    /// <summary>
    /// 出库类型
    /// </summary>
    public int? OutboundType { get; set; }

    /// <summary>
    /// 出库数量
    /// </summary>
    public int? OutboundCount { get; set; }

    /// <summary>
    /// 出库人
    /// </summary>
    public string? OutboundUser { get; set; }

    /// <summary>
    /// 出库时间
    /// </summary>
    public DateTime? OutboundTime { get; set; }

    /// <summary>
    /// 出库时间范围
    /// </summary>
    public List<DateTime?> OutboundTimeRange { get; set; }
    /// <summary>
    /// 仓库编号
    /// </summary>
    public string? WHNum { get; set; }

    /// <summary>
    /// 栈板编号
    /// </summary>
    public string? PalletNum { get; set; }

    /// <summary>
    /// 料箱编号
    /// </summary>
    public string? WorkBinNum { get; set; }

    /// <summary>
    /// 出库备注
    /// </summary>
    public string? OutboundRemake { get; set; }

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
/// EGOutBound增加输入参数
/// </summary>
public class AddEGOutBoundInput : EGOutBoundBaseInput
{
    /// <summary>
    /// 出库编号
    /// </summary>
    [Required(ErrorMessage = "出库编号不能为空")]
    public override string OutboundNum { get; set; }

}

/// <summary>
/// EGOutBound删除输入参数
/// </summary>
public class DeleteEGOutBoundInput : BaseIdInput
{
}

/// <summary>
/// EGOutBound更新输入参数
/// </summary>
public class UpdateEGOutBoundInput : EGOutBoundBaseInput
{
    /// <summary>
    /// Id
    /// </summary>
    [Required(ErrorMessage = "Id不能为空")]
    public long Id { get; set; }

}

/// <summary>
/// EGOutBound主键查询输入参数
/// </summary>
public class QueryByIdEGOutBoundInput
{

    /// <summary>
    /// 出库编号
    /// </summary>
    public virtual string OutboundNum { get; set; }
}
