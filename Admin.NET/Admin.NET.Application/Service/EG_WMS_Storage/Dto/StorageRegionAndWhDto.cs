namespace Admin.NET.Application.Service.EG_WMS_Storage.Dto;

public class StorageRegionAndWhDto
{

    /// <summary>
    ///  库位id
    /// </summary>
    public long ID { get; set; }

    /// <summary>
    /// 库位编号
    /// </summary>
    public string StorageNum { get; set; }

    /// <summary>
    /// 库位名称
    /// </summary>
    public string StorageName { get; set; }

    /// <summary>
    /// 所属仓库
    /// </summary>
    public string WHName { get; set; }

    /// <summary>
    /// 所属区域
    /// </summary>
    public string RegionName { get; set; }

    /// <summary>
    /// 巷道号
    /// </summary>
    public int RoadwayNum { get; set; }

    /// <summary>
    /// 货架号
    /// </summary>
    public int ShelfNum { get; set; }

    /// <summary>
    /// 层号
    /// </summary>
    public int FloorNumber { get; set; }

    /// <summary>
    /// 库位状态 0.正常 1.异常
    /// </summary>
    public int StorageStatus { get; set; }

    /// <summary>
    /// 组别
    /// </summary>
    public string StorageGroup { get; set; }

    /// <summary>
    /// 是否占用（0.未占用 1.已占用 2.预占用）
    /// </summary>
    public int StorageOccupy { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string StorageRemake { get; set; }

    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string CreateUserName { get; set; }

    /// <summary>
    /// 新增日期
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 库位类别
    /// </summary>
    public string StorageType { get; set; }

    public int TotalCount { get; set; }

}