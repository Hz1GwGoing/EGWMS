namespace Admin.NET.Application.Service.AGV.V2.Task.DTO
{
    public class UpdateTaskPointDto
    {
        /// <summary>
        /// 任务单号
        /// </summary>
        public  string TaskNo { get; set; }
        /// <summary>
        /// 变更点位
        /// </summary>
        public string PointPath { get; set; }
    }
}