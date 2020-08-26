using System;
using System.Data;
using DapperExtensions;
using System.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using System.Reflection;
using System.Text.RegularExpressions;
using BW.Dapper.Attribute;
using BW.Dapper.Page;
using DapperExtensions.Sql;  
using System.Text;
using System.Security.Cryptography;

namespace BW.Dapper
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">实体</typeparam>
    /// <typeparam name="TM">Mapper</typeparam>
    public class Repository<T, TM> where T : class
    {
        /// <summary>
        /// 默认主键排序
        /// </summary>
        public IEnumerable<Sort> ListSort;

        private readonly string _connectionString;
        private readonly DatabaseType _databaseType;
        private readonly int? _timeout = null;
        private Type _entityType = null;
        /// <summary>
        /// 数据库加密解密秘钥
        /// </summary>
        public const string DBKey = "BowWell.Com";

        /// <summary>
        /// 表名
        /// </summary>
        protected string TableName { get; private set; }

        /// <summary>
        /// 主键
        /// </summary>
        protected string KeyName { get; private set; }

        /// <summary>
        /// 数据库名称
        /// </summary>
        private string DatabaseName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Repository()
        {
            var temp = ConfigurationManager.ConnectionStrings["connectionString"];
            if (!string.IsNullOrEmpty(temp?.ConnectionString) && !string.IsNullOrEmpty(temp.ProviderName))
            {
                _connectionString = temp.ConnectionString;
                if (_connectionString.IndexOf("Server", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    _connectionString =ConnectionCust.DecryptParameter(_connectionString);
                }
                _databaseType = DbFactory.DatabaseTypeEnumParse(temp.ProviderName);
                GetDatabaseName(_connectionString);
            }
            else
            {
                throw new ArgumentNullException($"请检查配置文件中connectionString,与providerName");
            }
            _timeout = 60;
            GetDefault();        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="providerName"></param>
        /// <param name="timeout"></param>
        public Repository(string connectionString, string providerName, int? timeout = null)
        {
            _connectionString = connectionString;
            if (_connectionString.IndexOf("Server", StringComparison.OrdinalIgnoreCase) == -1)
            {
                _connectionString = ConnectionCust.DecryptParameter(_connectionString);
            }
            _databaseType = DbFactory.DatabaseTypeEnumParse(providerName);
            _timeout = timeout;
            GetDefault();
            GetDatabaseName(connectionString);
        }

        /// <summary>
        /// 
        /// </summary>
        ~Repository()
        {
            _entityType = null;
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// 获取数据库名称
        /// </summary>
        /// <param name="connectionString"></param>
        private void GetDatabaseName(string connectionString)
        {
            Regex regex = new Regex(@"(?<=Catalog=|Database=)\w*");
            var match = regex.Match(connectionString);
            DatabaseName = match.Value;
        }
        private void GetDefault()
        {
            try
            {
                DapperExtensions.DapperExtensions.SetMappingAssemblies(new[] { typeof(TM).Assembly });
                var info = DapperExtensions.DapperExtensions.GetMap<T>();
                if (info == null) return;
                TableName = info.TableName;
                _entityType = info.EntityType;
                var map = info.Properties
                    .FirstOrDefault(t => t.KeyType != DapperExtensions.Mapper.KeyType.NotAKey);
                if (map == null) return;
                KeyName = map.ColumnName;
                ListSort = new List<Sort>()
            {
                new Sort()
                {
                    PropertyName = KeyName,
                    Ascending = true
                }
            };  
            }
            catch { }
        }

        /// <summary>
        /// 是否是公司平台库
        /// </summary>
        /// <returns></returns>
        public bool IsCompanyDatabase()
        {
            if (string.IsNullOrEmpty(DatabaseName)) return false;
            return DatabaseName.Contains("Platform_U");
        }
        /// <summary>
        /// 创建事务
        /// </summary>
        /// <returns></returns>
        public virtual IDbTransaction BeginTractionand()
        {
            var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString);
            return con.BeginTransaction();
        }
        /// <summary>
        /// 创建连接对象，使用后记得释放
        /// </summary>
        public IDbConnection CreateConnection()
        {
            return DbFactory.CreateSqlConnection(_databaseType, _connectionString);
        }
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns> 
        public virtual dynamic Insert(T entity, IDbTransaction transaction = null)
        {
            if (transaction != null) return transaction.Connection.Insert<T>(entity, transaction, _timeout);
            using (var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString))
            {
                return con.Insert<T>(entity, null, _timeout);

            }
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <param name="transaction">事务</param>
        public virtual void Insert(IEnumerable<T> entities, IDbTransaction transaction = null)
        {
            if (transaction != null)
            {
                transaction.Connection.Insert<T>(entities, transaction, _timeout);
                return;
            }

            using (var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString))
            {
                con.Insert<T>(entities, null, _timeout);
            }
        }

        /// <summary>
        ///  更新实体
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public virtual bool Update(T entity, IDbTransaction transaction = null)
        {
            if (transaction != null) return transaction.Connection.Update<T>(entity, transaction, _timeout);
            using (var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString))
            {
                return con.Update<T>(entity, null, _timeout);
            }
        }
        /// <summary>
        /// 如果Key已存在，反之则更新
        /// </summary>
        /// <param name="key"></param>
        /// <param name="entity"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public virtual bool InsertOrUpdate(object key, T entity, IDbTransaction transaction = null)
        {
            var sql = $" select {KeyName} FROM {TableName} where {KeyName}=@Id";
            var info = QueryFirstOrDefault(sql, new {Id = key}, transaction);
            if (info != null) return Update(entity, transaction);
            var t = Insert(entity, transaction);
            return t != null;

        }

        /// <summary>
        ///  更新实体,此方法有问题，不能使用
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="entities">实体</param>
        /// <returns></returns>
        public virtual void BulkUpdate(string sql, IEnumerable<object> entities)
        {
            var list = entities.ToList();
            if (list.Count == 0)
            {
                return;
            }

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                using (var comm = con.CreateCommand())
                {
                    comm.CommandText = sql;
                    comm.CommandType = CommandType.Text;
                    var table = new DataTable();
                    var tmep = list[0];
                    Type t = tmep.GetType();
                    PropertyInfo[] pInfo = t.GetProperties();
                    foreach (var prop in pInfo)
                    {
                        var type = prop.PropertyType;
                        table.Columns.Add(prop.Name, type);
                        var sqlType = SqlDbType.NVarChar;
                        switch (type.FullName)
                        {
                            case "System.Int32":
                                sqlType = SqlDbType.Int;
                                break;
                            case "System.DateTime":
                                sqlType = SqlDbType.DateTime;
                                break;
                            case "System.Single":
                                sqlType = SqlDbType.Float;
                                break;
                            default:
                                sqlType = SqlDbType.NVarChar;
                                break;
                        }
                        //comm.Parameters.Add($"@{prop.Name}", sqlType);
                    }

                    var values = new object[pInfo.Count()];
                    foreach (var item in list)
                    {

                        for (var i = 0; i < values.Length; i++)
                        {
                            values[i] = pInfo[i].GetValue(item, null);
                        }

                        table.Rows.Add(values);
                    }

                    using (var adapter = new SqlDataAdapter(comm) {UpdateBatchSize = 4000})
                    {
                        adapter.Fill(table);
                        var rows = table.Rows;
                        foreach (DataRow row in rows)
                        {
                            row.BeginEdit();
                            //row["Name"]=s
                        }

                        adapter.Update(table);
                    }
                }
            }
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public virtual bool Delete(T entity, IDbTransaction transaction = null)
        {
            if (transaction != null) return transaction.Connection.Delete<T>(entity, transaction, _timeout);
            using (var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString))
            {
                return con.Delete<T>(entity, null, _timeout);
            }
        }

        /// <summary>
        /// 删除对象
        /// </summary>
        /// <param name="predicate">谓词</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public virtual bool Delete(object predicate = null, IDbTransaction transaction = null)
        {
            if (transaction != null) return transaction.Connection.Delete<T>(predicate, transaction, _timeout);
            using (var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString))
            {
                return con.Delete<T>(predicate, null, _timeout);
            }
        }

        /// <summary>
        /// 统计
        /// </summary>
        /// <param name="predicate">谓词</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public virtual int Count(object predicate, IDbTransaction transaction = null)
        {
            if (transaction != null) return transaction.Connection.Count<T>(predicate, transaction, _timeout);
            using (var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString))
            {
                return con.Count<T>(predicate, null, _timeout);
            }
        }

        /// <summary>
        /// 查找实体
        /// </summary>
        /// <param name="key">主键</param>
        /// <returns></returns>
        public virtual T GetByKey(dynamic key)
        {
            using (var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString))
            {
                return con.Get<T>(key as object, null, _timeout);
            }
        }

        /// <summary>
        /// 查找实体，返回满足条件地一个
        /// </summary>
        /// <param name="predicate">谓词</param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public virtual T FindSingle(object predicate, IEnumerable<Sort> sort = null)
        {
            var list = GetList(predicate, sort);
            var enumerable = list as T[] ?? list.ToArray();
            return !enumerable.Any() ? null : enumerable.First();
        }

        /// <summary>
        /// 获取实体集
        /// </summary>
        /// <param name="predicate">谓词</param>
        /// <param name="sort">排序</param>
        /// <param name="buffered"></param>
        /// <returns></returns>
        public virtual IEnumerable<T> GetList(object predicate, IEnumerable<Sort> sort = null, bool buffered = true)
        {
            using (var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString))
            {
                return con.GetList<T>(predicate, sort?.ToList<ISort>(), null, _timeout, buffered);
            }
        }

        /// <summary>
        /// 简单分页
        /// </summary>
        /// <param name="pageIndex">当前页序号</param>
        /// <param name="pageSize">页记录数</param>
        /// <param name="predicate">筛选</param>
        /// <param name="sort">排序</param>
        /// <param name="buffered"></param>
        /// <returns></returns>
        public virtual PageDataView<T> GetPageList(int pageIndex, int pageSize,
            IEnumerable<Sort> sort, object predicate = null, bool buffered = true)
        {
            var list = new PageDataView<T>()
            {
                PageIndex = pageIndex
            };
            using (var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString))
            {
                list.Total = con.Count<T>(predicate, null, _timeout);
                list.Items = con.GetPage<T>(predicate, sort?.ToList<ISort>(), pageIndex - 1, pageSize, null, _timeout,
                    buffered);
            }

            return list;
        }
        
        /// <summary>
        /// 高级分页，支持多表
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public virtual PageDataView<T> GetPageList(PageCriteria criteria, IDbTransaction transaction = null)
        {
            const string proName = "ProcGetPageData";
            if (this.CheckKeyWord(criteria.Sort)||this.CheckKeyWord(criteria.Condition))
            {
                throw new Exception("语句中存在,敏感字符。");
            }
            var p = new DynamicParameters();
            p.Add("TableName", criteria.TableName);
            p.Add("PrimaryKey", criteria.PrimaryKey);
            p.Add("Fields", criteria.Fields);
            p.Add("Condition", criteria.Condition);
            p.Add("CurrentPage", criteria.CurrentPage);
            p.Add("PageSize", criteria.PageSize);
            p.Add("Sort", criteria.Sort);
            p.Add("RecordCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            
            if (transaction != null)
            {
                var pageData = new PageDataView<T>
                {
                    Items = transaction.Connection.Query<T>(proName, p, transaction: transaction, commandType: CommandType.StoredProcedure).ToList(),
                    Total = p.Get<int>("RecordCount")
                };
                return pageData;
            }
            using (var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString))
            {
                var pageData = new PageDataView<T>
                {
                    Items = con.Query<T>(proName, p, transaction: transaction, commandType: CommandType.StoredProcedure).ToList(),
                    Total = p.Get<int>("RecordCount")
                };
                return pageData;
            }
        }
        /// <summary>
        ///  高效率分页，支持多表
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public virtual PageDataView<T> GetPageList(PageCriteriaLevel criteria, IDbTransaction transaction = null)
        {
            var sqlCount = $"select COUNT(0) from {criteria.TableName} ";
            var sb = new StringBuilder(" select ",500);
            sb.AppendLine(criteria.Fields);
            sb.AppendLine(" from ");
            sb.AppendLine(criteria.TableName);

            var condition = criteria.PredicateList.GetConditionStr(out DynamicParameters dynamicParameters);
            if (!string.IsNullOrWhiteSpace(condition))
            {
                var where = $"where {condition}";
                sb.AppendLine(where);
                sqlCount = $"{sqlCount}{where}";
            }            
            var  sort = criteria.Sort;
            if (sort == null || !sort.Any())
            {
                sort = ListSort;
            }
            sb.AppendLine($"ORDER BY {sort.GetSortStr()}");
            sb.AppendLine("	OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;");
            sb.AppendLine(sqlCount);
            var page = criteria.CurrentPage == 0 ? 0 : criteria.CurrentPage - 1;
            dynamicParameters.Add("Offset", page * criteria.PageSize);
            dynamicParameters.Add("PageSize", criteria.PageSize);

            if (transaction != null)
            {
                using (var multi = transaction.Connection.QueryMultiple(sb.ToString(), param: dynamicParameters, commandTimeout: _timeout, transaction: transaction))
                {
                    var pageData = new PageDataView<T>
                    {
                        Items = multi.Read<T>(),
                        Total = multi.ReadFirst<int>()
                    };
                    return pageData;
                }
            }
            using (var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString))
            {

                using (var multi = con.QueryMultiple(sb.ToString(), param: dynamicParameters, commandTimeout: _timeout))
                {
                    var pageData = new PageDataView<T>
                    {
                        Items = multi.Read<T>(),
                        Total = multi.ReadFirst<int>()
                    };
                    return pageData;
                }
            }

        }
        /// <summary>
        /// 高效率分页，支持多表
        /// </summary>
        /// <param name="sql">分页SQL语句</param>
        /// <param name="dynamicParameters">参数</param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public virtual PageDataView<T> GetPageList(string sql, DynamicParameters dynamicParameters, IDbTransaction transaction = null)
        {

            if (transaction != null)
            {
                using (var multi = transaction.Connection.QueryMultiple(sql, param: dynamicParameters, transaction: transaction))
                {
                    var pageData = new PageDataView<T>
                    {
                        Items = multi.Read<T>(),
                        Total = multi.ReadFirst<int>()
                    };
                    return pageData;
                }
            }
            using (var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString))
            {

                using (var multi = con.QueryMultiple(sql, param: dynamicParameters,commandTimeout: _timeout))
                {
                    var pageData = new PageDataView<T>
                    {
                        Items = multi.Read<T>(),
                        Total = multi.ReadFirst<int>()
                    };
                    return pageData;
                }
            }
        }
        /// <summary>
        /// 高效率分页，支持多表
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public virtual PageDataView<dynamic> GetPageList(string sql, object param = null)
        {
            using (var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString))
            {

                using (var multi = con.QueryMultiple(sql, param, commandTimeout: _timeout))
                {
                    var pageData = new PageDataView<dynamic>
                    {
                        Items = multi.Read<dynamic>(),
                        Total = multi.ReadFirst<int>()
                    };
                    return pageData;
                }
            }
        }
        /// <summary>
        /// 高效率分页，支持多表
        /// </summary>
        /// <param name="pageIndex">当前页序号</param>
        /// <param name="pageSize">页记录数</param>
        /// <param name="predicateList">查询条件</param>
        /// <param name="sort">排序</param>
        /// <returns></returns>
        public virtual PageDataView<T> GetPageList(int pageIndex, int pageSize, IList<Predicate> predicateList = null,
           IEnumerable<Sort> sort = null)
        {
            return GetPageList(new PageCriteriaLevel
            {
                TableName = $"{TableName}",
                Fields = "*",
                CurrentPage = pageIndex,
                PageSize = pageSize,
                PredicateList = predicateList,
                Sort = sort
            });
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="parameters">参数</param>
        /// <example>
        /// 获取输出参数值方式： procedureName.Get("XX")
        /// </example> 
        /// <returns></returns>
        public virtual IEnumerable<T> ExeProcedure(string procedureName, DynamicParameters parameters)
        {
            using (var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString))
            {
                return con.Query<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="entity">实体</param>
        /// <param name="transaction">事物</param>
        /// <returns>返回受影响行数</returns>
        public virtual int Execute(string sql, T entity, IDbTransaction transaction = null)
        {
            if (transaction != null)
                return transaction.Connection.Execute(sql, entity, transaction, _timeout, CommandType.Text);
            using (var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString))
            {
                return con.Execute(sql, entity, null, _timeout, CommandType.Text);
            }
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="entities">实体</param>
        /// <param name="transaction">事物</param>
        /// <returns>返回受影响行数</returns>
        public virtual int Execute(string sql, IEnumerable<T> entities, IDbTransaction transaction = null)
        {
            if (transaction != null)
                return transaction.Connection.Execute(sql, entities, transaction, _timeout, CommandType.Text);
            using (var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString))
            {
                return con.Execute(sql, entities, null, _timeout, CommandType.Text);
            }
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="transaction">事物</param>
        /// <returns>返回受影响行数</returns>
        public virtual int Execute(string sql, IDbTransaction transaction = null)
        {
            if (transaction != null)
                return transaction.Connection.Execute(sql, null, transaction, _timeout, CommandType.Text);
            using (var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString))
            {
                return con.Execute(sql, null, null, _timeout, CommandType.Text);
            }
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="param"></param>
        /// <param name="transaction">事物</param>
        /// <returns>返回受影响行数</returns>
        public virtual int Execute(string sql, object param, IDbTransaction transaction = null)
        {
            if (transaction != null)
                return transaction.Connection.Execute(sql, param, transaction, _timeout, CommandType.Text);
            using (var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString))
            {
                return con.Execute(sql, param, null, _timeout, CommandType.Text);
            }
        }

        /// <summary>
        /// 批量执行SQL语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="entities"></param>
        /// <param name="transaction">事物</param>
        /// <returns>返回受影响行数</returns>
        public virtual int Execute(string sql, IEnumerable<object> entities, IDbTransaction transaction = null)
        {
            var parameters = new List<DynamicParameters>();
            foreach (var item in entities)
            {
                Type t = item.GetType();
                PropertyInfo[] pInfo = t.GetProperties();
                var dynamicParameters = new DynamicParameters();
                // 遍历公共属性
                foreach (PropertyInfo prop in pInfo)
                {
                    dynamicParameters.Add(prop.Name, prop.GetValue(item, null));
                }

                parameters.Add(dynamicParameters);
            }

            if (transaction != null)
            {
                return transaction.Connection.Execute(sql, parameters, transaction, _timeout, CommandType.Text);
            }
            using (var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString))
            {
                return con.Execute(sql, parameters, null, _timeout, CommandType.Text);
            }
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public virtual IEnumerable<T> Query(string sql, object param = null)
        {
            using (var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString))
            {
                return con.Query<T>(sql, param, null, true, _timeout);
            }
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public virtual IEnumerable<dynamic> QueryDynamic(string sql, object param = null)
        {
            using (var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString))
            {
                return con.Query<dynamic>(sql, param, null, true, _timeout);
            }
        }

        /// <summary>
        /// 查询第一个
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public virtual T QueryFirstOrDefault(string sql, object param = null, IDbTransaction transaction = null)
        {
            if (transaction != null)
                return transaction.Connection.QueryFirstOrDefault<T>(sql, param, transaction, _timeout,
                    CommandType.Text);
            using (var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString))
            {
                return con.QueryFirstOrDefault<T>(sql, param, null, _timeout);
            }
        }

        /// <summary>
        /// 执行查询语句,返回第一个单元格
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns>第一个单元格</returns>
        public virtual object ExecuteScalar(string sql, object param = null)
        {
            using (var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString))
            {
                return con.ExecuteScalar(sql, param, null, _timeout);
            }
        }



        /// <summary>
        /// 批量插入功能,只支持SQL Server
        /// </summary>
        /// <remarks>
        /// 【MrLi】【2019-9-25 15:07:24】【修改】：去掉了代码中事物的提交和回滚逻辑
        /// </remarks>
        /// <param name="entityList"></param>
        /// <param name="transaction"></param>
        public virtual void InsertBatch(IEnumerable<T> entityList, SqlTransaction transaction = null)
        {
            SqlConnection conn = null;
            if (transaction == null)
            {
                conn = new SqlConnection(_connectionString);
                conn.Open();
            }
            else
            {
                conn = transaction.Connection;
            }
            SqlBulkCopy bulkCopy = null;
            if (transaction != null)
            {
                bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, transaction);
            }
            else
            {
                bulkCopy = new SqlBulkCopy(conn);
            }
            bulkCopy.BatchSize = entityList.Count();
            bulkCopy.DestinationTableName = TableName;
            var table = new DataTable();
            ISqlGenerator sqlGenerator = new SqlGeneratorImpl(new DapperExtensionsConfiguration());
            var classMap = sqlGenerator.Configuration.GetMap<T>();
            var props = classMap.Properties.Where(x => x.Ignored == false).ToArray();
            foreach (var propertyInfo in props)
            {
                bulkCopy.ColumnMappings.Add(propertyInfo.Name, propertyInfo.Name);
                table.Columns.Add(propertyInfo.Name,
                    Nullable.GetUnderlyingType(propertyInfo.PropertyInfo.PropertyType) ??
                    propertyInfo.PropertyInfo.PropertyType);
            }
            var values = new object[props.Count()];
            foreach (var itemm in entityList)
            {
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].PropertyInfo.GetValue(itemm, null);
                }

                table.Rows.Add(values);
            }
            bulkCopy.WriteToServer(table);
            table = null;
            values = null;
            props = null;
            classMap = null;

            bulkCopy.Close();
            bulkCopy = null;
            if (transaction == null)
            {
                conn.Close();
                conn.Dispose();
            }
        }

        /// <summary>
        /// 执行参数化的SQL并返回一个DataTable
        /// </summary>
        /// <param name="sql">sql语句或过程名称</param>
        /// <param name="param">参数</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public virtual DataTable ExecuteReader(string sql, object param = null, CommandType type = CommandType.Text)
        {
            var dt = new DataTable();
            using (var con = DbFactory.CreateSqlConnection(_databaseType, _connectionString))
            {
                dt.Load(con.ExecuteReader(sql, param, null, _timeout, type));
                return dt;
            }
        }

        /// <summary>
        /// 判断数据类型
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual bool IsNormal(Predicate item)
        {
            item.ColumnType = "";
            var props = _entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var reg = new Regex(@"[^\.][a-zA-Z0-9_]+");
            var columnItem = reg.Match(item.ColumnItem).Value;
            var infos = props.FirstOrDefault(
                t => t.Name.Equals(columnItem, StringComparison.CurrentCultureIgnoreCase));
            if (infos == null)
            {
                return false;
            }

            var nullableType = Nullable.GetUnderlyingType(infos.PropertyType);
            var isNullableType = nullableType != null;
            item.ColumnType = isNullableType ? nullableType.Name : infos.PropertyType.Name;

            return item.IsNormal();
        }

        /// <summary>
        /// 获取需要查询字段
        /// </summary>
        /// <returns></returns>
        public virtual List<Coumn> GetQueryField()
        {
            var props = _entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            return (from pi in props
                    let columns = pi.GetCustomAttributes(typeof(ColumnAttribute), false)
                    where columns.Length == 1
                    let attribute = columns[0] as ColumnAttribute
                    where attribute != null
                    let nullableType = Nullable.GetUnderlyingType(pi.PropertyType)
                    let isNullableType = nullableType != null
                    let column = attribute
                    select new Coumn()
                    {
                        Name = column.Description,
                        Value = column.FieldName,
                        Type = string.IsNullOrEmpty(column.Type) ? (isNullableType ? nullableType.Name : pi.PropertyType.Name) : column.Type,
                        Seq = column.Seq
                    }).OrderBy(t => t.Seq).ToList();
        }

        /// <summary>
        /// 数据库是否能连接成功
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="providerName">数据库类型</param>
        /// <param name="timeOut">连接超时时间（单位：秒）</param>
        /// <returns></returns>
        public virtual bool ConnectAble(string connectionString, int timeOut = 2,
            string providerName = "System.Data.SqlClient")
        {
            var databaseType = DbFactory.DatabaseTypeEnumParse(providerName);
            return DbFactory.ConnectAble(databaseType, connectionString, timeOut);
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">任意类型</typeparam>
    public  class Repository<T> where T : class
    {
        /// <summary>
        /// 数据库加密解密秘钥
        /// </summary>
        public const string DBKey = "BowWell.Com";

        /// <summary>
        /// 创建连接字符串
        /// </summary>
        /// <returns></returns>
        public static IDbConnection CreateConnection()
        {
            var temp = ConfigurationManager.ConnectionStrings["connectionString"];
            if (!string.IsNullOrEmpty(temp?.ConnectionString) && !string.IsNullOrEmpty(temp.ProviderName))
            {
                var connectionString = temp.ConnectionString;
                if (connectionString.IndexOf("Server", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    connectionString = ConnectionCust.DecryptParameter(connectionString); 
                }
                var databaseType = DbFactory.DatabaseTypeEnumParse(temp.ProviderName);
                return DbFactory.CreateSqlConnection(databaseType, connectionString);
            }
            else
            {
                throw new ArgumentNullException($"请检查配置文件中connectionString,与providerName");
            }
        }

        /// <summary>
        /// 创建连接字符串
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="providerName">数据库类型</param>
        /// <returns></returns>
        public static IDbConnection CreateConnection(string connectionString,
            string providerName = "System.Data.SqlClient")
        {

            if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(providerName))
            {
                var databaseType = DbFactory.DatabaseTypeEnumParse(providerName);
                return DbFactory.CreateSqlConnection(databaseType, connectionString);
            }
            else
            {
                throw new ArgumentNullException($"请检查connectionString,与providerName");
            }
        }


        /// <summary>
        /// 创建事务
        /// </summary>
        /// <param name="cnConnection"></param>
        /// <returns></returns>
        public static IDbTransaction BeginTractionand(IDbConnection cnConnection = null)
        {
            return cnConnection == null ? CreateConnection().BeginTransaction() : cnConnection.BeginTransaction();
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="parameters">参数</param>
        /// <param name="cnConnection">连接对象</param>
        /// <example>
        /// 获取输出参数值方式： procedureName.Get("XX")
        /// </example> 
        /// <returns></returns>
        public static IEnumerable<T> ExeProcedure(string procedureName, DynamicParameters parameters,
            IDbConnection cnConnection = null)
        {
            try
            {
                if (cnConnection == null)
                {
                    cnConnection = CreateConnection();
                }
                return cnConnection.Query<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);
            }
            finally
            {
                if (cnConnection != null)
                {
                    cnConnection.Close();
                    cnConnection.Dispose();
                }
            }
            
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="entity">实体</param>
        /// <param name="cnConnection">连接对象</param>
        /// <param name="transaction">事物</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>返回受影响行数</returns>
        public static int Execute(string sql, T entity, IDbConnection cnConnection = null,
            IDbTransaction transaction = null, int timeout = 60)
        {
            try
            {
                if (cnConnection == null && transaction == null)
                {
                    cnConnection = CreateConnection();
                }

                return transaction?.Connection.Execute(sql, entity, transaction, timeout, CommandType.Text) ??
                       cnConnection.Execute(sql, entity, null, timeout, CommandType.Text);
            }
            finally
            {
                if (cnConnection != null)
                {
                    cnConnection.Close();
                    cnConnection.Dispose();
                }
            }
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="entities">实体</param>
        /// <param name="cnConnection">连接对象</param>
        /// <param name="transaction">事物</param>
        /// <param name="timeout"></param>
        /// <returns>返回受影响行数</returns>
        public static int Execute(string sql, IEnumerable<T> entities, IDbConnection cnConnection = null,
            IDbTransaction transaction = null, int timeout = 60)
        {

            try
            {
                if (cnConnection == null && transaction == null)
                {
                    cnConnection = CreateConnection();
                }

                return transaction?.Connection.Execute(sql, entities, transaction, timeout, CommandType.Text) ??
                       cnConnection.Execute(sql, entities, null, timeout, CommandType.Text);
            }
            finally
            {
                if (cnConnection != null)
                {
                    cnConnection.Close();
                    cnConnection.Dispose();
                }
            }
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="cnConnection">连接对象</param>
        /// <param name="transaction">事物</param>
        /// <param name="timeout"></param>
        /// <returns>返回受影响行数</returns>
        public static int Execute(string sql, IDbConnection cnConnection = null, IDbTransaction transaction = null,
            int timeout = 60)
        {
            try
            {
                return transaction?.Connection.Execute(sql, null, transaction, timeout, CommandType.Text) ??
                       cnConnection.Execute(sql, null, null, timeout, CommandType.Text);
            }
            finally
            {
                if (cnConnection != null)
                {
                    cnConnection.Close();
                    cnConnection.Dispose();
                }
            }
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="param">参数</param>
        /// <param name="transaction">事物</param>
        /// <param name="cnConnection">连接对象></param>
        /// <param name="timeout">超时时间</param>
        /// <returns>返回受影响行数</returns>
        public static int Execute(string sql, object param, IDbTransaction transaction = null,
            IDbConnection cnConnection = null,
            int timeout = 60)
        {
            try
            {
                return transaction?.Connection.Execute(sql, param, transaction, timeout, CommandType.Text) ?? cnConnection.Execute(sql, param, null, timeout, CommandType.Text);
            }
            finally
            {
                if (cnConnection != null)
                {
                    cnConnection.Close();
                    cnConnection.Dispose();
                }
            }
            
        }

        /// <summary>
        /// 批量执行SQL语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="entities"></param>
        /// <param name="transaction">事物</param>
        /// <param name="cnConnection"></param>
        /// <param name="timeout"></param>
        /// <returns>返回受影响行数</returns>
        public static int Execute(string sql, IEnumerable<object> entities, IDbTransaction transaction = null,
            IDbConnection cnConnection = null,
            int timeout = 60)
        {
            try
            {
                var parameters = new List<DynamicParameters>();
                foreach (var item in entities)
                {
                    var t = item.GetType();
                    var pInfo = t.GetProperties();
                    var dynamicParameters = new DynamicParameters();
                    // 遍历公共属性
                    foreach (var prop in pInfo)
                    {
                        dynamicParameters.Add(prop.Name, prop.GetValue(item, null));
                    }

                    parameters.Add(dynamicParameters);
                }

                return transaction?.Connection.Execute(sql, parameters, transaction, timeout, CommandType.Text) ??
                       cnConnection.Execute(sql, parameters, null, timeout, CommandType.Text);
            }
            finally
            {
                if (cnConnection != null)
                {
                    cnConnection.Close();
                    cnConnection.Dispose();
                }
            }

        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="cnConnection">连接对象</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        public static IEnumerable<T> Query(string sql, object param = null, IDbConnection cnConnection = null,
            int timeout = 120)
        {
            try
            {
                if (cnConnection == null)
                {
                    cnConnection = CreateConnection();
                }

                return cnConnection.Query<T>(sql, param, null, true, timeout);
            }
            finally
            {
                if (cnConnection != null)
                {
                    cnConnection.Close();
                    cnConnection.Dispose();
                }
            }
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="cnConnection">连接对象</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        public static IEnumerable<dynamic> QueryDynamic(string sql, object param = null,
            IDbConnection cnConnection = null,
            int timeout = 60)
        {
            try
            {
                if (cnConnection == null)
                {
                    cnConnection = CreateConnection();
                }

                return cnConnection.Query<dynamic>(sql, param, null, true, timeout);
            }
            finally
            {
                if (cnConnection != null)
                {
                    cnConnection.Close();
                    cnConnection.Dispose();
                }
            }
        }

        /// <summary>
        /// 查询返回结果第一个
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param">参数</param>
        /// <param name="cnConnection">连接对象</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        public static T QueryFirstOrDefault(string sql, object param = null, IDbConnection cnConnection = null,
            int timeout = 60)
        {
            try
            {
                if (cnConnection == null)
                {
                    cnConnection = CreateConnection();
                }

                return cnConnection.QueryFirstOrDefault<T>(sql, param, null, timeout);
            }
            finally
            {
                if (cnConnection != null)
                {
                    cnConnection.Close();
                    cnConnection.Dispose();
                }
            }
        }

        /// <summary>
        /// 执行查询语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param">参数</param>
        /// <param name="cnConnection">连接对象</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>第一个单元格</returns>
        public static object ExecuteScalar(string sql, object param = null, IDbConnection cnConnection = null,
            int timeout = 60)
        {
            try
            {
                if (cnConnection == null)
                {
                    cnConnection = CreateConnection();
                }

                return cnConnection.ExecuteScalar<T>(sql, param, null, timeout);
            }
            finally
            {
                if (cnConnection != null)
                {
                    cnConnection.Close();
                    cnConnection.Dispose();
                }
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public static class RepositoryExtend
    {

        /// <summary>
        ///  检查参数是否包含SQL关键字
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TM"></typeparam>
        /// <param name="value"></param>
        /// <param name="param"></param>
        public static bool CheckKeyWord<T, TM>(this Repository<T, TM> value, string param) where T : class
        {
            return SQLInjection.CheckKeyWord(param);
        }
        /// <summary>
        ///  检查参数是否包含SQL关键字
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public static bool CheckKeyWord<T>(this Repository<T> value, string param) where T : class
        {
            return SQLInjection.CheckKeyWord(param);
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public static class ConnectionCust
    {
        static ConnectionStringSettings stringSettingsCollection = ConfigurationManager.ConnectionStrings["connectionString"];

        /// <summary>
        /// 数据库加密解密秘钥
        /// </summary>
        public const string DBKey = "BowWell.Com";

        /// <summary>
        /// 获取字符串
        /// </summary>
        /// <returns></returns>
        public static string GetConnectionString()
        {
            if (!string.IsNullOrEmpty(stringSettingsCollection?.ConnectionString) && !string.IsNullOrEmpty(stringSettingsCollection.ProviderName))
            {
                var _connectionString = stringSettingsCollection.ConnectionString;
                if (_connectionString.IndexOf("Server", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    _connectionString = DecryptParameter(_connectionString);
                }
                return _connectionString;
            }
            else
            {
                throw new ArgumentNullException($"请检查配置文件中connectionString,与providerName");
            }
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string DecryptParameter(string str)
        {
            try
            {
                var key = string.Join("", DBKey.Reverse());
                key = key.Length > 8 ? key.Substring(0, 8) : key;
                var keyBytes = Encoding.UTF8.GetBytes(key);
                var provider = new DESCryptoServiceProvider
                {
                    Key = keyBytes,
                    IV = keyBytes
                };
                byte[] buffer = new byte[str.Length / 2];
                for (int i = 0; i < (str.Length / 2); i++)
                {
                    int num2 = Convert.ToInt32(str.Substring(i * 2, 2), 0x10);
                    buffer[i] = (byte)num2;
                }
                using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
                {

                    using (var stream2 = new CryptoStream(stream, provider.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        stream2.Write(buffer, 0, buffer.Length);
                        stream2.FlushFinalBlock();
                        stream.Close();
                        return Encoding.UTF8.GetString(stream.ToArray());
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EncryptParameter(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            var builder = new StringBuilder();
            try
            {

                var key = string.Join("", DBKey.Reverse());
                key = key.Length > 8 ? key.Substring(0, 8) : key;
                var keyBytes = Encoding.UTF8.GetBytes(key);
                DESCryptoServiceProvider provider = new DESCryptoServiceProvider
                {
                    Key = keyBytes,
                    IV = keyBytes
                };
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
                {

                    using (CryptoStream stream2 = new CryptoStream(stream, provider.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        stream2.Write(bytes, 0, bytes.Length);
                        stream2.FlushFinalBlock();
                        foreach (byte num in stream.ToArray())
                        {
                            builder.AppendFormat("{0:X2}", num);
                        }
                        stream.Close();
                        return builder.ToString();
                    }
                }
            }
            catch
            {
                return null;
            }

        }
    }
}
