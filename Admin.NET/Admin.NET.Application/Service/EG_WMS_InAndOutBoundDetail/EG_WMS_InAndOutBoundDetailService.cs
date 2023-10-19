//namespace Admin.NET.Application;
///// <summary>
///// 出入库详情服务
///// </summary>
//[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
//public class EG_WMS_InAndOutBoundDetailService : IDynamicApiController, ITransient
//{
//    private readonly SqlSugarRepository<EG_WMS_InAndOutBoundDetail> _rep;
//    public EG_WMS_InAndOutBoundDetailService(SqlSugarRepository<EG_WMS_InAndOutBoundDetail> rep)
//    {
//        _rep = rep;
//    }

//    /// <summary>
//    /// 分页查询出入库详情
//    /// </summary>
//    /// <param name="input"></param>
//    /// <returns></returns>
//    [HttpPost]
//    [ApiDescriptionSettings(Name = "Page")]
//    public async Task<SqlSugarPagedList<EG_WMS_InAndOutBoundDetailOutput>> Page(EG_WMS_InAndOutBoundDetailInput input)
//    {
//        var query = _rep.AsQueryable()
//                    .WhereIF(!string.IsNullOrWhiteSpace(input.InAndOutBoundNum), u => u.InAndOutBoundNum.Contains(input.InAndOutBoundNum.Trim()))
//                    .WhereIF(!string.IsNullOrWhiteSpace(input.WHNum), u => u.WHNum.Contains(input.WHNum.Trim()))
//                    .WhereIF(!string.IsNullOrWhiteSpace(input.PalletNum), u => u.PalletNum.Contains(input.PalletNum.Trim()))
//                    .WhereIF(!string.IsNullOrWhiteSpace(input.WorkBinNum), u => u.WorkBinNum.Contains(input.WorkBinNum.Trim()))
//                    .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielNum), u => u.MaterielNum.Contains(input.MaterielNum.Trim()))
//                    //处理外键和TreeSelector相关字段的连接
//                    .LeftJoin<EG_WMS_InAndOutBound>((u, inandoutboundnum) => u.InAndOutBoundNum == inandoutboundnum.InAndOutBoundNum)
//                    .LeftJoin<EG_WMS_WareHouse>((u, inandoutboundnum, whnum) => u.WHNum == whnum.WHNum)
//                    .LeftJoin<EG_WMS_Pallet>((u, inandoutboundnum, whnum, palletnum) => u.PalletNum == palletnum.PalletNum)
//                    .LeftJoin<EG_WMS_WorkBin>((u, inandoutboundnum, whnum, palletnum, workbinnum) => u.WorkBinNum == workbinnum.WorkBinNum)
//                    .LeftJoin<EG_WMS_Materiel>((u, inandoutboundnum, whnum, palletnum, workbinnum, materielnum) => u.MaterielNum == materielnum.MaterielNum)
//                    .Select((u, inandoutboundnum, whnum, palletnum, workbinnum, materielnum) => new EG_WMS_InAndOutBoundDetailOutput
//                    {
//                        Id = u.Id,
//                        InAndOutBoundNum = u.InAndOutBoundNum,
//                        EG_WMS_InAndOutBoundInAndOutBoundNum = inandoutboundnum.InAndOutBoundNum,
//                        WHNum = u.WHNum,
//                        EG_WMS_WareHouseWHNum = whnum.WHNum,
//                        PalletNum = u.PalletNum,
//                        EG_WMS_PalletPalletNum = palletnum.PalletNum,
//                        WorkBinNum = u.WorkBinNum,
//                        EG_WMS_WorkBinWorkBinNum = workbinnum.WorkBinNum,
//                        MaterielNum = u.MaterielNum,
//                        EG_WMS_MaterielMaterielNum = materielnum.MaterielNum,
//                    })
//;
//        query = query.OrderBuilder(input);
//        return await query.ToPagedListAsync(input.Page, input.PageSize);
//    }

//    /// <summary>
//    /// 增加出入库详情
//    /// </summary>
//    /// <param name="input"></param>
//    /// <returns></returns>
//    [HttpPost]
//    [ApiDescriptionSettings(Name = "Add")]
//    public async Task Add(AddEG_WMS_InAndOutBoundDetailInput input)
//    {
//        var entity = input.Adapt<EG_WMS_InAndOutBoundDetail>();
//        await _rep.InsertAsync(entity);
//    }

//    /// <summary>
//    /// 删除出入库详情
//    /// </summary>
//    /// <param name="input"></param>
//    /// <returns></returns>
//    [HttpPost]
//    [ApiDescriptionSettings(Name = "Delete")]
//    public async Task Delete(DeleteEG_WMS_InAndOutBoundDetailInput input)
//    {
//        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
//        await _rep.FakeDeleteAsync(entity);   //假删除
//    }

