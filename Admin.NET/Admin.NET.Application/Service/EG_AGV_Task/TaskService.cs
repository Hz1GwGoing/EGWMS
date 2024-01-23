using Admin.NET.Application.Service.AGV.Task.DTO;
using Admin.NET.Application.Util;
using Microsoft.AspNetCore.Authorization;
using Admin.NET.Application.Service.AGV.V2.Task.DTO;
using Newtonsoft.Json;
using Admin.NET.Application.Service.EG_AGV_Task.DTO;

namespace Admin.NET.Application.Service.EG_AGV_Task
{
    [ApiDescriptionSettings("AGV模块接口V3.0", Name = "任务信息", Order = 100)]
    public class TaskService : IDynamicApiController, ITransient
    {
        private static readonly DHRequester _DHRequester = new DHRequester();
        private static readonly EG_WMS_InAndOutBoundService _EG_WMS_InAndOutBoundService = new EG_WMS_InAndOutBoundService();

        #region 关系注入
        private readonly SqlSugarRepository<TaskEntity> _TaskEntity = App.GetService<SqlSugarRepository<TaskEntity>>();
        private readonly SqlSugarRepository<TaskDetailEntity> _TaskDetailEntity = App.GetService<SqlSugarRepository<TaskDetailEntity>>();
        private readonly SqlSugarRepository<TemLogicEntity> _TemLogicEntity = App.GetService<SqlSugarRepository<TemLogicEntity>>();
        private readonly SqlSugarRepository<InfoEntity> _InfoEntity = App.GetService<SqlSugarRepository<InfoEntity>>();
        private readonly SqlSugarRepository<Entity.EG_WMS_InAndOutBound> _InAndOutBound = App.GetService<SqlSugarRepository<Entity.EG_WMS_InAndOutBound>>();
        private readonly SqlSugarRepository<EG_WMS_Tem_Inventory> _TemInventory = App.GetService<SqlSugarRepository<EG_WMS_Tem_Inventory>>();
        private readonly SqlSugarRepository<EG_WMS_Tem_InventoryDetail> _TemInventoryDetail = App.GetService<SqlSugarRepository<EG_WMS_Tem_InventoryDetail>>();
        private readonly SqlSugarRepository<Entity.EG_WMS_Inventory> _Inventory = App.GetService<SqlSugarRepository<Entity.EG_WMS_Inventory>>();
        private readonly SqlSugarRepository<EG_WMS_InventoryDetail> _InventoryDetail = App.GetService<SqlSugarRepository<EG_WMS_InventoryDetail>>();
        private readonly SqlSugarRepository<EG_WMS_InAndOutBoundDetail> _InAndOutBoundDetail = App.GetService<SqlSugarRepository<EG_WMS_InAndOutBoundDetail>>();
        private readonly SqlSugarRepository<Entity.EG_WMS_Region> _Region = App.GetService<SqlSugarRepository<Entity.EG_WMS_Region>>();
        private readonly SqlSugarRepository<Entity.EG_WMS_Storage> _Storage = App.GetService<SqlSugarRepository<Entity.EG_WMS_Storage>>();
        private readonly SqlSugarRepository<Entity.EG_WMS_WorkBin> _WorkBin = App.GetService<SqlSugarRepository<Entity.EG_WMS_WorkBin>>();
        private readonly SqlSugarRepository<InfoAgvEntity> _InfoAgvEntity = App.GetService<SqlSugarRepository<InfoAgvEntity>>();
        private readonly SqlSugarRepository<AlarmEntity> _AlarmEntity = App.GetService<SqlSugarRepository<AlarmEntity>>();

        #endregion

        #region 构造函数
        public TaskService()
        {

        }
        #endregion

        #region 任务下达
        public async Task<DHMessage> AddAsync(TaskEntity taskEntity)
        {
            // 查询是否有传入的模板
            var temLogItem = _TemLogicEntity.GetFirst(p => p.TemLogicNo == taskEntity.ModelNo);
            if (temLogItem == null || string.IsNullOrEmpty(temLogItem.TemLogicNo))
            {
                throw Oops.Oh($"未找到对应的任务模版！");
            }

            // 得到任务点位数
            var positions = taskEntity.TaskPath.Split(',');

            // 判断是不是需要去等待点再次获取任务点位
            // 判断点位数量和是否需要去等待点是否同同时满足，同时满足则需要返回错误
            if (temLogItem.PointNum != positions.Length && temLogItem.HoldingPoint == "0")
            {

                throw Oops.Oh($"传入的点位数量与模版预设的点位数不符，并且模板不是需要前往等待点！");

            }
            #region 本地入库
            if (taskEntity.Id == 0) taskEntity.Id = SnowFlakeSingle.Instance.NextId();
            taskEntity.TaskNo = taskEntity.TaskNo ?? taskEntity.Id.ToString();
            taskEntity.CreateTime = DateTime.Now;
            taskEntity.UpdateTime = DateTime.Now;
            taskEntity.Source = taskEntity.Source ?? "手工下发";
            taskEntity.TaskState = null;
            if (taskEntity.Priority == 0) taskEntity.Priority = 6;
            taskEntity.TaskName = taskEntity.TaskName ?? temLogItem.TemLogicName;

            var taskDetailList = positions.Select(t => new TaskDetailEntity
            {
                TaskID = taskEntity.Id,
                TaskPath = t,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now
            }).ToList();
            //await _TaskDetailEntity.InsertOrUpdateAsync(taskDetailList); //任务详细
            #endregion

            #region DH请求
            DHMessage dHMessage = await _DHRequester.AddTaskAsync(taskEntity);
            if (dHMessage.code == 500)
            {
                throw Oops.Oh($"{dHMessage.desc}");
            }
            //var nowTaskItemJsonString = JsonConvert.SerializeObject(input);
            #endregion

            var item = await _TaskEntity.InsertAsync(taskEntity); //任务主表

            await _TaskDetailEntity.InsertOrUpdateAsync(taskDetailList); //任务详细
            return dHMessage;
        }


