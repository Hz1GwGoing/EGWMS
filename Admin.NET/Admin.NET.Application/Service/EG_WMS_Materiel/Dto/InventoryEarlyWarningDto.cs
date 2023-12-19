namespace Admin.NET.Application.Service.EG_WMS_Materiel.Dto;

/// <summary>
/// 库存预警Dto
/// </summary>
public class InventoryEarlyWarningDto
{
    public string MaterielNum { get; set; }
    public string MaterielName { get; set; }
    public string MaterielSpecs { get; set; }
    // 当前在库数量
    public int? InventoryCount { get; set; } = 0;

}