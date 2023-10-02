namespace Admin.NET.Application.Entity;


/// <summary>
/// 入库详细表实体（山东宇翔）
/// agv入库
/// </summary>

[SugarTable("EGJoinBoundNewEntity", "入库详细表（山东宇翔）")]
public class EGJoinBoundNewEntity : EntityBase
{
    /// <summary>
    /// 入库详细表编号
    /// </summary>
    [SugarColumn(ColumnDescription = "入库详细表编号")]
    public string JoinBoundNewNum { get; set; }

    /// <summary>
    /// 起始点
    /// </summary>
    public string StartPoint { get; set; }

    /// <summary>
    /// 目标点
    /// </summary>
    public string TargetPoint { get; set; }



}
