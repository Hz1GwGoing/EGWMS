namespace Admin.NET.Application.Service.EG_WMS_TakeStock.Dto;

public class GetMaterielWorkBinData
{
    /// <summary>
    /// 库位编号
    /// </summary>
    public string StorageNum { get; set; }

    /// <summary>
    /// 盘点备注
    /// </summary>
    public string? TakeStockRemake { get; set; }

    /// <summary>
    /// 料箱产品
    /// </summary>

    public List<MaterielWorkBin> materielWorkBins { get; set; }

}