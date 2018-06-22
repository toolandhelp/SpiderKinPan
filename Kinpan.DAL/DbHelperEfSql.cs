using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Kinpan.CommonLib;
using Kinpan.Model;

namespace Kinpan.DAL
{
    /// <summary>
    /// <para>linq通用方法</para>
    /// <para>泛型类和泛型方法同时具备可重用性、类型安全和效率</para>
    /// </summary>
    /// <typeparam name="T">new():T必须要有一个无参构造函数</typeparam>
    public class DbHelperEfSql<T> : BaseServiceEf<KinpanEntities>, IRepository<T> where T : class, new()
    {
        private System.Data.Entity.DbSet<T> DbQueryTracking
        {
            get
            {
                return _context.Set<T>();
            }
        }
        private System.Data.Entity.Infrastructure.DbQuery<T> DbQueryNoTracking
        {
            get
            {
                return _context.Set<T>().AsNoTracking();
            }
        }
        public int? GetMaxId(Expression<Func<T, int?>> predicate)
        {
            try
            {
                return DbQueryNoTracking.Max(predicate);
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "GetMaxId:" + ex.Message);
                return null;
            }
        }
        public int? GetMaxId(Expression<Func<T, bool>> where, Expression<Func<T, int?>> max)
        {
            try
            {
                return DbQueryNoTracking.Where(where).Max(max);
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "GetMaxId:" + ex.Message);
                return null;
            }
        }
        public int GetMaxId(Expression<Func<T, int>> predicate)
        {
            try
            {
                return DbQueryNoTracking.Max(predicate);
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "GetMaxId:" + ex.Message);
                return -1;
            }
        }
        public int GetMaxId(Expression<Func<T, bool>> where, Expression<Func<T, int>> max)
        {
            try
            {
                return DbQueryNoTracking.Where(where).Max(max);
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "GetMaxId:" + ex.Message);
                return -1;
            }
        }

        #region Add/Insert
        public int Add(T entity, Expression<Func<T, int>> predicate)
        {
            try
            {

                DbQueryTracking.Add(entity);
                _context.SaveChanges();
                return DbQueryNoTracking.Max(predicate);
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "Add→entity,int:" + ex.Message);
                return -1;
            }
        }

        public Guid Add(T entity, Guid guid)
        {
            try
            {
                DbQueryTracking.Add(entity);
                _context.SaveChanges();
                return guid;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "Add→entity,Guid:" + ex.Message);
                return Guid.Empty;
            }
        }

        public bool Add(T entity)
        {
            try
            {
                DbQueryTracking.Add(entity);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "Add:" + ex.Message);
                return false;
            }
        }

        public bool AddAll(IEnumerable<T> entities)
        {
            try
            {
                DbQueryTracking.AddRange(entities);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "AddAll:" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Transaction(事务) 物理删除数据后新增集合数据
        /// </summary>
        /// <param name="entities">新增的集合数据</param>
        /// <param name="predicate">删除的条件</param>
        /// <returns></returns>
        public bool TransDeleteCAddL(IEnumerable<T> entities, Expression<Func<T, bool>> predicate)
        {
            bool isFlag = false;
            try
            {
                var query = DbQueryTracking.Where(predicate).ToList();
                if (query.Count > 0)
                {
                    DbQueryTracking.RemoveRange(query);
                }
                if (entities != null)
                {
                    var enumerable = entities as T[] ?? entities.ToArray();
                    if (enumerable.Any())
                    {
                        DbQueryTracking.AddRange(enumerable);
                    }
                }
                _context.SaveChanges();
                isFlag = true;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "TransDeleteCAddL→entities,bool:" + ex.Message);
                isFlag = false;
            }
            return isFlag;
        }
        #endregion

        #region Update
        /// <summary>
        /// <para>通用更新数据赋值 更新必须为主键赋值（目前暂只支持单主键）</para>
        /// <para>1.注意例如为int类型，但又不给int值赋值，这样int会自动默认为0，造成数据不完整</para>
        /// <para>2.注意当然是值类型的，如果不赋值，系统默认赋值，造成数据不完整</para>
        /// <para>3.为避免数据不完整，如果是值类型，就给值类型的数据赋最大值或最小值</para>
        /// <para>(对于Datetime类型，DateTime类型是值类型时默认是最小值可以不进行赋值操作，也能维护数据完整性)</para>
        /// </summary>
        /// <param name="entity">更新值实体</param>
        /// <param name="id">更新主键</param>
        /// <returns>rubbish</returns>
        private bool Update(T entity, object id)
        {
            try
            {
                var query = Load(id);
                if (query == null) return false;
                query.SameValueCopier(entity);
                _context.SaveChanges();
                return true;

                #region
                //if (query != null)
                //{
                //    _entities.Attach(entity, query);
                //    context.SaveChanges();
                //    return true;
                //}
                //else
                //{
                //    return false;
                //}
                #endregion
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "Update→entity,id:" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 更新多主键
        /// </summary>
        /// <param name="entity">更新值实体</param>
        /// <param name="ids">主键集合</param>
        /// <returns>rubbish</returns>
        private bool UpdateMorePK(T entity, object[] ids)
        {
            try
            {
                var query = Load(ids);
                if (query == null) return false;
                query.SameValueCopier(entity);
                _context.SaveChanges();
                return true;

                #region
                //if (query != null)
                //{
                //    _entities.Attach(entity, query);
                //    context.SaveChanges();
                //    return true;
                //}
                //else
                //{
                //    return false;
                //}
                #endregion
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "UpdateMorePK:" + ex.Message);
                return false;
            }
        }

        public bool Update(T entity, Expression<Func<T, bool>> predicate)
        {
            try
            {
                var query = DbQueryTracking.FirstOrDefault(predicate);
                if (query != null)
                {
                    query.SameValueCopier(entity);
                    _context.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "Update→entity,bool:" + ex.Message);
                return false;
            }
        }

        //rubbish
        private bool UpdateWithDelegate(Expression<Func<T, bool>> predicate, Action<T> action)
        {
            try
            {
                var query = DbQueryTracking.FirstOrDefault(predicate);
                if (query == null) return false;
                action(query);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "UpdateWithDelegate:" + ex.Message);
                return false;
            }
        }
        #endregion

        #region RealityDelete
        #region 非级联删除
        public bool PhysicalDelete(T entity)
        {
            try
            {
                DbQueryTracking.Remove(entity);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "PhysicalDelete:" + ex.Message);
                return false;
            }
        }

        public bool PhysicalDeleteByCondition(Expression<Func<T, bool>> predicate)
        {
            try
            {
                var query = DbQueryTracking.Where(predicate).ToList();
                if (query != null)
                {
                    DbQueryTracking.RemoveRange(query);
                    _context.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "PhysicalDeleteByCondition→bool:" + ex.Message);
                return false;
            }
        }

        public bool PhysicalDeleteAll(IEnumerable<T> entities)
        {
            try
            {
                DbQueryTracking.RemoveRange(entities);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "PhysicalDeleteAll:" + ex.Message);

                return false;
            }
        }
        #endregion

        #region 级联删除实体
        private bool PhysicalCascadingDeleteOnSubmit(T entity)
        {
            var entityType = entity.GetType();
            var entityProperties = entityType.GetProperties();
            //查找是否有“AssociationAttribute”标记的属性
            //（Linq中有“AssociationAttribute”标记的属性代表外表）
            var associationProperties = entityProperties.Where(
                c => c.GetCustomAttributes(true).Any(
                   attrbute => attrbute.GetType().Name == "AssociationAttribute")
                  & c.PropertyType.IsGenericType);//该属性必需是泛型
            //其他表有外键关联的记录
            foreach (var associationProperty in associationProperties)
            {
                //获取Property值
                object propertyValue = associationProperty.GetValue(entity, null);
                //Property是EntitySet`1类型的值，如EntitySet<DataSetStructure>，
                //而EntitySet`1有IEnumerable接口
                IEnumerable enumerable = (IEnumerable)propertyValue;
                foreach (object o in enumerable)
                {
                    //递归
                    PhysicalCascadingDeleteOnSubmit(o as T);
                }
            }

            try
            {
                //删除没外键关联的记录
                DbQueryTracking.Remove(entity);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "PhysicalCascadingDeleteOnSubmit:" + ex.Message);
                return false;
            }
        }
        #endregion
        #endregion

        #region 操作某字段
        public bool OperateColumnMoreByCondition(Expression<Func<T, bool>> predicate, string columnName, object columnValue)
        {
            try
            {
                var queryList = SearchListByCondition(predicate);
                if (queryList != null)
                {
                    foreach (var item in queryList)
                    {
                        //item.Stage = 1;
                        item.GetType().GetProperty(columnName).SetValue(item, columnValue, null);
                    }
                    _context.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "OperateColumnMoreByCondition→bool columnName columnValue:" + ex.Message);
                return false;
            }
        }

        public bool OperateColumnSingleByCondition(Expression<Func<T, bool>> predicate, string columnName, object columnValue)
        {
            try
            {
                var query = SearchBySingle(predicate);
                if (query == null) return false;
                query.GetType().GetProperty(columnName).SetValue(query, columnValue, null);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "OperateColumnSingleByCondition→bool colName colVlaue:" + ex.Message);
                return false;
            }
        }
        #endregion

        #region 操作状态
        private bool LogicDeleteSingleById(object id)
        {
            try
            {
                var query = Load(id);
                if (query == null) return false;
                query.GetType().GetProperty("Stage").SetValue(query, -1, null);
                //query.Stage = 0;
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "LogicDeleteSingleById→id" + ex.Message);
                return false;
            }
        }

        public bool LogicDeleteSingleByCondition(Expression<Func<T, bool>> predicate)
        {
            try
            {
                var query = SearchBySingle(predicate);
                if (query != null)
                {
                    //query.Stage = -1;
                    query.GetType().GetProperty("Stage").SetValue(query, -1, null);
                    _context.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "LogicDeleteSingleByCondition→bool:" + ex.Message);
                return false;
            }
        }
        /// <summary>
        /// 删除-1
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        [Obsolete("以后禁用，请用DeleteOrUpdate方法代替.")]
        public bool LogicDeleteMoreByCondition(Expression<Func<T, bool>> predicate)
        {
            try
            {
                var queryList = SearchListByCondition(predicate);
                if (queryList == null)
                    return false;
                foreach (var item in queryList)
                {
                    //item.Stage = -1;
                    item.GetType().GetProperty("Stage").SetValue(item, -1, null);
                }
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "LogicDeleteMoreByCondition→bool:" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 启用1
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        [Obsolete("以后禁用，请用DeleteOrUpdate方法代替.")]
        public bool EnableMoreByCondition(Expression<Func<T, bool>> predicate)
        {
            try
            {
                var queryList = SearchListByCondition(predicate);
                if (queryList == null) return false;
                foreach (var item in queryList)
                {
                    //item.Stage = 1;
                    item.GetType().GetProperty("Stage").SetValue(item, 1, null);
                }
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "LogicDeleteMoreByCondition→bool:" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 禁用2
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        [Obsolete("以后禁用，请用DeleteOrUpdate方法代替.")]
        public bool DisableMoreByCondition(Expression<Func<T, bool>> predicate)
        {
            try
            {
                var queryList = SearchListByCondition(predicate);
                if (queryList == null) return false;
                foreach (var item in queryList)
                {
                    //item.Stage = 2;
                    item.GetType().GetProperty("Stage").SetValue(item, 2, null);
                }
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "LogicDeleteMoreByCondition→bool:" + ex.Message);
                return false;
            }
        }
        #endregion

        #region SQL语句执行增加，删除、修改
        /// <summary>
        ///  批量删除或更新效率更高
        /// </summary>
        /// <param name="sql"></param>
        public bool DeleteOrUpdate(string sql)
        {
            try
            {
                _context.Database.ExecuteSqlCommand(sql);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "DeleteOrUpdate→sql语句" + ex.Message);
                return false;
            }
        }
        #endregion

        #region Load

        //一个主键
        private T Load(object id)
        {
            return DbQueryTracking.Find(id);
        }

        //多个主键
        private T Load(object[] ids)
        {
            try
            {
                return DbQueryTracking.Find(ids);
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), ex.Message);
                return null;
            }
        }

        #endregion

        #region 聚和函数
        public int Count(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return DbQueryNoTracking.Count(predicate);
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "Count:" + ex.Message);
                return 0;
            }
        }
        public int Sum(Expression<Func<T, int>> predicate)
        {
            try
            {
                return DbQueryNoTracking.Sum(predicate);
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "Sum:" + ex.Message);
                return 0;
            }
        }
        #endregion

        #region Search
        public T SearchBySingle(Expression<Func<T, bool>> predicate)
        {
            try
            {
                //First,返回序列中的第一条记录，如果没有记录，则引发异常
                //FirstOrDefault,返回序列中的第一条记录，如果序列中不包含任何记录，则返回默认值。
                //Single,返回序列中的唯一一条记录，如果没有或返回多条，则引发异常。
                //SingleOrDefault，返回序列中的唯一一条记录，如果序列中不包含任何记录，则返回默认值，如果返回多条，则引发异常。
                //注:以上默认值为NULL。
                var query = DbQueryNoTracking.FirstOrDefault(predicate);
                //MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "SearchBySingle->SQL语句：",context.Log);
                return query;
            }
            catch (ArgumentNullException ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "SearchBySingle:" + ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "SearchBySingle:" + ex.Message);
                return null;
            }
        }

        public IList<T> SearchListByCondition(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return DbQueryNoTracking.Where(predicate).ToList();
            }
            catch (ArgumentNullException ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "SearchListByCondition→Expression bool:" + ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "SearchListByCondition→Expression bool:" + ex.Message);
                return null;
            }
        }

        public IList<T> SearchListByCondition<TS>(Expression<Func<T, bool>> predicate, bool isAsc, Expression<Func<T, TS>> orderLamdba)
        {
            try
            {
                return isAsc ? DbQueryNoTracking.Where(predicate).OrderBy(orderLamdba).ToList() : DbQueryNoTracking.Where(predicate).OrderByDescending(orderLamdba).ToList();
            }
            catch (ArgumentNullException ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "SearchListByCondition→bool," + (isAsc ? "asc" : "desc") + " orderLamdba:" + ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "SearchListByCondition→bool," + (isAsc ? "asc" : "desc") + " orderLamdba:" + ex.Message);
                return null;
            }
        }

        public IList<T> SearchListByCondition<TS>(bool isAsc, Expression<Func<T, TS>> orderLamdba)
        {
            try
            {
                return isAsc ? DbQueryNoTracking.OrderBy(orderLamdba).ToList() : DbQueryNoTracking.OrderByDescending(orderLamdba).ToList();
            }
            catch (ArgumentNullException ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "SearchByAll→" + (isAsc ? "asc" : "desc") + " orderLamdba:" + ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "SearchByAll→" + (isAsc ? "asc" : "desc") + " orderLamdba:" + ex.Message);
                return null;
            }
        }

        public IList<T> SearchByAll()
        {
            try
            {
                return DbQueryNoTracking.ToList();
            }
            catch (ArgumentNullException ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "SearchByAll:" + ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "SearchByAll:" + ex.Message);
                return null;
            }
        }


        //ef
        private IList<T> SearchByPage(int iPageIndex, int iPageSize, ref int iTotalRecords)
        {
            try
            {
                iTotalRecords = DbQueryNoTracking.Count();
                return DbQueryNoTracking.Skip(iPageIndex * iPageSize).Take(iPageSize).ToList();
            }
            catch (ArgumentNullException ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "SearchByPage→iPageIndex,iPageSize,iTotalRecords:" + ex.Message);
                return null;
            }
            catch (OverflowException ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "SearchByPage→iPageIndex,iPageSize,iTotalRecords:" + ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "SearchByPage→iPageIndex,iPageSize,iTotalRecords:" + ex.Message);
                return null;
            }
        }
        //ef
        private IList<T> SearchByPageCondition(Expression<Func<T, bool>> predicate, int iPageIndex, int iPageSize, ref int iTotalRecord)
        {
            try
            {
                iTotalRecord = DbQueryNoTracking.Count(predicate);
                return DbQueryNoTracking.Where(predicate).Skip(iPageIndex * iPageSize).Take(iPageSize).ToList();
            }
            catch (ArgumentNullException ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "SearchByPageCondition→bool,iPageIndex,iPageSize,iTotalRecord:" + ex.Message);
                return null;
            }
            catch (OverflowException ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "SearchByPageCondition→bool,iPageIndex,iPageSize,iTotalRecord:" + ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "SearchByPageCondition→bool,iPageIndex,iPageSize,iTotalRecord:" + ex.Message);
                return null;
            }
        }

        public IList<T> SearchByPageCondition<TS>(bool isAsc, Expression<Func<T, TS>> orderLamdba, int iPageIndex, int iPageSize, ref int iTotalRecord)
        {
            try
            {
                iTotalRecord = DbQueryNoTracking.Count();
                return isAsc ? DbQueryNoTracking.OrderBy(orderLamdba).Skip(iPageIndex * iPageSize).Take(iPageSize).ToList() : DbQueryNoTracking.OrderByDescending(orderLamdba).Skip(iPageIndex * iPageSize).Take(iPageSize).ToList();
            }
            catch (ArgumentNullException ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "SearchByPageCondition<S>→" + (isAsc ? "asc" : "desc") + " orderLamdba,iPageIndex,iPageSize,iTotalRecord:" + ex.Message);
                return null;
            }
            catch (OverflowException ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "SearchByPageCondition<S>→" + (isAsc ? "asc" : "desc") + " orderLamdba,iPageIndex,iPageSize,iTotalRecord:" + ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "SearchByPageCondition→" + (isAsc ? "asc" : "desc") + " bool,iPageIndex,iPageSize,iTotalRecord:" + ex.Message);
                return null;
            }
        }

        public IList<T> SearchByPageCondition<TS>(Expression<Func<T, bool>> predicate, bool isAsc, Expression<Func<T, TS>> orderLamdba, int iPageIndex, int iPageSize, ref int iTotalRecord)
        {
            try
            {
                iTotalRecord = DbQueryNoTracking.Count(predicate);
                return isAsc ? DbQueryNoTracking.Where(predicate).OrderBy(orderLamdba).Skip(iPageIndex * iPageSize).Take(iPageSize).ToList() : DbQueryNoTracking.Where(predicate).OrderByDescending(orderLamdba).Skip(iPageIndex * iPageSize).Take(iPageSize).ToList();
            }
            catch (ArgumentNullException ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "SearchByPageCondition<S>→bool," + (isAsc ? "asc" : "desc") + " orderLamdba,iPageIndex,iPageSize,iTotalRecord:" + ex.Message);
                return null;
            }
            catch (OverflowException ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "SearchByPageCondition<S>→bool," + (isAsc ? "asc" : "desc") + " orderLamdba,iPageIndex,iPageSize,iTotalRecord:" + ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "SearchByPageCondition→bool," + (isAsc ? "asc" : "desc") + " bool,iPageIndex,iPageSize,iTotalRecord:" + ex.Message);
                return null;
            }
        }
        public IList<T> SearchListByCondition(ConditionModel conditionItem)
        {
            var query = SearchAction(conditionItem);
            if (query == null) return null;
            return query.ToList();
        }

        //ef
        public IList<T> SearchByPageCondition(int iPageIndex, int iPageSize, ref int iTotalRecord, ConditionModel conditionItem)
        {
            try
            {
                var query = SearchAction(conditionItem);
                if (query == null) return null;
                iTotalRecord = query.Count();
                return query.Skip(iPageIndex * iPageSize).Take(iPageSize).ToList();
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), ex.Message);
                return null;
            }
        }

        private IQueryable<T> SearchAction(ConditionModel conditionItem)
        {
            try
            {
                IQueryable<T> queryable = DbQueryNoTracking;
                if (conditionItem != null)
                {
                    ParameterExpression param = Expression.Parameter(typeof(T), "c");
                    MethodCallExpression callExpression = null;
                    if (conditionItem.WhereList != null && conditionItem.WhereList.Count > 0)
                    {
                        #region Where
                        var dicExpression = new Dictionary<WhereCondition, Expression>();
                        foreach (var itemWhere in conditionItem.WhereList)
                        {
                            #region 构建单个条件
                            Type propertyType = typeof(T).GetProperty(itemWhere.FieldName).PropertyType;
                            //创建常量FieldValue
                            ConstantExpression right = DynamicRight(itemWhere.FieldValue, propertyType);
                            if (right == null)
                                continue;
                            //c.FieldName
                            MemberExpression left = Expression.Property(param, typeof(T).GetProperty(itemWhere.FieldName));
                            //c=>c.FieldName == FieldValue 
                            Expression filter = null;
                            if (propertyType == typeof(string))
                            {
                                #region 字符串类型String
                                switch (itemWhere.FieldOperator)
                                {
                                    case EnumOper.Equal:
                                    case EnumOper.DoubleEqual:
                                        filter = Expression.Equal(left, right);
                                        break;
                                    case EnumOper.ExclamationEqual:
                                    case EnumOper.LessGreater:
                                        filter = Expression.NotEqual(left, right);
                                        break;
                                    case EnumOper.Contains://查姓名infos.UserName.Contains("tony")                        
                                        //using System.Reflection;指定实现Contains方法                                                                                
                                        if (itemWhere.FieldValue.GetType() == typeof(List<string>))
                                        {
                                            var containsMethod = typeof(List<string>).GetMethod("Contains");
                                            right = Expression.Constant(itemWhere.FieldValue);
                                            filter = Expression.Call(right, containsMethod, left);
                                        }
                                        else if (itemWhere.FieldValue.GetType() == typeof(string[]))
                                        {
                                            var containsMethod = typeof(string[]).GetMethod("Contains");
                                            right = Expression.Constant(itemWhere.FieldValue);
                                            filter = Expression.Call(right, containsMethod, left);
                                        }
                                        else
                                        {
                                            var containsMethod = typeof(string).GetMethod("Contains");
                                            right = Expression.Constant(itemWhere.FieldValue, typeof(string));
                                            filter = Expression.Call(left, containsMethod, right);
                                        }
                                        break;
                                    case EnumOper.IndexOf:
                                        var indexOfMethod = typeof(string).GetMethod("IndexOf", new Type[] { typeof(string) });
                                        right = Expression.Constant(itemWhere.FieldValue, typeof(string));
                                        filter = Expression.Call(left, indexOfMethod, right);
                                        break;
                                    case EnumOper.StartsWith:
                                        //表示string的StartsWith(string)方法
                                        var startsWithMethod = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
                                        right = Expression.Constant(itemWhere.FieldValue, typeof(string));
                                        filter = Expression.Call(left, startsWithMethod, right);
                                        break;
                                    case EnumOper.EndsWith:
                                        var endsWithMethod = typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });
                                        right = Expression.Constant(itemWhere.FieldValue, typeof(string));
                                        filter = Expression.Call(left, endsWithMethod, right);
                                        break;
                                    default:
                                        filter = Expression.Equal(left, right);
                                        break;
                                }
                                #endregion
                            }
                            else if (propertyType == typeof(Guid))
                            {
                                #region Guid
                                switch (itemWhere.FieldOperator)
                                {
                                    case EnumOper.Equal:
                                    case EnumOper.DoubleEqual:
                                        filter = Expression.Equal(left, right);
                                        break;
                                    case EnumOper.ExclamationEqual:
                                    case EnumOper.LessGreater:
                                        filter = Expression.NotEqual(left, right);
                                        break;
                                    case EnumOper.Contains:
                                        MethodInfo containsMethod = null;
                                        if (itemWhere.FieldValue.GetType() == typeof(List<Guid>))
                                        {
                                            containsMethod = typeof(List<Guid>).GetMethod("Contains");
                                            right = Expression.Constant(itemWhere.FieldValue);
                                            filter = Expression.Call(right, containsMethod, left);
                                        }
                                        else if (itemWhere.FieldValue.GetType() == typeof(Guid[]))
                                        {
                                            containsMethod = typeof(Guid[]).GetMethod("Contains");
                                            right = Expression.Constant(itemWhere.FieldValue);
                                            filter = Expression.Call(right, containsMethod, left);
                                        }
                                        else if (itemWhere.FieldValue.GetType() == typeof(List<Guid?>))
                                        {
                                            var tempListGuid = (from tempItemGuid in (List<Guid?>)itemWhere.FieldValue where tempItemGuid != null select tempItemGuid.Value).ToList();
                                            containsMethod = typeof(List<Guid>).GetMethod("Contains");
                                            right = Expression.Constant(tempListGuid);
                                            filter = Expression.Call(right, containsMethod, left);
                                        }
                                        else if (itemWhere.FieldValue.GetType() == typeof(Guid?[]))
                                        {
                                            var temc = (Guid?[])itemWhere.FieldValue;
                                            var tempArrayGuid = (from tempItemGuid in temc where tempItemGuid != null select tempItemGuid.Value).ToArray();
                                            containsMethod = typeof(Guid[]).GetMethod("Contains");
                                            right = Expression.Constant(tempArrayGuid);
                                            filter = Expression.Call(right, containsMethod, left);
                                        }
                                        else
                                        {
                                            MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "Guid构建Contains参数无效.");
                                        }

                                        break;
                                    default:
                                        filter = Expression.Equal(left, right);
                                        break;
                                }
                                #endregion
                            }
                            else if (propertyType == typeof(Guid?))
                            {
                                #region Guid?
                                switch (itemWhere.FieldOperator)
                                {
                                    case EnumOper.Equal:
                                    case EnumOper.DoubleEqual:
                                        filter = Expression.Equal(left, right);
                                        break;
                                    case EnumOper.ExclamationEqual:
                                    case EnumOper.LessGreater:
                                        filter = Expression.NotEqual(left, right);
                                        break;
                                    case EnumOper.Contains:
                                        MethodInfo containsMethod = null;
                                        if (itemWhere.FieldValue.GetType() == typeof(List<Guid?>))
                                        {
                                            containsMethod = typeof(List<Guid?>).GetMethod("Contains");
                                            right = Expression.Constant(itemWhere.FieldValue);
                                            filter = Expression.Call(right, containsMethod, left);
                                        }
                                        else if (itemWhere.FieldValue.GetType() == typeof(Guid?[]))
                                        {
                                            containsMethod = typeof(Guid?[]).GetMethod("Contains");
                                            right = Expression.Constant(itemWhere.FieldValue);
                                            filter = Expression.Call(right, containsMethod, left);
                                        }
                                        else if (itemWhere.FieldValue.GetType() == typeof(List<Guid>))
                                        {
                                            var tempListGuid = ((List<Guid>)itemWhere.FieldValue).Select(tempItemGuid => (Guid?)tempItemGuid).ToList();
                                            containsMethod = typeof(List<Guid?>).GetMethod("Contains");
                                            right = Expression.Constant(tempListGuid);
                                            filter = Expression.Call(right, containsMethod, left);
                                        }
                                        else if (itemWhere.FieldValue.GetType() == typeof(Guid[]))
                                        {
                                            var tempArrayGuid = ((Guid[])itemWhere.FieldValue).Select(tempItemGuid => (Guid?)tempItemGuid).ToList();
                                            containsMethod = typeof(Guid?[]).GetMethod("Contains");
                                            right = Expression.Constant(tempArrayGuid);
                                            filter = Expression.Call(right, containsMethod, left);
                                        }
                                        else
                                        {
                                            MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), "Guid构建Contains参数无效.");
                                        }

                                        break;
                                    default:
                                        filter = Expression.Equal(left, right);
                                        break;
                                }
                                #endregion
                            }
                            else
                            {
                                #region 非字符类型

                                switch (itemWhere.FieldOperator)
                                {
                                    case EnumOper.Equal:
                                    case EnumOper.DoubleEqual:
                                        filter = Expression.Equal(left, right);
                                        break;
                                    case EnumOper.ExclamationEqual:
                                    case EnumOper.LessGreater:
                                        filter = Expression.NotEqual(left, right);
                                        break;
                                    case EnumOper.GreaterThan:
                                        filter = Expression.GreaterThan(left, right);
                                        break;
                                    case EnumOper.GreaterThanEqual:
                                        filter = Expression.GreaterThanOrEqual(left, right);
                                        break;
                                    case EnumOper.LessThan:
                                        filter = Expression.LessThan(left, right);
                                        break;
                                    case EnumOper.LessThanEqual:
                                        filter = Expression.LessThanOrEqual(left, right);
                                        break;
                                    default:
                                        filter = Expression.Equal(left, right);
                                        break;
                                }

                                #endregion
                            }
                            if (filter != null)
                            {
                                dicExpression.Add(itemWhere, filter);
                            }

                            #endregion
                        }
                        //多条件组合成表达式c.FieldName == FieldValue and c.FieldName == FieldValue
                        Expression whereExpression = null;
                        foreach (var item in dicExpression)
                        {
                            if (whereExpression == null)
                            {
                                whereExpression = item.Value;
                            }
                            else
                            {
                                whereExpression = string.IsNullOrEmpty(item.Key.Relation) ?
                                    Expression.And(whereExpression, item.Value) :
                                    (item.Key.Relation.ToUpper() == "OR" ?
                                    Expression.Or(whereExpression, item.Value) :
                                    Expression.And(whereExpression, item.Value));
                            }
                        }
                        //生成Lambda表达式 c=>c.FieldName == FieldValue and c.FieldName == FieldValue
                        Expression pred = Expression.Lambda(whereExpression, param);
                        //生成Where
                        callExpression = Expression.Call(
                           typeof(Queryable), "Where",
                           new[] { typeof(T) },
                           Expression.Constant(queryable), pred);
                        #endregion
                    }
                    if (callExpression == null)
                    {
                        var constantLeft = Expression.Constant(1);
                        var constantRight = Expression.Constant(1);
                        Expression whereExpression = Expression.Equal(constantLeft, constantRight);
                        Expression pred = Expression.Lambda(whereExpression, param);
                        callExpression = Expression.Call(typeof(Queryable), "Where", new[] { typeof(T) }, Expression.Constant(queryable), pred);
                    }
                    if (conditionItem.OrderList != null && conditionItem.OrderList.Count > 0)
                    {
                        #region Order
                        //OrderBy(ContactName => ContactName)
                        var methodAsc = "OrderBy";
                        var methodDesc = "OrderByDescending";
                        foreach (var orderItem in conditionItem.OrderList)
                        {
                            //这里找的是Queryable类拥有的方法                    
                            callExpression = Expression.Call(typeof(Queryable),
                             orderItem.Ascending ? methodAsc : methodDesc,
                             new[] { typeof(T), typeof(T).GetProperty(orderItem.FiledOrder).PropertyType },
                             callExpression,
                             Expression.Lambda(Expression.Property(param, orderItem.FiledOrder),
                              param));
                            methodAsc = "ThenBy";//多条件排序
                            methodDesc = "ThenByDescending";
                        }
                        #endregion
                    }
                    if (conditionItem.GroupingList != null && conditionItem.GroupingList.Count > 0)
                    {
                        #region GroupBy
                        foreach (var filedGrouping in conditionItem.GroupingList)
                        {
                            callExpression = Expression.Call(
                       typeof(Queryable), "GroupBy",
                       new[] { typeof(T), typeof(T).GetProperty(filedGrouping).PropertyType },
                       callExpression,
                       Expression.Lambda(Expression.Property
                       (param, filedGrouping), param));
                        }
                        #endregion
                    }
                    //生成动态查询
                    queryable = queryable.AsQueryable().Provider.CreateQuery<T>(callExpression);
                }
                return queryable;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql(typeof(T).ToString(), ex.Message);
                return null;
            }
        }

        //转换失败仍然返回原来值
        private ConstantExpression DynamicRight(object filedValue, Type propertyType)
        {
            try
            {
                ConstantExpression right;
                //判断是否为泛型 ， 并且判断是否具有类型Nullable<>
                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    #region 构造right
                    if (propertyType == typeof(char?))
                    {
                        //默认值'\0'是最小值              
                        right = Expression.Constant(filedValue.ToString().ConvertTo<char?>(), typeof(char?));
                    }
                    else if (propertyType == typeof(byte?))
                    {
                        //默认值0是最小值
                        right = Expression.Constant(filedValue.ToString().ConvertTo<byte?>(), typeof(byte?));
                    }
                    else if (propertyType == typeof(sbyte?))
                    {
                        //默认值0
                        right = Expression.Constant(filedValue.ToString().ConvertTo<sbyte?>(), typeof(sbyte?));
                    }
                    else if (propertyType == typeof(DateTime?))
                    {
                        right = Expression.Constant(filedValue.ToString().ConvertTo<DateTime?>(), typeof(DateTime?));
                    }
                    else if (propertyType == typeof(short?))
                    {
                        right = Expression.Constant(filedValue.ToString().ConvertTo<short?>(), typeof(short?));
                    }
                    else if (propertyType == typeof(int?))
                    {
                        right = Expression.Constant(filedValue.ToString().ConvertTo<int?>(), typeof(int?));
                    }
                    else if (propertyType == typeof(long?))
                    {
                        right = Expression.Constant(filedValue.ToString().ConvertTo<long?>(), typeof(long?));
                    }
                    else if (propertyType == typeof(ushort?))
                    {
                        right = Expression.Constant(filedValue.ToString().ConvertTo<ushort?>(), typeof(ushort?));
                    }
                    else if (propertyType == typeof(uint?))
                    {
                        right = Expression.Constant(filedValue.ToString().ConvertTo<uint?>(), typeof(uint?));
                    }
                    else if (propertyType == typeof(ulong?))
                    {
                        right = Expression.Constant(filedValue.ToString().ConvertTo<ulong?>(), typeof(ulong?));
                    }
                    else if (propertyType == typeof(float?))
                    {
                        //0.0F
                        right = Expression.Constant(filedValue.ToString().ConvertTo<float?>(), typeof(float?));
                    }
                    else if (propertyType == typeof(double?))
                    {
                        //0.0D
                        right = Expression.Constant(filedValue.ToString().ConvertTo<double?>(), typeof(double?));
                    }
                    else if (propertyType == typeof(decimal?))
                    {
                        //0.0M
                        right = Expression.Constant(filedValue.ToString().ConvertTo<decimal?>(), typeof(decimal?));
                    }
                    else if (propertyType == typeof(Guid?))
                    {
                        right = Expression.Constant(new Guid(filedValue.ToString()), typeof(Guid?));
                    }
                    else
                    {
                        right = Expression.Constant(filedValue);
                    }
                    #endregion
                }
                else
                {
                    #region 构造right
                    if (propertyType == typeof(char))
                    {
                        filedValue = Convert.ToChar(filedValue);
                    }
                    else if (propertyType == typeof(byte))
                    {
                        filedValue = Convert.ToByte(filedValue);
                    }
                    else if (propertyType == typeof(sbyte))
                    {
                        filedValue = Convert.ToSByte(filedValue);
                    }
                    else if (propertyType == typeof(DateTime))
                    {
                        filedValue = Convert.ToDateTime(filedValue);
                    }
                    else if (propertyType == typeof(short))
                    {
                        filedValue = Convert.ToInt16(filedValue);
                    }
                    else if (propertyType == typeof(int))
                    {
                        filedValue = Convert.ToInt32(filedValue);
                    }
                    else if (propertyType == typeof(long))
                    {
                        filedValue = Convert.ToInt64(filedValue);
                    }
                    else if (propertyType == typeof(ushort))
                    {
                        filedValue = Convert.ToUInt16(filedValue);
                    }
                    else if (propertyType == typeof(uint))
                    {
                        filedValue = Convert.ToUInt32(filedValue);
                    }
                    else if (propertyType == typeof(ulong))
                    {
                        filedValue = Convert.ToUInt64(filedValue);
                    }
                    else if (propertyType == typeof(float))
                    {
                        filedValue = Convert.ToSingle(filedValue);
                    }
                    else if (propertyType == typeof(double))
                    {
                        filedValue = Convert.ToDouble(filedValue);
                    }
                    else if (propertyType == typeof(decimal))
                    {
                        filedValue = Convert.ToDecimal(filedValue);
                    }
                    else if (propertyType == typeof(Guid))
                    {
                        filedValue = new Guid(filedValue.ToString());
                    }
                    right = Expression.Constant(filedValue);
                    #endregion
                }
                return right;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogDbEfSql("构造right错误", ex.Message);
                return null;
            }
        }
        #endregion
    }
}