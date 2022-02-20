using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Slpk.Server.Exceptions;

namespace Slpk.Server.Filters
{
    public class GlobalExceptionFilter:ExceptionFilterAttribute
    {
        private readonly ILogger<GlobalExceptionFilter> logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            this.logger = logger;
        }

        public override Task OnExceptionAsync(ExceptionContext context)
        {
            var result = new ObjectResult(context.Exception.Message);

            if(context.Exception is BusnessException exception)
            {
                result.StatusCode = exception.StatusCode;
            }
            else
            {
                logger.LogError(context.Exception.Message);
                result.StatusCode = 500;
            }

            context.Result = result;

            return base.OnExceptionAsync(context);
        }
    }
}
