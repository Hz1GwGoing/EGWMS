namespace Admin.NET.Application.Service.EG_WMS_Storage.Dto;

/// <summary>
/// 查询库位在库数据Dto
/// </summary>
public class StorageInventoryDataDto
{
    public string StorageNum { get; set; }
    public string MaterielNum { get; set; }
    public string WorkBinNum { get; set; }
    public int ICountAll { get; set; }
    public string ProductionLot { get; set; }
    public DateTime ProductionDate { get; set; }


}