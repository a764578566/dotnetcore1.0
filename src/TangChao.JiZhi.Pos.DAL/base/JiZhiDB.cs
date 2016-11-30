using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TangChao.JiZhi.Pos.Models;

namespace TangChao.JiZhi.Pos.DAL
{
    public class JiZhiDB : Database<JiZhiDB>
    {
        /// <summary>
        /// 用户
        /// </summary>
        public Table<UserNameLogin> Members { get; set; }
    }
}
