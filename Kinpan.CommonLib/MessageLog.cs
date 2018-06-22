using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Kinpan.CommonLib
{
    /// <summary>
    /// 错误日志类
    /// </summary>
    public class MessageLog
    {
        public static void AddWebParamEmptyLog(string paramName, string source, string requestURL)
        {
            AddWebLog(string.Format("Error:参数{0}为空  错误源:{1} 请求路径:{2}", paramName, source, requestURL));
        }
        public static void AddWebParamFormatLog(string paramName, string source, string requestURL)
        {
            AddWebLog(string.Format("Error:参数{0}格式错误  错误源:{1} 请求路径:{2}", paramName, source, requestURL));
        }
        public static void AddWebOtherLog(string errorInfo, string source, string requestURL)
        {
            AddWebLog(string.Format("Error:{0}  错误源:{1} 请求路径:{2}", errorInfo, source, requestURL));
        }
        private static void AddWebLog(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            string path = "/Web" + DateTime.Now.ToString("yyyyMMdd") + ".log";
            string directory = GetMapPath("/log");
            if (string.IsNullOrEmpty(directory))
            {
                return;
            }
            try
            {

                if (!System.IO.Directory.Exists(directory))
                    System.IO.Directory.CreateDirectory(directory);
                path = directory + path;
                if (!System.IO.File.Exists(path))
                {
                    using (System.IO.StreamWriter sw = System.IO.File.CreateText(path))
                    {
                        sw.WriteLine(DateTime.Now + "       " + message);
                        sw.WriteLine();
                        sw.Close();
                    }
                }
                else
                {
                    System.IO.FileInfo fileinfo = new System.IO.FileInfo(path);
                    using (System.IO.StreamWriter sw = fileinfo.AppendText())
                    {
                        sw.WriteLine(DateTime.Now + "       " + message);
                        sw.WriteLine();
                        sw.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            ClearLog();
        }
        public static void AddLog(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            string path = "/" + DateTime.Now.ToString("yyyyMMdd") + ".log";
            string directory = GetMapPath("//log");
            if (string.IsNullOrEmpty(directory))
            {
                return;
            }
            try
            {

                if (!System.IO.Directory.Exists(directory))
                    System.IO.Directory.CreateDirectory(directory);
                path = directory + path;
                if (!System.IO.File.Exists(path))
                {
                    using (System.IO.StreamWriter sw = System.IO.File.CreateText(path))
                    {
                        sw.WriteLine(DateTime.Now + "       " + message);
                        sw.WriteLine();
                        sw.Close();
                    }
                }
                else
                {
                    System.IO.FileInfo fileinfo = new System.IO.FileInfo(path);
                    using (System.IO.StreamWriter sw = fileinfo.AppendText())
                    {
                        sw.WriteLine(DateTime.Now + "       " + message);
                        sw.WriteLine();
                        sw.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            ClearLog();
        }
        public static void AddErrorLogDalSpecial(string title, string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            string path = string.Format("/DalSpecial{0}.log", DateTime.Now.ToString("yyyyMMdd")); ;
            string directory = GetMapPath("~/log");
            if (string.IsNullOrEmpty(directory))
            {
                return;
            }
            try
            {

                if (!System.IO.Directory.Exists(directory))
                    System.IO.Directory.CreateDirectory(directory);
                path = directory + path;
                var szLog = new StringBuilder();
                if (!System.IO.File.Exists(path))
                {
                    using (var sw = System.IO.File.CreateText(path))
                    {
                        szLog.Append(DateTime.Now);
                        szLog.Append("        ");
                        szLog.Append(title);
                        szLog.Append("        ");
                        szLog.Append(message);
                        sw.WriteLine(szLog);
                        sw.WriteLine();
                        sw.Close();
                    }
                }
                else
                {
                    var fileinfo = new System.IO.FileInfo(path);
                    using (var sw = fileinfo.AppendText())
                    {
                        szLog.Append(DateTime.Now);
                        szLog.Append("        ");
                        szLog.Append(title);
                        szLog.Append("        ");
                        szLog.Append(message);
                        sw.WriteLine(szLog);
                        sw.WriteLine();
                        sw.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            ClearLog();
        }

        public static void AddErrorLogWechat(string title, string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            string path = string.Format("/Wechat{0}.log", DateTime.Now.ToString("yyyyMMdd")); ;
            string directory = GetMapPath("~/log");
            if (string.IsNullOrEmpty(directory))
            {
                return;
            }
            try
            {

                if (!System.IO.Directory.Exists(directory))
                    System.IO.Directory.CreateDirectory(directory);
                path = directory + path;
                var szLog = new StringBuilder();
                if (!System.IO.File.Exists(path))
                {
                    using (var sw = System.IO.File.CreateText(path))
                    {
                        szLog.Append(DateTime.Now);
                        szLog.Append("        ");
                        szLog.Append(title);
                        szLog.Append("        ");
                        szLog.Append(message);
                        sw.WriteLine(szLog);
                        sw.WriteLine();
                        sw.Close();
                    }
                }
                else
                {
                    var fileinfo = new System.IO.FileInfo(path);
                    using (var sw = fileinfo.AppendText())
                    {
                        szLog.Append(DateTime.Now);
                        szLog.Append("        ");
                        szLog.Append(title);
                        szLog.Append("        ");
                        szLog.Append(message);
                        sw.WriteLine(szLog);
                        sw.WriteLine();
                        sw.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            ClearLog();
        }

        public static void AddErrorLogDbLinqSql(string title, string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            string path = string.Format("/DB{0}.log", DateTime.Now.ToString("yyyyMMdd")); ;
            string directory = GetMapPath("~/log");
            if (string.IsNullOrEmpty(directory))
            {
                return;
            }
            try
            {

                if (!System.IO.Directory.Exists(directory))
                    System.IO.Directory.CreateDirectory(directory);
                path = directory + path;
                var szLog = new StringBuilder();
                if (!System.IO.File.Exists(path))
                {
                    using (var sw = System.IO.File.CreateText(path))
                    {
                        szLog.Append(DateTime.Now);
                        szLog.Append("        ");
                        szLog.Append(title);
                        szLog.Append("        ");
                        szLog.Append(message);
                        sw.WriteLine(szLog);
                        sw.WriteLine();
                        sw.Close();
                    }
                }
                else
                {
                    var fileinfo = new System.IO.FileInfo(path);
                    using (var sw = fileinfo.AppendText())
                    {
                        szLog.Append(DateTime.Now);
                        szLog.Append("        ");
                        szLog.Append(title);
                        szLog.Append("        ");
                        szLog.Append(message);
                        sw.WriteLine(szLog);
                        sw.WriteLine();
                        sw.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            ClearLog();
        }


        public static void AddErrorLogDbSql(string title, string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            string path = string.Format("/sqlDB{0}.log", DateTime.Now.ToString("yyyyMMdd")); ;
            string directory = GetMapPath("~/log");
            if (string.IsNullOrEmpty(directory))
            {
                return;
            }
            try
            {

                if (!System.IO.Directory.Exists(directory))
                    System.IO.Directory.CreateDirectory(directory);
                path = directory + path;
                var szLog = new StringBuilder();
                if (!System.IO.File.Exists(path))
                {
                    using (var sw = System.IO.File.CreateText(path))
                    {
                        szLog.Append(DateTime.Now);
                        szLog.Append("        ");
                        szLog.Append(title);
                        szLog.Append("        ");
                        szLog.Append(message);
                        sw.WriteLine(szLog);
                        sw.WriteLine();
                        sw.Close();
                    }
                }
                else
                {
                    var fileinfo = new System.IO.FileInfo(path);
                    using (var sw = fileinfo.AppendText())
                    {
                        szLog.Append(DateTime.Now);
                        szLog.Append("        ");
                        szLog.Append(title);
                        szLog.Append("        ");
                        szLog.Append(message);
                        sw.WriteLine(szLog);
                        sw.WriteLine();
                        sw.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            ClearLog();
        }

        public static void AddErrorLogDbEfSql(string title, string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            string path = string.Format("/efDB{0}.log", DateTime.Now.ToString("yyyyMMdd")); ;
            string directory = GetMapPath("~/log");
            if (string.IsNullOrEmpty(directory))
            {
                return;
            }
            try
            {

                if (!System.IO.Directory.Exists(directory))
                    System.IO.Directory.CreateDirectory(directory);
                path = directory + path;
                var szLog = new StringBuilder();
                if (!System.IO.File.Exists(path))
                {
                    using (var sw = System.IO.File.CreateText(path))
                    {
                        szLog.Append(DateTime.Now);
                        szLog.Append("        ");
                        szLog.Append(title);
                        szLog.Append("        ");
                        szLog.Append(message);
                        sw.WriteLine(szLog);
                        sw.WriteLine();
                        sw.Close();
                    }
                }
                else
                {
                    var fileinfo = new System.IO.FileInfo(path);
                    using (var sw = fileinfo.AppendText())
                    {
                        szLog.Append(DateTime.Now);
                        szLog.Append("        ");
                        szLog.Append(title);
                        szLog.Append("        ");
                        szLog.Append(message);
                        sw.WriteLine(szLog);
                        sw.WriteLine();
                        sw.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            ClearLog();
        }

        public static void AddErrorLogJson(string title, string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            string path = string.Format("/JSON{0}.log", DateTime.Now.ToString("yyyyMMdd")); ;
            string directory = GetMapPath("~/log");
            if (string.IsNullOrEmpty(directory))
            {
                return;
            }
            try
            {

                if (!System.IO.Directory.Exists(directory))
                    System.IO.Directory.CreateDirectory(directory);
                path = directory + path;
                var szLog = new StringBuilder();
                if (!System.IO.File.Exists(path))
                {
                    using (var sw = System.IO.File.CreateText(path))
                    {
                        szLog.Append(DateTime.Now);
                        szLog.Append("        ");
                        szLog.Append(title);
                        szLog.Append("        ");
                        szLog.Append(message);
                        sw.WriteLine(szLog);
                        sw.WriteLine();
                        sw.Close();
                    }
                }
                else
                {
                    var fileinfo = new System.IO.FileInfo(path);
                    using (var sw = fileinfo.AppendText())
                    {
                        szLog.Append(DateTime.Now);
                        szLog.Append("        ");
                        szLog.Append(title);
                        szLog.Append("        ");
                        szLog.Append(message);
                        sw.WriteLine(szLog);
                        sw.WriteLine();
                        sw.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            ClearLog();
        }

        private static string GetMapPath(string strPath)
        {
            try
            {
                if (HttpContext.Current != null)
                {
                    return HttpContext.Current.Server.MapPath(strPath);
                }
                strPath = strPath.Replace("/", "\\");
                if (strPath.StartsWith("\\"))
                {
                    strPath = strPath.Substring(strPath.IndexOf('\\', 1)).TrimStart('\\');
                }
                return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, strPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        private static string GetMapPath_ex(string strPath)
        {
            try
            {
                if (HttpContext.Current != null)
                {
                    return HttpContext.Current.Server.MapPath(strPath);
                }
                strPath = strPath.Replace("/", "\\");
                if (strPath.StartsWith("\\"))
                {
                    strPath = strPath.Substring(strPath.IndexOf('\\', 1)).TrimStart('\\');
                }
                return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, strPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public static void ClearLog()
        {
            string szPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "log";
            if (!System.IO.Directory.Exists(szPath))
                return;
            string[] szLogFiles = System.IO.Directory.GetFiles(szPath);
            string szFileDate = string.Empty;
            TimeSpan span = new TimeSpan();
            var indexofDot = 0;
            foreach (string szFileName in szLogFiles)
            {
                if (szFileName == null) continue;
                indexofDot = szFileName.LastIndexOf(".");
                if (indexofDot > 8)
                {
                    szFileDate = szFileName.Substring(indexofDot - 8, 8);
                    szFileDate = szFileDate.Insert(4, "-");
                    szFileDate = szFileDate.Insert(7, "-");
                    try
                    {
                        span = DateTime.Today - DateTime.Parse(szFileDate);
                        if (span.Days > 30)
                        {
                            System.IO.File.Delete(szFileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        continue;
                    }
                }
            }
        }
    }
}
