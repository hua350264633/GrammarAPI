using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ML.Dapper;
using ML.Dapper.Base;

namespace GrammarAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DapperClient _SqlDB;
        public UserController(IDapperFactory dapperFactory)
        {
            _SqlDB = dapperFactory.CreateClient(EnumDbStoreType.SqlServer);
        }

        [HttpGet]
        public object Get()
        {
            //var testQuery = _OracleDB.Query<dynamic>(@"SELECT * FROM BASE_DEPT where ROWNUM<=5");

            var result = _SqlDB.Query<dynamic>(@"select top 10 * from t_acl_user");

            //return new Result<object>() { data = result };
            return result;
        }

    }

}