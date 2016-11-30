using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TangChao.JiZhi.Pos.Models;
using TangChao.JiZhi.Pos.ViewModels;

namespace TangChao.JiZhi.Pos.IBizlogic
{
    public interface IUserNameBLL
    {
        UserNameLoginVM GetUserName(int id);
    }
}
