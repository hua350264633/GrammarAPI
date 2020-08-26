using Microsoft.Extensions.DependencyInjection;
using ML.Dapper.Base;
using ML.Dapper.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Dapper
{
    /// <summary>
    /// 默认工厂构建器
    /// </summary>
    internal class DefaultDapperFactoryBuilder : IDapperFactoryBuilder
    {
        /// <summary>
        /// 有参构造
        /// </summary>
        /// <param name="services"></param>
        /// <param name="DBType"></param>
        public DefaultDapperFactoryBuilder(IServiceCollection services, EnumDBType DBType)
        {
            Services = services;
            Name = DBType.ToString();
        }

        public string Name { get; }

        public IServiceCollection Services { get; }
    }
}
