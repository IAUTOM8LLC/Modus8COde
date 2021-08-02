using IAutoM8.Global.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace IAutoM8.Infrastructure.Filters
{
    public class GlobalExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger<GlobalExceptionFilterAttribute> _logger;

        public GlobalExceptionFilterAttribute(ILogger<GlobalExceptionFilterAttribute> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            string message;
            HttpStatusCode status;

            if (context.Exception is ValidationException ex)
            {
                // handle explicit 'known' API errors
                message = ex.Message;
                status = HttpStatusCode.BadRequest;
                context.ExceptionHandled = true;
            }
            else if (context.Exception is ForbiddenException fbEx)
            {
                message = fbEx.Message;
                status = fbEx.ShouldRedirect? HttpStatusCode.Forbidden : HttpStatusCode.BadRequest;
                context.ExceptionHandled = true;
            }
            else
            {
                // Unhandled errors
                _logger.LogError(context.Exception, context.Exception.Message);

                message = context.Exception.Message;
                status = HttpStatusCode.InternalServerError;
            }

            var response = context.HttpContext.Response;
            response.StatusCode = (int)status;
            response.ContentType = "application/json";

            context.Result = new JsonResult(new
            {
                message = message,
                exception = context.Exception
            });

            base.OnException(context);
        }
    }
}