        /// <summary>
        /// 下达任务
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/AGV/Task/AddAsync")]
        public async Task AddAsync(AddDTO input)
        {
            //var para = JsonConvert.SerializeObject(input);
            //_ = FileUtil.DebugTxt("V2.0 新增任务记录", MessageTypeEnum.记录, para, "", "新增任务记录");
            try
            {
                #region 请求数据验证
                if (input.IsAdd == 1 && string.IsNullOrEmpty(input.TaskNo))
                {
                    throw Oops.Oh($"isAdd等于1为追加任务，追加任务必须传入被追加的任务编号！");
                    //throw new Exception("isAdd等于1为追加任务，追加任务必须传入被追加的任务编号！");
                }

                if (string.IsNullOrEmpty(input.ModelNo))
                {
                    throw Oops.Oh($"请传入任务模版编号！");
                }

                if (string.IsNullOrEmpty(input.TaskDetail[0].Positions))
                {
                    throw Oops.Oh($"请传入运行的点位信息！");
                }
                #endregion

                TaskEntity taskEntity = input.Adapt<TaskEntity>();
                taskEntity.TaskPath = input.TaskDetail[0].Positions;
                taskEntity.AGV = input.TaskDetail[0].AgvNo;
                await AddAsync(taskEntity);
            }
            catch (Exception ex)
            {
                //_ = FileUtil.DebugTxt(ex.Message, MessageTypeEnum.记录, para, ex.StackTrace, "新增任务Error");
                throw Oops.Oh($"{ex.Message}");
            }
        }
        #endregion

        #region 追加点位
        /// <summary>
        /// 追加点位信息
        /// </summary>
        /// <param name="input"> </param>
        /// <returns></returns>
        [HttpPost("/AGV/Task/AppendAsync")]
        public async Task AppendAsync(AppendAsyncDTO input)
        {
            //var para = JsonConvert.SerializeObject(dto);
            //_ = FileUtil.DebugTxt("V2.0 AppendAsync", MessageTypeEnum.记录, para, "", "追加点位信息");
            try
            {
                #region 请求数据验证
                //var temLogicNo = App.Configuration["RCS:TemLogicNo"];
                //if (string.IsNullOrEmpty(temLogicNo))
                //{
                //    throw new Exception("现场实施未在配置文件内配置对应的模版编号！");
                //}

                if (string.IsNullOrEmpty(input.taskNo))
                {
                    throw Oops.Oh($"任务编号必须传入！");
                }

                if (string.IsNullOrEmpty(input.positions))
                {
                    throw Oops.Oh($"点位信息必须传入！");
                }

                //if (string.IsNullOrEmpty(input.source))
                //{
                //    throw Oops.Oh($"来源必须传入！");
                //}
                #endregion

                #region 本地入库

                #endregion

                #region DH请求

                #endregion
            }
            catch (Exception ex)
            {
                //_ = FileUtil.DebugTxt(ex.Message, MessageTypeEnum.记录, para, ex.StackTrace, "新增任务Error");
                throw Oops.Oh(ex.Message);
            }
        }
        #endregion

        #region 变更任务点位
        /// <summary>
        /// 变更任务点位
        /// </summary>
        /// <param name="updateTaskPointDto"></param>
        /// <returns></returns>
        [HttpPost("/AGV/Task/UpdateTaskPoint")]
        public async Task UpdateTaskPoint(UpdateTaskPointDto updateTaskPointDto)
        {
            //var para = JsonConvert.SerializeObject(updateTaskPointDto);
            try
            {
                if (string.IsNullOrEmpty(updateTaskPointDto.TaskNo))
                {
                    throw Oops.Oh($"必须传入需要变更的任务单号！");
                }
                if (string.IsNullOrEmpty(updateTaskPointDto.PointPath))
                {
                    throw Oops.Oh("必须传入点位信息！");
                }
                var taskItem = _TaskEntity.GetFirst(p => p.TaskNo == updateTaskPointDto.TaskNo);
                if (taskItem == null || string.IsNullOrEmpty(taskItem.TaskNo))
                {
                    throw Oops.Oh($"未找到{updateTaskPointDto.TaskNo}的任务单！");
                }
                var taskDetalList = _TaskDetailEntity.GetList(p => p.TaskID == taskItem.Id);
                if (taskDetalList != null && taskDetalList.Any())
                {
                    await _TaskDetailEntity.DeleteAsync(taskDetalList);
                }
                var paths = updateTaskPointDto.PointPath.Split(',');
                List<TaskDetailEntity> newTaskDetailList = new List<TaskDetailEntity>();
                foreach (var item in paths)
                {
                    TaskDetailEntity newTaskDetail = new TaskDetailEntity();
                    newTaskDetail.TaskPath = item;
                    newTaskDetail.TaskID = taskItem.Id;
                    newTaskDetailList.Add(newTaskDetail);
                }
                #region 调用DH
                DHMessage dHMessage = await _DHRequester.UpdateTaskAsync(updateTaskPointDto.TaskNo, updateTaskPointDto.PointPath);
                //_ = FileUtil.DebugTxt("DH返回信息", MessageTypeEnum.记录, JsonConvert.SerializeObject(dHMessage), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "V2.0任务变更请求记录");
                if (dHMessage.code == 500)
                {
                    throw Oops.Oh(dHMessage.desc);
                }
                #endregion

                await _TaskDetailEntity.InsertOrUpdateAsync(newTaskDetailList);
                //_ = FileUtil.DebugTxt("请求日志", MessageTypeEnum.记录, para, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "V2.0任务变更请求记录");

            }
            catch (Exception ex)
            {
                //_ = FileUtil.DebugTxt(ex.Message, MessageTypeEnum.错误, para, ex.StackTrace, "V2.0任务变更错误记录");
                throw Oops.Oh(ex.Message);
            }
        }
        #endregion

