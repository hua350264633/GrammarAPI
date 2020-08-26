using ML.Dapper.Models;
using System;

namespace ML.Dapper.Base
{
    /// <summary>
    /// 工厂接口
    /// </summary>
    public interface IDapperFactory
    {
        /// <summary>
        /// 创建客户端对象
        /// </summary>
        /// <param name="DBType">数据库类型名称</param>
        /// <returns></returns>
        DapperClient CreateClient(EnumDBType DBType);
    }
}
