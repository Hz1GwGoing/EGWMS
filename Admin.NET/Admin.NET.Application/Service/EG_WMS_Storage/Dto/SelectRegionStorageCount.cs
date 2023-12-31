﻿namespace Admin.NET.Application.Service.EG_WMS_Storage.Dto;

public class SelectRegionStorageCountDto
{
    public long id { get; set; }

    public string RegionNum { get; set; }
    public string RegionName { get; set; }

    public string WHNum { get; set; }
    /// <summary>
    /// 库位总数
    /// </summary>
    public int TotalStorage { get; set; }
    /// <summary>
    /// 可使用的库位
    /// </summary>
    public int EnabledStorage { get; set; }
    /// <summary>
    /// 已占用库位
    /// </summary>
    public int UsedStorage { get; set; }
    public string WHName { get; set; }
    public string Remake { get; set; }
    public string CreateUserName { get; set; }
    public string UpdateUserName { get; set; }

    /// <summary>
    /// 区域绑定物料
    /// </summary>
    public string RegionMaterielNum { get; set; }

}