using Admin.NET.Core;
using System.ComponentModel.DataAnnotations;

namespace Admin.NET.Application;

/// <summary>
/// EGInventory基础输入参数
/// </summary>
public class EGInventoryBaseInput
{
    /// <summary>
    /// 库存编号
    /// </summary>
    public virtual string? InventoryNum { get; set; }

    /// <summary>
    /// 库存总数
    /// </summary>
    public virtual int? ICountAll { get; set; }

    /// <summary>
    /// 可用库存
    /// </summary>
    public virtual int? IUsable { get; set; }

    /// <summary>
    /// 冻结数量
    /// </summary>
    public virtual int? IFrostCount { get; set; }

    /// <summary>
    /// 待检数量
    /// </summary>
    public virtual int? IWaitingCount { get; set; }

    /// <summary>
    /// 出库状态
    /// </summary>
    public int? OutboundStatus { get; set; } = 0;
}

/// <summary>
/// EGInventory分页查询输入参数
/// </summary>
public class EGInventoryInput : BasePageInput
{
    /// <summary>
    /// 库存编号
    /// </summary>
    //public string? InventoryNum { get; set; }

    /// <summary>
    /// 库存总数
    /// </summary>
    public int? ICountAll { get; set; }

    /// <summary>
    /// 可用库存
    /// </summary>
    public int? IUsable { get; set; }

    /// <summary>
    /// 冻结数量
    /// </summary>
    public int? IFrostCount { get; set; }

    /// <summary>
    /// 待检数量
    /// </summary>
    public int? IWaitingCount { get; set; }


    /// <summary>
    /// 创建时间
    /// </summary>
    /// ！
    public virtual DateTime? CreateTime { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    public string? MaterielNum { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? InventoryRemake { get; set; }

    /// <summary>
    /// 出库状态
    /// </summary>
    public int? OutboundStatus { get; set; } = 0;

}

/// <summary>
/// EGInventory增加输入参数
/// </summary>
public class AddEGInventoryInput : EGInventoryBaseInput
{
    /// <summary>
    /// 库存编号
    /// </summary>
    //[Required(ErrorMessage = "库存编号不能为空")]
    //public override string? InventoryNum { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    public string? MaterielNum { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? InventoryRemake { get; set; }
}

/// <summary>
/// EGInventory删除输入参数
/// </summary>
public class DeleteEGInventoryInput : BaseIdInput
{
}

/// <summary>
/// EGInventory更新输入参数
/// </summary>
public class UpdateEGInventoryInput : EGInventoryBaseInput
{
    /// <summary>
    /// Id
    /// </summary>
    [Required(ErrorMessage = "Id不能为空")]
    public long Id { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? InventoryRemake { get; set; }


}

/// <summary>
/// EGInventory主键查询输入参数
/// </summary>
public class QueryByIdEGInventoryInput
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
