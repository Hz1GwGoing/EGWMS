﻿namespace Admin.NET.Application;

/// <summary>
/// EGTakeStock输出参数
/// </summary>
public class EGTakeStockOutput
{
    /// <summary>
    /// 盘点编号
    /// </summary>
    public string? TakeStockNum { get; set; }

    /// <summary>
    /// 盘点状态
    /// </summary>
    public int? TakeStockStatus { get; set; } = 0;

    /// <summary>
    /// 盘点类别（0.根据物料盘点 1.根据库位盘点）
    /// </summary>
    public int? TakeStockType { get; set; }

    /// <summary>
    /// 盘点数量
    /// </summary>
    public int? TakeStockCount { get; set; } = null;

    /// <summary>
    /// 差值数量
    /// </summary>
    public int? TakeStockDiffCount { get; set; } = null;

    /// <summary>
    /// 盘点时间
    /// </summary>
    public DateTime? TakeStockTime { get; set; }

    /// <summary>
    /// 盘点人员
    /// </summary>
    public string? TakeStockUser { get; set; }

    /// <summary>
    /// 盘点备注
    /// </summary>
    public string? TakeStockRemake { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    public string? MaterielNum { get; set; }

    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 盘点库位编号
    /// </summary>
    public string? TakeStockStorageNum { get; set; }


}


