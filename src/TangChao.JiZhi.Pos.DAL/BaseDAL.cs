using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TangChao.JiZhi.Pos.Common;
using TangChao.JiZhi.Pos.IDAL;

namespace TangChao.JiZhi.Pos.DAL
{
    public class BaseDAL<T> : IDAL<T> where T : EntityBase
    {
        public virtual bool Save(T entity)
        {
            using (var db = DBFactory.GetDBContext())
            {
                return db.Insert<T>(entity) > 0;
            }
        }

        public virtual IEnumerable<T> Query()
        {
            using (var db = DBFactory.GetDBContext())
            {
                return db.All<T>();
            }
        }

        public virtual bool Remove(string id)
        {
            using (var db = DBFactory.GetDBContext())
            {
                return db.Delete<T>(id);
            }
        }

        public T GetByKey(int key)
        {
            using (var db = DBFactory.GetDBContext())
            {
                return db.Get<T>(key.ToString());
            }
        }

        public T GetByKey(string id)
        {
            using (var db = DBFactory.GetDBContext())
            {
                return db.Get<T>(id);
            }
        }

        public virtual int Count()
        {
            using (var db = DBFactory.GetDBContext())
            {
                return db.Count<T>();
            }
        }

        public virtual bool Save(IEnumerable<T> entitys)
        {
            using (var db = DBFactory.GetDBContext())
            {
                db.BeginTransaction();

                int result = db.Insert<T>(entitys);

                if (result > 0)
                {
                    db.CommitTransaction();
                    return true;
                }
                return false;
            }
        }

        public bool Update(string id, T entity)
        {
            using (var db = DBFactory.GetDBContext())
            {
                return db.Update<T>(id, entity) > 0;
            }
        }




    }
}
