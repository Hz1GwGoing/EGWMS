namespace Admin.NET.Application.Service.EG_WMS_Storage.Dto;

/// <summary>
/// 返回查询库位占用情况Dto
/// </summary>
public class QueryStorageOccupancyDto
{
    public string StorageNum { get; set; }
    public int StorageOccupy { get; set; }
    public string StorageGroup { get; set; }
    public string RegionNum { get; set; }
}