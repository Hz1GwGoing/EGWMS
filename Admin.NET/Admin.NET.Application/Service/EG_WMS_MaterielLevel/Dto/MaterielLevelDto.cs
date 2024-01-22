namespace Admin.NET.Application.Service.EG_WMS_MaterielLevel.Dto;

/// <summary>
/// 物料级别返回Dto
/// </summary>
public class MaterielLevelDto
{
    public string MaterielLevelNum { get; set; }
    public string MaterielLevelName { get; set; }
    public string MatetielLevelDescription { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime UpdateTime { get; set; }
}