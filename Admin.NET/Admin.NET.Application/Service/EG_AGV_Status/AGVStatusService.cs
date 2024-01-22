using Admin.NET.Application.Service.EG_AGV_Status.Dto;

namespace Admin.NET.Application.Service.EG_AGV_Status;

/// <summary>
/// agv信息接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]

public class AGVStatusService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<InfoAgvEntity> _InfoAgv = App.GetService<SqlSugarRepository<InfoAgvEntity>>();


    /// <summary>
    /// 分页按条件查询agv状态信息
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "GetAGVInformationData")]
    public async Task<SqlSugarPagedList<AGVDataInfoAgvDto>> GetAGVInformationData(AgvInfoPageBO input)
    {
        var data = _InfoAgv.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.deviceCode), x => x.deviceCode.Contains(input.deviceCode))
                         .WhereIF(!string.IsNullOrWhiteSpace(input.deviceName), x => x.deviceName.Contains(input.deviceName))
                         .WhereIF(!string.IsNullOrWhiteSpace(input.orderId), x => x.orderId.Contains(input.orderId))
                         .WhereIF(!string.IsNullOrWhiteSpace(input.state), x => x.state.Contains(input.state))
                         .OrderBy(x => x.deviceCode, OrderByType.Asc)
                         .Select<AGVDataInfoAgvDto>();

        return await data.ToPagedListAsync(input.page, input.pagesize);
    }


    /// <summary>
    /// 当前在线车辆平均电量百分比
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "GetAGVBatteryPercentage")]
    public async Task<string> GetAGVBatteryPercentage()
    {

        var infoagemessage = await _InfoAgv.AsQueryable()
                 .Where(x => x.state != "Offline")
                 .Select(x => x)
                 .ToListAsync();

        int batterySum = 0;
        for (int i = 0; i < infoagemessage.Count; i++)
        {
            // 所有车辆的总电量
            batterySum += infoagemessage[i].battery.ToInt();
        }

        // 将电量除以现在有多少车辆在线

        double avgbattery = batterySum / infoagemessage.Count;

        return avgbattery + "%";
    }



}
