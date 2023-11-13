namespace Admin.NET.Application.Service.EG_WMS_InAndOutBound;

/// <summary>
/// AGV出入库接口
/// </summary>

[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EG_WMS_InAndOutBoundMessage : IDynamicApiController, ITransient
{
    private static readonly EG_WMS_InAndOutBoundService _inandoutboundservics = new EG_WMS_InAndOutBoundService();

    #region 关系注入
    private readonly SqlSugarRepository<TemLogicEntity> _TemLogicEntity = App.GetService<SqlSugarRepository<TemLogicEntity>>();
    #endregion

    #region AGV入库接口（包括：是否为前往等待点、自动推荐库位）

    /// <summary>
    /// AGV入库接口（包括：是否为前往等待点、自动推荐库位）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "AgvJoinStorageTask")]
    public async Task AgvJoinStorageTask(AgvJoinDto input)
    {
        try
        {
            // 根据模板编号去查询是否需要前往等待点
            var temLogicModel = await _TemLogicEntity.GetFirstAsync(x => x.TemLogicNo == input.ModelNo);

            if (temLogicModel != null && temLogicModel.HoldingPoint == "1")
            {

                await _inandoutboundservics.AgvJoinBoundTaskGOPoint(input);

            }
            else
            {
                await _inandoutboundservics.AgvJoinBoundTask(input);
            }
        }
        catch (Exception ex)
        {

            throw Oops.Oh("错误：" + ex);
        }

    }

    #endregion


}
