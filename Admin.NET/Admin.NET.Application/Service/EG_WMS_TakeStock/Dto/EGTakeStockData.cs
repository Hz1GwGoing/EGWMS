﻿namespace Admin.NET.Application.Service.EGTakeStock.Dto;


/// <summary>
/// 盘点返回数据
/// </summary>
public class EGTakeStockData
{

    /// <summary>
    /// 物料编号
    /// </summary>
    public string MaterielNum { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    public string MaterielName { get; set; }

    /// <summary>
    /// 物料规格
    /// </summary>
    public string MaterielSpecs { get; set; }

    /// <summary>
    /// 盘点状态
    /// </summary>
    public int TakeStockStatus { get; set; }

    /// <summary>
    /// 盘点类别（0.根据物料盘点 1.根据库位盘点）
    /// </summary>
    public int? TakeStockType { get; set; }

    /// <summary>
    /// 系统存量
    /// </summary>
    public int ICountAll { get; set; }

    /// <summary>
    /// 盘点数量
    /// </summary>
    public int? TakeStockCount { get; set; }

    /// <summary>
    /// 差值数量
    /// </summary>
    public int? DiffCount { get; set; }


}
