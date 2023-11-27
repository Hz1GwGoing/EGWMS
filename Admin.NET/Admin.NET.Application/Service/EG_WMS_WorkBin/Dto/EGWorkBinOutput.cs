namespace Admin.NET.Application;

/// <summary>
/// EGWorkBin输出参数
/// </summary>
public class EGWorkBinOutput
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
    public string? WorkBinSpecs { get; set; }

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
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 产品数量
    /// </summary>
    public int ProductCount { get; set; }

}


