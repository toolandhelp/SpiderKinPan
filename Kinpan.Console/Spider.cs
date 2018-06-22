using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kinpan.Console
{
    public class Spider
    {

        /// <summary> 获取远程HTML内容</summary>   
        /// <param name="url">远程网页地址URL</param>   
        /// <returns>成功返回远程HTML的代码内容</returns>   
        private string GetWebContent(string strUrl)
        {
            string str = "";
            try
            {
                WebClient wc = new WebClient();
                wc.Credentials = CredentialCache.DefaultCredentials;
                Encoding enc = Encoding.GetEncoding("UTF-8");// 如果是乱码就改成 UTF-8 / GB2312            
                Stream res = wc.OpenRead(strUrl);//以流的形式打开URL
                StreamReader sr = new StreamReader(res, enc);//以指定的编码方式读取数据流
                str = sr.ReadToEnd();//输出(HTML代码)
                res.Close();

                wc.Dispose();
            }
            catch (Exception ex)
            {
                return "";
            }
            return str;
        }
    }

}
