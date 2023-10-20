using System;
using System.Collections.Generic;
//using Admin.NET.Application.Entity.AGV.V2;

namespace Admin.NET.Application.Service.AGV.V2.Task.DTO
{
    /// <summary>
    /// 任务单获取翻页DTO
    /// </summary>
    public class PageDTO
    {
        public long Id { get; set; }
        /// <summary>
        /// 编号
        /// </summary>
        public string TaskNo { get; set; }
        /// <summary>
        /// 任务名称
        /// </summary>
        public string TaskName { get; set; }
        /// <summary>
        /// AGV编号
        /// </summary>
        public string AGVNo { get; set; }
        /// <summary>
        /// 任务状态
        /// </summary>
        public string TaskState { get; set; }
        /// <summary>
        /// 执行等级
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// 模版编号
        /// </summary>
        public string ModelNo { get; set; }
        /// <summary>
        /// 来源
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 执行结果信息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? STime { get; set; }
        /// <summary>
        /// 结果时间
        /// </summary>
        public DateTime? ETime { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset? CreatedTime { get; set; }

        public List<PageDetailDTO> PageDetailDTOList { get; set; }
    }

    public class PageDetailDTO
    {
        /// <summary>
        /// 任务单ID
        /// </summary>
        public long TaskID { get; set; }

        /// <summary>
        /// 任务单路径
        /// </summary>
        public string TaskPath { get; set; }
        /// <summary>
        /// 任务点位名称
        /// </summary>
        public string TaskPathName { get; set; }
        /// <summary>
        /// 货架编号
        /// </summary>
        public string ShelfNo { get; set; }

    }
}
