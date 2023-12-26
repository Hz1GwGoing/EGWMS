using Admin.NET.Application.Service.AGV.V2.Task.DTO;

namespace Admin.NET.Application.Service.AGV.V2.Task
{
    /// <summary>
    /// 任务信息 V3.0
    /// </summary>
    public interface ITaskService
    {
        /// <summary>
        /// 获取点位信息
        /// </summary>
        /// <param name="inDTO"></param>
        /// <returns></returns>
       Task<SqlSugarPagedList<PageDTO>> PageAsync(InDTO inDTO);

        /// <summary>
        /// 新增任务
        /// </summary>
        /// <param name="addDTO"></param>
        /// <returns></returns>
        System.Threading.Tasks.Task AddAsync(AGV.Task.DTO.AddDTO addDTO);

        /// <summary>
        /// 取消任务
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        System.Threading.Tasks.Task CancelAsync(List<DTO.CancelDTO> list);

        /// <summary>
        /// 接受RCS上报结果
        /// </summary>
        /// <param name="addDTO"></param>
        /// <returns></returns>
        System.Threading.Tasks.Task<string> AcceptAsync(AcceptDTO addDTO);
    }
}
