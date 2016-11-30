using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace TangChao.JiZhi.Pos.Common
{
    public class MysqlConfig 
    {
        public string Name
        {
            get;
            set;
        }
        public string ConnectionString
        {
            get;
            set;
        }

        public bool IsEncryption { get; set; } = false;

    }
}
