using log4net;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GrammarAPI.Models
{
    /// <summary>
    /// 全局异常类
    /// </summary>
    public class HttpGlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILog log = LogManager.GetLogger(Startup.Repository.Name, typeof(HttpGlobalExceptionFilter));

        public void OnException(ExceptionContext context)
        {
            log.Error(context.Exception);
        }
    }
}
