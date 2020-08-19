using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

/// <summary>
/// 返回结果中间件
/// </summary>
public class WebApiResultMiddleware : ActionFilterAttribute
{
    /// <summary>
    /// 重写结果返回事件
    /// </summary>
    /// <param name="context"></param>
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        //根据实际需求进行具体实现
        if (context.Result is ObjectResult)
        {
            var objectResult = context.Result as ObjectResult;
            if (objectResult.Value == null)
            {
                context.Result = new ObjectResult(new { code = HttpStatusCode.BadRequest, sub_msg = "未找到资源", msg = "" });
            }
            else
            {
                context.Result = new ObjectResult(new { code = HttpStatusCode.OK, msg = "", result = objectResult.Value });
            }
        }
        else if (context.Result is EmptyResult)
        {
            context.Result = new ObjectResult(new { code = HttpStatusCode.NotFound, sub_msg = "未找到资源", msg = "" });
        }
        else if (context.Result is ContentResult)
        {
            context.Result = new ObjectResult(new { code = HttpStatusCode.OK, msg = "", result = (context.Result as ContentResult).Content });
        }
        else if (context.Result is StatusCodeResult)
        {
            context.Result = new ObjectResult(new { code = (context.Result as StatusCodeResult).StatusCode, sub_msg = "", msg = "" });
        }
    }
}

