namespace Admin.NET.Application.Service.EG_AGV_Task.DTO;


/// <summary>
/// 添加任务模板Model
/// </summary>
public class TemlogicModel
{
    /// <summary>
    /// 模板名称
    /// </summary>
    [Required(ErrorMessage = "模板名称不能为空")]
    public string TemlogicName { get; set; }

    /// <summary>
    /// 模板编号
    /// </summary>
    [Required(ErrorMessage = "模板编号不能为空")]
    public string TemLogicNo { get; set; }

    /// <summary>
    /// 任务点位数
    /// </summary>
    [Required(ErrorMessage = "任务点位数不能为空")]
    public int PointNum { get; set; }

}