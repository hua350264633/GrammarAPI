using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GrammarAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        /// <summary>
        /// 日志对象
        /// </summary>
        private readonly ILog log = LogManager.GetLogger(Startup.Repository.Name, typeof(WeatherForecastController));

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        
        [HttpGet]
        public IEnumerable<WeatherForecast> Get(int id, string name)
        {
            try
            {
                //throw new Exception("牛腩自定义异常！！！");
                log.Debug("test");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //_logger.LogInformation($"id：{id}，name：{name}");
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
