using Furion.Extras.Admin.NET.Util;
using Newtonsoft.Json;

namespace Admin.NET.Application.Util
{
    /// <summary>
    /// DH 调用集合类
    /// wk 2022-07-21
    /// </summary>
    public class DHRequester
    {
        private static string _Host = App.GetOptions<RcsOptions>().Host;
        private static int _Port = App.GetOptions<RcsOptions>().Port;
        private static readonly HttpClientHelper _HttpClientHelper = new HttpClientHelper();
        private static string _AddTaskUrl;
        private static string _GetTaskUrl;
        private static string _GetTaskUrlV2;
        private static string _CancelTaskUrl;
        private static string _GoOnTaskUrl;
        private static string _UpdateTaskUrl;
        public DHRequester()
        {
            _AddTaskUrl = $"http://{_Host}:{_Port}/ics/taskOrder/addTask";
            _GetTaskUrl = $"http://{_Host}:{_Port}/ics/out/task/getTaskOrderStatus";
            _GetTaskUrlV2=$"http://{_Host}:{_Port}/ics/out/task/getTaskOrderStatusV2";
            _CancelTaskUrl = $"http://{_Host}:{_Port}/ics/out/task/cancelTask";
            _GoOnTaskUrl = $"http://{_Host}:{_Port}/ics/out/task/continueTask";
            _UpdateTaskUrl=$"http://{_Host}:{_Port}/ics/out/task/updateOrderPointInfo";
        }

        #region 任务相关
        /// <summary>
        /// 任务下发
        /// </summary>
        /// <param name="taskEntity">V3版本内的任务主表实体类</param>
        public async System.Threading.Tasks.Task<DHMessage> AddTaskAsync(TaskEntity taskEntity)
        {
            DHMessage dHMessage = new DHMessage();
            try
            {
                // 壹格参数与DH参数转化
                DHTask dHTask = new DHTask();
                dHTask.fromSystem = taskEntity.Source;
                dHTask.modelProcessCode = taskEntity.ModelNo;
                dHTask.orderId = taskEntity.TaskNo;
                dHTask.priority = taskEntity.Priority;
                dHTask.isAdd = taskEntity.IsAdd.ToString();

                dHTask.taskOrderDetail = new List<TaskOrderDetail>(); 
                var taskOrderDetail = new TaskOrderDetail
                {
                    assignRobotIds = taskEntity.AGV,
                    taskPath = taskEntity.TaskPath,
                };
                dHTask.taskOrderDetail.Add(taskOrderDetail);
                dHMessage = await PostToolAsync(_AddTaskUrl, dHTask);
            }
            catch (Exception ex)
            {
                string msg = $"下发任务接口{ex.Message}";
                dHMessage.code = 500;
                dHMessage.desc = msg;
            }
            return dHMessage;
        }

        /// <summary>
        /// 获取任务
        /// </summary>
        /// <param name="taskNO"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task<DHMessage> GetTaskAsync(string taskNO)
        {
            DHMessage dHMessage = new DHMessage();
            try
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("orderId", taskNO);
                dHMessage = await PostToolAsync(_GetTaskUrlV2, dic);
                //_ = FileUtil.DebugTxt(_GetTaskUrlV2, MessageTypeEnum.记录, taskNO,JsonConvert.SerializeObject(dHMessage) , "DH返回获取任务的信息记录");
            }
            catch (Exception ex)
            {
                string msg = $"获取任务接口{ex.Message}";
                dHMessage.code = 500;
                dHMessage.desc = msg;
            }
            return dHMessage;
        }

        /// <summary>
        /// 修改任务
        /// </summary>
        /// <param name="taskNo"></param>
        /// <param name="pointPath"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task<DHMessage> UpdateTaskAsync(string taskNo,string pointPath)
        {
            DHMessage dHMessage = new DHMessage();
            try
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("orderId", taskNo);
                dic.Add("pointPath", pointPath);
                dHMessage = await PostToolAsync(_UpdateTaskUrl, dic);
            }
            catch (Exception ex)
            {
                string msg = $"修改任务接口{ex.Message}";
                dHMessage.code = 500;
                dHMessage.desc = msg;
            }
            return dHMessage;
        } 
        
        /// <summary>
        /// 取消任务
        /// </summary>
        /// <param name="cancelDTO"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task<DHMessage> CancelTaskAsync(Service.AGV.V2.Task.DTO.CancelDTO cancelDTO)
        {
            DHMessage dHMessage = new DHMessage();
            try
            {
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    if (string.IsNullOrEmpty(cancelDTO.TaskNo))
                    {
                        throw Oops.Oh("必须传入任务ID！");
                    }
                    dic.Add("orderId", cancelDTO.TaskNo);
                    dic.Add("destPosition", cancelDTO.DestPosition);
                dHMessage = await PostToolAsync(_CancelTaskUrl, dic);
            }
            catch (Exception ex)
            {
                string msg = $"取消任务接口{ex.Message}";
                dHMessage.code = 500;
                dHMessage.desc = msg;
            }
            return dHMessage;
        }

        /// <summary>
        /// 继续任务
        /// </summary>
        /// <param name="taskNo"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task<DHMessage> GoOnTaskAsync(string taskNo)
        {
            var dHMessage = new DHMessage();
            try
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("orderId",taskNo.Trim());
                dHMessage = await PostToolAsync(_GoOnTaskUrl, dic);
            }
            catch (Exception ex)
            {
                var msg = $"继续任务接口{ex.Message}";
                dHMessage.code = 500;
                dHMessage.desc = msg;
            }
            return dHMessage;
        }
        #endregion

        #region 公用方法
        /// <summary>
        /// DH Post请求公用方法
        /// </summary>
        /// <param name="url">路径</param>
        /// <param name="obj">数据</param>
        /// <returns></returns>
        private async System.Threading.Tasks.Task<DHMessage> PostToolAsync(string url, object obj)
        {
            var dHMessage = new DHMessage();
            try
            {
                var rcsInput = JsonConvert.SerializeObject(obj);
                var result = await _HttpClientHelper.PostToolAsync(url, rcsInput);
                if (!result.IsSuccessStatusCode)
                {
                    throw Oops.Oh($"请求失败！");
                    //throw new Exception("请求失败！");
                }
                //获取消息状态
                var responseBody = await result.Content.ReadAsStringAsync();
                dHMessage = JsonConvert.DeserializeObject<DHMessage>(responseBody);
                if (dHMessage != null && dHMessage.code != 1000)
                {
                    throw Oops.Oh($"请求成功，但RCS返回错误：{dHMessage.desc}");
                    //throw new Exception($"请求成功，但RCS返回错误：{dHMessage.desc}");
                }
            }
            catch (Exception ex)
            {
                var msg = $"PostToolAsync失败：{ex.Message}={obj}";
                if (ex.Message.Contains("last point task not ok") || ex.Message.Contains("the order not allow 'add' opt"))
                {
                    msg = "请等待车辆到目的地后再执行！";
                } 
                if (dHMessage != null)
                {
                    dHMessage.code = 500;
                    dHMessage.desc = msg;
                }
                //_ = FileUtil.DebugTxt("V1.0 任务下达 PostToolAsync", MessageTypeEnum.错误, ex.Message, ex.StackTrace, "V1Error");
            }
            return dHMessage;
        }
        #endregion

    }
}
