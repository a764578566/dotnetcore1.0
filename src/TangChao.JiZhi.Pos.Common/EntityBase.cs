using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TangChao.JiZhi.Pos.Common
{
    public class EntityBase
    {
        public int ID { get; set; }

        /// <summary>
        /// 是否启动
        /// </summary>
        public int Active { get; set; }


        /// <summary>
        /// 备注
        /// </summary>
        public string Demo { get; set; }


        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime SysCreated { set; get; }
    }
}
