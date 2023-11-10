namespace Admin.NET.Application.Service.AGV.V2.Task.DTO
{
    /// <summary>
    /// DH 任务主动上报DTO
    /// </summary>
    public class AcceptDTO
    {
        /// <summary>
        /// 第三方系统任务 id
        /// </summary>
        [Required(ErrorMessage = "任务编号不能为空")]
        public string orderId { get; set; }

        /// <summary>
        /// 当前车所在的位置，对应任务下发时下发的点位
        /// </summary>
        public string? qrContent { get; set; }

        /// <summary>
        /// Agv编号
        /// </summary>
        public string? deviceNum { get; set; }

        /// <summary>
        /// Agv序列号
        /// </summary>
        public string? deviceCode { get; set; }

        /// <summary>
        /// 任务状态（3 已取消 5 发送失败 6 运行中 7 执行失败 8 已完成 
        /// 9 已下发 10 等待确认 20 取货中 21 取货完成 22 放货中 23 放货完成）
        /// </summary>
        [Required(ErrorMessage = "任务状态不能为空")]
        public int status { get; set; }

        /// <summary>
        /// 立体库编号，对应立体库位置
        /// </summary>
        public string? storageNum { get; set; }

        /// <summary>
        /// 任务流程模版编号
        /// </summary>
        //[Required(ErrorMessage = "任务流程模版编号不能为空")]
        public string modelProcessCode { get; set; }

        /// <summary>
        /// 任务进程 当前完成子任务数量/总子任务数量
        /// </summary>
        public string? processRate { get; set; }

        /// <summary>
        /// 任务执行失败，或者发送给 AGV 时失败的失败原因
        /// </summary>
        public string? errorDesc { get; set; }

        /// <summary>
        /// 货架编号
        /// </summary>
        public string? shelfNumber { get; set; }

        /// <summary>
        /// 货架当前位置
        /// </summary>
        public string? shelfCurrPosition { get; set; }

        /// <summary>
        /// RCS 任务模版中的 AGV 执行到 的动作状态，扩展字段
        /// 1 未开始 2 运行中 3 完成中 4 失败 5 取消
        /// </summary>
        //[Required(ErrorMessage = "AGV执行到的动作状态不能为空")]
        public string subTaskStatus { get; set; }

        /// <summary>
        /// AGV 动作类型，扩展字段
        /// </summary>
        //[Required(ErrorMessage = "AGV动作类型不能为空")]

        public string subTaskTypeId { get; set; }

        /// <summary>
        /// RCS 子任务编号，备用字段
        /// </summary>
        //[Required(ErrorMessage = "RCS子任务编号不能为空")]

        public string subTaskId { get; set; }

        /// <summary>
        /// 第几个动作，从 0 开始，扩展字段
        /// </summary>
        //[Required(ErrorMessage = "执行到第几个动作不能为空")]

        public string subTaskSeq { get; set; }

        /// <summary>
        /// ICS 记录的此任务的 id 值，扩展字段
        /// </summary>
        //[Required(ErrorMessage = "ICS记录此任务id不能为空")]

        public string icsTaskOrderDetailId { get; set; }
    }
}
