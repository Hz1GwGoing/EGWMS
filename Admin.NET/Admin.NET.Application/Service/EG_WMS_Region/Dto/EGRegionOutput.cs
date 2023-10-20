namespace Admin.NET.Application;

    /// <summary>
    /// EGRegion输出参数
    /// </summary>
    public class EGRegionOutput
    {
       /// <summary>
       /// Id
       /// </summary>
       public long Id { get; set; }
    
       /// <summary>
       /// 区域编号
       /// </summary>
       public string? RegionNum { get; set; }
    
       /// <summary>
       /// 区域名称
       /// </summary>
       public string? RegionName { get; set; }
    
       /// <summary>
       /// 所属仓库
       /// </summary>
       public string? WareHouseName { get; set; }
    
       /// <summary>
       /// 区域状态
       /// </summary>
       public int? RegionStatus { get; set; }
    
       /// <summary>
       /// 库位总数
       /// </summary>
       public int? StoreroomCount { get; set; } = 0;
    
       /// <summary>
       /// 可用库位
       /// </summary>
       public int? StoreroomUsable { get; set; } = 0;
    
       /// <summary>
       /// 创建者姓名
       /// </summary>
       public string? CreateUserName { get; set; }
    
       /// <summary>
       /// 修改者姓名
       /// </summary>
       public string? UpdateUserName { get; set; }
    
       /// <summary>
       /// 备注
       /// </summary>
       public string? RegionRemake { get; set; }
    
       /// <summary>
       /// 仓库编号
       /// </summary>
       public string? WHNum { get; set; } 
       
       /// <summary>
       /// 仓库编号 描述
       /// </summary>
       public string EGWareHouseWHNum { get; set; } 
    
    }
 

