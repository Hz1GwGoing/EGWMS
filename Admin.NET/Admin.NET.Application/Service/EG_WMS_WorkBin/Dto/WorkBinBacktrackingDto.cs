namespace Admin.NET.Application.Service.EG_WMS_WorkBin.Dto;

/// <summary>
/// 料箱回溯DTO
/// </summary>
public class WorkBinBacktrackingDto
{
    public string WorkBinNum { get; set; }
    public string WorkBinName { get; set; }
    public string StorageNum { get; set; }
    public string MaterielNum { get; set; }
    public int IsCount { get; set; }
    public string ProductionLot { get; set; }
    public DateTime ProductionDate { get; set; }
    public string WorkBinRemake { get; set; }

    /// <summary>
    /// 入库时间
    /// </summary>
    public DateTime StorageTime { get; set; }

    /// <summary>
    /// 出库时间
    /// </summary>
    public DateTime OutBoundTime { get; set; }
}
