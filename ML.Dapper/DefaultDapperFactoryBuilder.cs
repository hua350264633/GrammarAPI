using Microsoft.Extensions.DependencyInjection;
using ML.Dapper.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Dapper
{
    internal class DefaultDapperFactoryBuilder : IDapperFactoryBuilder
    {
        public DefaultDapperFactoryBuilder(IServiceCollection services, EnumDbStoreType EnumDbStoreType)
        {
            Services = services;
            Name = EnumDbStoreType.ToString();
        }

        public string Name { get; }

        public IServiceCollection Services { get; }
    }
}
