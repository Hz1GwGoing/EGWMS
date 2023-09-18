namespace Admin.NET.Application;

    /// <summary>
    /// EGInventorydetail输出参数
    /// </summary>
    public class EGInventoryDetailDto
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }
        
        /// <summary>
        /// 物料编号
        /// </summary>
        public string? MaterielNum { get; set; }
        
        /// <summary>
        /// 库存编号
        /// </summary>
        public string? InventoryNum { get; set; }
        
        /// <summary>
        /// 生产批次
        /// </summary>
        public string? ProductionLot { get; set; }
        
        /// <summary>
        /// 当前数量
        /// </summary>
        public int? CurrentCount { get; set; }
        
        /// <summary>
        /// 库位编号
        /// </summary>
        public string? StorageNum { get; set; }
        
        /// <summary>
        /// 仓库编号
        /// </summary>
        public string? WHNum { get; set; }
        
        /// <summary>
        /// 区域编号
        /// </summary>
        public string? RegionNum { get; set; }
        
        /// <summary>
        /// 货架编号
        /// </summary>
        public string? ShelfNum { get; set; }
        
        /// <summary>
        /// 冻结状态
        /// </summary>
        public int? FrozenState { get; set; }
        
        /// <summary>
        /// 栈板编号
        /// </summary>
        public string? PalletNum { get; set; }
        
        /// <summary>
        /// 料箱编号
        /// </summary>
        public string? WorkBinNum { get; set; }
        
    }
