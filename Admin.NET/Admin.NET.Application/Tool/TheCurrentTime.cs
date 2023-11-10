namespace Admin.NET.Application.Tool;

/// <summary>
/// 时间有关函数
/// </summary>
internal class TheCurrentTime
{
    /// <summary>
    /// 获得当前时间时间戳
    /// </summary>
    /// <param name="name">首编号</param>
    /// <returns></returns>
    public string GetTheCurrentTimeTimeStamp(string name)
    {
        string timesstamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
        return name + timesstamp;
    }

}
