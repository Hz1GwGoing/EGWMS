namespace Admin.NET.Application;

    /// <summary>
    /// EG_WMS_InAndOutBoundDetail输出参数
    /// </summary>
    public class EG_WMS_InAndOutBoundDetailDto
    {
        /// <summary>
        /// 出入库编号
        /// </summary>
        public string EG_WMS_InAndOutBoundInAndOutBoundNum { get; set; }
        
        /// <summary>
        /// 仓库编号
        /// </summary>
        public string EG_WMS_WareHouseWHNum { get; set; }
        
        /// <summary>
        /// 栈板编号
        /// </summary>
        public string EG_WMS_PalletPalletNum { get; set; }
        
        /// <summary>
        /// 料箱编号
        /// </summary>
        public string? EG_WMS_WorkBinWorkBinNum { get; set; }
        
        /// <summary>
        /// 物料编号
        /// </summary>
        public string? EG_WMS_MaterielMaterielNum { get; set; }
        
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }
        
        /// <summary>
        /// 出入库编号
        /// </summary>
        public string InAndOutBoundNum { get; set; }
        
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
        /// 物料编号
        /// </summary>
        public string? MaterielNum { get; set; }
        
    }
