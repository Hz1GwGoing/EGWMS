namespace Admin.NET.Application.Service.EG_WMS_InAndOutBound.Dto;

/// <summary>
/// 出入库任务和agv信息
/// </summary>
public class InoutBoundAndAGVInforMationDto
{
    public string TaskNo { get; set; }
    public string AGV { get; set; }
    public string TaskState { get; set; }
    public string TaskPath { get; set; }
    public int InAndOutBoundType { get; set; }
    public string MaterielNum { get; set; }
    public int InAndOutBoundCount { get; set; }

}