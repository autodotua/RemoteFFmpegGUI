using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleFFmpegGUI.WebAPI.Converter;
using System.Collections.Generic;
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
            if (Program.WebApp)
            {
                app.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = new PhysicalFileProvider(env.ContentRootPath + "/html")
                });
            }
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
}