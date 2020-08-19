using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Dapper
{
    /// <summary>
    /// 工厂属性
    /// </summary>
    public class DapperFactoryOptions
    {
        public IList<Action<ConnectionConfig>> DapperActions { get; } = new List<Action<ConnectionConfig>>();
    }
}
