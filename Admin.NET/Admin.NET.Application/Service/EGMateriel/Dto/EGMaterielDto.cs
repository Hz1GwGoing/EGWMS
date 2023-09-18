﻿namespace Admin.NET.Application;

    /// <summary>
    /// EGMateriel输出参数
    /// </summary>
    public class EGMaterielDto
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
        /// Id
        /// </summary>
        public long Id { get; set; }
        
        /// <summary>
        /// 主单位
        /// </summary>
        public string? MaterielMainUnit { get; set; }
        
        /// <summary>
        /// 辅单位
        /// </summary>
        public string? MaterielAssistUnit { get; set; }
        
    }