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
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ML.Dapper;

namespace GrammarAPI
{
    public class Startup
    {
        //log4net��־
        public static ILoggerRepository Repository { get; set; }
        /// <summary>
        /// �����ļ�
        /// </summary>
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            //��ȡ�����ļ�
            Configuration = configuration;

            //����log4net��־�����ļ�
            Repository = LogManager.CreateRepository("NETCoreRepository1");
            XmlConfigurator.Configure(Repository, new FileInfo("log4net.config"));
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            #region ����ȫ���쳣����
            services.AddMvc(options =>
            {
                options.Filters.Add<Models.HttpGlobalExceptionFilter>(); //����ȫ���쳣��
            });
            #endregion

            #region �������ݿ�������Ϣ
            //����sqlserver
            services.AddDapper(EnumDbStoreType.SqlServer, m =>
            {
                m.ConnectionString = Configuration.GetConnectionString("DefaultConnection");
                m.DbType = EnumDbStoreType.SqlServer;
            });

            ////����Oracle
            //services.AddDapper("OracleConnection", m =>
            //{
            //    m.ConnectionString = Configuration.GetConnectionString("OracleConnectionString");
            //    m.DbType = EnumDbStoreType.Oracle;
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
            else
            {
                app.UseExceptionHandler("/Error");
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
