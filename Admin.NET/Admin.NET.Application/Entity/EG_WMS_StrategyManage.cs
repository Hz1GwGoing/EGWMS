namespace Admin.NET.Application.Entity;

[SugarTable("EG_WMS_StrategyManage", "策略管理表")]
public class EG_WMS_StrategyManage : EntityBase
{
    /// <summary>
    /// 策略编号
    /// </summary>
    [SugarColumn(ColumnDescription = "策略编号")]
    public string StrategyNum { get; set; }

    /// <summary>
    /// 策略名称
    /// </summary>
    [SugarColumn(ColumnDescription = "策略名称")]
    public string StrategyName { get; set; }

    /// <summary>
    /// 策略类别（0 - 入库，1 - 出库）
    /// </summary>
    [SugarColumn(ColumnDescription = "策略类别（0 - 入库，1 - 出库）")]
    public string StrategyType { get; set; }

    /// <summary>
    /// 策略状态（0 - 启用，1 - 未启用）
    /// </summary>
    [SugarColumn(ColumnDescription = "策略状态（0 - 启用，1 - 未启用）")]
    public int StrategyStatus { get; set; }

    /// <summary>
    /// 库位类别（0 - 密集库，1 - 立库）
    /// </summary>
    [SugarColumn(ColumnDescription = "库位类别（0 - 密集库，1 - 立库）")]
    public int StorageType { get; set; }

    /// <summary>
    /// 策略备注
    /// </summary>
    [SugarColumn(ColumnDescription = "策略备注")]
    public string StrategyRemake { get; set; }
}
