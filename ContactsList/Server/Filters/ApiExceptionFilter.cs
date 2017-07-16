using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ContactsList.Server.Filters
{
    public class ApiExceptionFilter : ExceptionFilterAttribute
    {
        private ILogger<ApiExceptionFilter> _logger;

        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
        {
            _logger = logger;
        }


        public override void OnException(ExceptionContext context)
        {
            string content = "";
            var result = new ContentResult()
            {
                ContentType = "application/json"
            };

            if (context.Exception is ApiException)
            {
                // handle explicit 'known' API errors
                var ex = context.Exception as ApiException;
                context.Exception = null;
                content = ConvertToJson(ex.Message);
                context.HttpContext.Response.StatusCode = 400;

                _logger.LogWarning($"Application thrown error: {ex.Message}", ex);
            }
            else if (context.Exception is UnauthorizedAccessException)
            {
                // it`s not necessary to send any message to client (it knows issue by the status code)
                context.HttpContext.Response.StatusCode = 401;
                _logger.LogWarning("Unauthorized Access in Controller Filter.");
            }
            else
            {
                // Unhandled errors
#if !DEBUG
                var msg = "";
                string stack = null;
#else
                var msg = context.Exception.GetBaseException().Message;
                string stack = context.Exception.StackTrace;
#endif

                content = ConvertToJson(msg);
                context.HttpContext.Response.StatusCode = 500;

                // handle logging here
                _logger.LogError(new EventId(0), context.Exception, context.Exception.Message);
            }

            // always return a JSON result
            result.Content = content;
            context.Result = result;

            base.OnException(context);
        }

        private string ConvertToJson(string error)
        {
            return JsonConvert.SerializeObject(new string[] { error },
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
        }
    }
}