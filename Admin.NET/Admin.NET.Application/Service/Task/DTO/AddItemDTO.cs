using System;
using System.Collections.Generic;
using Admin.NET.Application.Entity;

namespace Admin.NET.Application
{

    #region 壹格自有参数
    /// <summary>
    /// 
    /// </summary>
    public class AddItemDTO
    {
        /// <summary>
        /// 任务编号
        /// </summary>
        public string TaskNo { get; set; }
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// 来源
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 模版编号
        /// </summary>
        public string TemplateNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<TaskDetail> TaskDetail { get; set; }
    }
    /// <summary>
    /// 任务单详细
    /// </summary>
    public class TaskDetail
    {
        /// <summary>
        /// AGV路径点集：起始点位，任务中点位(选填)， 目标点位，任务点位间用英文“,”逗号分隔 
        /// </summary>
        public string Positions { get; set; }
        /// <summary>
        /// 指定具体AGV执行
        /// </summary>
        public string AgvNo { get; set; }
    }
    #endregion

    #region DH对应的参数
    public class DHTask
    {
        public string modelProcessCode { get; set; }
        public int priority { get; set; }
        public string fromSystem { get; set; }
        public string orderId { get; set; }
        /// <summary>
        /// 是否追加任务，0-否、1-是
        /// </summary>
        public string isAdd { get; set; } = "0";
        public List<TaskOrderDetail> taskOrderDetail { get; set; }
    }
    public class TaskOrderDetail
    {
        public string taskPath { get; set; }
        public string assignRobotIds { get; set; }
    }
    #endregion

}
