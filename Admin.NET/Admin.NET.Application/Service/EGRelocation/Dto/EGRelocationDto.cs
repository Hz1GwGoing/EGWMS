namespace Admin.NET.Application;

    /// <summary>
    /// EGRelocation输出参数
    /// </summary>
    public class EGRelocationDto
    {
        /// <summary>
        /// 移库编号
        /// </summary>
        public string RelocatioNum { get; set; }
        
        /// <summary>
        /// 移库类型
        /// </summary>
        public int? RelocationType { get; set; }
        
        /// <summary>
        /// 移库数量
        /// </summary>
        public int? RelocationCount { get; set; }
        
        /// <summary>
        /// 移库人
        /// </summary>
        public string? RelocationUser { get; set; }
        
        /// <summary>
        /// 移库时间
        /// </summary>
        public DateTime? RelocationTime { get; set; }
        
        /// <summary>
        /// 仓库编号
        /// </summary>
        public string? WHNum { get; set; }
        
        /// <summary>
        /// 栈板编号
        /// </summary>
        public string? PalletNum { get; set; }
        
        /// <summary>
        /// 料箱编号
        /// </summary>
        public string? WorkBinNum { get; set; }
        
        /// <summary>
        /// 移库备注
        /// </summary>
        public string? Relocation { get; set; }
        
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }
        
        /// <summary>
        /// 物料编号
        /// </summary>
        public string? MaterielNum { get; set; }
        
    }
