using Admin.NET.Application.Service.EG_WMS_MaterielLevel.Dto;

namespace Admin.NET.Application.Service.EG_WMS_MaterielLevel;

/// <summary>
/// 物料级别管理接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class MaterielLevelService : IDynamicApiController, ITransient
{
    #region Tool

    ToolTheCurrentTime currentTime = new ToolTheCurrentTime();

    #endregion

    #region 关系注入
    private readonly SqlSugarRepository<Entity.EG_WMS_MaterielLevel> _rep = App.GetService<SqlSugarRepository<Entity.EG_WMS_MaterielLevel>>();
    private readonly SqlSugarRepository<Entity.EG_WMS_Inventory> _Inventory = App.GetService<SqlSugarRepository<Entity.EG_WMS_Inventory>>();
    private readonly SqlSugarRepository<Entity.EG_WMS_WorkBin> _WorkBin = App.GetService<SqlSugarRepository<Entity.EG_WMS_WorkBin>>();
    #endregion

    #region 构造函数
    public MaterielLevelService()
    {

    }
    #endregion

    #region 新增物料级别

    /// <summary>
    /// 新增物料级别
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task AddMaterielLevel(MaterielLevelInput input)
    {
        if (!string.IsNullOrWhiteSpace(input.MaterielLevelNum))
        {
            input.MaterielLevelNum = currentTime.GetTheCurrentTimeTimeStamp("Level");
        }

        var data = input.Adapt<Entity.EG_WMS_MaterielLevel>();

        await _rep.InsertAsync(data);

    }


    #endregion

    #region 分页获取物料级别

    /// <summary>
    /// 分页获取物料级别
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<MaterielLevelDto>> Page(MaterielLevelInputPage input)
    {
        var data = _rep.AsQueryable()
             .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielLevelNum), x => x.MaterielLevelNum.Contains(input.MaterielLevelNum))
             .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielLevelName), x => x.MaterielLevelName.Contains(input.MaterielLevelName))
             .Select(x => new MaterielLevelDto
             {
                 MaterielLevelName = x.MaterielLevelName,
                 MaterielLevelNum = x.MaterielLevelNum,
                 MatetielLevelDescription = x.MaterielLevelDescription,
             }, true);

        return await data.ToPagedListAsync(input.page, input.pagesize);

    }


    #endregion
}
