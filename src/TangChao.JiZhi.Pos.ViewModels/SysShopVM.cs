using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TangChao.JiZhi.Pos.ViewModels
{
    public class SysShopVM
    {
        /// <summary>
        /// 项目序列    
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 项目id
        /// </summary>
        public int P_Id { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string P_Name { get; set; }

        /// <summary>
        /// 项目编号
        /// </summary>
        public string P_Code { get; set; }


        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Action { get; set; }


        /// <summary>
        /// 头像路径
        /// </summary>
        public string P_Logo { get; set; }


        /// <summary>
        /// 创建系统时间
        /// </summary>
        public DateTime SysCreated { get; set; }
    }
}
