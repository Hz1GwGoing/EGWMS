namespace Admin.NET.Application;

    /// <summary>
    /// EGTakeStock输出参数
    /// </summary>
    public class EGTakeStockDto
    {
        /// <summary>
        /// 盘点编号
        /// </summary>
        public string? TakeStockNum { get; set; }
        
        /// <summary>
        /// 盘点状态
        /// </summary>
        public int? TakeStockStatus { get; set; }
        
        /// <summary>
        /// 盘点时间
        /// </summary>
        public DateTime? TakeStockTime { get; set; }
        
        /// <summary>
        /// 盘点人员
        /// </summary>
        public string? TakeStockUser { get; set; }
        
        /// <summary>
        /// 盘点备注
        /// </summary>
        public string? TakeStockRemake { get; set; }
        
        /// <summary>
        /// 物料编号
        /// </summary>
        public string? MaterielNum { get; set; }
        
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }
        
    }
