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
/**
 * 系统操作日志表
 * @export
 * @interface SysLogOp
 */
export interface SysLogOp {
    /**
     * 雪花Id
     * @type {number}
     * @memberof SysLogOp
     */
    id?: number;
    /**
     * 创建时间
     * @type {Date}
     * @memberof SysLogOp
     */
    createTime?: Date | null;
    /**
     * 更新时间
     * @type {Date}
     * @memberof SysLogOp
     */
    updateTime?: Date | null;
    /**
     * 创建者Id
     * @type {number}
     * @memberof SysLogOp
     */
    createUserId?: number | null;
    /**
     * 修改者Id
     * @type {number}
     * @memberof SysLogOp
     */
    updateUserId?: number | null;
    /**
     * 软删除
     * @type {boolean}
     * @memberof SysLogOp
     */
    isDelete?: boolean;
    /**
     * 租户Id
     * @type {number}
     * @memberof SysLogOp
     */
    tenantId?: number | null;
    /**
     * 记录器类别名称
     * @type {string}
     * @memberof SysLogOp
     */
    logName?: string | null;
    /**
     * 日志级别
     * @type {string}
     * @memberof SysLogOp
     */
    logLevel?: string | null;
    /**
     * 事件Id
     * @type {string}
     * @memberof SysLogOp
     */
    eventId?: string | null;
    /**
     * 日志消息
     * @type {string}
     * @memberof SysLogOp
     */
    message?: string | null;
    /**
     * 异常对象
     * @type {string}
     * @memberof SysLogOp
     */
    exception?: string | null;
    /**
     * 当前状态值
     * @type {string}
     * @memberof SysLogOp
     */
    state?: string | null;
    /**
     * 日志记录时间
     * @type {Date}
     * @memberof SysLogOp
     */
    logDateTime?: Date;
    /**
     * 线程Id
     * @type {number}
     * @memberof SysLogOp
     */
    threadId?: number;
}
