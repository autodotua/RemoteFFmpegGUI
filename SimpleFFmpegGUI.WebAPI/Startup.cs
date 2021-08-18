using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using SimpleFFmpegGUI.WebAPI.Controllers;
using SimpleFFmpegGUI.WebAPI.Converter;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCorsAccessor();
            services.AddControllers(o =>
            {
                o.Filters.Add(new TokenFilter(Configuration));
            }).AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.Converters.Add(new DoubleConverter());
                o.JsonSerializerOptions.Converters.Add(new TimeSpanConverter());
            })
                .AddInject()
                .AddFriendlyException();
            services.Configure<FormOptions>(o =>
            {
                o.MultipartBodyLengthLimit = int.MaxValue;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCorsAccessor();
            app.UseAuthorization();
            app.UseInject(string.Empty);
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

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
                    || http.Request.Headers["Authorization"].FirstOrDefault()== "undefined")
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