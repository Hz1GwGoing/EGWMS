namespace Admin.NET.Application;

/// <summary>
/// EGTakeStock基础输入参数
/// </summary>
public class EGTakeStockBaseInput
{
    /// <summary>
    /// 盘点编号
    /// </summary>
    public virtual string? TakeStockNum { get; set; }

    /// <summary>
    /// 盘点状态
    /// </summary>
    public virtual int? TakeStockStatus { get; set; } = 0;

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
    public virtual DateTime? TakeStockTime { get; set; }

    /// <summary>
    /// 盘点人员
    /// </summary>
    public virtual string? TakeStockUser { get; set; }

    /// <summary>
    /// 盘点备注
    /// </summary>
    public virtual string? TakeStockRemake { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    public virtual string? MaterielNum { get; set; }

}

/// <summary>
/// EGTakeStock分页查询输入参数
/// </summary>
public class EGTakeStockInput : BasePageInput
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
    /// 盘点时间范围
    /// </summary>
    public List<DateTime?> TakeStockTimeRange { get; set; }
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
    /// 创建时间
    /// </summary>
    public virtual DateTime? CreateTime { get; set; }
}

/// <summary>
/// EGTakeStock增加输入参数
/// </summary>
public class AddEGTakeStockInput : EGTakeStockBaseInput
{
    /// <summary>
    /// 盘点编号
    /// </summary>
    [Required(ErrorMessage = "盘点编号不能为空")]
    public override string? TakeStockNum { get; set; }

}

/// <summary>
/// EGTakeStock删除输入参数
/// </summary>
public class DeleteEGTakeStockInput : BaseIdInput
{
}

/// <summary>
/// EGTakeStock更新输入参数
/// </summary>
public class UpdateEGTakeStockInput : EGTakeStockBaseInput
{
    /// <summary>
    /// Id
    /// </summary>
    [Required(ErrorMessage = "Id不能为空")]
    public long Id { get; set; }

}

/// <summary>
/// EGTakeStock主键查询输入参数
/// </summary>
public class QueryByIdEGTakeStockInput
{

    /// <summary>
    /// 盘点编号
    /// </summary>
    public virtual string? TakeStockNum { get; set; }
}
