using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TangChao.JiZhi.Pos.DAL;
using TangChao.JiZhi.Pos.IBizlogic;
using TangChao.JiZhi.Pos.IDAL;
using TangChao.JiZhi.Pos.Models;
using TangChao.JiZhi.Pos.ViewModels;

namespace TangChao.JiZhi.Pos.Bizlogic
{
    public class UserNameBLL : IUserNameBLL
    {
        public IUserNameDAL userNameDal;

        IMapper mapper;

        public UserNameBLL(IUserNameDAL userNameDal)
        {
            this.userNameDal = userNameDal;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UserNameLogin, UserNameLoginVM>();
            });

            mapper = config.CreateMapper();
        }

        public UserNameLoginVM GetUserName(int id)
        {

            return mapper.Map<UserNameLogin, UserNameLoginVM>(userNameDal.GetByKey(id));
        }
    }
}
