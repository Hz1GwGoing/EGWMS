namespace Admin.NET.Application.Service.EGWareHouse.Dto;

/// <summary>
/// EGWareHouse输出参数
/// </summary>
public class EGWareHouseDto
{
    /// <summary>
    /// 仓库编号
    /// </summary>
    public string WHNum { get; set; }

    /// <summary>
    /// 仓库名称
    /// </summary>
    public string WHName { get; set; }

    /// <summary>
    /// 仓库类型
    /// </summary>
    public int? WHType { get; set; }

    /// <summary>
    /// 仓库地址
    /// </summary>
    public string WHAddress { get; set; }

    /// <summary>
    /// 仓库状态
    /// </summary>
    public int? WHStatus { get; set; }

    /// <summary>
    /// 区域数量
    /// </summary>
    public int? RegionCount { get; set; }

    /// <summary>
    /// 库位总数
    /// </summary>
    public int? StoreroomCount { get; set; }

    /// <summary>
    /// 可用库位
    /// </summary>
    public int? StoreroomUsable { get; set; }

    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string CreateUserName { get; set; }

    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string UpdateUserName { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string WHRemake { get; set; }

    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

}
