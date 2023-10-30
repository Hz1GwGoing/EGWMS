namespace Admin.NET.Application.Service.EG_WMS_BaseServer.Dto;

public class GetMaterielNumDataList
{
    // 主表
    public string InventoryNum { get; set; }

    public int ICountAll { get; set; }

    //public int IUsable { get; set; }

    //public int IFrostCount { get; set; }

    //public int IWaitingCount { get; set; }

    public string InAndOutBoundNum { get; set; }

    public string MaterielNum { get; set; }

    public string InventoryRemake { get; set; }

    public int OutboundStatus { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime UpdateTime { get; set; }

    public int IsDelete { get; set; }

    // 详细表

    public string ProductionLot { get; set; }

    public string StorageNum { get; set; }

    public string WHNum { get; set; }

    public string RegionNum { get; set; }

    public string WorkBinNum { get; set; }

    public string InventoryDetailRemake { get; set; }

    // 物料表

    public string MaterielName { get; set; }

    public string MaterielType { get; set; }

    public string MaterielSpecs { get; set; }

    public string MaterielMainUnit { get; set; }

    public string MaterielAssistUnit { get; set; }

    // 区域表
    public string RegionName { get; set; }

    // 仓库表

    public string WHName { get; set; }

}