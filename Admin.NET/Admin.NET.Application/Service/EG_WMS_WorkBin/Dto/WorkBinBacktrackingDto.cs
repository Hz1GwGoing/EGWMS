namespace Admin.NET.Application.Service.EG_WMS_WorkBin.Dto;

/// <summary>
/// 料箱回溯DTO
/// </summary>
public class WorkBinBacktrackingDto
{
    public string WorkBinNum { get; set; }
    public string StorageNum { get; set; }
    public string MaterielName { get; set; }
    public int ICountAll { get; set; }

    /// <summary>
    /// 是否出库
    /// </summary>
    public int OutBoundStatus { get; set; }

    /// <summary>
    /// 生产批次
    /// </summary>
    public string ProductionLot { get; set; }

    /// <summary>
    /// 生产日期
    /// </summary>
    public DateTime ProductionDate { get; set; }

    /// <summary>
    /// 入库时间
    /// </summary>
    public DateTime StorageTime { get; set; }

    /// <summary>
    /// 出库时间
    /// </summary>
    public DateTime OutBoundTime { get; set; }
}
