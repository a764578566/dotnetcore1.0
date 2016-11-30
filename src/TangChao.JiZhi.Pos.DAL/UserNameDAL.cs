using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using TangChao.JiZhi.Pos.IDAL;
using TangChao.JiZhi.Pos.Models;

namespace TangChao.JiZhi.Pos.DAL
{
    public class UserNameDAL : BaseDAL<UserNameLogin>, IUserNameDAL
    {
        public UserNameDAL()
        {

        }
    }
}
