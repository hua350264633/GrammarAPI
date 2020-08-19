using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Dapper
{
    public class ConnectionConfig
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// 数据库类型
        /// </summary>
        public EnumDbStoreType DbType { get; set; }
    }
}
