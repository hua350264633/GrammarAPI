using ML.Dapper.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Dapper
{
    /// <summary>
    /// 
    /// </summary>
    public class DapperFactoryOptions
    {
        public IList<Action<DBConnectionConfig>> DapperActions { get; } = new List<Action<DBConnectionConfig>>();
    }
}
