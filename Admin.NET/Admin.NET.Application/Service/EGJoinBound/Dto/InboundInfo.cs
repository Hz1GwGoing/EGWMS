// 麻省理工学院许可证
//
// 版权所有 (c) 2021-2023 zuohuaijun，大名科技（天津）有限公司  联系电话/微信：18020030720  QQ：515096995
//
// 特此免费授予获得本软件的任何人以处理本软件的权利，但须遵守以下条件：在所有副本或重要部分的软件中必须包括上述版权声明和本许可声明。
//
// 软件按“原样”提供，不提供任何形式的明示或暗示的保证，包括但不限于对适销性、适用性和非侵权的保证。
// 在任何情况下，作者或版权持有人均不对任何索赔、损害或其他责任负责，无论是因合同、侵权或其他方式引起的，与软件或其使用或其他交易有关。

using System.Collections.Generic;

namespace Admin.NET.Application.Service.EGJoinBound.Dto;

/// <summary>
/// 入库信息实体
/// </summary>
public class InboundInfo
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
    /// 入库数量
    /// </summary>
    public int? JoinBoundCount { get; set; } = 0;

    /// <summary>
    /// 入库时间
    /// </summary>
    public DateTime? JoinBoundTime { get; set; }

    /// <summary>
    /// 入库状态 0 未入库 1 已入库（保存在库存表中）
    /// </summary>
    public int? JoinBoundStatus { get; set; }


    /// <summary>
    /// 回库更新时间
    /// </summary>
    public DateTime? JoinBoundOutTime { get; set; }

    /// <summary>
    /// 仓库编号
    /// </summary>
    public string? WHNum { get; set; }

    /// <summary>
    /// 栈板编号
    /// </summary>
    /// ！
    public string? PalletNum { get; set; }

    /// <summary>
    /// 料箱编号
    /// </summary>
    /// ！
    public string? WorkBinNum { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    public string? MaterielNum { get; set; }

    /// <summary>
    /// 入库备注
    /// </summary>
    public string? JoinBoundRemake { get; set; }
    public List<InventoryInfo> detail { get; set; }


}

/// <summary>
/// 库存信息实体
/// </summary>
public class InventoryInfo
{
    /// <summary>
    /// 库存总数
    /// </summary>
    public int? ICountAll { get; set; } = 0;

    /// <summary>
    /// 可用库存
    /// </summary>
    public int? IUsable { get; set; } = 0;

    /// <summary>
    /// 冻结数量
    /// </summary>
    public int? IFrostCount { get; set; } = 0;

    /// <summary>
    /// 待检数量
    /// </summary>
    public int? IWaitingCount { get; set; } = 0;


    /// <summary>
    /// 入库编号
    /// </summary>
    public string JoinBoundNum { get; set; }


    /// <summary>
    /// 仓库编号
    /// </summary>
    public string? WHNum { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    public string? MaterielNum { get; set; }


    //public string MaterielNum { get; set; }
    //public int JoinBoundCount { get; set; }
    //public string WorkBinNum { get; set; }
    ///// <summary>
    ///// 入库人
    ///// </summary>
    //public string? JoinBoundUser { get; set; }

}