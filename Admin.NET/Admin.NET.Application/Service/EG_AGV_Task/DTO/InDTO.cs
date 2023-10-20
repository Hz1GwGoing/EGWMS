using Admin.NET.Application.AGV.AGVEntity;

namespace Admin.NET.Application.Service.AGV.V2.Task.DTO
{
    /// <summary>
    /// 翻页查询任务DTO
    /// WK 2022-04-23
    /// </summary>
    public class InDTO
    {
        /// <summary>
        /// 任务编号
        /// </summary>
        public virtual string? TaskNo { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        public virtual string? TaskName { get; set; }

        /// <summary>
        /// AGV编号
        /// </summary>
        public virtual string? AGVNo { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public virtual string? TaskState { get; set; }

        /// <summary>
        /// 来源
        /// </summary>
        public virtual string? Source { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 页容量
        /// </summary>
        public int PageSize { get; set; }
    }

    /// <summary>
    /// 取消任务传入DTO
    /// WK 2022-04-23
    /// </summary>
    public class CancelDTO
    {
        /// <summary>
        /// 任务编号
        /// </summary>
        public string TaskNo { get; set; }
        /// <summary>
        /// 将货物放下的点位
        /// </summary>
        public string DestPosition { get; set; }
    }
}