        #region 请求第三方系统获取的任务点位

        public async Task a(RequestPointDto input)
        {
            // 查询得到符合任务的信息
            var dataTask = await _TaskEntity.GetFirstAsync(x => x.TaskNo == input.orderId);
            if (dataTask == null)
            {
                throw Oops.Oh("没有找到符合条件的任务信息");
            }

            // 根据agv任务的出入库编号，得到物料编号
            var dataInOutBound = await _InAndOutBoundDetail.GetFirstAsync(x => x.InAndOutBoundNum == dataTask.InAndOutBoundNum);

            if (dataInOutBound == null)
            {
                throw Oops.Oh("没有找到符合条件的出入库信息");
            }

            // 根据物料编号查找所在区域
            var dataRegion = await _Region.GetFirstAsync(x => x.RegionMaterielNum == dataInOutBound.MaterielNum);

            if (dataRegion == null)
            {
                throw Oops.Oh("该物料未指定存放区域");
            }

            // 在agv任务中查找正在进行任务的目标点，即出入库需要去到的库位编号

            var dataTaskStorage = _TaskEntity.AsQueryable()
                        // 执行中的任务状态 
                        .Where(x => x.TaskState == 5)
                        .ToList();

            List<string> storageArr = new List<string>();
            for (int i = 0; i < dataTaskStorage.Count; i++)
            {
                var taskpath = dataTaskStorage[i].TaskPath;

                string[] strings = taskpath.Split(",");

                storageArr.Add(strings[1]);


            }


            // 查找该区域下有多少个组（有多少组密集库）

            var dataStorage = _Storage.AsQueryable()
                      .Where(x => x.RegionNum == dataRegion.RegionNum)
                      .Select(x => new class2
                      {
                          StorageGroup = x.StorageGroup,
                          SumCount = SqlFunc.AggregateCount(x.StorageNum)
                      })
                      .GroupBy(x => x.StorageGroup)
                      .ToList();

            // 根据组别查找库位

        }

        #endregion

        #region 获取数据


