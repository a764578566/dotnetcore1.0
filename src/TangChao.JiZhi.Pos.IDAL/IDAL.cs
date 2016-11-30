using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TangChao.JiZhi.Pos.Common;

namespace TangChao.JiZhi.Pos.IDAL
{
    public interface IDAL<T> where T : EntityBase
    {
        bool Save(T entity);

        bool Save(IEnumerable<T> entitys);

        IEnumerable<T> Query();

        bool Remove(string key);

        T GetByKey(int key);

        bool Update(string id, T entity);

    }
}
