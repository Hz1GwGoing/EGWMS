using Admin.NET.Core;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Admin.NET.Application;

/// <summary>
/// EGJoinBound基础输入参数
/// </summary>
public class EGJoinBoundBaseInput
{
    /// <summary>
    /// 入库编号
    /// </summary>
    public virtual string JoinBoundNum { get; set; }

    /// <summary>
    /// 入库类型
    /// </summary>
    public virtual int? JoinBoundType { get; set; }

    /// <summary>
    /// 入库人
    /// </summary>
    public virtual string? JoinBoundUser { get; set; }

    /// <summary>
    /// 入库数量
    /// </summary>
    public virtual int? JoinBoundCount { get; set; } = 0;

    /// <summary>
    /// 入库状态 0 未入库 1 已入库（保存在库存表中）
    /// </summary>
    public int? JoinBoundStatus { get; set; }

    /// <summary>
    /// 入库时间
    /// </summary>
    public virtual DateTime? JoinBoundTime { get; set; }

    /// <summary>
    /// 回库更新时间
    /// </summary>
    public virtual DateTime? JoinBoundOutTime { get; set; }

    /// <summary>
    /// 仓库编号
    /// </summary>
    public virtual string? WHNum { get; set; }

    /// <summary>
    /// 栈板编号
    /// </summary>
    public virtual string? PalletNum { get; set; }

    /// <summary>
    /// 料箱编号
    /// </summary>
    public virtual string? WorkBinNum { get; set; }

    /// <summary>
    /// 入库备注
    /// </summary>
    public virtual string? JoinBoundRemake { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    public virtual string? MaterielNum { get; set; }

}

/// <summary>
/// EGJoinBound分页查询输入参数
/// </summary>
public class EGJoinBoundInput : BasePageInput
{
    /// <summary>
    /// 入库编号
    /// </summary>
    public string JoinBoundNum { get; set; }

    /// <summary>
    /// 入库类型
    /// </summary>
    public int? JoinBoundType { get; set; }

    /// <summary>
    /// 入库人
    /// </summary>
    public string? JoinBoundUser { get; set; }

    /// <summary>
    /// 入库状态
    /// </summary>
    public int? JoinBoundStatus { get; set; }

    /// <summary>
    /// 入库数量
    /// </summary>
    public int? JoinBoundCount { get; set; } = 0;

    /// <summary>
    /// 入库时间
    /// </summary>
    public DateTime? JoinBoundTime { get; set; }

    /// <summary>
    /// 入库时间范围
    /// </summary>
    public List<DateTime?> JoinBoundTimeRange { get; set; }
    /// <summary>
    /// 回库更新时间
    /// </summary>
    public DateTime? JoinBoundOutTime { get; set; }

    /// <summary>
    /// 回库更新时间范围
    /// </summary>
    public List<DateTime?> JoinBoundOutTimeRange { get; set; }

    /// <summary>
    /// 仓库编号
    /// </summary>
    public string? WHNum { get; set; }

    /// <summary>
    /// 栈板编号
    /// </summary>
    public string? PalletNum { get; set; }

    /// <summary>
    /// 料箱编号
    /// </summary>
    public string? WorkBinNum { get; set; }

    /// <summary>
    /// 入库备注
    /// </summary>
    public string? JoinBoundRemake { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    public string? MaterielNum { get; set; }


    /// <summary>
    /// 创建时间
    /// </summary>
    public virtual DateTime? CreateTime { get; set; }
}

/// <summary>
/// EGJoinBound增加输入参数
/// </summary>
//public class AddEGJoinBoundInput : EGInventoryInput
//{
//    /// <summary>
//    /// 入库编号
//    /// </summary>
//    [Required(ErrorMessage = "入库编号不能为空")]
//    public string JoinBoundNum { get; set; }

//    /// <summary>
//    /// 入库数量
//    /// </summary>
//    public int? JoinBoundCount { get; set; }
//}

/// <summary>
/// EGJoinBound增加输入参数
/// </summary>
public class AddEGJoinBoundInput
{
    /// <summary>
    /// 入库编号（业务单据）
    /// </summary>
    public string JoinBoundNum { get; set; }

    /// <summary>
    /// 入库数量
    /// </summary>
    public int? JoinBoundCount { get; set; } = 0;

    /// <summary>
    /// 库存编号
    /// </summary>
    public virtual string? InventoryNum { get; set; }

    /// <summary>
    /// 库存总数
    /// </summary>
    public virtual int? ICountAll { get; set; } = 0;

    /// <summary>
    /// 入库状态
    /// </summary>
    public int? JoinBoundStatus { get; set; }

    /// <summary>
    /// 仓库编号
    /// </summary>
    public string? WHNum { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    public string? MaterielNum { get; set; }

    /// <summary>
    /// 入库人
    /// </summary>
    public string? JoinBoundUser { get; set; }

    /// <summary>
    /// 入库时间
    /// </summary>
    public DateTime? JoinBoundTime { get; set; }

    /// <summary>
    /// 入库备注
    /// </summary>
    public string? JoinBoundRemake { get; set; }
}


/// <summary>
/// EGJoinBound删除输入参数
/// </summary>
public class DeleteEGJoinBoundInput : BaseIdInput
{
}

/// <summary>
/// EGJoinBound更新输入参数
/// </summary>
public class UpdateEGJoinBoundInput : EGJoinBoundBaseInput
{
    /// <summary>
    /// Id
    /// </summary>
    [Required(ErrorMessage = "Id不能为空")]
    public long Id { get; set; }

}

/// <summary>
/// EGJoinBound主键查询输入参数
/// </summary>
public class QueryByIdEGJoinBoundInput
{

    /// <summary>
    /// 入库编号
    /// </summary>
    public virtual string JoinBoundNum { get; set; }
}
