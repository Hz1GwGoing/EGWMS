namespace Admin.NET.Application.Service.EG_WMS_WareHouse.Dto;

/// <summary>
/// 返回查询仓库Dto
/// </summary>
public class GetWareHouseDataDto
{
    public long Id { get; set; }
    public string WHNum { get; set; }
    public string WHName { get; set; }

    public int WHType { get; set; }

    public string WHAddress { get; set; }

    public int WHStatus { get; set; }

    /// <summary>
    /// 当前仓库下区域总数
    /// </summary>
    public int CurrentRegionCount { get; set; }

    /// <summary>
    /// 当前仓库下库位总数
    /// </summary>
    public int CurrentStorageCount { get; set; }

    /// <summary>
    /// 当前仓库下可用库位总数
    /// </summary>
    public int CurrentStorageCountUsAble { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string WHRemake { get; set; }

    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string CreateUserName { get; set; }

    /// <summary>
    /// 修改则姓名
    /// </summary>
    public string UpdateUserName { get; set; }

}