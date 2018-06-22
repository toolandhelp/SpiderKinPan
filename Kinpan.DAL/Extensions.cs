using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Kinpan.DAL
{
    public static class Extensions
    {
        #region 暂时注释
        /*
        public static void Detach(this Object entity)
        {
            //FieldInfo fi=null;
            foreach (var fi in entity.GetType().
                      GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (fi.FieldType.ToString().Contains("EntityRef"))
                {
                    var value = fi.GetValue(entity);
                    if (value != null)
                    {
                        fi.SetValue(entity, null);
                    }
                }
                if (fi.FieldType.ToString().Contains("EntitySet"))
                {
                    var value = fi.GetValue(entity);
                    if (value != null)
                    {
                        MethodInfo mi = value.GetType().GetMethod("Clear");
                        if (mi != null)
                        {
                            mi.Invoke(value, null);
                        }

                        fi.SetValue(entity, value);
                    }
                }
            }
        }

        public static void Detach<T>(T entity)
        {
            Type t = entity.GetType();
            System.Reflection.PropertyInfo[] properties = t.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var property in properties)
            {
                string name = property.Name;

                if (property.PropertyType.IsGenericType &&
                property.PropertyType.GetGenericTypeDefinition() == typeof(EntitySet<>))
                {
                    property.SetValue(entity, null, null);
                }
            }
            System.Reflection.FieldInfo[] fields = t.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var field in fields)
            {
                string name = field.Name;
                if (field.FieldType.IsGenericType &&
                field.FieldType.GetGenericTypeDefinition() == typeof(EntityRef<>))
                {
                    field.SetValue(entity, null);
                }
            }
            System.Reflection.EventInfo eventPropertyChanged = t.GetEvent("PropertyChanged");
            System.Reflection.EventInfo eventPropertyChanging = t.GetEvent("PropertyChanging");
            if (eventPropertyChanged != null)
            {
                eventPropertyChanged.RemoveEventHandler(entity, null);
            }
            if (eventPropertyChanging != null)
            {
                eventPropertyChanging.RemoveEventHandler(entity, null);
            }
        } 
        **/        
        #endregion
        #region 更新数据赋值
        /// <summary>
        /// <para>数据赋值</para>
        /// <para>1.注意例如为int类型，但又不给int值赋值，这样int会自动默认为0，造成数据不完整</para>
        /// <para>2.注意当然是值类型的，如果不赋值，系统默认赋值，造成数据不完整</para>
        /// <para>3.为避免数据不完整，如果是值类型，就给值类型的数据赋最大值或最小值</para>
        /// <para>(对于Datetime类型，DateTime类型是值类型时默认是最小值可以不进行赋值操作，也能维护数据完整性)</para>
        /// </summary>
        /// <param name="dataSetter">需要设置值的对象</param>
        /// <param name="dataGetter">值来源注意数据类型</param> 
        public static bool SameValueCopier(this object dataSetter, object dataGetter)
        {
            try
            {
                PropertyInfo[] propertyInfoList = dataSetter.GetType().GetProperties();
                foreach (PropertyInfo propertyInfo in propertyInfoList)
                {
                    if (propertyInfo.MemberType == MemberTypes.Property)
                    {
                        string propertyName = propertyInfo.Name;
                        object value = dataGetter.GetType().GetProperty(propertyName).GetValue(dataGetter, null);
                        string typeFullName = dataGetter.GetType().GetProperty(propertyName).PropertyType.FullName;
                        bool isFlag = IsValueType(value, typeFullName);
                        if (!isFlag && value != null)
                        {
                            dataSetter.GetType().GetProperty(propertyName).SetValue(dataSetter, value, null);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private static bool IsValueType(object value, string typeFullName)
        {
            //Type t = propertyType;
            ////判断是否为泛型 ， 并且判断是否具有类型Nullable<>
            //if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            //{
            //    if (propertyType.GetGenericArguments().Length > 0)
            //    {
            //        //默认第一个参数为正真的类型，数据类型。。
            //        t = propertyType.GetGenericArguments()[0];
            //    }
            //}
            #region 兼容性操作
            bool isFlag = false;
            switch (typeFullName)
            {
                //case "System.Char"://默认值'\0'是最小值
                //    char aboutChar = Convert.ToChar(value);
                //    if (aboutChar == Char.MinValue || aboutChar == Char.MaxValue)
                //    {
                //        IsFlag = true;
                //    }
                //    break;
                //case "System.Byte"://默认值0是最小值
                //    byte aboutByte = Convert.ToByte(value);
                //    if (aboutByte == Byte.MinValue || aboutByte == Byte.MaxValue)
                //    {
                //        IsFlag = true;
                //    }
                //    break;
                //case "System.SByte"://默认值0
                //    sbyte aboutSByte = Convert.ToSByte(value);
                //    if (aboutSByte == SByte.MinValue || aboutSByte == SByte.MaxValue)
                //    {
                //        IsFlag = true;
                //    }
                //    break;
                case "System.DateTime":
                    DateTime aboutDate = Convert.ToDateTime(value);
                    if (aboutDate == DateTime.MinValue || aboutDate == DateTime.MaxValue)
                    {
                        isFlag = true;
                    }
                    break;
                case "System.Int16"://0
                    short aboutInt16 = Convert.ToInt16(value);
                    if (aboutInt16 == Int16.MinValue || aboutInt16 == Int16.MaxValue)
                    {
                        isFlag = true;
                    }
                    break;
                case "System.Int32"://0
                    int aboutInt32 = Convert.ToInt32(value);
                    if (aboutInt32 == Int32.MinValue || aboutInt32 == Int32.MaxValue)
                    {
                        isFlag = true;
                    }
                    break;
                case "System.Int64": //0L
                    long aboutInt64 = Convert.ToInt64(value);
                    if (aboutInt64 == Int64.MinValue || aboutInt64 == Int64.MaxValue)
                    {
                        isFlag = true;
                    }
                    break;
                case "System.UInt16"://0
                    ushort aboutUInt16 = Convert.ToUInt16(value);
                    if (aboutUInt16 == UInt16.MinValue || aboutUInt16 == UInt16.MaxValue)
                    {
                        isFlag = true;
                    }
                    break;
                case "System.UInt32"://0
                    uint aboutUInt32 = Convert.ToUInt32(value);
                    if (aboutUInt32 == UInt32.MinValue || aboutUInt32 == UInt32.MaxValue)
                    {
                        isFlag = true;
                    }
                    break;
                case "System.UInt64"://0L
                    ulong aboutUInt64 = Convert.ToUInt64(value);
                    if (aboutUInt64 == UInt64.MinValue || aboutUInt64 == UInt64.MaxValue)
                    {
                        isFlag = true;
                    }
                    break;
                case "System.Single"://0.0F
                    float aboutFloat = Convert.ToSingle(value);
                    if (aboutFloat == Single.MinValue || aboutFloat == Single.MaxValue)
                    {
                        isFlag = true;
                    }
                    break;
                case "System.Double"://0.0D
                    double aboutDouble = Convert.ToDouble(value);
                    if (aboutDouble == Double.MinValue || aboutDouble == Double.MaxValue)
                    {
                        isFlag = true;
                    }
                    break;
                case "System.Decimal"://0.0M
                    decimal aboutDecimal = Convert.ToDecimal(value);
                    if (aboutDecimal == Decimal.MinValue || aboutDecimal == Decimal.MaxValue)
                    {
                        isFlag = true;
                    }
                    break;
                default:
                    break;
            }
            return isFlag;
            #endregion
        }
        #endregion
        /// <summary>
        /// 扩展OrderBy动态
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertyStr"></param>
        /// <returns></returns>
        public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string propertyStr, string sorttype) where TEntity : class
        {
            ParameterExpression param = Expression.Parameter(typeof(TEntity), "c");
            PropertyInfo property = typeof(TEntity).GetProperty(propertyStr);
            Expression propertyAccessExpression = Expression.MakeMemberAccess(param, property);
            LambdaExpression le = Expression.Lambda(propertyAccessExpression, param);
            Type type = typeof(TEntity);
            MethodCallExpression resultExp = Expression.Call(typeof(Queryable), sorttype, new Type[] { type, property.PropertyType }, source.Expression, Expression.Quote(le));
            return source.Provider.CreateQuery<TEntity>(resultExp);
        }
        public static T ConvertTo<T>(this IConvertible convertibleValue)
        {
            if (null == convertibleValue)
            {
                return default(T);
            }
            if (!typeof(T).IsGenericType)
            {
                return (T)Convert.ChangeType(convertibleValue, typeof(T));
            }
            else
            {
                Type genericTypeDefinition = typeof(T).GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(Nullable<>))
                {
                    return (T)Convert.ChangeType(convertibleValue, Nullable.GetUnderlyingType(typeof(T)));
                }
            }
            throw new InvalidCastException(string.Format("Invalid cast from type \"{0}\" to type \"{1}\".", convertibleValue.GetType().FullName, typeof(T).FullName));
        }

        #region 功能方法
        /// <summary>
        /// 在linq to entity中使用SqlServer.NEWID函数
        /// </summary>
        //[System.Data.Objects.DataClasses.EdmFunction("SqlServer", "NEWID")]
        //public static Guid NewId()
        //{
        //    return Guid.NewGuid();
        //}
        #endregion

        #region 扩展方法
        /// <summary>
        /// 随机排序扩展方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        //public static IQueryable<T> OrderByNewId<T>(this IQueryable<T> source)
        //{
        //    return source.AsQueryable().OrderBy(d => NewId());
        //}
        #endregion
    }


    internal class ParameterExpressionVisitor : ExpressionVisitor
    {
        public ParameterExpression ParameterExpression { get; private set; }
        
        public ParameterExpressionVisitor(ParameterExpression parameterExpression)
        {
            this.ParameterExpression = parameterExpression;
        }        

        public Expression Replace(Expression expression)
        {
            return this.Visit(expression);
        }

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            return this.ParameterExpression;
        }
    }

    public static class PredicateExtensionses
    {
        public static Expression<Func<T, bool>> True<T>() { return f => true; }

        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> exp_left, Expression<Func<T, bool>> exp_right)
        {
            var param = Expression.Parameter(typeof(T), "c");
            var parameterExpressionVisitor = new ParameterExpressionVisitor(param);

            var left = parameterExpressionVisitor.Replace(exp_left.Body);
            var right = parameterExpressionVisitor.Replace(exp_right.Body);
            var body = Expression.And(left, right);

            return Expression.Lambda<Func<T, bool>>(body, param);
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> exp_left, Expression<Func<T, bool>> exp_right)
        {
            var param = Expression.Parameter(typeof(T), "candidate");
            var parameterExpressionVisitor = new ParameterExpressionVisitor(param);

            var left = parameterExpressionVisitor.Replace(exp_left.Body);
            var right = parameterExpressionVisitor.Replace(exp_right.Body);
            var body = Expression.Or(left, right);

            return Expression.Lambda<Func<T, bool>>(body, param);
        }
    }
}
