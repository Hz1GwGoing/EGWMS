﻿namespace Admin.NET.Application.Service.EG_WMS_Pallet.Dto;

/// <summary>
/// EGPallet输出参数
/// </summary>
public class EGPalletDto
{
    /// <summary>
    /// 栈板编号
    /// </summary>
    public string PalletNum { get; set; }

    /// <summary>
    /// 栈板名称
    /// </summary>
    public string PalletName { get; set; }

    /// <summary>
    /// 栈板规格
    /// </summary>
    public string PalletSpecs { get; set; }

    /// <summary>
    /// 有效日期
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// 栈板状态（0.正常 1.异常）
    /// </summary>
    public int? PalletStatus { get; set; }

    /// <summary>
    /// 新增人
    /// </summary>
    public string CreateUserName { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string PalletRemake { get; set; }

    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 库位编号
    /// </summary>
    public string StorageNum { get; set; }

}
