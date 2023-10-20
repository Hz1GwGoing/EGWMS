using System;
namespace Admin.NET.Application.Service.AGV.V2.Task.DTO
{
    public class SumTaskDTO
    {
        /// <summary>
        /// 完成数
        /// </summary>
        public int FinisQTY { get; set; }
        /// <summary>
        /// 开始中
        /// </summary>
        public int StartQTY { get; set; }
        /// <summary>
        /// 未开始数 
        /// </summary>
        public int NotStartedQTY { get; set; }
        /// <summary>
        /// 取消数
        /// </summary>
        public int CancelQTY { get; set; }
        /// <summary>
        /// 失败数
        /// </summary>
        public int FailQTY { get; set; }
    }
}
