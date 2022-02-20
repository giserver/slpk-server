using Microsoft.AspNetCore.Mvc.Filters;

namespace Slpk.Server.Filters
{
    public class ResponseHeaderAttribute:ActionFilterAttribute
    {
        private readonly string _name;
        private readonly string _value;

        public ResponseHeaderAttribute(string name, string value) =>
            (_name, _value) = (name, value);

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            context.HttpContext.Response.Headers.Add(_name, _value);

            base.OnResultExecuting(context);
        }
    }
}
