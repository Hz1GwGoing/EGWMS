using Admin.NET.Application.Service.EG_WMS_Pallet.Dto;

namespace Admin.NET.Application.IService.EG_WMS_Pallet;
internal interface IEGPalletServics
{
    /// <summary>
    /// 分页查询栈板
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<SqlSugarPagedList<EGPalletOutput>> Page(EGPalletInput input);

    /// <summary>
    /// 增加栈板
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task Add(AddEGPalletInput input);

    /// <summary>
    /// 删除栈板
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task Delete(DeleteEGPalletInput input);

    /// <summary>
    /// 更新栈板
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task Update(UpdateEGPalletInput input);

    /// <summary>
    /// 获取栈板
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<Entity.EG_WMS_Pallet> Get(QueryByIdEGPalletInput input);

    /// <summary>
    /// 获取栈板列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<List<EGPalletOutput>> List(EGPalletInput input);
}
