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
    /// 区域数量
    /// </summary>
    public virtual int? RegionCount { get; set; }

    /// <summary>
    /// 库位总数
    /// </summary>
    public virtual int? StoreroomCount { get; set; }

    /// <summary>
    /// 可用库位
    /// </summary>
    public virtual int? StoreroomUsable { get; set; }

    /// <summary>
    /// 创建者姓名
    /// </summary>
    public virtual string CreateUserName { get; set; }

    /// <summary>
    /// 修改者姓名
    /// </summary>
    public virtual string UpdateUserName { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public virtual string WHRemake { get; set; }

}

/// <summary>
/// EGWareHouse分页查询输入参数
/// </summary>
public class EGWareHouseInput : BasePageInput
{
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

    /// <summary>
    /// 区域数量
    /// </summary>
    public int? RegionCount { get; set; }

    /// <summary>
    /// 库位总数
    /// </summary>
    public int? StoreroomCount { get; set; }

    /// <summary>
    /// 可用库位
    /// </summary>
    public int? StoreroomUsable { get; set; }

    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string CreateUserName { get; set; }

    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string UpdateUserName { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string WHRemake { get; set; }


    /// <summary>
    /// 创建时间
    /// </summary>
    public virtual DateTime? CreateTime { get; set; }
}

/// <summary>
/// EGWareHouse增加输入参数
/// </summary>
public class AddEGWareHouseInput : EGWareHouseBaseInput
{
    /// <summary>
    /// 仓库编号
    /// </summary>
    [Required(ErrorMessage = "仓库编号不能为空")]
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
