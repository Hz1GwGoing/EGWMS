﻿using Admin.NET.Core.Service;
using Furion;
using Furion.EventBus;
using Furion.JsonSerialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using UAParser;

namespace Admin.NET.Core
{
    /// <summary>
    /// 请求操作拦截
    /// </summary>
    public class RequestActionFilter : IAsyncActionFilter
    {
        private readonly IEventPublisher _eventPublisher;

        public RequestActionFilter(IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 是否开启操作日志
            var value = await App.GetService<SysConfigService>().GetConfigCache(CommonConst.SysOpLogFlag);
            if (string.IsNullOrWhiteSpace(value) || !bool.Parse(value)) return;

            var httpContext = context.HttpContext;
            var httpRequest = httpContext.Request;

            var sw = new Stopwatch();
            sw.Start();
            var actionContext = await next();
            sw.Stop();

            var isRequestSucceed = actionContext.Exception == null; // 判断是否请求成功（没有异常就是成功）
            var headers = httpRequest.Headers;
            var clientInfo = headers.ContainsKey("User-Agent") ? Parser.GetDefault().Parse(headers["User-Agent"]) : null;
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            var ip = httpContext.GetRemoteIpAddressToIPv4();

            await _eventPublisher.PublishAsync(new ChannelEventSource("Add:OpLog",
                new SysLogOp
                {
                    Success = isRequestSucceed ? YesNoEnum.Y : YesNoEnum.N,
                    Ip = ip,
                    Location = httpRequest.GetRequestUrlAddress(),
                    Browser = clientInfo?.UA.Family + clientInfo?.UA.Major,
                    Os = clientInfo?.OS.Family + clientInfo?.OS.Major,
                    Url = httpRequest.Path,
                    ClassName = context.Controller.ToString(),
                    MethodName = actionDescriptor?.ActionName,
                    ReqMethod = httpRequest.Method,
                    Param = context.ActionArguments.Count < 1 ? string.Empty : JSON.Serialize(context.ActionArguments),
                    Result = actionContext.Result?.GetType() == typeof(JsonResult) ? JSON.Serialize(actionContext.Result) : string.Empty,
                    ElapsedTime = sw.ElapsedMilliseconds,                    
                    UserName = httpContext.User?.FindFirstValue(ClaimConst.UserName),
                    RealName = httpContext.User?.FindFirstValue(ClaimConst.RealName)
                }));
        }
    }
}