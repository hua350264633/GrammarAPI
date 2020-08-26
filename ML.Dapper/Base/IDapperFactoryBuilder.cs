using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Dapper.Base
{

    public interface IDapperFactoryBuilder
    {
        /// <summary>
        /// 依赖注入名称（对应DBType类的枚举名称）
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// 注入名称对应的服务对象
        /// </summary>

        IServiceCollection Services { get; }
    }
}
