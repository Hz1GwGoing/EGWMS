using System.ComponentModel.DataAnnotations;

namespace Admin.NET.Application;

/// <summary>
/// EG_WMS_InAndOutBound基础输入参数
/// </summary>
public class EG_WMS_InAndOutBoundBaseInput
{
    /// <summary>
    /// 出入库编号
    /// </summary>
    public virtual string InAndOutBoundNum { get; set; }

    /// <summary>
    /// 出入库类型（0-入库，1-出库）
    /// </summary>
    public virtual int? InAndOutBoundType { get; set; }

    /// <summary>
    /// 出入库状态（0-未入库，1-已入库，2-未出库，3-已出库）
    /// </summary>
    public virtual int? InAndOutBoundStatus { get; set; }

    /// <summary>
    /// 出入库数量
    /// </summary>
    public virtual int? InAndOutBoundCount { get; set; }

    /// <summary>
    /// 出入库人员
    /// </summary>
    public virtual string? InAndOutBoundUser { get; set; }

    /// <summary>
    /// 出入库时间
    /// </summary>
    public virtual DateTime? InAndOutBoundTime { get; set; }

    /// <summary>
    /// 出入库备注
    /// </summary>
    public virtual string? InAndOutBoundRemake { get; set; }

}

/// <summary>
/// EG_WMS_InAndOutBound分页查询输入参数
/// </summary>
public class EG_WMS_InAndOutBoundInput : BasePageInput
{

    /// <summary>
    /// 物料编号
    /// </summary>
    public string MaterielNum { get; set; }


    /// <summary>
    /// 出入库编号
    /// </summary>
    public string InAndOutBoundNum { get; set; }

    /// <summary>
    /// 出入库类型（0-入库，1-出库）
    /// </summary>
    public int? InAndOutBoundType { get; set; }

    /// <summary>
    /// 出入库状态（0-未入库，1-已入库，2-未出库，3-已出库）
    /// </summary>
    public int? InAndOutBoundStatus { get; set; }

    /// <summary>
    /// 出入库数量
    /// </summary>
    public int? InAndOutBoundCount { get; set; }

    /// <summary>
    /// 出入库人员
    /// </summary>
    public string? InAndOutBoundUser { get; set; }

    /// <summary>
    /// 出入库时间
    /// </summary>
    public DateTime? InAndOutBoundTime { get; set; }

    /// <summary>
    /// 出入库时间范围
    /// </summary>
    public List<DateTime?> InAndOutBoundTimeRange { get; set; }
    /// <summary>
    /// 出入库备注
    /// </summary>
    public string? InAndOutBoundRemake { get; set; }

}

/// <summary>
/// EG_WMS_InAndOutBound增加输入参数
/// </summary>
public class AddEG_WMS_InAndOutBoundInput : EG_WMS_InAndOutBoundBaseInput
{
    /// <summary>
    /// 出入库编号
    /// </summary>
    [Required(ErrorMessage = "出入库编号不能为空")]
    public override string InAndOutBoundNum { get; set; }

}

/// <summary>
/// EG_WMS_InAndOutBound删除输入参数
/// </summary>
public class DeleteEG_WMS_InAndOutBoundInput : BaseIdInput
{
}

/// <summary>
/// EG_WMS_InAndOutBound更新输入参数
/// </summary>
public class UpdateEG_WMS_InAndOutBoundInput : EG_WMS_InAndOutBoundBaseInput
{
    /// <summary>
    /// Id
    /// </summary>
    [Required(ErrorMessage = "Id不能为空")]
    public long Id { get; set; }

}

/// <summary>
/// EG_WMS_InAndOutBound主键查询输入参数
/// </summary>
public class QueryByIdEG_WMS_InAndOutBoundInput : DeleteEG_WMS_InAndOutBoundInput
{

}
