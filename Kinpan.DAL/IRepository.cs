using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Kinpan.DAL
{
    public interface IRepository<T> where T : class, new()
    {
        bool Add(T entity);
        bool AddAll(IEnumerable<T> entities);

        bool Update(T entity, Expression<Func<T, bool>> expression);

        bool PhysicalDelete(T entity);
        bool PhysicalDeleteAll(IEnumerable<T> entities);
        bool PhysicalDeleteByCondition(Expression<Func<T, bool>> expression);

        #region Search
        T SearchBySingle(Expression<Func<T, bool>> expression);
        IList<T> SearchListByCondition(Expression<Func<T, bool>> expression);
        IList<T> SearchListByCondition<TS>(Expression<Func<T, bool>> expression, bool isAsc,Expression<Func<T, TS>> orderLamdba);
        IList<T> SearchByAll();
        IList<T> SearchByPageCondition<TS>(Expression<Func<T, bool>> expression,bool isAsc, Expression<Func<T, TS>> orderLamdba, int iPageIndex, int iPageSize, ref int iTotalRecord);
       // IList<T> SearchByPageCondition(int iPageIndex, int iPageSize, ref int iTotalRecord, ConditionModel conditionItem);
        #endregion
    }
}
