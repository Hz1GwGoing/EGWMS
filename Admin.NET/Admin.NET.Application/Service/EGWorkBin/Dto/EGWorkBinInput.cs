using Admin.NET.Core;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Admin.NET.Application;

/// <summary>
/// EGWorkBin基础输入参数
/// </summary>
public class EGWorkBinBaseInput
{
    /// <summary>
    /// 料箱编号
    /// </summary>
    public virtual string? WorkBinNum { get; set; }

    /// <summary>
    /// 料箱名称
    /// </summary>
    public virtual string? WorkBinName { get; set; }

    /// <summary>
    /// 科箱规格
    /// </summary>
    public virtual int? WorkBinSpecs { get; set; }

    /// <summary>
    /// 机台号
    /// </summary>
    public virtual int? MachineNum { get; set; }

    /// <summary>
    /// 班次
    /// </summary>
    public virtual string? Classes { get; set; }

    /// <summary>
    /// 生产批次
    /// </summary>
    public virtual string? ProductionLot { get; set; }

    /// <summary>
    /// 生产日期
    /// </summary>
    public virtual DateTime? ProductionDate { get; set; }

    /// <summary>
    /// 生产员
    /// </summary>
    public virtual string? ProductionStaff { get; set; }

    /// <summary>
    /// 检验员
    /// </summary>
    public virtual string? Inspector { get; set; }

    /// <summary>
    /// 打印人
    /// </summary>
    public virtual string? Printer { get; set; }

    /// <summary>
    /// 料箱状态
    /// </summary>
    public virtual int? WorkBinStatus { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    public virtual string? MaterielNum { get; set; }

    /// <summary>
    /// 打印时间
    /// </summary>
    public virtual DateTime? PrintTime { get; set; }

    /// <summary>
    /// 料箱备注
    /// </summary>
    public virtual string? WorkBinRemake { get; set; }

    /// <summary>
    /// 库位编号
    /// </summary>
    public virtual string? StorageNum { get; set; }

    /// <summary>
    /// 栈板编号
    /// </summary>
    public virtual string? PalletNum { get; set; }

}

/// <summary>
/// EGWorkBin分页查询输入参数
/// </summary>
public class EGWorkBinInput : BasePageInput
{
    /// <summary>
    /// 料箱编号
    /// </summary>
    public string? WorkBinNum { get; set; }

    /// <summary>
    /// 料箱名称
    /// </summary>
    public string? WorkBinName { get; set; }

    /// <summary>
    /// 科箱规格
    /// </summary>
    public int? WorkBinSpecs { get; set; }

    /// <summary>
    /// 机台号
    /// </summary>
    public int? MachineNum { get; set; }

    /// <summary>
    /// 班次
    /// </summary>
    public string? Classes { get; set; }

    /// <summary>
    /// 生产批次
    /// </summary>
    public string? ProductionLot { get; set; }

    /// <summary>
    /// 生产日期
    /// </summary>
    public DateTime? ProductionDate { get; set; }

    /// <summary>
    /// 生产日期范围
    /// </summary>
    public List<DateTime?> ProductionDateRange { get; set; }

    /// <summary>
    /// 生产员
    /// </summary>
    public string? ProductionStaff { get; set; }

    /// <summary>
    /// 检验员
    /// </summary>
    public string? Inspector { get; set; }

    /// <summary>
    /// 打印人
    /// </summary>
    public string? Printer { get; set; }

    /// <summary>
    /// 料箱状态
    /// </summary>
    public int? WorkBinStatus { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    public string? MaterielNum { get; set; }

    /// <summary>
    /// 打印时间
    /// </summary>
    public DateTime? PrintTime { get; set; }

    /// <summary>
    /// 打印时间范围
    /// </summary>
    public List<DateTime?> PrintTimeRange { get; set; }

    /// <summary>
    /// 料箱备注
    /// </summary>
    public string? WorkBinRemake { get; set; }

    /// <summary>
    /// 库位编号
    /// </summary>
    public string? StorageNum { get; set; }

    /// <summary>
    /// 栈板编号
    /// </summary>
    public string? PalletNum { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public virtual DateTime? CreateTime { get; set; }
}

/// <summary>
/// EGWorkBin增加输入参数
/// </summary>
public class AddEGWorkBinInput : EGWorkBinBaseInput
{
    /// <summary>
    /// 料箱编号
    /// </summary>
    [Required(ErrorMessage = "料箱编号不能为空")]
    public override string? WorkBinNum { get; set; }

}

/// <summary>
/// EGWorkBin删除输入参数
/// </summary>
public class DeleteEGWorkBinInput : BaseIdInput
{
}

/// <summary>
/// EGWorkBin更新输入参数
/// </summary>
public class UpdateEGWorkBinInput : EGWorkBinBaseInput
{
    /// <summary>
    /// Id
    /// </summary>
    [Required(ErrorMessage = "Id不能为空")]
    public long Id { get; set; }

}

/// <summary>
/// EGWorkBin主键查询输入参数
/// </summary>
public class QueryByIdEGWorkBinInput
{

    /// <summary>
    /// 料箱编号
    /// </summary>
    public string? WorkBinNum { get; set; }

    /// <summary>
    /// 料箱名称
    /// </summary>
    public virtual string? WorkBinName { get; set; }
}
