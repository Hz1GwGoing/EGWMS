namespace Admin.NET.Application.Service.EG_WMS_TakeStock.Dto;

public class MaterielWorkBinData
{
    /// <summary>
    /// 盘点编号
    /// </summary>
    public string TakeStockNum { get; set; }

    /// <summary>
    /// 料箱产品
    /// </summary>
    public List<MaterielWorkBin> materielWorkBins { get; set; }


}