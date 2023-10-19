namespace Admin.NET.Application.Service.AGV.Task.DTO
{
    /// <summary>
    /// 新增任务
    /// </summary>
    public class AddDTO
    {
        /// <summary>
        /// 任务编号
        /// </summary>
        public string TaskNo { get; set; }
        /// <summary>
        /// 任务名称
        /// </summary>
        public string TaskName { get; set; }
        /// <summary>
        /// 优先级
        /// </summary>
        public string Priority { get; set; }
        /// <summary>
        /// 来源
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 模版编号
        /// </summary>
        public string ModelNo { get; set; }
        /// <summary>
        /// 是否追加任务，0-否(默认)、1-是
        /// </summary>
        public int IsAdd { get; set; } = 0;
        /// <summary>
        /// 任务下达人
        /// </summary>
        public string AddName { get; set; }

        public List<TaskDetailDTO> TaskDetail { get; set; }
    }
    /// <summary>
    /// 任务详细
    /// </summary>
    public class TaskDetailDTO
    {
        //internal readonly string AgvNo;
        /// <summary>
        /// 指定具体AGV执行
        /// </summary>
        public string AgvNo { get; set; }

        /// <summary>
        /// AGV路径点集：起始点位，任务中点位(选填)， 目标点位，任务点位间用英文“,”逗号分隔 
        /// </summary>
        public string Positions { get; set; }
    }
}
