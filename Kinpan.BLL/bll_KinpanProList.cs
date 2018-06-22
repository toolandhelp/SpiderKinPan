using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kinpan.Model;
using Kinpan.DAL;

namespace Kinpan.BLL
{
  public  class bll_KinpanProList
    {
        #region dbContext
        public DbHelperEfSql<t_KinpanProList> dbContext { get; set; }
        public bll_KinpanProList()
        {
            dbContext = new DbHelperEfSql<t_KinpanProList>();
        }
        #endregion

        #region 增改删 CUD Entity
        public bool AddOrUpdate(t_KinpanProList entity)
        {
            if (entity.id < 1)
            {
                return dbContext.Add(entity);
            }
            return dbContext.Update(entity, c => c.id == entity.id);
        }
        public bool PhysicalDelete(int[] ids)
        {
            return dbContext.PhysicalDeleteByCondition(c => ids.Contains(c.id));
        }

        #endregion

        #region  查询 Search Entity
        /// <summary>
        /// 根据Id获取单条数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public t_KinpanProList GetObjectById(int orgId)
        {
            return dbContext.SearchBySingle(c => c.id == orgId);
        }
        /// <summary>
        /// 根据查询对象获取多条记录
        /// </summary>
        /// <param name="conditionItem"></param>
        /// <returns></returns>
        public IList<t_KinpanProList> GetListByCondition(ConditionModel conditionItem)
        {
            return dbContext.SearchListByCondition(conditionItem);
        }

        /// <summary>
        /// 获取所有记录
        /// </summary>
        /// <returns></returns>
        public IList<t_KinpanProList> GetListByAll()
        {
            return dbContext.SearchByAll();
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orgIds"></param>
        /// <returns></returns>
        public IList<t_KinpanProList> GetListByCondition(int[] orgIds)
        {
            return dbContext.SearchListByCondition(c => orgIds.Contains(c.id), true, c => c.id);
        }
    }
}
