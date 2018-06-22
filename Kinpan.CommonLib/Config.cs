using System;

namespace Kinpan.CommonLib
{
    public class Config
    {
      
        /// <summary>
        /// 版本号
        /// </summary>
        public static readonly string Version = System.Configuration.ConfigurationManager.AppSettings["Version"];
        private static readonly string EntityName = System.Configuration.ConfigurationManager.AppSettings["EntityName"];
        private static readonly string DatabaseServer = System.Configuration.ConfigurationManager.AppSettings["DatabaseServer"];
        private static readonly string DatabaseName = System.Configuration.ConfigurationManager.AppSettings["DatabaseName"];
        private static readonly string DatabaseUid = System.Configuration.ConfigurationManager.AppSettings["DatabaseUid"];
        private static readonly string DatabasePwd = System.Configuration.ConfigurationManager.AppSettings["DatabasePwd"];
        /// <summary>
        /// 默认Ef
        /// </summary>
        /// <param name="isEf"></param>
        /// <returns></returns>
        public static string PlatformConnectionString(bool isEf = true)
        {
            if (isEf)
            {
                return string.Concat("metadata=res://*/",
                    EntityName, ".csdl|res://*/",
                    EntityName, ".ssdl|res://*/",
                    EntityName, ".msl;provider=System.Data.SqlClient;provider connection string='Data Source=",
                    DatabaseServer, ";Initial Catalog=",
                    DatabaseName, ";Persist Security Info=True;User ID=",
                    DatabaseUid, ";Password=",
                    DatabasePwd, ";MultipleActiveResultSets=True'");
                //return System.Configuration.ConfigurationManager.ConnectionStrings["PlatformConnectionEFMSSQL"].ConnectionString;
            }
            else
            {
                return string.Concat("server=", DatabaseServer, ";database=", DatabaseName, ";uid=", DatabaseUid, ";pwd=", DatabasePwd, ";");
                //return System.Configuration.ConfigurationManager.ConnectionStrings["PlatformConnectionMSSQL"].ConnectionString;
            }
        }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public static string DataBaseType
        {
            get
            {
                string dbType = System.Configuration.ConfigurationManager.AppSettings["DatabaseType"];
                return string.IsNullOrEmpty(dbType) ? "MSSQL" : dbType.ToUpper();
            }
        }

        /// <summary>
        /// 系统初始密码
        /// </summary>
        public static string SystemInitPassword
        {
            get
            {
                string pass = System.Configuration.ConfigurationManager.AppSettings["InitPassword"];
                return string.IsNullOrEmpty(pass) ? "111111" : pass.Trim();
            }
        }

        /// <summary>
        /// 得到当前主题
        /// </summary>
        public static string Theme
        {
            get
            {
                var cookie = System.Web.HttpContext.Current.Request.Cookies["theme_platform"];
                return cookie != null && !string.IsNullOrEmpty(cookie.Value) ? cookie.Value : "Blue";
            }
        }

        /// <summary>
        /// 得到网站域名
        /// </summary>
        public static string WebDomian
        {
            get
            {
                string webDomian = System.Configuration.ConfigurationManager.AppSettings["WebDomian"];
                return string.IsNullOrEmpty(webDomian) ? "" : webDomian.Trim();
            }
        }

    }
}
