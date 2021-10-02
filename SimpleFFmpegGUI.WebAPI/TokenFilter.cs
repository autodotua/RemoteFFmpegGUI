using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using SimpleFFmpegGUI.WebAPI.Controllers;
using System.Linq;

namespace SimpleFFmpegGUI.WebAPI
{
    public class TokenFilter : ActionFilterAttribute
    {
        private readonly IConfiguration config;
        private string token;

        public TokenFilter(IConfiguration config)
        {
            this.config = config;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.Controller is TokenController)
            {
                return;
            }
            var http = context.HttpContext;

            if (token == null)
            {
                token = config.GetValue<string>("token") ?? "";
            }
            if (token != "")
            {
                if (!http.Request.Headers.ContainsKey("Authorization")
                    || StringValues.IsNullOrEmpty(http.Request.Headers["Authorization"])
                    || http.Request.Headers["Authorization"].FirstOrDefault() == "undefined")
                {
                    context.Result = new UnauthorizedObjectResult("需要Token");
                    return;
                }
                if (http.Request.Headers["Authorization"] != token)
                {
                    context.Result = new UnauthorizedObjectResult("Token不正确");
                    return;
                }
            }
        }
    }
}