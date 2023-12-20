namespace Admin.NET.Application.Service.EG_WMS_Materiel.Dto;

/// <summary>
/// 物料预警Dto
/// </summary>
public class MaterielStorageTimeWarringDto
{
    public string MaterielNum { get; set; }
    public string MaterielName { get; set; }
    public string WorkBin { get; set; }
    public int Icount { get; set; }
    public string StorageNum { get; set; }
    /// <summary>
    /// 生产时间
    /// </summary>
    public DateTime InventoryTime { get; set; }

}