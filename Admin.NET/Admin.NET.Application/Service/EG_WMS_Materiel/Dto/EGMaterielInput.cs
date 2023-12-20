using Admin.NET.Core;
using System.ComponentModel.DataAnnotations;

namespace Admin.NET.Application;

/// <summary>
/// EGMateriel基础输入参数
/// </summary>
public class EGMaterielBaseInput
{
    /// <summary>
    /// 物料编号
    /// </summary>
    public virtual string? MaterielNum { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    public virtual string? MaterielName { get; set; }

    /// <summary>
    /// 物料类别
    /// </summary>
    public virtual string? MaterielType { get; set; }

    /// <summary>
    /// 物料规格
    /// </summary>
    public virtual string? MaterielSpecs { get; set; }

    /// <summary>
    /// 物料描述
    /// </summary>
    public virtual string? MaterielDescribe { get; set; }

    /// <summary>
    /// 物料来源
    /// </summary>
    public virtual string? MaterielSource { get; set; }

    /// <summary>
    /// 创建者姓名
    /// </summary>
    public virtual string? CreateUserName { get; set; }

    /// <summary>
    /// 修改者姓名
    /// </summary>
    public virtual string? UpdateUserName { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? MaterielReamke { get; set; }

    /// <summary>
    /// 主单位
    /// </summary>
    public virtual string? MaterielMainUnit { get; set; }

    /// <summary>
    /// 辅单位
    /// </summary>
    public virtual string? MaterielAssistUnit { get; set; }

}

/// <summary>
/// EGMateriel分页查询输入参数
/// </summary>
public class EGMaterielInput : BasePageInput
{
    /// <summary>
    /// 物料编号
    /// </summary>
    public string? MaterielNum { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    public string? MaterielName { get; set; }

    /// <summary>
    /// 物料类别
    /// </summary>
    public string? MaterielType { get; set; }

    /// <summary>
    /// 物料规格
    /// </summary>
    public string? MaterielSpecs { get; set; }

    /// <summary>
    /// 物料描述
    /// </summary>
    public string? MaterielDescribe { get; set; }

    /// <summary>
    /// 物料来源
    /// </summary>
    public string? MaterielSource { get; set; }

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
    public string? MaterielReamke { get; set; }

    /// <summary>
    /// 主单位
    /// </summary>
    public string? MaterielMainUnit { get; set; }

    /// <summary>
    /// 辅单位
    /// </summary>
    public string? MaterielAssistUnit { get; set; }


    /// <summary>
    /// 创建时间
    /// </summary>
    public virtual DateTime? CreateTime { get; set; }
}

/// <summary>
/// EGMateriel增加输入参数
/// </summary>
public class AddEGMaterielInput : EGMaterielBaseInput
{
    /// <summary>
    /// 物料编号
    /// </summary>
    [Required(ErrorMessage = "物料编号不能为空")]
    public override string? MaterielNum { get; set; }

    /// <summary>
    /// 需在库数量
    /// </summary>
    public int? QuantityNeedCount { get; set; } = null;

    /// <summary>
    /// 提醒时间/h
    /// </summary>
    public double? InventoryDateTime { get; set; } = null;

}

/// <summary>
/// EGMateriel删除输入参数
/// </summary>
public class DeleteEGMaterielInput : BaseIdInput
{
}

/// <summary>
/// EGMateriel更新输入参数
/// </summary>
public class UpdateEGMaterielInput : EGMaterielBaseInput
{
    /// <summary>
    /// Id
    /// </summary>
    [Required(ErrorMessage = "Id不能为空")]
    public long Id { get; set; }

}

/// <summary>
/// EGMateriel主键查询输入参数
/// </summary>
//public class QueryByIdEGMaterielInput : DeleteEGMaterielInput
//{
//    /// <summary>
//    /// 物料编号
//    /// </summary>
//    public string? MaterielNum { get; set; }

//}
public class QueryByIdEGMaterielInput
{
    /// <summary>
    /// 物料编号
    /// </summary>
    public string? MaterielNum { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    public string? MaterielName { get; set; }

}