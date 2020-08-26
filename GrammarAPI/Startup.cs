using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ML.Dapper;
using ML.Dapper.Models;

namespace GrammarAPI
{
    public class Startup
    {
        //log4net日志
        public static ILoggerRepository Repository { get; set; }
        /// <summary>
        /// 配置文件
        /// </summary>
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            //获取配置文件
            Configuration = configuration;

            //加载log4net日志配置文件
            Repository = LogManager.CreateRepository("NETCoreRepository1");
            XmlConfigurator.Configure(Repository, new FileInfo("log4net.config"));
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// 此方法由运行时调用。使用此方法可将服务添加到容器中。
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            #region 配置中间件
            services.AddMvc(options =>
            {
                options.Filters.Add<Models.HttpGlobalExceptionFilter>(); //加入全局异常类

                //注册返回结果中间件
                options.Filters.Add(typeof(WebApiResultMiddleware));
                options.RespectBrowserAcceptHeader = true;
            });
            #endregion

            #region 配置数据库连接信息
            //连接sqlserver
            services.AddDapper(EnumDBType.SqlServer, configureClient =>
            {
                configureClient.ConnectionString = Configuration.GetConnectionString("DefaultConnection");
                configureClient.DbType = EnumDBType.SqlServer;
            });

            ////连接Oracle
            //services.AddDapper("OracleConnection", m =>
            //{
            //    m.ConnectionString = Configuration.GetConnectionString("OracleConnectionString");
            //    m.DbType = DBType.Oracle;
            //});
            #endregion
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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
