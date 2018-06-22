using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinpan.Model
{
    #region 通用简单信息JSON数据
    /// <summary>
    /// 通用简单信息JSON数据
    /// </summary>
    public class ReturnMessageModel
    {
        public ReturnMessageModel()
        {
            ErrorType = 0;
            MessageContent = "";
        }

        /// <summary>
        /// <para>错误类型 错误0-5系统预定 其它为自定义错误</para>
        /// <para>0错误 1成功 2请求地址不正确 3未登录 4无页面权限 5无操作权限</para>
        /// </summary>
        public int ErrorType { get; set; }
        /// <summary>
        /// 提示信息内容
        /// </summary>
        public string MessageContent { get; set; }
    }
    /// <summary>
    /// 通用列表JSON数据
    /// </summary>
    public class ReturnListModel : ReturnMessageModel
    {
        public ReturnListModel()
        {
            CurrentPage = 1;
            PageSize = 20;
        }

        /// <summary>
        /// 当前页码默认1
        /// </summary>
        public int CurrentPage { get; set; }
        /// <summary>
        /// 分页条数，默认20条
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalRecord { get; set; }
        /// <summary>
        /// 页的总数
        /// </summary>
        public int PageCount { get; set; }
        /// <summary>
        /// 数据内容
        /// </summary>
        public object Data { get; set; }
    }
    /// <summary>
    /// 通用详细JSON数据
    /// </summary>
    public class ReturnDetailModel : ReturnMessageModel
    {
        public ReturnDetailModel()
        {

        }
        /// <summary>
        /// 实体对象
        /// </summary>
        public object Entity { get; set; }
    }
    #endregion
    #region  Lambda查询条件
    public class ConditionModel
    {
        /// <summary>
        /// 查询条件集合
        /// </summary>
        public IList<WhereCondition> WhereList { get; set; }
        /// <summary>
        /// 排序方式
        /// </summary>
        public IList<OrderCondition> OrderList { get; set; }
        /// <summary>
        /// 分组字段名称
        /// </summary>
        public IList<String> GroupingList { get; set; }

    }
    //=, ==, !=, <>, >, >=, <, <= operators
    public enum EnumOper
    {
        /// <summary>
        /// =
        /// </summary>
        Equal,//=
        /// <summary>
        /// ==
        /// </summary>
        DoubleEqual,//==
        /// <summary>
        /// !=
        /// </summary>
        ExclamationEqual,//!=
        /// <summary>
        /// 《》
        /// </summary>
        LessGreater,//<>
        /// <summary>
        /// 》
        /// </summary>
        GreaterThan,//>
        /// <summary>
        /// >=
        /// </summary>
        GreaterThanEqual,//>=
        /// <summary>
        /// 《
        /// </summary>
        LessThan,//<
        /// <summary>
        /// 《=
        /// </summary>
        LessThanEqual,//<=
        /// <summary>
        /// 包括
        /// </summary>
        Contains, //包括
        IndexOf,
        StartsWith,
        EndsWith
    }
    /// <summary>
    /// 查询条件
    /// </summary>
    public class WhereCondition
    {
        /// <summary>
        /// 字段名称
        /// </summary>
        public String FieldName { get; set; }

        /// <summary>
        /// 字段对应值
        /// </summary>
        public Object FieldValue { get; set; }
        /// <summary>
        /// 字段和值的关系（如>、>=、== 等等关系运算符）
        /// </summary>
        public EnumOper FieldOperator { get; set; }

        /// <summary>
        /// 连接条件对应关系 and,or
        /// </summary>
        public String Relation { get; set; }
    }
    /// <summary>
    /// 排序
    /// </summary>
    public class OrderCondition
    {
        /// <summary>
        /// 排序字段
        /// </summary>
        public String FiledOrder { get; set; }
        /// <summary>
        /// 排序值两种DESC或ASC
        /// </summary>        
        public bool Ascending { get; set; }
    }
    #endregion
 
}