//    /// <summary>
//    /// 更新出入库详情
//    /// </summary>
//    /// <param name="input"></param>
//    /// <returns></returns>
//    [HttpPost]
//    [ApiDescriptionSettings(Name = "Update")]
//    public async Task Update(UpdateEG_WMS_InAndOutBoundDetailInput input)
//    {
//        var entity = input.Adapt<EG_WMS_InAndOutBoundDetail>();
//        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
//    }

//    /// <summary>
//    /// 获取出入库详情
//    /// </summary>
//    /// <param name="input"></param>
//    /// <returns></returns>
//    [HttpGet]
//    [ApiDescriptionSettings(Name = "Detail")]
//    public async Task<EG_WMS_InAndOutBoundDetail> Get([FromQuery] QueryByIdEG_WMS_InAndOutBoundDetailInput input)
//    {
//        return await _rep.GetFirstAsync(u => u.Id == input.Id);
//    }

//    /// <summary>
//    /// 获取出入库详情
//    /// </summary>
//    /// <param name="input"></param>
//    /// <returns></returns>
//    [HttpGet]
//    [ApiDescriptionSettings(Name = "List")]
//    public async Task<List<EG_WMS_InAndOutBoundDetailOutput>> List([FromQuery] EG_WMS_InAndOutBoundDetailInput input)
//    {
//        return await _rep.AsQueryable().Select<EG_WMS_InAndOutBoundDetailOutput>().ToListAsync();
//    }

//    /// <summary>
//    /// 获取出入库编号列表
//    /// </summary>
//    /// <param name="input"></param>
//    /// <returns></returns>
//    [ApiDescriptionSettings(Name = "EG_WMS_InAndOutBoundInAndOutBoundNumDropdown"), HttpGet]
//    public async Task<dynamic> EG_WMS_InAndOutBoundInAndOutBoundNumDropdown()
//    {
//        return await _rep.Context.Queryable<EG_WMS_InAndOutBound>()
//                .Select(u => new
//                {
//                    Label = u.InAndOutBoundNum,
//                    Value = u.Id
//                }
//                ).ToListAsync();
//    }
//    /// <summary>
//    /// 获取仓库编号列表
//    /// </summary>
//    /// <param name="input"></param>
//    /// <returns></returns>
//    [ApiDescriptionSettings(Name = "EG_WMS_WareHouseWHNumDropdown"), HttpGet]
//    public async Task<dynamic> EG_WMS_WareHouseWHNumDropdown()
//    {
//        return await _rep.Context.Queryable<EG_WMS_WareHouse>()
//                .Select(u => new
//                {
//                    Label = u.WHNum,
//                    Value = u.Id
//                }
//                ).ToListAsync();
//    }
//    /// <summary>
//    /// 获取栈板编号列表
//    /// </summary>
//    /// <param name="input"></param>
//    /// <returns></returns>
//    [ApiDescriptionSettings(Name = "EG_WMS_PalletPalletNumDropdown"), HttpGet]
//    public async Task<dynamic> EG_WMS_PalletPalletNumDropdown()
//    {
//        return await _rep.Context.Queryable<EG_WMS_Pallet>()
//                .Select(u => new
//                {
//                    Label = u.PalletNum,
//                    Value = u.Id
//                }
//                ).ToListAsync();
//    }
//    /// <summary>
//    /// 获取料箱编号列表
//    /// </summary>
//    /// <param name="input"></param>
//    /// <returns></returns>
//    [ApiDescriptionSettings(Name = "EG_WMS_WorkBinWorkBinNumDropdown"), HttpGet]
//    public async Task<dynamic> EG_WMS_WorkBinWorkBinNumDropdown()
//    {
//        return await _rep.Context.Queryable<EG_WMS_WorkBin>()
//                .Select(u => new
//                {
//                    Label = u.WorkBinNum,
//                    Value = u.Id
//                }
//                ).ToListAsync();
//    }
//    /// <summary>
//    /// 获取物料编号列表
//    /// </summary>
//    /// <param name="input"></param>
//    /// <returns></returns>
//    [ApiDescriptionSettings(Name = "EG_WMS_MaterielMaterielNumDropdown"), HttpGet]
//    public async Task<dynamic> EG_WMS_MaterielMaterielNumDropdown()
//    {
//        return await _rep.Context.Queryable<EG_WMS_Materiel>()
//                .Select(u => new
//                {
//                    Label = u.MaterielNum,
//                    Value = u.Id
//                }
//                ).ToListAsync();
//    }




//}

