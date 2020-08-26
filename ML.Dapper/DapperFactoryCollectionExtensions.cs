using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ML.Dapper.Base;
using ML.Dapper.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Dapper
{
    /// <summary>
    /// 扩展类
    /// </summary>
    public static class DapperFactoryCollectionExtensions
    {
        /// <summary>
        /// 添加服务到容器中
        /// </summary>
        /// <param name="services">服务容器</param>
        /// <param name="DBType">数据库类型</param>
        /// <param name="configureClient"></param>
        /// <returns></returns>
        public static IDapperFactoryBuilder AddDapper(this IServiceCollection services, EnumDBType DBType,
            Action<DBConnectionConfig> configureClient)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configureClient == null)
                throw new ArgumentNullException(nameof(configureClient));

            //services.AddLogging();
            //添加使用选项所需的服务。
            services.AddOptions();

            //将TService中指定类型的单例服务添加到指定的Microsoft.Extensions.DependencyInjection.IServiceCollection。
            services.AddSingleton<DefaultDapperFactory>();

            //将指定的服务添加为Microsoft.Extensions.DependencyInjection.服务寿命。单例
            //使用implementationFactory中指定的工厂的服务
            //如果服务类型尚未注册。
            services.TryAddSingleton<IDapperFactory>(serviceProvider => serviceProvider.GetRequiredService<DefaultDapperFactory>());

            var builder = new DefaultDapperFactoryBuilder(services, DBType);
            builder.ConfigureDapper(configureClient);
            return builder;
        }
        /// <summary>
        /// 配置Dapper
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureClient"></param>
        /// <returns></returns>
        public static IDapperFactoryBuilder ConfigureDapper(this IDapperFactoryBuilder builder, Action<DBConnectionConfig> configureClient)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (configureClient == null)
                throw new ArgumentNullException(nameof(configureClient));

            builder.Services.Configure<DapperFactoryOptions>(builder.Name, options => options.DapperActions.Add(configureClient));

            return builder;
        }

    }
}
