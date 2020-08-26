using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ML.Dapper;
using ML.Dapper.Base;
using ML.Dapper.Models;

namespace GrammarAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IDapperFactory DapperFactory;

        public UserController(IDapperFactory dapperFactory)
        {
            DapperFactory = dapperFactory;
        }

        [HttpGet]
        public object Get()
        {
            var list = new List<dynamic>();
            var _SqlDB = DapperFactory.CreateClient(EnumDBType.SqlServer);
            list.AddRange(_SqlDB.Query<dynamic>(@"select top 100 * from t_acl_user"));
            return list;
        }
    }
}