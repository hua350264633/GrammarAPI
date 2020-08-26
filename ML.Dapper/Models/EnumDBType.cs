using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Dapper.Models
{
    /// <summary>
    /// 数据库类型枚举类
    /// </summary>
    public enum EnumDBType
    {
        /// <summary>
        /// MySQL数据库
        /// </summary>
        MySql = 0,
        /// <summary>
        /// SQLServer数据库
        /// </summary>
        SqlServer = 1,
        /// <summary>
        /// SQLite数据库
        /// </summary>
        Sqlite = 2,
        /// <summary>
        /// Oracle数据库
        /// </summary>
        Oracle = 3
    }
}
