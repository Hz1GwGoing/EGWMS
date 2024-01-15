using Admin.NET.Core;
using System.ComponentModel.DataAnnotations;

namespace Admin.NET.Application.Service.EGWareHouse.Dto;

/// <summary>
/// EGWareHouse基础输入参数
/// </summary>
public class EGWareHouseBaseInput
{
    /// <summary>
    /// 仓库编号
    /// </summary>
    public virtual string WHNum { get; set; }

    /// <summary>
    /// 仓库名称
    /// </summary>
    public virtual string WHName { get; set; }

    /// <summary>
    /// 仓库类型
    /// </summary>
    public virtual int? WHType { get; set; }

    /// <summary>
    /// 仓库地址
    /// </summary>
    public virtual string WHAddress { get; set; }

    /// <summary>
    /// 仓库状态
    /// </summary>
    public virtual int? WHStatus { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public virtual string WHRemake { get; set; }

}

/// <summary>
/// EGWareHouse分页查询输入参数
/// </summary>
public class EGWareHouseInput
{

    public int page { get; set; }
    public int pageSize { get; set; }

    /// <summary>
    /// 仓库编号
    /// </summary>
    public string WHNum { get; set; }

    /// <summary>
    /// 仓库名称
    /// </summary>
    public string WHName { get; set; }

    /// <summary>
    /// 仓库类型
    /// </summary>
    public int? WHType { get; set; }

    /// <summary>
    /// 仓库地址
    /// </summary>
    public string WHAddress { get; set; }

    /// <summary>
    /// 仓库状态
    /// </summary>
    public int? WHStatus { get; set; }
}

/// <summary>
/// EGWareHouse增加输入参数
/// </summary>
public class AddEGWareHouseInput : EGWareHouseBaseInput
{
    /// <summary>
    /// 仓库编号
    /// </summary>
    public override string WHNum { get; set; }

    /// <summary>
    /// 仓库名称
    /// </summary>
    [Required(ErrorMessage = "仓库名称不能为空")]
    public override string WHName { get; set; }

}

/// <summary>
/// EGWareHouse删除输入参数
/// </summary>
public class DeleteEGWareHouseInput : BaseIdInput
{
}

/// <summary>
/// EGWareHouse更新输入参数
/// </summary>
public class UpdateEGWareHouseInput : EGWareHouseBaseInput
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [Required(ErrorMessage = "主键ID不能为空")]
    public long Id { get; set; }

}

/// <summary>
/// EGWareHouse主键查询输入参数
/// </summary>
public class QueryByIdEGWareHouseInput
{
    /// <summary>
    /// 仓库编号
    /// </summary>
    public string WHNum { get; set; }

    /// <summary>
    /// 仓库名称
    /// </summary>
    public string WHName { get; set; }
}
