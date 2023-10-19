using System.ComponentModel.DataAnnotations;

namespace Admin.NET.Application;

    /// <summary>
    /// EG_WMS_InAndOutBoundDetail基础输入参数
    /// </summary>
    public class EG_WMS_InAndOutBoundDetailBaseInput
    {
        /// <summary>
        /// 出入库编号
        /// </summary>
        public virtual string InAndOutBoundNum { get; set; }
        
        /// <summary>
        /// 仓库编号
        /// </summary>
        public virtual string? WHNum { get; set; }
        
        /// <summary>
        /// 栈板编号
        /// </summary>
        public virtual string? PalletNum { get; set; }
        
        /// <summary>
        /// 料箱编号
        /// </summary>
        public virtual string? WorkBinNum { get; set; }
        
        /// <summary>
        /// 物料编号
        /// </summary>
        public virtual string? MaterielNum { get; set; }
        
    }

    /// <summary>
    /// EG_WMS_InAndOutBoundDetail分页查询输入参数
    /// </summary>
    public class EG_WMS_InAndOutBoundDetailInput : BasePageInput
    {
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

    /// <summary>
    /// EG_WMS_InAndOutBoundDetail增加输入参数
    /// </summary>
    public class AddEG_WMS_InAndOutBoundDetailInput : EG_WMS_InAndOutBoundDetailBaseInput
    {
        /// <summary>
        /// 出入库编号
        /// </summary>
        [Required(ErrorMessage = "出入库编号不能为空")]
        public override string InAndOutBoundNum { get; set; }
        
    }

    /// <summary>
    /// EG_WMS_InAndOutBoundDetail删除输入参数
    /// </summary>
    public class DeleteEG_WMS_InAndOutBoundDetailInput : BaseIdInput
    {
    }

    /// <summary>
    /// EG_WMS_InAndOutBoundDetail更新输入参数
    /// </summary>
    public class UpdateEG_WMS_InAndOutBoundDetailInput : EG_WMS_InAndOutBoundDetailBaseInput
    {
        /// <summary>
        /// Id
        /// </summary>
        [Required(ErrorMessage = "Id不能为空")]
        public long Id { get; set; }
        
    }

    /// <summary>
    /// EG_WMS_InAndOutBoundDetail主键查询输入参数
    /// </summary>
    public class QueryByIdEG_WMS_InAndOutBoundDetailInput : DeleteEG_WMS_InAndOutBoundDetailInput
    {

    }
