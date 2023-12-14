namespace Admin.NET.Application.Service.EG_WMS_BaseServer.Dto;

/// <summary>
/// 根据在库库存查询物料Dto
/// </summary>
public class AccordingInventoryDto
{
    public string MaterielNum { get; set; }
    public string MaterielType { get; set; }
    public string MaterielName { get; set; }
    public string MaterielSpecs { get; set; }
    public string MaterielMainUnit { get; set; }
}