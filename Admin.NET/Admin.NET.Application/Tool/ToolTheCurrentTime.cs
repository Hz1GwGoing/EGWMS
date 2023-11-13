namespace Admin.NET.Application.Tool;

/// <summary>
/// 时间有关函数
/// </summary>
internal class ToolTheCurrentTime
{
    /// <summary>
    /// 获得当前特定状态时间戳
    /// </summary>
    /// <param name="name">首编号</param>
    /// <returns></returns>
    public string GetTheCurrentTimeTimeStamp(string name)
    {
        string timesstamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
        return name + timesstamp;
    }

    /// <summary>
    /// 获得当前时间戳
    /// </summary>
    /// <returns></returns>
    public string GetTheCurrentTimeTimeStamp()
    {
        return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
    }

}
