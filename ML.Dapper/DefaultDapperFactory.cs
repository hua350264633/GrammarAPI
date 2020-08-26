using Microsoft.Extensions.Options;
using ML.Dapper.Base;
using ML.Dapper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ML.Dapper
{
    /// <summary>
    /// 默认工厂类
    /// </summary>
    public class DefaultDapperFactory : IDapperFactory
    {

        private readonly IServiceProvider _services;

        private readonly IOptionsMonitor<DapperFactoryOptions> _optionsMonitor;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="services"></param>
        /// <param name="optionsMonitor"></param>
        public DefaultDapperFactory(IServiceProvider services, IOptionsMonitor<DapperFactoryOptions> optionsMonitor)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        }
        /// <summary>
        /// 创建客户端实现
        /// </summary>
        /// <param name="DBType"></param>
        /// <returns></returns>
        public DapperClient CreateClient(EnumDBType DBType)
        {
            var client = new DapperClient(new DBConnectionConfig { });

            var option = _optionsMonitor.Get(DBType.ToString()).DapperActions.FirstOrDefault();
            if (option != null)
                option(client.CurrentConnectionConfig);
            else
                throw new ArgumentNullException(nameof(option));
            return client;
        }
    }
}
