namespace Admin.NET.Application.Service.EGJoinBoundNew.Dto;
public class EGJoinBoundNewDto : EntityBase
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
    public int? JoinBoundCount { get; set; }

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

    public List<EGWorkBinDetailDto> workBinDetailDto { get; set; }
}

public class EGWorkBinDetailDto
{
    /// <summary>
    /// 料箱编号
    /// </summary>
    public string WorkBinNum { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    public string MaterielNum { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    public int ProductCount { get; set; }

    /// <summary>
    /// 生产时间
    /// </summary>
    public DateTime ProductionDate { get; set; }

    /// <summary>
    /// 生产批次
    /// </summary>
    public string ProductionLot { get; set; }

}
