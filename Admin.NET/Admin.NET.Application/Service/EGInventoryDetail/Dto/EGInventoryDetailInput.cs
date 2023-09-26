using Admin.NET.Core;
using System.ComponentModel.DataAnnotations;

namespace Admin.NET.Application;

/// <summary>
/// EGInventorydetail基础输入参数
/// </summary>
public class EGInventoryDetailBaseInput
{
    /// <summary>
    /// 物料编号
    /// </summary>
    public virtual string? MaterielNum { get; set; }

    /// <summary>
    /// 库存编号
    /// </summary>
    public virtual string? InventoryNum { get; set; }

    /// <summary>
    /// 生产批次
    /// </summary>
    public virtual string? ProductionLot { get; set; }

    /// <summary>
    /// 当前数量
    /// </summary>
    public virtual int? CurrentCount { get; set; }

    /// <summary>
    /// 库位编号
    /// </summary>
    public virtual string? StorageNum { get; set; }

    /// <summary>
    /// 仓库编号
    /// </summary>
    public virtual string? WHNum { get; set; }

    /// <summary>
    /// 区域编号
    /// </summary>
    public virtual string? RegionNum { get; set; }

    /// <summary>
    /// 货架编号
    /// </summary>
    public virtual string? ShelfNum { get; set; }

    /// <summary>
    /// 冻结状态
    /// </summary>
    public virtual int? FrozenState { get; set; }

    /// <summary>
    /// 栈板编号
    /// </summary>
    public virtual string? PalletNum { get; set; }

    /// <summary>
    /// 料箱编号
    /// </summary>
    public virtual string? WorkBinNum { get; set; }

}

/// <summary>
/// EGInventorydetail分页查询输入参数
/// </summary>
public class EGInventoryDetailInput : BasePageInput
{
    /// <summary>
    /// 物料编号
    /// </summary>
    public string? MaterielNum { get; set; }

    /// <summary>
    /// 库存编号
    /// </summary>
    //public string? InventoryNum { get; set; }

    /// <summary>
    /// 生产批次
    /// </summary>
    public string? ProductionLot { get; set; }

    /// <summary>
    /// 当前数量
    /// </summary>
    public int? CurrentCount { get; set; }

    /// <summary>
    /// 库位编号
    /// </summary>
    public string? StorageNum { get; set; }

    /// <summary>
    /// 仓库编号
    /// </summary>
    public string? WHNum { get; set; }

    /// <summary>
    /// 区域编号
    /// </summary>
    public string? RegionNum { get; set; }

    /// <summary>
    /// 货架编号
    /// </summary>
    public string? ShelfNum { get; set; }

    /// <summary>
    /// 冻结状态
    /// </summary>
    public int? FrozenState { get; set; }

    /// <summary>
    /// 栈板编号
    /// </summary>
    public string? PalletNum { get; set; }

    /// <summary>
    /// 料箱编号
    /// </summary>
    public string? WorkBinNum { get; set; }


    /// <summary>
    /// 创建时间
    /// </summary>
    public virtual DateTime? CreateTime { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? InventoryDetailRemake { get; set; }


}

/// <summary>
/// EGInventorydetail增加输入参数
/// </summary>
public class AddEGInventoryDetailInput : EGInventoryDetailBaseInput
{
    /// <summary>
    /// 物料编号
    /// </summary>
    [Required(ErrorMessage = "物料编号不能为空")]
    public override string? MaterielNum { get; set; }

}

/// <summary>
/// EGInventorydetail删除输入参数
/// </summary>
public class DeleteEGInventoryDetailInput : BaseIdInput
{
}

/// <summary>
/// EGInventorydetail更新输入参数
/// </summary>
public class UpdateEGInventoryDetailInput : EGInventoryDetailBaseInput
{
    /// <summary>
    /// Id
    /// </summary>
    [Required(ErrorMessage = "Id不能为空")]
    public long Id { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? InventoryDetailRemake { get; set; }
}

/// <summary>
/// EGInventorydetail主键查询输入参数
/// </summary>
public class QueryByIdEGInventoryDetailInput
{

    /// <summary>
    /// 库存编号
    /// </summary>
    //public virtual string? InventoryNum { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    public virtual string? MaterielNum { get; set; }

}
