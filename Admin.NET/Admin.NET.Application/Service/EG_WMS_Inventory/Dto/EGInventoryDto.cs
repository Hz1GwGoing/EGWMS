namespace Admin.NET.Application;

    /// <summary>
    /// EGInventory输出参数
    /// </summary>
    public class EGInventoryDto
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }
        
        /// <summary>
        /// 库存编号
        /// </summary>
        public string? InventoryNum { get; set; }
        
        /// <summary>
        /// 库存总数
        /// </summary>
        public int? ICountAll { get; set; }
        
        /// <summary>
        /// 可用库存
        /// </summary>
        public int? IUsable { get; set; }
        
        /// <summary>
        /// 冻结数量
        /// </summary>
        public int? IFrostCount { get; set; }
        
        /// <summary>
        /// 待检数量
        /// </summary>
        public int? IWaitingCount { get; set; }
        
    }
