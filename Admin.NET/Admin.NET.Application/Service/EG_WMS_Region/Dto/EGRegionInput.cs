namespace Admin.NET.Application;

/// <summary>
/// EGRegion基础输入参数
/// </summary>
public class EGRegionBaseInput
{
    /// <summary>
    /// 区域编号
    /// </summary>
    public virtual string? RegionNum { get; set; }

    /// <summary>
    /// 区域名称
    /// </summary>
    public virtual string? RegionName { get; set; }

    /// <summary>
    /// 所属仓库
    /// </summary>
    public virtual string? WareHouseName { get; set; }

    /// <summary>
    /// 区域状态
    /// </summary>
    public virtual int? RegionStatus { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? RegionRemake { get; set; }

    /// <summary>
    /// 仓库编号
    /// </summary>
    public virtual string? WHNum { get; set; }

    /// <summary>
    /// 区域绑定物料编号
    /// </summary>
    public string? RegionMaterielNum { get; set; }

    /// <summary>
    /// 区域类型
    /// </summary>
    public string? RegionType { get; set; }
}

/// <summary>
/// EGRegion分页查询输入参数
/// </summary>
public class EGRegionInput
{

    public int page { get; set; }

    public int pageSize { get; set; }
    /// <summary>
    /// 区域编号
    /// </summary>
    public string? RegionNum { get; set; }

    /// <summary>
    /// 区域名称
    /// </summary>
    public string? RegionName { get; set; }

    /// <summary>
    /// 区域状态
    /// </summary>
    public int? RegionStatus { get; set; }

    /// <summary>
    /// 仓库编号
    /// </summary>
    public string? WHNum { get; set; }

}

/// <summary>
/// EGRegion增加输入参数
/// </summary>
public class AddEGRegionInput : EGRegionBaseInput
{
    /// <summary>
    /// 区域编号
    /// </summary>
    public override string? RegionNum { get; set; }

}

/// <summary>
/// EGRegion删除输入参数
/// </summary>
public class DeleteEGRegionInput : BaseIdInput
{
}

/// <summary>
/// EGRegion更新输入参数
/// </summary>
public class UpdateEGRegionInput
{
    /// <summary>
    /// Id
    /// </summary>
    [Required(ErrorMessage = "Id不能为空")]
    public long Id { get; set; }

    /// <summary>
    /// 区域名称
    /// </summary>
    public string? RegionName { get; set; }

    /// <summary>
    /// 区域绑定物料编号
    /// </summary>
    public string? RegionMaterielNum { get; set; }

    /// <summary>
    /// 区域类型
    /// </summary>
    public string? RegionType { get; set; }

    /// <summary>
    /// 仓库编号
    /// </summary>
    public string? WHNum { get; set; }
}

/// <summary>
/// EGRegion主键查询输入参数
/// </summary>
public class QueryByIdEGRegionInput
{
    /// <summary>
    /// 区域编号
    /// </summary>
    public string? RegionNum { get; set; }

    /// <summary>
    /// 区域名称
    /// </summary>
    public string? RegionName { get; set; }
}
