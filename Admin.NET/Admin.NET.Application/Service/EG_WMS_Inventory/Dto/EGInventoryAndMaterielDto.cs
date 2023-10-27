namespace Admin.NET.Application.Service.EGInventory.Dto;
public class EGInventoryAndMaterielDto
{
    /// <summary>
    /// 物料编号
    /// </summary>
    public string MaterielNum { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    public string MaterielName { get; set; }

    /// <summary>
    /// 物料规格
    /// </summary>
    public string? MaterielSpecs { get; set; }

    /// <summary>
    /// 库存总数
    /// </summary>
    public int? ICountAll { get; set; }

    /// <summary>
    /// 可用库存
    /// </summary>
    public int? IUsable { get; set; }

}
