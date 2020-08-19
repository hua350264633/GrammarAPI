using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

            //捕获全局异常，并格式化错误内容到前端
            context.Result = new ObjectResult(
                new
                {
                    code = HttpStatusCode.InternalServerError,
                    msg = $"异常内容：{context.Exception.Message}",
                    result = "服务器错误"
                });
        }
    }
}
