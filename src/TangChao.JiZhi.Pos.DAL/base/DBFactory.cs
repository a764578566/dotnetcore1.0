using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TangChao.JiZhi.Pos.Common;

namespace TangChao.JiZhi.Pos.DAL
{
    public class DBFactory
    {
        private const string MySqlDbConnectionKey = "MySqlDbConnection";

        private static string _connectionString;
        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    //如果加密,则执行揭秘操作
                    if (Config.MysqlConfigStr.IsEncryption)
                    {
                        //todo 加密 解密
                        _connectionString = Config.MysqlConfigStr.ConnectionString;
                    }
                    else
                    {
                        _connectionString = Config.MysqlConfigStr.ConnectionString;
                    }
                }

                return _connectionString;
            }
        }

        public static JiZhiDB GetDBContext()
        {
            MySqlConnection conn = new MySqlConnection(ConnectionString);
            return JiZhiDB.Init(conn, 60);
        }
    }
}
