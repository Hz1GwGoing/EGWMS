namespace Admin.NET.Application;

    /// <summary>
    /// EGOutBound输出参数
    /// </summary>
    public class EGOutBoundOutput
    {
       /// <summary>
       /// 出库编号
       /// </summary>
       public string OutboundNum { get; set; }
    
       /// <summary>
       /// 出库类型
       /// </summary>
       public int? OutboundType { get; set; }
    
       /// <summary>
       /// 出库数量
       /// </summary>
       public int? OutboundCount { get; set; }
    
       /// <summary>
       /// 出库人
       /// </summary>
       public string? OutboundUser { get; set; }
    
       /// <summary>
       /// 出库时间
       /// </summary>
       public DateTime? OutboundTime { get; set; }
    
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
       /// 出库备注
       /// </summary>
       public string? OutboundRemake { get; set; }
    
       /// <summary>
       /// Id
       /// </summary>
       public long Id { get; set; }
    
       /// <summary>
       /// 物料编号
       /// </summary>
       public string? MaterielNum { get; set; }
    
    }
 

