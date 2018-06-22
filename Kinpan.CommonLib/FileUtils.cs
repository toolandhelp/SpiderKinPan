using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Kinpan.CommonLib
{
    public static class FileUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="filename">带后缀</param>
        /// <returns></returns>
        public static string SaveFile(string url, string filename)
        {
            string filePath = "";
            try
            {
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage();
                httpRequestMessage.RequestUri = new Uri(url);
                httpRequestMessage.Method = HttpMethod.Get;
                HttpClient httpClient = new HttpClient();
                var httpResponse = httpClient.SendAsync(httpRequestMessage);
                filePath = Environment.CurrentDirectory + "\\img\\" + filename ;
                if (!File.Exists(filePath))
                {
                    try
                    {
                        string folder = Path.GetDirectoryName(filePath);
                        if (!string.IsNullOrWhiteSpace(folder))
                        {
                            if (!Directory.Exists(folder))
                            {
                                Directory.CreateDirectory(folder);
                            }
                        }

                        File.WriteAllBytes(filePath, httpResponse.Result.Content.ReadAsByteArrayAsync().Result);
                    }
                    catch
                    {
                        MessageLog.AddLog("SaveFile()==> 路径：" + url + "；文件名：" + filename);
                    }
                }
                httpClient.Dispose();
            }
            catch (Exception)
            {
               
            }

            return filePath;
        }
    }
}
