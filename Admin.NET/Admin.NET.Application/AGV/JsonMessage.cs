namespace Admin.NET.Application
{
    #region 壹格参数
    public class JsonMessage
    {
        public bool Success { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
        public Object Data { get; set; }

        public void Error(string message = "操作失败", int code = 0)
        {
            Success = false;
            Code = code;
            Message = message;
        }
    }
    #endregion

    #region DH参数
    public class DHMessage
    {
        public int code { get; set; }
        public object data { get; set; }
        public string desc { get; set; }

    }

    public class DHMessage<T>
    {
        public int code { get; set; }
        public T data { get; set; }
        public string desc { get; set; }

    }

    /// <summary>
    /// 获取任务点返回
    /// </summary>
    //public class ThirdPartyMessage : DHMessage
    //{
    //    public string pointName { get; set; }

    //}

    public class taskV2
    {
        public bool? fromCache { get; set; }
        public object statusObj { get; set; }
    }

    public class data
    {
        public int areaId { get; set; }
        public long createTime { get; set; }
        public string fromSystem { get; set; }
        public int status { get; set; }
        public List<taskOrderDetail> taskOrderDetail { get; set; }
    }

    public class taskOrderDetail
    {
        public string deviceNum { get; set; }
        public string qrContent { get; set; }
        public string? devicecode { get; set; }
        public int status { get; set; }
        public string? subTaskStatus { get; set; }
        public string? subTaskSeq { get; set; }
        public string? subTaskTypeId { get; set; }
        public string? subTaskId { get; set; }
        public long time { get; set; }
    }
    #endregion

}
