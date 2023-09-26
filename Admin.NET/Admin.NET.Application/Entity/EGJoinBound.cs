using Admin.NET.Application.Service.EGJoinBound.Dto;
using Admin.NET.Core;
using System.Collections.Generic;

namespace Admin.NET.Application.Entity;

/// <summary>
/// 入库实体
/// </summary>
[SugarTable("EGJoinBound", "入库信息表")]
public class EGJoinBound : EntityBase
{
    /// <summary>
    /// 入库编号
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "入库编号", Length = 50)]
    public string JoinBoundNum { get; set; }

    /// <summary>
    /// 入库类型
    /// </summary>
    [SugarColumn(ColumnDescription = "入库类型")]
    public int? JoinBoundType { get; set; }

    /// <summary>
    /// 入库人
    /// </summary>
    [SugarColumn(ColumnDescription = "入库人", Length = 50)]
    public string? JoinBoundUser { get; set; }

    /// <summary>
    /// 入库数量
    /// </summary>
    [SugarColumn(ColumnDescription = "入库数量")]
    public int? JoinBoundCount { get; set; }

    /// <summary>
    /// 入库时间
    /// </summary>
    [SugarColumn(ColumnDescription = "入库时间")]
    public DateTime? JoinBoundTime { get; set; }

    /// <summary>
    /// 入库状态 0 未入库 1 已入库（保存在库存表中）
    /// </summary>
    [SugarColumn(ColumnDescription = "入库状态")]
    public int? JoinBoundStatus { get; set; }


    /// <summary>
    /// 回库更新时间
    /// </summary>
    [SugarColumn(ColumnDescription = "回库更新时间")]
    public DateTime? JoinBoundOutTime { get; set; }

    /// <summary>
    /// 仓库编号
    /// </summary>
    [SugarColumn(ColumnDescription = "仓库编号", Length = 50)]
    public string? WHNum { get; set; }

    /// <summary>
    /// 栈板编号
    /// </summary>
    /// ！
    [SugarColumn(ColumnDescription = "栈板编号", Length = 50)]
    public string? PalletNum { get; set; }

    /// <summary>
    /// 料箱编号
    /// </summary>
    /// ！
    [SugarColumn(ColumnDescription = "料箱编号", Length = 50)]
    public string? WorkBinNum { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    [SugarColumn(ColumnDescription = "物料编号", Length = 50)]
    public string? MaterielNum { get; set; }

    /// <summary>
    /// 入库备注
    /// </summary>
    [SugarColumn(ColumnDescription = "入库备注", Length = 500)]
    public string? JoinBoundRemake { get; set; }


}
