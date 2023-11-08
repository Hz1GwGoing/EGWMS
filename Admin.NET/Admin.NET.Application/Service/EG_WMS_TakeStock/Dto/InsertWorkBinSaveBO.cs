namespace Admin.NET.Application.Service.EG_WMS_TakeStock.Dto;

/// <summary>
/// 得到扫描料箱数据BO
/// </summary>
public class InsertWorkBinSaveBO
{
    /// <summary>
    /// 盘点编号
    /// </summary>
    public string takestocknum { get; set; }

    /// <summary>
    /// 料箱数据
    /// </summary>
    public List<MaterielWorkBin> MaterielWorkBins { get; set; }
}