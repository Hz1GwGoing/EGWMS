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
    public string ProductionDate { get; set; }
    public string WorkBinRemake { get; set; }

}
