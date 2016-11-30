using Dapper;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TangChao.JiZhi.Pos.DAL
{
    public abstract partial class Database<TDatabase> : IDisposable where TDatabase : Database<TDatabase>, new()
    {
        public partial class Table<T, TId>
        {
            internal Database<TDatabase> database;
            internal string tableName;
            internal string likelyTableName;

            public Table(Database<TDatabase> database, string likelyTableName)
            {
                this.database = database;
                this.likelyTableName = likelyTableName;
            }

            public string TableName
            {
                get
                {
                    tableName = tableName ?? OperationCache.DetermineTableName<T>(likelyTableName);
                    return tableName;
                }
            }

            #region 表操作
            /// <summary>
            /// Insert a row into the db
            /// </summary>
            /// <param name="data">Either DynamicParameters or an anonymous type or concrete type</param>
            /// <returns></returns>
            public virtual int Insert(dynamic data)
            {
                //this.database.BeginTransaction();

                var o = (object)data;
                database.InitializeModelData(o, OperationType.Insert);
                List<string> paramNames = OperationCache.GetParamNames(o);
                //paramNames.Remove("Id");

                string cols = string.Join(",", paramNames);
                string cols_params = string.Join(",", paramNames.Select(p => "@" + p));
                //var sql = "set nocount on insert " + TableName + " (" + cols + ") values (" + cols_params + ") select cast(scope_identity() as int)";
                var sql = "insert into" + TableName + " (" + cols + ") values (" + cols_params + ")";

                return database.Execute(sql, o);
            }
            #endregion

            /// <summary>
            /// Update a record in the DB
            /// </summary>
            /// <param name="id"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            public int Update(TId id, dynamic data)
            {
                database.InitializeModelData((object)data, OperationType.Update);
                List<string> paramNames = OperationCache.GetParamNames((object)data);

                var builder = new StringBuilder();
                builder.Append("update ").Append(TableName).Append(" set ");
                builder.AppendLine(string.Join(",", paramNames.Where(n => n != "Id").Select(p => p + "= @" + p)));
                builder.Append("where Id = @Id");

                DynamicParameters parameters = new DynamicParameters(data);
                parameters.Add("Id", id);

                return database.Execute(builder.ToString(), parameters);
            }

            /// <summary>
            /// Delete a record for the DB
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            public bool Delete(TId id)
            {
                return database.Execute("delete from " + TableName + " where Id = @id", new { id }) > 0;
            }

            /// <summary>
            /// Grab a record with a particular Id from the DB 
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            public T Get(TId id)
            {
                return database.Query<T>("select * from " + TableName + " where Id = @id", new { id }).FirstOrDefault();
            }

            public virtual T First()
            {
                return database.Query<T>("select top 1 * from " + TableName).FirstOrDefault();
            }

            public IEnumerable<T> All()
            {
                return database.Query<T>("select * from " + TableName);
            }
        }

        public class Table<T> : Table<T, string>
        {
            public Table(Database<TDatabase> database, string likelyTableName)
                : base(database, likelyTableName)
            {
            }
        }

        DbConnection connection;
        int commandTimeout;
        DbTransaction transaction;


        public static TDatabase Init(DbConnection connection, int commandTimeout)
        {
            TDatabase db = new TDatabase();
            db.InitDatabase(connection, commandTimeout);
            return db;
        }

        internal static Action<TDatabase> tableConstructor;

        internal void InitDatabase(DbConnection connection, int commandTimeout)
        {
            this.connection = connection;
            this.commandTimeout = commandTimeout;
            if (tableConstructor == null)
            {
                tableConstructor = CreateTableConstructorForTable();
            }

            tableConstructor(this as TDatabase);
        }

        internal virtual Action<TDatabase> CreateTableConstructorForTable()
        {
            return CreateTableConstructor(typeof(Table<>));
        }

        #region 事务
        public void BeginTransaction(IsolationLevel isolation = IsolationLevel.ReadCommitted)
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
            transaction = connection.BeginTransaction(isolation);
        }

        public void CommitTransaction()
        {
            transaction.Commit();
            transaction = null;
        }

        public void RollbackTransaction()
        {
            transaction.Rollback();
            transaction = null;
        }
        #endregion

        protected Action<TDatabase> CreateTableConstructor(Type tableType)
        {
            var dm = new DynamicMethod("ConstructInstances", null, new Type[] { typeof(TDatabase) }, true);
            var il = dm.GetILGenerator();

            var setters = GetType().GetProperties()
                .Where(p => p.PropertyType.IsGenericParameter && p.PropertyType.GetGenericTypeDefinition() == tableType)
                .Select(p => Tuple.Create(
                        p.GetSetMethod(true),
                        p.PropertyType.GetConstructor(new Type[] { typeof(TDatabase), typeof(string) }),
                        p.Name,
                        p.DeclaringType
                 ));

            foreach (var setter in setters)
            {
                il.Emit(OpCodes.Ldarg_0);
                // [db]

                il.Emit(OpCodes.Ldstr, setter.Item3);
                // [db, likelyname]

                il.Emit(OpCodes.Newobj, setter.Item2);
                // [table]

                var table = il.DeclareLocal(setter.Item2.DeclaringType);
                il.Emit(OpCodes.Stloc, table);
                // []

                il.Emit(OpCodes.Ldarg_0);
                // [db]

                il.Emit(OpCodes.Castclass, setter.Item4);
                // [db cast to container]

                il.Emit(OpCodes.Ldloc, table);
                // [db cast to container, table]

                il.Emit(OpCodes.Callvirt, setter.Item1);
                // []
            }

            il.Emit(OpCodes.Ret);
            return (Action<TDatabase>)dm.CreateDelegate(typeof(Action<TDatabase>));
        }



        private bool TableExists(string name)
        {
            string schemaName = null;

            name = name.Replace("[", "");
            name = name.Replace("]", "");

            if (name.Contains("."))
            {
                var parts = name.Split('.');
                if (parts.Count() == 2)
                {
                    schemaName = parts[0];
                    name = parts[1];
                }
            }

            var builder = new StringBuilder("select 1 from INFORMATION_SCHEMA.TABLES where ");
            if (!String.IsNullOrEmpty(schemaName)) builder.Append("TABLE_SCHEMA = @schemaName AND ");
            builder.Append("TABLE_NAME = @name");

            return connection.Query(builder.ToString(), new { schemaName, name }, transaction: transaction).Count() == 1;
        }

        #region 基本查询
        public int Execute(string sql, dynamic param = null)
        {
            return SqlMapper.Execute(connection, sql, param as object, transaction, commandTimeout: this.commandTimeout);
        }

        public T ExecuteScalar<T>(string sql, dynamic param = null)
        {
            return SqlMapper.ExecuteScalar<T>(connection, sql, param as object, transaction, commandTimeout: this.commandTimeout);
        }

        public IEnumerable<T> Query<T>(string sql, dynamic param = null, bool buffered = true)
        {
            return SqlMapper.Query<T>(connection, sql, param as object, transaction, buffered, commandTimeout);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null)
        {
            return SqlMapper.Query(connection, sql, map, param as object, transaction, buffered, splitOn);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null)
        {
            return SqlMapper.Query(connection, sql, map, param as object, transaction, buffered, splitOn);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null)
        {
            return SqlMapper.Query(connection, sql, map, param as object, transaction, buffered, splitOn);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null)
        {
            return SqlMapper.Query(connection, sql, map, param as object, transaction, buffered, splitOn);
        }

        public IEnumerable<dynamic> Query(string sql, dynamic param = null, bool buffered = true)
        {
            return SqlMapper.Query(connection, sql, param as object, transaction, buffered);
        }

        public Dapper.SqlMapper.GridReader QueryMultiple(string sql, dynamic param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return SqlMapper.QueryMultiple(connection, sql, param, transaction, commandTimeout, commandType);
        }

        #endregion

        #region 表操作

        /// <summary>
        /// Insert a row into the db
        /// </summary>
        /// <param name="data">Either DynamicParameters or an anonymous type or concrete type</param>
        /// <returns></returns>
        public virtual int Insert<T>(dynamic data)
        {
            //this.database.BeginTransaction();

            var o = (object)data;
            InitializeModelData(o, OperationType.Insert);

            List<string> paramNames = OperationCache.GetParamNames(o);
            //paramNames.Remove("Id");
            string tableName = OperationCache.DetermineTableName<T>();
            string cols = string.Join(",", paramNames);
            string cols_params = string.Join(",", paramNames.Select(p => "@" + p));
            var sql = "insert into" + tableName + " (" + cols + ") values (" + cols_params + ")";

            return this.Execute(sql, o);
        }

        /// <summary>
        /// Update a record in the DB
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public int Update<T>(string id, dynamic data)
        {
            InitializeModelData((object)data, OperationType.Update);
            List<string> paramNames = OperationCache.GetParamNames((object)data);
            string tableName = OperationCache.DetermineTableName<T>();
            var builder = new StringBuilder();
            builder.Append("update ").Append(tableName).Append(" set ");
            builder.AppendLine(string.Join(",", paramNames.Where(n => n != "Id").Select(p => p + "= @" + p)));
            builder.Append("where Id = @Id");

            DynamicParameters parameters = new DynamicParameters(data);
            parameters.Add("Id", id);

            return this.Execute(builder.ToString(), parameters);
        }

        /// <summary>
        /// Delete a record for the DB
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete<T>(string id)
        {
            string tableName = OperationCache.DetermineTableName<T>();
            return this.Execute("delete from " + tableName + " where Id = @id", new { id }) > 0;
        }

        /// <summary>
        /// Grab a record with a particular Id from the DB 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T Get<T>(string id)
        {
            string tableName = OperationCache.DetermineTableName<T>();
            return this.Query<T>("select * from " + tableName + " where Id = @id", new { id }).FirstOrDefault();
        }

        /// <summary>
        /// 获取数量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public int Count<T>()
        {
            string tableName = OperationCache.DetermineTableName<T>();
            return this.ExecuteScalar<int>("select count(*) from " + tableName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T First<T>()
        {
            string tableName = OperationCache.DetermineTableName<T>();
            return this.Query<T>("select top 1 * from " + tableName).FirstOrDefault();
        }

        public IEnumerable<T> All<T>()
        {
            string tableName = OperationCache.DetermineTableName<T>();
            return this.Query<T>("select * from " + tableName);
        }

        #endregion

        internal enum OperationType
        {
            Insert = 0,
            Update,
            Delete,
            Select
        }

        public void Dispose()
        {
            if (connection.State != ConnectionState.Closed)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }

                connection.Close();
                connection = null;
            }
        }

        internal void InitializeModelData(object o, OperationType operationType)
        {
            //if (operationType == OperationType.Insert)
            //{
            //    if (o is IEnumerable)
            //    {
            //        var temp = o as IEnumerable;
            //        foreach (var item in temp)
            //        {
            //            if (item is IHasCreationTime)
            //            {
            //                var itemHasCreationTime = item as IHasCreationTime;
            //                itemHasCreationTime.CreateTime = DateTime.Now;
            //            }
            //            if (item is IHasUpdateTime)
            //            {
            //                var itemHasUpdateTime = item as IHasUpdateTime;
            //                itemHasUpdateTime.UpdateTime = DateTime.Now;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        if (o is IHasCreationTime)
            //        {
            //            var itemHasCreationTime = o as IHasCreationTime;
            //            itemHasCreationTime.CreateTime = DateTime.Now;
            //        }
            //        if (o is IHasUpdateTime)
            //        {
            //            var itemHasUpdateTime = o as IHasUpdateTime;
            //            itemHasUpdateTime.UpdateTime = DateTime.Now;
            //        }
            //    }
            //}
            //else if (operationType == OperationType.Update)
            //{
            //    if (o is IEnumerable)
            //    {
            //        var temp = o as IEnumerable;
            //        foreach (var item in temp)
            //        {
            //            if (item is IHasUpdateTime)
            //            {
            //                var itemHasUpdateTime = item as IHasUpdateTime;
            //                itemHasUpdateTime.UpdateTime = DateTime.Now;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        if (o is IHasUpdateTime)
            //        {
            //            var itemHasUpdateTime = o as IHasUpdateTime;
            //            itemHasUpdateTime.UpdateTime = DateTime.Now;
            //        }
            //    }
            //}
        }

        internal class OperationCache
        {
            internal static ConcurrentDictionary<Type, string> tableNameMap = new ConcurrentDictionary<Type, string>();

            internal static ConcurrentDictionary<Type, List<string>> paramNameCache = new ConcurrentDictionary<Type, List<string>>();

            internal static List<string> GetParamNames(object o)
            {
                if (o is DynamicParameters)
                {
                    return (o as DynamicParameters).ParameterNames.ToList();
                }

                List<string> paramNames;
                if (!paramNameCache.TryGetValue(o.GetType(), out paramNames))
                {
                    paramNames = new List<string>();
                    foreach (var prop in o.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public))
                    {
                        var attribs = prop.GetCustomAttributes(typeof(IgnorePropertyAttribute), true);
                        var attr = attribs.FirstOrDefault() as IgnorePropertyAttribute;
                        if (attr == null || (attr != null && !attr.Value))
                        {
                            paramNames.Add(prop.Name);
                        }
                    }
                    paramNameCache[o.GetType()] = paramNames;
                }
                return paramNames;
            }

            internal static List<string> GetParamNames(Type type)
            {
                List<string> paramNames;
                if (!paramNameCache.TryGetValue(type, out paramNames))
                {
                    paramNames = new List<string>();
                    foreach (var prop in type.GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public))
                    {
                        var attribs = prop.GetCustomAttributes(typeof(IgnorePropertyAttribute), true);
                        var attr = attribs.FirstOrDefault() as IgnorePropertyAttribute;
                        if (attr == null || (attr != null && !attr.Value))
                        {
                            paramNames.Add(prop.Name);
                        }
                    }
                    paramNameCache[type] = paramNames;
                }
                return paramNames;
            }

            /// <summary>
            /// 获取表名
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="likelyTableName"></param>
            /// <returns></returns>
            internal static string DetermineTableName<T>(string likelyTableName)
            {
                string name;

                if (!tableNameMap.TryGetValue(typeof(T), out name))
                {
                    #region 原
                    //name = likelyTableName;
                    //if (!TableExists(name))
                    //{
                    //    //name = "[" + typeof(T).Name + "]";
                    //    name = "`" + typeof(T).Name + "`";
                    //}

                    //tableNameMap[typeof(T)] = name; 
                    #endregion

                    name = "`" + typeof(T).Name + "`";
                    tableNameMap[typeof(T)] = name;
                }
                return name;
            }

            /// <summary>
            /// 获取表名
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="likelyTableName"></param>
            /// <returns></returns>
            internal static string DetermineTableName<T>()
            {
                string name;
                if (!tableNameMap.TryGetValue(typeof(T), out name))
                {
                    name = "`" + typeof(T).Name + "`";
                    tableNameMap[typeof(T)] = name;
                }
                return name;
            }
        }
    }
}
