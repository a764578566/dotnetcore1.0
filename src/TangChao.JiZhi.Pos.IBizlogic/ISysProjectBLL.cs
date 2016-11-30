using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TangChao.JiZhi.Pos.ViewModels;

namespace TangChao.JiZhi.Pos.IBizlogic
{
    public interface ISysProjectBLL
    {
        SysProjectVM GetProject(int id);
    }
}
