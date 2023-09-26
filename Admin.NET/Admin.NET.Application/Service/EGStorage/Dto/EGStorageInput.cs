using Admin.NET.Core;
using System.ComponentModel.DataAnnotations;

namespace Admin.NET.Application;

/// <summary>
/// EGStorage基础输入参数
/// </summary>
public class EGStorageBaseInput
{
    /// <summary>
    /// 库位编号
    /// </summary>
    public virtual string? StorageNum { get; set; }

    /// <summary>
    /// 库位名称
    /// </summary>
    public virtual string? StorageName { get; set; }

    /// <summary>
    /// 库位地址
    /// </summary>
    public virtual string? StorageAddress { get; set; }

    /// <summary>
    /// 库位类别
    /// </summary>
    public virtual string? StorageType { get; set; }

    /// <summary>
    /// 库位状态
    /// </summary>
    public virtual int? StorageStatus { get; set; }

    /// <summary>
    /// 仓库名称
    /// </summary>
    public virtual string? WareHouseName { get; set; }

    /// <summary>
    /// 库位长
    /// </summary>
    public virtual decimal? StorageLong { get; set; }

    /// <summary>
    /// 库位宽
    /// </summary>
    public virtual decimal? StorageWidth { get; set; }

    /// <summary>
    /// 库位高
    /// </summary>
    public virtual decimal? StorageHigh { get; set; }

    /// <summary>
    /// 是否占用
    /// </summary>
    public virtual string? StorageOccupy { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? StorageRemake { get; set; }

    /// <summary>
    /// 创建者姓名
    /// </summary>
    public virtual string? CreateUserName { get; set; }

    /// <summary>
    /// 修改者姓名
    /// </summary>
    public virtual string? UpdateUserName { get; set; }

    /// <summary>
    /// 巷道号
    /// </summary>
    public virtual int? RoadwayNum { get; set; }

    /// <summary>
    /// 货架号
    /// </summary>
    public virtual int? ShelfNum { get; set; }

    /// <summary>
    /// 层号
    /// </summary>
    public virtual int? FloorNumber { get; set; }

    /// <summary>
    /// 仓库编号
    /// </summary>
    public virtual string? WHNum { get; set; }

    /// <summary>
    /// 区域编号
    /// </summary>
    public virtual string? RegionNum { get; set; }

}

/// <summary>
/// EGStorage分页查询输入参数
/// </summary>
public class EGStorageInput : BasePageInput
{
    /// <summary>
    /// 库位编号
    /// </summary>
    public string? StorageNum { get; set; }

    /// <summary>
    /// 库位名称
    /// </summary>
    public string? StorageName { get; set; }

    /// <summary>
    /// 库位地址
    /// </summary>
    public string? StorageAddress { get; set; }

    /// <summary>
    /// 库位状态
    /// </summary>
    public int? StorageStatus { get; set; }
    /// <summary>
    /// 库位类别
    /// </summary>
    public string? StorageType { get; set; }

    /// <summary>
    /// 仓库名称
    /// </summary>
    public string? WareHouseName { get; set; }

    /// <summary>
    /// 库位长
    /// </summary>
    public decimal? StorageLong { get; set; }

    /// <summary>
    /// 库位宽
    /// </summary>
    public decimal? StorageWidth { get; set; }

    /// <summary>
    /// 库位高
    /// </summary>
    public decimal? StorageHigh { get; set; }

    /// <summary>
    /// 是否占用
    /// </summary>
    public string? StorageOccupy { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? StorageRemake { get; set; }

    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string? CreateUserName { get; set; }

    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? UpdateUserName { get; set; }

    /// <summary>
    /// 巷道号
    /// </summary>
    public int? RoadwayNum { get; set; }

    /// <summary>
    /// 货架号
    /// </summary>
    public int? ShelfNum { get; set; }

    /// <summary>
    /// 层号
    /// </summary>
    public int? FloorNumber { get; set; }

    /// <summary>
    /// 仓库编号
    /// </summary>
    public string? WHNum { get; set; }

    /// <summary>
    /// 区域编号
    /// </summary>
    public string? RegionNum { get; set; }

}

/// <summary>
/// EGStorage增加输入参数
/// </summary>
public class AddEGStorageInput : EGStorageBaseInput
{
    /// <summary>
    /// 库位编号
    /// </summary>
    [Required(ErrorMessage = "库位编号不能为空")]
    public override string? StorageNum { get; set; }

}

/// <summary>
/// EGStorage删除输入参数
/// </summary>
public class DeleteEGStorageInput : BaseIdInput
{
}

/// <summary>
/// EGStorage更新输入参数
/// </summary>
public class UpdateEGStorageInput : EGStorageBaseInput
{
    /// <summary>
    /// Id
    /// </summary>
    [Required(ErrorMessage = "Id不能为空")]
    public long Id { get; set; }

}

/// <summary>
/// EGStorage主键查询输入参数
/// </summary>
public class QueryByIdEGStorageInput
{

    /// <summary>
    /// 库位编号
    /// </summary>
    public string? StorageNum { get; set; }
    /// <summary>
    /// 库位名称
    /// </summary>
    public string? StorageName { get; set; }
}
