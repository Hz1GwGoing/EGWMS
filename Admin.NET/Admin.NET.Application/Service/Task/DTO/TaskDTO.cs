using System.ComponentModel.DataAnnotations.Schema;
using Admin.NET.Application.AGV.AGVEntity;

namespace Admin.NET.Application.Service.AGV.V2.Task.DTO
{

    [NotMapped]
    public class TaskDTO: TaskEntity
    {
        /// <summary>
        /// 资产编号
        /// </summary>
        public string AssetNumber { get; set; }
        
        //public string SubTaskStatus { get; set; }
    }
}