        /// <summary>
        /// 分页查询任务单信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/AGV/Task/PageAsync")]
        public async Task<SqlSugarPagedList<TaskEntity>> PageAsync(InDTO input)
        {
            try
            {
                DateTime? sTime = null;
                DateTime? eTime = null;

                if (input.CreateTime != null)
                {
                    sTime = Convert.ToDateTime(
                        $"{Convert.ToDateTime(input.CreateTime).ToString("yyyy-MM-dd")} 00:00:00");
                    eTime = Convert.ToDateTime(
                        $"{Convert.ToDateTime(input.CreateTime).ToString("yyyy-MM-dd")} 23:59:59");
                }
                var taskPageList = await _TaskEntity.AsQueryable()
                    .WhereIF(!string.IsNullOrWhiteSpace(input.AGVNo), u => u.AGV.Contains(input.AGVNo.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.TaskName), u => u.TaskName.Contains(input.TaskName.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.TaskNo), u => u.TaskNo.Contains(input.TaskNo.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.Source), u => u.Source.Contains(input.Source.Trim()))
                    .WhereIF(input.CreateTime != null, p => p.CreateTime >= sTime && p.CreateTime <= eTime)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.TaskState), u => input.TaskState.Trim().Contains(u.TaskState.ToString()))
                    .OrderByDescending(p => p.Id)
                    .ToPagedListAsync(input.Page, input.PageSize);
                return taskPageList;
            }
            catch (Exception ex)
            {
                //_ = FileUtil.DebugTxt(ex.Message, MessageTypeEnum.错误, para, ex.StackTrace, "获取任务列表Error");
                throw Oops.Oh($"{ex.Message}");
            }
        }

        /// <summary>
        /// 获取任务信息
        /// </summary>
        /// <param name="taskNo"></param>
        /// <returns></returns>
        [HttpGet("/AGV/Task/GetItemAsync")]
        public async Task<TaskDTO> GetItemAsync(string taskNo)
        {
            //_ = FileUtil.DebugTxt("获取任务详细", MessageTypeEnum.记录, taskNo, "", "获取任务详细记录");
            //TaskDTO taskDto = new TaskDTO();
            try
            {
                if (string.IsNullOrEmpty(taskNo))
                {
                    throw Oops.Oh($"任务ID不可为空！");
                }
                TaskEntity taskItem = _TaskEntity.GetFirst(p => p.TaskNo == taskNo);
                TaskDTO taskDto = taskItem.Adapt<TaskDTO>();
                if (taskItem != null && !string.IsNullOrEmpty(taskItem.TaskNo))
                {
                    var dHMessage = await _DHRequester.GetTaskAsync(taskNo);
                    if (dHMessage.code == 500)
                    {
                        throw Oops.Oh($"{dHMessage.desc}");
                    }
                    var daString = JsonConvert.SerializeObject(dHMessage.data);
                    var da = JsonConvert.DeserializeObject<taskV2>(daString);

                    if (!string.IsNullOrEmpty(taskItem.AGV))
                    {
                        var infoItem = await _InfoEntity.GetFirstAsync(p => p.AgvNo == taskItem.AGV);
                        taskDto.AssetNumber = infoItem?.AssetNumber;
                    }
                    if (da.statusObj != null)
                    {
                        var detaiItem = JsonConvert.DeserializeObject<AcceptDTO>(da.statusObj.ToString());
                        //var detaiItem = da.taskOrderDetail[0];
                        taskDto.SubTaskSeq = detaiItem?.subTaskSeq;
                        taskDto.SubTaskStatus = detaiItem?.subTaskStatus;
                        taskDto.TaskState = detaiItem.status;
                        taskDto.AGV = detaiItem?.deviceNum;
                    }
                    taskItem.TaskState = taskDto.TaskState;
                    taskItem.SubTaskSeq = taskDto.SubTaskSeq;
                    await _TaskEntity.UpdateAsync(taskItem);
                }
                return taskDto;
            }
            catch (Exception ex)
            {
                //_ = FileUtil.DebugTxt(ex.Message, MessageTypeEnum.错误, taskNo, ex.StackTrace, "获取任务详细Error");
                throw Oops.Oh($"{ex.Message}");
            }
        }


        /// <summary>
        /// 任务单各状态数量汇总
        /// </summary>
        /// <param name="dateTime">汇总日期</param>
        /// <returns></returns>
        [HttpGet("/AGV/Task/SumTask")]
        public SumTaskDTO SumTask(DateTime dateTime)
        {
            SumTaskDTO sumTaskDto = new SumTaskDTO();
            //var taskList = _TaskEntity.SqlQueryAsync<TaskEntity>($"select * from agv_task where STR_TO_DATE({nameof(TaskEntity.CreatedTime)}, '%Y-%m-%d')='{dateTime.ToString("yyyy-MM-dd")}'").Result;
            // null：请求成功；1：未发送；2：正在取消；3：已取消；4：正在发送；5：发送失败；6：执行中；7：执行失败；8：任务完成；9：已下发;
            var taskList = _TaskEntity.GetList(p =>
                Convert.ToDateTime(p.CreateTime).ToString("yyyy-MM-dd") == dateTime.ToString("yyyy-MM-dd"));
            sumTaskDto.CancelQTY = taskList.Where(p => p.TaskState == 2 || p.TaskState == 3)?.Count() ?? 0;
            sumTaskDto.FailQTY = taskList.Where(p => p.TaskState == 5 || p.TaskState == 7)?.Count() ?? 0;
            sumTaskDto.FinisQTY = taskList.Where(p => p.TaskState == 8)?.Count() ?? 0;
            sumTaskDto.NotStartedQTY =
                taskList.Where(p => p.TaskState == null || p.TaskState == 9 || p.TaskState == 4 || p.TaskState == 1)
                    ?.Count() ?? 0;
            sumTaskDto.StartQTY = taskList.Where(p => p.TaskState == 6)?.Count() ?? 0;
            return sumTaskDto;
        }

        #endregion

        #region 增加任务模板

        /// <summary>
        /// 增加任务模板
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/AGV/Task/AddTemLogicModel")]
        public async Task AddTemLogicModel(TemlogicModel input)
        {
            var model = input.Adapt<TemLogicEntity>();
            await _TemLogicEntity.InsertAsync(model);

        }

        #endregion

        #region 查询任务模板

        /// <summary>
        /// 查询任务模板
        /// </summary>
        /// <returns></returns>
        [HttpGet("/AGV/Task/GetTemLogicsAll")]
        public List<TemLogicEntity> GetTemLogicsAll()
        {
            return _TemLogicEntity.AsQueryable()
                            .Select(x => x)
                            .ToList();

        }


        #endregion

        #region 删除任务模板

        /// <summary>
        /// 删除任务模板
        /// </summary>
        /// <param name="num">任务模板编号</param>
        /// <param name="id">任务模板id</param>
        /// <returns></returns>
        [HttpPost("/AGV/Task/DeleteTemLogic")]
        public async Task DeleteTemLogic(string? num, long? id)
        {
            var entity = await _TemLogicEntity.GetFirstAsync(x => x.TemLogicNo == num || x.Id == id);
            await _TemLogicEntity.FakeDeleteAsync(entity);
        }


        #endregion

        #region 更新任务模板

        /// <summary>
        /// 更新任务模板
        /// </summary>
        /// <returns></returns>
        [HttpPost("/AGV/Task/UpdateTemLogic")]
        public async Task UpdateTemLogic(UpdateTem input)
        {
            var entity = input.Adapt<TemLogicEntity>();
            await _TemLogicEntity.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

        }

        #endregion

        #region 按条件模糊查询符合条件的任务模板

        /// <summary>
        /// 按条件模糊查询符合条件的任务模板
        /// </summary>
        /// <param name="num">任务模板编号</param>
        /// <param name="name">任务模板名称</param>
        /// <returns></returns>
        [HttpPost("/AGV/Task/GetTemLogicsContains")]
        public List<TemLogicEntity> GetTemLogicsContains(string? num, string? name)
        {

            return _TemLogicEntity.AsQueryable()
                            .Where(x => x.TemLogicNo.Contains(num) || x.TemLogicName.Contains(name))
                            .ToList();

        }

        #endregion

        #region 取消、继续、完成
        /// <summary>
        /// 取消任务
        /// </summary>
        /// <param name="cancelDTO"></param>
        /// <returns></returns>
        [HttpPost("/AGV/Task/CancelAsync")]
        public async Task CancelAsync(CancelDTO cancelDTO)
        {
            if (cancelDTO == null || string.IsNullOrEmpty(cancelDTO.TaskNo))
            {
                throw Oops.Oh($"取消任务时未传入对应的任务ID！");
            }

            //var para = JsonConvert.SerializeObject(cancelDTO);
            //_ = FileUtil.DebugTxt("取消日志记录", MessageTypeEnum.记录, para, "", "V2.0取消日志记录");
            try
            {
                #region 调用DH接口
                var dHMessage = await _DHRequester.CancelTaskAsync(cancelDTO);
                if (dHMessage.code == 500)
                {
                    throw Oops.Oh($"{dHMessage.desc}");
                }
                #endregion

                #region 写本地库
                var taskList = _TaskEntity.GetList(p => p.TaskNo == cancelDTO.TaskNo);
                // var taskEntity = await _TaskEntity.FirstOrDefaultAsync(p => p.TaskNo == cancelDTO.TaskNo);
                foreach (var item in taskList)
                {
                    // 1：未发送；2：正在取消；3：已取消；4：正在发送；5：发送失败；6：执行中；7：执行失败；8：任务完成；9：已下发;
                    item.TaskState = 3;
                    item.UpdateTime = DateTime.Now;
                }
                await _TaskEntity.UpdateRangeAsync(taskList);
                #endregion
            }
            catch (Exception ex)
            {
                //_ = FileUtil.DebugTxt(ex.Message, MessageTypeEnum.记录, para, ex.StackTrace, "V2.0取消日志Error");
                throw Oops.Oh($"{ex.Message}");
            }
        }

        /// <summary>
        /// 继续任务
        /// 如已是最后一个点位，该任务会自动完成
        /// </summary>
        /// <param name="goOnAsyncDTO"></param>
        /// <returns></returns>
        [HttpPost("/AGV/Task/GoOnAsync")]
        public async Task GoOnAsync(GoOnAsyncDTO goOnAsyncDTO)
        {
            //_ = FileUtil.DebugTxt("继续任务接口请求", MessageTypeEnum.记录, goOnAsyncDTO.taskNo, "", "继续任务记录");
            try
            {
                var dHMessage = await _DHRequester.GoOnTaskAsync(goOnAsyncDTO.taskNo);
                //_ = FileUtil.DebugTxt("DH返回信息", MessageTypeEnum.记录, goOnAsyncDTO.taskNo, JsonConvert.SerializeObject(dHMessage), "继续任务记录");
                if (dHMessage.code == 500)
                {
                    throw Oops.Oh(dHMessage.desc);
                }
                var taskList = _TaskEntity.GetList(p => p.TaskNo == goOnAsyncDTO.taskNo);
                foreach (var item in taskList)
                {
                    // 1：未发送；2：正在取消；3：已取消；4：正在发送；5：发送失败；6：执行中；7：执行失败；8：任务完成；9：已下发;
                    item.TaskState = 8;
                    item.UpdateTime = DateTime.Now;
                }
                await _TaskEntity.UpdateRangeAsync(taskList);
            }
            catch (Exception ex)
            {
                //_ = FileUtil.DebugTxt(ex.Message, MessageTypeEnum.错误, goOnAsyncDTO.taskNo, ex.StackTrace, "V1继续任务Error");
                throw Oops.Oh($"{ex.Message}");
            }
        }

        #endregion

        #region 接受RCS上报信息

        /// <summary>
        ///  接受RCS上报信息
        /// </summary>
        /// <param name="acceptDTO"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("/AGV/Task/AcceptAsync")]
        [AllowAnonymous]
        [UnifyProvider("easygreat")]
        public async Task<string> AcceptAsyncNew(AcceptDTO acceptDTO)
        {
            try
            {

                // 查找是否有相同任务
                var item = await _TaskEntity.GetFirstAsync(u => u.TaskNo == acceptDTO.orderId);

                if (item == null)
                {
                    throw Oops.Oh($"未找到有对应{acceptDTO.orderId}编号的AGV任务");
                }
                // 任务执行的状态
                int AgvStatus = 0;
                //if (!string.IsNullOrEmpty(acceptDTO.deviceNum))
                //{
                item.AGV = acceptDTO.deviceNum;

                // 已下发
                if (acceptDTO.status == 9)
                {
                    // 任务开始时间
                    item.STime = DateTime.Now;
                }
                // 任务结束时间
                // 3 已取消 5 发送失败 7 执行失败 8 已已完成
                if (acceptDTO.status == 3 || acceptDTO.status == 5 || acceptDTO.status == 7 || acceptDTO.status == 8)
                {
                    item.ETime = DateTime.Now;
                }

                // 任务失败的原因
                item.Message = acceptDTO.errorDesc;
                switch (acceptDTO.status)
                {
                    // 已取消
                    case 3:
                        AgvStatus = 3;
                        break;
                    // 发送失败
                    case 5:
                        AgvStatus = 5;
                        break;
                    // 运行中
                    case 6:
                        AgvStatus = 6;
                        break;
                    // 执行失败
                    case 7:
                        AgvStatus = 7;
                        break;
                    // 已完成
                    case 8:
                        AgvStatus = 8;
                        break;
                    // 已下发
                    case 9:
                        AgvStatus = 9;
                        break;
                    // 等待确认
                    case 10:
                        AgvStatus = 10;
                        break;
                }
                // 任务状态
                item.TaskState = AgvStatus;
                // 子任务序列
                item.SubTaskSeq = acceptDTO.subTaskSeq;
                // 子任务状态
                item.SubTaskStatus = acceptDTO.subTaskStatus;

                // 寻找到当前正在进行任务的任务编号
                var listTaskData = _TaskEntity.AsQueryable()
                             .Where(x => x.TaskNo == acceptDTO.orderId)
                             .Select(x => x)
                             .ToList();

                // 通过出入库编号将需要出入库的数据相关联（出入库详情表，得到出入库编号）
                var listInBoundData = _InAndOutBound.AsQueryable()
                                .Where(x => x.InAndOutBoundNum == listTaskData[0].InAndOutBoundNum)
                                .Select(x => x)
                                .ToList();

                #region 判断是不是取消任务或则任务失败
                if (AgvStatus == 3 || AgvStatus == 7)
                {
                    // 入库情况
                    if (listInBoundData[0].InAndOutBoundType == 0)
                    {
                        try
                        {
                            await _InAndOutBound.AsUpdateable()
                                   .AS("EG_WMS_InAndOutBound")
                                   .SetColumns(it => new Entity.EG_WMS_InAndOutBound
                                   {
                                       // 未入库
                                       InAndOutBoundStatus = 0,
                                       SuccessOrNot = 1,
                                       UpdateTime = DateTime.Now,
                                   })
                                   .Where(x => x.InAndOutBoundNum == listInBoundData[0].InAndOutBoundNum)
                                   .ExecuteCommandAsync();

                            // 取消入库占用

                            await _Storage.AsUpdateable()
                                     .AS("EG_WMS_Storage")
                                     .SetColumns(it => new Entity.EG_WMS_Storage
                                     {
                                         // 未占用
                                         StorageOccupy = 0,
                                         UpdateTime = DateTime.Now,
                                         TaskNo = null,
                                         StorageProductionDate = null,
                                     })
                                     .Where(x => x.TaskNo == acceptDTO.orderId)
                                     .ExecuteCommandAsync();

                            // TODO:这里可以判断需不需要把取消或者失败的任务里面的临时数据给删除，根据实现来讨论

                        }
                        catch (Exception ex)
                        {
                            throw Oops.Oh("错误：" + ex.Message);
                        }
                    }

                    // 出库情况
                    else if (listInBoundData[0].InAndOutBoundType == 1)
                    {
                        try
                        {
                            await _InAndOutBound.AsUpdateable()
                                                .AS("EG_WMS_InAndOutBound")
                                                .SetColumns(it => new Entity.EG_WMS_InAndOutBound
                                                {
                                                    // 未出库
                                                    InAndOutBoundStatus = 2,
                                                    UpdateTime = DateTime.Now,
                                                    SuccessOrNot = 1,
                                                })
                                                .Where(x => x.InAndOutBoundNum == listInBoundData[0].InAndOutBoundNum)
                                                .ExecuteCommandAsync();

                            // 取消出库预占用

                            await _Storage.AsUpdateable()
                                     .AS("EG_WMS_Storage")
                                     .SetColumns(it => new Entity.EG_WMS_Storage
                                     {
                                         // 占用
                                         StorageOccupy = 1,
                                         UpdateTime = DateTime.Now,
                                         TaskNo = null,
                                     })
                                     .Where(x => x.TaskNo == acceptDTO.orderId)
                                     .ExecuteCommandAsync();

                            // 将临时表里面的数据修改回去
                            await _TemInventory.AsUpdateable()
                                          .AS("EG_WMS_Tem_Inventory")
                                          .SetColumns(it => new EG_WMS_Tem_Inventory
                                          {
                                              OutboundStatus = 0,
                                              OutBoundNum = null,
                                              UpdateTime = DateTime.Now,
                                          })
                                          .Where(x => x.OutBoundNum == listInBoundData[0].InAndOutBoundNum)
                                          .ExecuteCommandAsync();
                        }
                        catch (Exception ex)
                        {
                            throw Oops.Oh("错误：" + ex.Message);
                        }
                    }
                }
                #endregion

                #region 判断为入库成功的场景

                // 判断为入库成功的场景
                if (acceptDTO.status == 8 && listInBoundData[0].InAndOutBoundType == 0)
                {
                    // 根据这个出入库编号去库存临时表中将符合条件的库存保存到正式的库存表中

                    try
                    {
                        // 查询得到临时库存主表里面所有的数据
                        var listTemInvData = _TemInventory.AsQueryable()
                                       .Where(x => x.InBoundNum == listInBoundData[0].InAndOutBoundNum)
                                       .Select(x => x)
                                       .ToList();

                        var listTemInvDetailData = _TemInventoryDetail.AsQueryable()
                                          .InnerJoin<EG_WMS_Tem_Inventory>((a, b) => a.InventoryNum == b.InventoryNum)
                                          .Where((a, b) => b.InBoundNum == listInBoundData[0].InAndOutBoundNum)
                                          .ToList();

                        List<Entity.EG_WMS_Inventory> invDataList = listTemInvData.Adapt<List<Entity.EG_WMS_Inventory>>();
                        List<EG_WMS_InventoryDetail> invdDataList = listTemInvDetailData.Adapt<List<EG_WMS_InventoryDetail>>();

                        _Inventory.InsertOrUpdate(invDataList);
                        _InventoryDetail.InsertOrUpdate(invdDataList);

                        // 修改库位表中的信息

                        await _Storage.AsUpdateable()
                                 .AS("EG_WMS_Storage")
                                 .SetColumns(it => new Entity.EG_WMS_Storage
                                 {
                                     StorageOccupy = 1,
                                     UpdateTime = DateTime.Now,
                                     TaskNo = null,
                                 })
                                 .Where(x => x.TaskNo == acceptDTO.orderId)
                                 .ExecuteCommandAsync();

                    }
                    catch (Exception ex)
                    {

                        throw Oops.Oh("错误：" + ex.Message);
                    }

                    // 修改入库状态
                    await _InAndOutBound.AsUpdateable()
                                  .AS("EG_WMS_InAndOutBound")
                                  .SetColumns(it => new Entity.EG_WMS_InAndOutBound
                                  {
                                      InAndOutBoundStatus = 1,
                                      SuccessOrNot = 0,
                                  })
                                  .Where(x => x.InAndOutBoundNum == listInBoundData[0].InAndOutBoundNum)
                                  .ExecuteCommandAsync();

                }
                #endregion

                #region 判断为出库成功的场景

                else if (acceptDTO.status == 8 && listInBoundData[0].InAndOutBoundType == 1)
                {
                    try
                    {

                        // 查询得到临时库存主表中修改的数据

                        List<EG_WMS_Tem_Inventory> dataTemInvtory = new List<EG_WMS_Tem_Inventory>();

                        dataTemInvtory.AddRange(_TemInventory.AsQueryable()
                                             .Where(x => x.OutBoundNum == listInBoundData[0].InAndOutBoundNum)
                                             .Select(x => x)
                                             .ToList());

                        List<Entity.EG_WMS_Inventory> invData = dataTemInvtory.Adapt<List<Entity.EG_WMS_Inventory>>();

                        _Inventory.InsertOrUpdate(invData);

                        // 修改库位表中的信息

                        await _Storage.AsUpdateable()
                                 .AS("EG_WMS_Storage")
                                 .SetColumns(it => new Entity.EG_WMS_Storage
                                 {
                                     // 未占用
                                     StorageOccupy = 0,
                                     TaskNo = null,
                                     // 料箱生产时间
                                     StorageProductionDate = null,
                                     UpdateTime = DateTime.Now,
                                 })
                                 .Where(x => x.TaskNo == acceptDTO.orderId)
                                 .ExecuteCommandAsync();

                        // 修改料箱表里面的出入库编号
                        await _WorkBin.AsUpdateable()
                                 .AS("EG_WMS_WorkBin")
                                 .SetColumns(it => new Entity.EG_WMS_WorkBin
                                 {
                                     InAndOutBoundNum = null,
                                 })
                                 .Where(x => x.InAndOutBoundNum == item.InAndOutBoundNum)
                                 .ExecuteCommandAsync();

                    }
                    catch (Exception ex)
                    {
                        throw Oops.Oh("错误：" + ex.Message);
                    }

                    // 修改出库状态
                    await _InAndOutBound.AsUpdateable()
                                  .AS("EG_WMS_InAndOutBound")
                                  .SetColumns(it => new Entity.EG_WMS_InAndOutBound
                                  {
                                      InAndOutBoundStatus = 3,
                                      SuccessOrNot = 0,
                                  })
                                  .Where(x => x.InAndOutBoundNum == listInBoundData[0].InAndOutBoundNum)
                                  .ExecuteCommandAsync();

                }
                #endregion

                // 将rcs得到的数据保存

                await _TaskEntity.InsertOrUpdateAsync(item);
                //}

                return "上报成功";
            }
            catch (Exception ex)
            {
                // 错误信息
                throw Oops.Oh("错误：" + ex.Message);
            }
        }

        #endregion

        #region 获取AGV状态

        /// <summary>
        /// 获取AGV状态
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/AGV/Task/ObtainAGVStatus")]
        [AllowAnonymous]
        [UnifyProvider("easygreat")]
        public async Task ObtainAGVStatus(List<ObtainAgvStatusModel> input)
        {
            #region 参数
            //{
            //    "payLoad":"0.0", 设备负载状态
            //    "devicePosition":"57540086", 设备当前位置
            //    "devicePostionRec":[  设备所在二维码的 x,y 坐标，前边的值是x，后边的是y
            //        4983,
            //        -6093
            //    ],
            //    "state":"InCharging",  设备状态
            //    "deviceCode":"CE35592BAK00001",  设备序列号
            //    "battery":"79",  电池电量
            //    "deviceName":"M001"  设备名称
            //}
            #endregion
            try
            {
                string devicepostionrec = "";
                for (int i = 0; i < input.Count; i++)
                {
                    if (input[i].devicePostionRec == null)
                    {
                        devicepostionrec = 0.ToString();
                    }
                    else
                    {
                        // 将数组转换成string
                        devicepostionrec = string.Join(",", input[i].devicePostionRec);
                    }
                    InfoAgvEntity info = input[i].Adapt<InfoAgvEntity>();
                    info.devicePostionRec = devicepostionrec;

                    // 查询是否已经有相同的数据
                    var agvinfo = await _InfoAgvEntity.GetFirstAsync(x => x.deviceCode == info.deviceCode);

                    if (agvinfo == null)
                    {
                        await _InfoAgvEntity.InsertAsync(info);
                    }
                    else
                    {
                        await _InfoAgvEntity.AsUpdateable()
                                            .SetColumns(it => new InfoAgvEntity
                                            {
                                                orderId = info.orderId,
                                                shelfNumber = info.shelfNumber,
                                                deviceStatus = info.deviceStatus,
                                                oritation = info.oritation,
                                                speed = info.speed,
                                                payLoad = info.payLoad,
                                                devicePosition = info.devicePosition,
                                                devicePostionRec = devicepostionrec,
                                                state = info.state,
                                                battery = info.battery,
                                                deviceName = info.deviceName,
                                                UpdateTime = DateTime.Now,
                                            })
                                            .Where(x => x.deviceCode == info.deviceCode)
                                            .ExecuteCommandAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw Oops.Oh(ex);
            }
        }


        #endregion

        #region 报警消息上报

        /// <summary>
        /// 报警消息上报
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/AGV/Task/AlarmInformationReporting")]
        [AllowAnonymous]
        [UnifyProvider("easygreat")]
        public async Task AlarmInformationReporting(AlarmInformationModel input)
        {
            try
            {
                 AlarmEntity data = new AlarmEntity()
                {
                    deviceNum = input.deviceNum,
                    deviceName = input.deviceName,
                    alarmDesc = input.alarmDesc,
                    alarmType = input.alarmType,
                    areaId = input.areaId,
                    alarmReadFlag = input.alarmReadFlag,
                    channelDeviceId = input.channelDeviceId,
                    alarmSource = input.alarmSource,
                    alarmGrade = input.alarmGrade,

                };
                long datetime = input.alarmDate.ToLong();
                // 将时间戳转化成日期格式
                DateTime times = DateTimeUtil.ToLocalTimeDateBySeconds(datetime);
                data.alarmDate = times;
                await _AlarmEntity.InsertAsync(data);

            }
            catch (Exception ex)
            {

                throw Oops.Oh(ex);
            }
        }


        #endregion

    }


    //-------------------------------------//-------------------------------------//


    #region 接受RCS的上报信息
    ///// <summary>
    ///// 接受RCS的上报信息
    ///// </summary>
    ///// <param name="acceptDto"></param>
    ///// <returns></returns>
    //[HttpPost("/AGV/Task/AcceptAsync")]
    //[AllowAnonymous]
    //public string AcceptAsync(AcceptDTO acceptDto)
    //{
    //    //var para = JsonConvert.SerializeObject(acceptDto);
    //    //_ = FileUtil.DebugTxt("任务上报参数记录", MessageTypeEnum.记录, para, "", "DH主动上报记录");
    //    if (acceptDto.status == null)
    //    {
    //        throw Oops.Oh($"上报的状态不能为空值！");
    //        //throw new Exception("DH上报的状态为空值！");
    //    }
    //    try
    //    {
    //        if (string.IsNullOrEmpty(acceptDto.orderId))
    //        {
    //            //throw new Exception("任务ID不可为空！");
    //            throw Oops.Oh($"任务ID不可为空！");
    //        }
    //        var item = _TaskEntity.GetFirst(p => p.TaskNo == acceptDto.orderId.Trim());
    //        // （3 已取消 5 发送失败 6 运行中 7 执行失败 8 已完成 
    //        // 9 已下发 10 等待确认 20 取货中 21 取货完成 22 放货中 23 放货完成）
    //        if (item.TaskState == 8 || item.TaskState == 3)
    //        {
    //            //_ = FileUtil.DebugTxt(item.TaskState, MessageTypeEnum.记录, para, "", "DH状态上报顺序Error");
    //            return "";
    //        }
    //        #region 数据入库
    //        try
    //        {
    //            if (!string.IsNullOrEmpty(acceptDto.deviceNum))
    //            {
    //                item.AGV = acceptDto.deviceNum;
    //                //9-已下发
    //                if (acceptDto.status == 9)
    //                {
    //                    item.STime = DateTime.Now;
    //                }
    //                //3-已取消，5-发送失败，7-执行失败，8-已完成
    //                if (acceptDto.status == 3 || acceptDto.status == 5 || acceptDto.status == 7 ||
    //                    acceptDto.status == 8)
    //                {
    //                    item.ETime = DateTime.Now;
    //                }
    //            }
    //            item.Message = acceptDto.errorDesc; //失败原因
    //            item.TaskState = acceptDto.status;
    //            item.SubTaskSeq = acceptDto.subTaskSeq;
    //            item.SubTaskStatus = acceptDto.subTaskStatus;
    //            _TaskEntity.InsertOrUpdate(item);
    //        }
    //        catch (Exception ex)
    //        {
    //            //_ = FileUtil.DebugTxt($"{ex.Message}", MessageTypeEnum.错误, para, ex.StackTrace, "任务状态数据入库Error");
    //        }
    //        #endregion

    //        #region 触发主动上报第三方

    //        #endregion
    //    }
    //    catch (Exception ex)
    //    {
    //        //_ = FileUtil.DebugTxt($"{ex.Message}", MessageTypeEnum.错误, para, ex.StackTrace, "接受RCS的上报信息Error");
    //        throw Oops.Oh($"{ex.Message}");
    //        //throw new Exception(ex.Message);
    //    }
    //    return "上报成功";
    //}
    #endregion


}

public class class2
{
    public string StorageGroup { get; set; }

    public int SumCount { get; set; }

}
