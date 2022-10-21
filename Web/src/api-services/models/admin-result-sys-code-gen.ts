/* tslint:disable */
/* eslint-disable */
/**
 * Admin.NET
 * 让 .NET 开发更简单、更通用、更流行。前后端分离架构(.NET6/Vue3)，开箱即用紧随前沿技术。<br/><a href='https://gitee.com/zuohuaijun/Admin.NET/'>https://gitee.com/zuohuaijun/Admin.NET</a>
 *
 * OpenAPI spec version: 1.0.0
 * Contact: 515096995@qq.com
 *
 * NOTE: This class is auto generated by the swagger code generator program.
 * https://github.com/swagger-api/swagger-codegen.git
 * Do not edit the class manually.
 */
import { SysCodeGen } from './sys-code-gen';
/**
 * 全局返回结果
 * @export
 * @interface AdminResultSysCodeGen
 */
export interface AdminResultSysCodeGen {
    /**
     * 状态码
     * @type {number}
     * @memberof AdminResultSysCodeGen
     */
    code?: number;
    /**
     * 类型success、warning、error
     * @type {string}
     * @memberof AdminResultSysCodeGen
     */
    type?: string | null;
    /**
     * 错误信息
     * @type {string}
     * @memberof AdminResultSysCodeGen
     */
    message?: string | null;
    /**
     * 
     * @type {SysCodeGen}
     * @memberof AdminResultSysCodeGen
     */
    result?: SysCodeGen;
    /**
     * 附加数据
     * @type {any}
     * @memberof AdminResultSysCodeGen
     */
    extras?: any | null;
    /**
     * 时间戳
     * @type {number}
     * @memberof AdminResultSysCodeGen
     */
    timestamp?: number;
}