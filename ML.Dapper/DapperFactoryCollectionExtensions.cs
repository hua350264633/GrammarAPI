using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ML.Dapper.Base;
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
        /// 添加配置
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDapper(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            //services.AddLogging();
            services.AddOptions();

            services.AddSingleton<DefaultDapperFactory>();
            services.TryAddSingleton<IDapperFactory>(serviceProvider => serviceProvider.GetRequiredService<DefaultDapperFactory>());

            return services;
        }
        /// <summary>
        /// 添加配置
        /// </summary>
        /// <param name="services"></param>
        /// <param name="EnumDbStoreType"></param>
        /// <param name="configureClient"></param>
        /// <returns></returns>
        public static IDapperFactoryBuilder AddDapper(this IServiceCollection services, EnumDbStoreType EnumDbStoreType,
            Action<ConnectionConfig> configureClient)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configureClient == null)
                throw new ArgumentNullException(nameof(configureClient));

            AddDapper(services);

            var builder = new DefaultDapperFactoryBuilder(services, EnumDbStoreType);
            builder.ConfigureDapper(configureClient);
            return builder;
        }

        public static IDapperFactoryBuilder ConfigureDapper(this IDapperFactoryBuilder builder, Action<ConnectionConfig> configureClient)
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
