namespace Admin.NET.Application.Service.EG_WMS_VisualDataDisplay;


/// <summary>
/// 可视化数据
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupDisplay, Order = 100)]
internal class DataDisplayService : IDynamicApiController, ITransient
{
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

    #endregion



}
