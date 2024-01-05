namespace Admin.NET.Application.Service.EG_WMS_InAndOutBound.Dto;

public class AgvBoundDto
{
    /// <summary>
    /// 起始点
    /// </summary>
    public string? StartPoint { get; set; }

    /// <summary>
    /// 目标点
    /// </summary>
    [Required(ErrorMessage = "目标点不能为空")]
    public string EndPoint { get; set; }

    /// <summary>
    /// 任务编号
    /// </summary>
    public string? TaskNo { get; set; }

    /// <summary>
    /// 任务名称
    /// </summary>
    public string? TaskName { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    public string? Priority { get; set; } = "6";

    /// <summary>
    /// 来源
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// 模版编号
    /// </summary>
    [Required(ErrorMessage = "模版编号不能为空")]
    public string ModelNo { get; set; }

    /// <summary>
    /// 是否追加任务，0-否(默认)、1-是
    /// </summary>
    public int? IsAdd { get; set; } = 0;

    /// <summary>
    /// 任务下达人
    /// </summary>
    public string? AddName { get; set; }

    /// <summary>
    /// 出入库备注
    /// </summary>
    public string? InAndOutBoundRemake { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    [Required(ErrorMessage = "模版编号不能为空")]
    public string MaterielNum { get; set; }

    /// <summary>
    /// 出库数量
    /// </summary>
    public int? Sumcount { get; set; }
}