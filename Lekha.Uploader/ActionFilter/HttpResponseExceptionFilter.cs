using Lekha.Uploader.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;

namespace Lekha.Uploader.ActionFilter
{
    /// <summary>
    /// Reference: https://docs.microsoft.com/en-us/aspnet/core/web-api/handle-errors?view=aspnetcore-5.0
    /// </summary>
    public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
    {
        private readonly UploaderApplicationContext appContext;
        private readonly ILogger<HttpResponseExceptionFilter> logger;

        public int Order { get; } = int.MaxValue - 10;

        public HttpResponseExceptionFilter(UploaderApplicationContext appContext, ILogger<HttpResponseExceptionFilter> logger)
        {
            this.appContext = appContext;
            this.logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is ServiceException exception)
            {
                context.Result = new ObjectResult(exception.Value)
                {
                    StatusCode = exception.Status,
                };
                context.ExceptionHandled = true;
            }
            else if (context.Exception is AggregateException ae)
            {
                var values = new List<object>();
                foreach (var e in ae.Flatten().InnerExceptions)
                {
                    if (e is ServiceException)
                    {
                        logger.LogError(e, "Error encountered");
                    }
                }
                context.Result = new ObjectResult(values)
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
                context.ExceptionHandled = true;
            }
        }
    }
}
