using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TangChao.JiZhi.Pos.IBizlogic;
using TangChao.JiZhi.Pos.IDAL;
using TangChao.JiZhi.Pos.Models;
using TangChao.JiZhi.Pos.ViewModels;

namespace TangChao.JiZhi.Pos.Bizlogic
{
    public class SysProjectBLL : ISysProjectBLL
    {
        public ITbSysProjectDAL tbSysProjectDal;

        IMapper mapper;

        public SysProjectBLL(ITbSysProjectDAL tbSysProjectDal)
        {
            this.tbSysProjectDal = tbSysProjectDal;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Tb_sys_project, SysProjectVM>();
            });

            mapper = config.CreateMapper();
        }

        public SysProjectVM GetProject(int id)
        {

            return mapper.Map<Tb_sys_project, SysProjectVM>(tbSysProjectDal.GetByKey(id));
        }

    }
}
