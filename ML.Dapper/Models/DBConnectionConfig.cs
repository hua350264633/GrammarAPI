using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Dapper.Models
{
    /// <summary>
    /// 连接配置实体类
    /// </summary>
    public class DBConnectionConfig
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// 数据库类型
        /// </summary>
        public EnumDBType DbType { get; set; }
    }
}
