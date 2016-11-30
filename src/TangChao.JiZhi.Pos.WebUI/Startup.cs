using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TangChao.JiZhi.Pos.Common;
using TangChao.JiZhi.Pos.IDAL;
using TangChao.JiZhi.Pos.DAL;
using TangChao.JiZhi.Pos.IBizlogic;
using TangChao.JiZhi.Pos.Bizlogic;

namespace TangChao.JiZhi.Pos.WebUI
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);


            #region BLL

            services.AddSingleton<IUserNameBLL, UserNameBLL>();

            #endregion

            #region DAL

            services.AddSingleton<IUserNameDAL, UserNameDAL>();

            #endregion


            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            InitConfigData();
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseApplicationInsightsExceptionTelemetry();

            app.UseStaticFiles();

            app.UseStatusCodePages("text/plain", "这是 response返回的status code：{0}");

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }


        /// <summary>
        /// 读取配置文件
        /// </summary>
        private void InitConfigData()
        {
            Config.MysqlConfigStr = new MysqlConfig();

            var connectionStrings = Configuration.GetSection("ConnectionStrings");

            Config.MysqlConfigStr.Name = connectionStrings["Name"];

            Config.MysqlConfigStr.ConnectionString = connectionStrings["ConnectionString"];

            Config.MysqlConfigStr.IsEncryption = Convert.ToBoolean(connectionStrings["IsEncryption"].ToString());
        }
    }
}
