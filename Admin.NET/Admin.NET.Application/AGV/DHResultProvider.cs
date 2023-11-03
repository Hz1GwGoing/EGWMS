
using Furion.DataValidation;
using Furion.JsonSerialization;
using Furion.UnifyResult;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Admin.NET.Core;

/// <summary>
/// DH全局规范化结果
/// </summary>
[UnifyModel(typeof(DHResult<>))]
public class DHResultProvider : IUnifyResultProvider
{
    /// <summary>
    /// 异常返回值
    /// </summary>
    /// <param name="context"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    public IActionResult OnException(ExceptionContext context, ExceptionMetadata metadata)
    {
        return new JsonResult(RESTfulResult(metadata.StatusCode, data: metadata.Data, errors: metadata.Errors));
    }

    /// <summary>
    /// 成功返回值1000
    /// </summary>
    /// <param name="context"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public IActionResult OnSucceeded(ActionExecutedContext context, object data)
    {
        return new JsonResult(RESTfulResult(1000, true, data));
    }

    /// <summary>
    /// 验证失败返回值
    /// </summary>
    /// <param name="context"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    public IActionResult OnValidateFailed(ActionExecutingContext context, ValidationMetadata metadata)
    {
        return new JsonResult(RESTfulResult(metadata.StatusCode ?? StatusCodes.Status400BadRequest, data: metadata.Data, errors: metadata.ValidationResult));
    }

    /// <summary>
    /// 特定状态码返回值
    /// </summary>
    /// <param name="context"></param>
    /// <param name="statusCode"></param>
    /// <param name="unifyResultSettings"></param>
    /// <returns></returns>
    public async Task OnResponseStatusCodes(HttpContext context, int statusCode, UnifyResultSettingsOptions unifyResultSettings)
    {
        // 设置响应状态码
        UnifyContext.SetResponseStatusCodes(context, statusCode, unifyResultSettings);

        switch (statusCode)
        {
            // 处理 401 状态码
            case StatusCodes.Status401Unauthorized:
                await context.Response.WriteAsJsonAsync(RESTfulResult(statusCode, errors: "401 登录已过期，请重新登录"),
                    App.GetOptions<JsonOptions>()?.JsonSerializerOptions);
                break;
            // 处理 403 状态码
            case StatusCodes.Status403Forbidden:
                await context.Response.WriteAsJsonAsync(RESTfulResult(statusCode, errors: "403 禁止访问，没有权限"),
                    App.GetOptions<JsonOptions>()?.JsonSerializerOptions);
                break;

            default: break;
        }
    }

    /// <summary>
    /// 返回 RESTful 风格结果集
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="succeeded"></param>
    /// <param name="data"></param>
    /// <param name="errors"></param>
    /// <returns></returns>
    private static DHResult<object> RESTfulResult(int statusCode, bool succeeded = default, object data = default, object errors = default)
    {
        //// 统一返回值脱敏处理
        //if (data?.GetType() == typeof(String))
        //{
        //    data = App.GetRequiredService<ISensitiveDetectionProvider>().ReplaceAsync(data.ToString(), '*').GetAwaiter().GetResult();
        //}
        //else if (data?.GetType() == typeof(JsonResult))
        //{
        //    data = App.GetRequiredService<ISensitiveDetectionProvider>().ReplaceAsync(JSON.Serialize(data), '*').GetAwaiter().GetResult();
        //}

        return new DHResult<object>
        {
            code = statusCode,
            desc = succeeded ? "success" : JSON.Serialize(errors),
            data = data,
        };
    }
}

/// <summary>
/// 全局返回结果
/// </summary>
/// <typeparam name="T"></typeparam>
public class DHResult<T>
{
    /// <summary>
    /// 状态码
    /// </summary>
    public int code { get; set; }

    /// <summary>
    /// 状态码描述信息
    /// </summary>
    public string desc { get; set; }

    /// <summary>
    /// 数据
    /// </summary>
    public T data { get; set; }
}