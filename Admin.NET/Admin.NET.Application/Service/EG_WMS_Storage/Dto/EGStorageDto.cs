namespace Admin.NET.Application;

    /// <summary>
    /// EGStorage输出参数
    /// </summary>
    public class EGStorageDto
    {
        /// <summary>
        /// 仓库编号
        /// </summary>
        public string EGWareHouseWHNum { get; set; }
        
        /// <summary>
        /// 区域编号
        /// </summary>
        public string? EGRegionRegionNum { get; set; }
        
        /// <summary>
        /// 库位编号
        /// </summary>
        public string? StorageNum { get; set; }
        
        /// <summary>
        /// 库位名称
        /// </summary>
        public string? StorageName { get; set; }
        
        /// <summary>
        /// 库位地址
        /// </summary>
        public string? StorageAddress { get; set; }
        
        /// <summary>
        /// 库位类别
        /// </summary>
        public string? StorageType { get; set; }
        
        /// <summary>
        /// 仓库名称
        /// </summary>
        public string? WareHouseName { get; set; }
        
        /// <summary>
        /// 库位长
        /// </summary>
        public decimal? StorageLong { get; set; }
        
        /// <summary>
        /// 库位宽
        /// </summary>
        public decimal? StorageWidth { get; set; }
        
        /// <summary>
        /// 库位高
        /// </summary>
        public decimal? StorageHigh { get; set; }
        
        /// <summary>
        /// 是否占用
        /// </summary>
        public string? StorageOccupy { get; set; }
        
        /// <summary>
        /// 备注
        /// </summary>
        public string? StorageRemake { get; set; }
        
        /// <summary>
        /// 创建者姓名
        /// </summary>
        public string? CreateUserName { get; set; }
        
        /// <summary>
        /// 修改者姓名
        /// </summary>
        public string? UpdateUserName { get; set; }
        
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }
        
        /// <summary>
        /// 巷道号
        /// </summary>
        public int? RoadwayNum { get; set; }
        
        /// <summary>
        /// 货架号
        /// </summary>
        public int? ShelfNum { get; set; }
        
        /// <summary>
        /// 层号
        /// </summary>
        public int? FloorNumber { get; set; }
        
        /// <summary>
        /// 仓库编号
        /// </summary>
        public string? WHNum { get; set; }
        
        /// <summary>
        /// 区域编号
        /// </summary>
        public string? RegionNum { get; set; }
        
    }
