using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kinpan.DAL;
using Kinpan.Model;


namespace Kinpan.BLL
{
    public class bll_KinpanImgDetail
    {
        #region dbContext
        public DbHelperEfSql<t_KinpanImgDetail> dbContext { get; set; }
        public bll_KinpanImgDetail()
        {
            dbContext = new DbHelperEfSql<t_KinpanImgDetail>();
        }
        #endregion

        #region 增改删 CUD Entity
        public bool AddOrUpdate(t_KinpanImgDetail entity)
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
        public t_KinpanImgDetail GetObjectById(int orgId)
        {
            return dbContext.SearchBySingle(c => c.id == orgId);
        }
        /// <summary>
        /// 根据查询对象获取多条记录
        /// </summary>
        /// <param name="conditionItem"></param>
        /// <returns></returns>
        public IList<t_KinpanImgDetail> GetListByCondition(ConditionModel conditionItem)
        {
            return dbContext.SearchListByCondition(conditionItem);
        }

        /// <summary>
        /// 获取所有记录
        /// </summary>
        /// <returns></returns>
        public IList<t_KinpanImgDetail> GetListByAll()
        {
            return dbContext.SearchByAll();
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orgIds"></param>
        /// <returns></returns>
        public IList<t_KinpanImgDetail> GetListByCondition(int[] orgIds)
        {
            return dbContext.SearchListByCondition(c => orgIds.Contains(c.id), true, c => c.id);
        }
    }
}
