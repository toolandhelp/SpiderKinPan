using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using HtmlAgilityPack;
using Kinpan.CommonLib;
using Kinpan.Model;
using Kinpan.BLL;
using Newtonsoft.Json;

namespace Kinpan.HandlerSpider
{

    /// <summary>
    /// 数据实体类
    /// </summary>
    public class ReturnModel
    {
        /// <summary>
        /// 计算结果
        /// </summary>
        private string result;

        public string Result
        {
            get { return result; }
            set { result = value; }
        }
    }
    /// <summary>
    /// Handler 的摘要说明
    /// </summary>
    public class Handler : IHttpHandler
    {
        private static bll_KinpanWard Bllward = new bll_KinpanWard();
        private static bll_KinpanDetails BllDetails = new bll_KinpanDetails();
        private static bll_KinpanImgDetail BllImgDetails = new bll_KinpanImgDetail();
        private static bll_KinpanProList BllProList = new bll_KinpanProList();

        //所有根Web
        private static string WebUrl = "http://www.kinpan.com/";
        //图片保存 
        private static string ImgPath = HttpRuntime.AppDomainAppPath.ToString() + "ProImg\\";

        public void ProcessRequest(HttpContext context)
        {

            context.Response.ContentType = "text/plain";
            string returnStr = "";
            ReturnModel model = new ReturnModel();


            model.Result = Run();

            returnStr = ToJson(model);
            context.Response.Write(returnStr);
        }

        /// <summary>
        /// 开始跑
        /// </summary>
        /// <returns></returns>
        private static string Run()
        {
            MessageLog.AddLog("Run() ===> 开始");
            string rate = "NO";
            int iAllCount = 0;
            try
            {
               //  test list_test
                //string WebList = "http://www.kinpan.com/kpaward/zz_dk101";
                //string ListHtmlTest = HtmlCodeRequest(WebList).Trim();

                //List<t_KinpanProList> dtAllProListTest = GetKinpanAllLists(ListHtmlTest);
                //return rate;

                string html = GetWebContent(WebUrl).Trim();
                List<t_KinpanWard> listWard = GetKinpanMenusLists(html);

                if (listWard == null)
                    return rate;
                else
                {
                    //Thread t2 = new Thread(new ParameterizedThreadStart(PrintNumbers));//有参数的委托
                    //t2.Start(10);
                    for (int i = 0; i < listWard.Count; i++)
                    {
                        MessageLog.AddLog("开始爬第（" + i + "）个【"+ listWard[i].name + "】菜单类型的数据==>");
                        if (listWard[i].Url.ToString().Length > 0)
                        {
                            string WebUrlList = WebUrl + listWard[i].Url.ToString().TrimStart('/'); ;

                            string ListHtml = HtmlCodeRequest(WebUrlList).Trim();

                            List<t_KinpanProList> dtAllProList = GetKinpanAllLists(ListHtml);
                            iAllCount += dtAllProList.Count;
                            MessageLog.AddLog("开始爬第（" + i + "）个菜单类型的数据结束，共计(" + dtAllProList.Count + ")条数据==>");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogJson("Run()==>错误：", ex.ToString());
                return rate = "No==>出差错了,已经有(" + iAllCount + ")条";
            }
            return rate = "OK==>共计（" + iAllCount + "）条数据";
        }

        /// <summary>
        /// 获取Kinpan菜单List
        /// </summary>
        /// <param name="strHtml"></param>
        /// <returns></returns>
        private static List<t_KinpanWard> GetKinpanMenusLists(string strHtml)
        {
            List<t_KinpanWard> lKinpanWard = new List<t_KinpanWard>();

            lKinpanWard = Bllward.GetListByAll().ToList();
            if (lKinpanWard.Count <= 300)
            {
                //删除所有
                Bllward.PhysicalDeleteAll(lKinpanWard);
                try
                {
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(strHtml);

                    doc.OptionOutputAsXml = true;
                    HtmlNode node = doc.DocumentNode.SelectSingleNode(".//div[@class=\"all-sort-list\"]");
                    if (node == null)
                    {
                        return null;
                    }
                    HtmlNodeCollection hrefList = node.SelectNodes(".//*[@class=\"subitem\"]/ul/li/a");

                    //  HtmlNodeCollection hrefNodeList = node.SelectNodes(".//*[@class=\"subitem\"]/ul/li/a[@href]");
                    //if (hrefNodeList != null)
                    //{

                    //    foreach (var href in hrefNodeList)
                    //    {
                    //        string name = trNode.InnerText;
                    //         HtmlAttribute att = href.Attributes["href"];
                    //        string Url = att.Value;

                    //    }

                    //}

                    int thisCount = hrefList.Count;
                    int rid = 0;
                    MessageLog.AddLog("开始添加Kinpan菜单= 共(" + thisCount + ")==>");
                    foreach (HtmlNode strHref in hrefList)
                    {
                        t_KinpanWard mKinpanWard = new t_KinpanWard();

                        string name = strHref.InnerText.Trim() ?? "";

                        HtmlAttribute att = strHref.Attributes["href"];
                        string url = att.Value.Trim() ?? "";


                        mKinpanWard.name = name;
                        mKinpanWard.Url = url;


                        if (Bllward.AddOrUpdate(mKinpanWard))
                        {
                            MessageLog.AddLog("添加成功= 共(" + thisCount + ")第（" + rid + "）=>" + JsonConvert.SerializeObject(mKinpanWard));
                        }
                        else
                        {
                            MessageLog.AddLog("失败--- 共(" + thisCount + ")第（" + rid + "）----" + JsonConvert.SerializeObject(mKinpanWard));
                        }
                        lKinpanWard.Add(mKinpanWard);
                        rid++;
                    }
                }
                catch (Exception ex)
                {
                    MessageLog.AddErrorLogJson("GetKinpanMenusLists()==>错误：", ex.ToString());
                }
            }
            return lKinpanWard;
        }


        /// <summary>
        /// 获取Kinpan单个类型全部项目List
        /// </summary>
        /// <param name="strHtml"></param>
        /// <returns></returns>
        public static List<t_KinpanProList> GetKinpanAllLists(string strHtml)
        {
            List<t_KinpanProList> lKinpanProList = new List<t_KinpanProList>();
            if (strHtml.Length > 0) {           
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(strHtml);

                doc.OptionOutputAsXml = true;
                HtmlNode node = doc.DocumentNode.SelectSingleNode(".//div[@class=\"SCon_pic03\"]");
                if (node == null)
                {
                    return null;
                }

                HtmlNodeCollection ProList = node.SelectNodes(".//*[@class=\"con_list_wrap\"]/li");

                int thisCount = ProList.Count;
                int rid = 0;

                MessageLog.AddLog("开始添加当前页的项目集合==>");
                foreach (HtmlNode pro in ProList)
                {
                    t_KinpanProList mKinpanProList = new t_KinpanProList();

                    ////得到第一个A标签
                    HtmlNodeCollection lablaA = pro.SelectNodes(".//a");

                    HtmlAttribute Name = lablaA[1].Attributes["title"];
                    string sName = Name.Value.Trim() ?? "";

                    HtmlAttribute ProUrl = lablaA[0].Attributes["href"];
                    string sProUrl = ProUrl.Value.Trim() ?? "";

                    HtmlAttribute gongsiUrl = lablaA[2].Attributes["href"];
                    string sgongsiUrl = gongsiUrl.Value.Trim() ?? "";
                    string sgongsi = lablaA[2].InnerText;

                    HtmlNodeCollection jiage = pro.SelectNodes(".//span[@class=\"singlep\"]");
                    string sjiage = jiage[0].InnerText.Trim();
                    string jiageType = jiage[0].InnerText.Trim();
                    sjiage = Regex.Replace(sjiage, @"[^0-9]+", "");
                    float fjiage = float.Parse(sjiage);
                    int jiagelx = jiageType.Substring(0, 4) == "销售单价" ? 1 : 0;

                    HtmlNodeCollection liulan = pro.SelectNodes(".//span[@class=\"browse\"]");
                    string sliulan = liulan[0].InnerText.Trim();
                    sliulan = Regex.Replace(sliulan, @"[^0-9]+", "");
                    int iliulan = int.Parse(sliulan);

                    HtmlNodeCollection lablaImg = pro.SelectNodes(".//img");
                    HtmlAttribute img = lablaImg[0].Attributes["src"];
                    string simgurl = img.Value.Trim() ?? "";

                    HtmlAttribute Typeimg = lablaImg[1].Attributes["src"];
                    string sType = Typeimg.Value.Trim() ?? "";
                    var temp = sType.Split('.');
                    sType = temp[0].Substring(temp[0].Length - 3) == "qjd" ? "旗舰店" : "专营店";
                    ////得到第一个Img
                    //HtmlNodeCollection lablaImg = pro.SelectNodes(".//a[1]/img");
                    //HtmlAttribute ImgUrl = lablaImg.Attributes["src"];
                    //string sImgUrl = ImgUrl.Value;

                    string sGProId = Guid.NewGuid().ToString();

                    mKinpanProList.ProName = sName;
                    mKinpanProList.ProID = sGProId;
                    mKinpanProList.SalesPrice = fjiage;
                    mKinpanProList.SalesPriceType = jiagelx;
                    mKinpanProList.Views = iliulan;
                    mKinpanProList.ProImgUrl = simgurl;
                    mKinpanProList.DesignCompany = sgongsi;
                    mKinpanProList.DesignCompanyUrl = sgongsiUrl;
                    mKinpanProList.CompanyType = sType;
                    mKinpanProList.ProDetailsUrl = sProUrl;
                    var fName = simgurl.Split('/');
                    // FileUtils.SaveFile(simgurl, fName.Last());

                    //图片保存 路径+公司+项目；
                    string PathGongsi = ImgPath + sgongsi + "\\" + sName;
                    string simgpath = PathGongsi + "\\" + fName.Last();
                    //mKinpanProList.ProImgSavePath = FileUtils.SaveFile(simgurl, sName + "\\" + fName.Last());
                    mKinpanProList.ProImgSavePath = simgpath;

                    if (BllProList.AddOrUpdate(mKinpanProList))
                    {
                        List<string> lStr = new List<string>();
                        lStr.Add(simgpath);
                        lStr.Add(simgurl);
                      
                        Thread startDownload = new Thread(new ParameterizedThreadStart(DownLoadimg));
                        startDownload.Start(lStr);


                        MessageLog.AddLog("添加成功= 共(" + thisCount + ")第（" + rid + "）=>" + JsonConvert.SerializeObject(mKinpanProList));
                    }
                    else
                    {
                        MessageLog.AddLog("失败--- 共(" + thisCount + ")第（" + rid + "）----" + JsonConvert.SerializeObject(mKinpanProList));
                    }

                    //详情
                    string tempWebUrl = WebUrl + sProUrl;
                    string ListHtml = HtmlCodeRequest(tempWebUrl).Trim();
                    GetKinpanDetails(ListHtml, PathGongsi,sGProId, sName);

                    rid++;

                }

                #region 用于递归
                //翻页
                HtmlNodeCollection fy = node.SelectNodes(".//div[@class=\"SCon_picfy\"]/p");
                string yeCount = fy[0].InnerText.TrimStart('共');
                yeCount = yeCount.Substring(0, 5);
                string Count = Regex.Replace(yeCount, @"[^0-9]+", "");
                int iCount = int.Parse(Count);
                //当前curpage
                HtmlNodeCollection thisYe = node.SelectNodes(".//a[@class=\"curpage\"]");
                string sthisYe = thisYe[0].InnerText.Trim();
                sthisYe = Regex.Replace(sthisYe, @"[^0-9]+", "");
                int ithisYe = int.Parse(sthisYe);

                //当前search url
                HtmlNodeCollection url = node.SelectNodes(".//div[@class=\"SCon_picfy\"]/p/a");
                HtmlAttribute urltemp = url[0].Attributes["href"];
                string Surltemp = urltemp.Value.Trim() ?? "";

                // kpaward/search/12?stp=&dt=&lm=&g=112&sc=&st=&f=&ht=&ha=&sort=3  //添加 &page=?
                // Surltemp = Surltemp.Substring(0, Surltemp.LastIndexOf("="));//移除最后数字 (十位数有问题）

                if (ithisYe <= (iCount + 1))
                {
                    string tepWebUrl = WebUrl + Surltemp + "&page=" + (ithisYe + 1);
                    string ListHtml = HtmlCodeRequest(tepWebUrl).Trim();
                    GetKinpanAllLists(ListHtml);
                }

                #endregion

                MessageLog.AddLog("共（"+ iCount + "）页，当前页是（"+ ithisYe + "），==>");

            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogJson("GetKinpanAllLists()==>错误：", ex.ToString());
            }
            }
            return lKinpanProList;
           
        }

        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="ProDetailsUrl">详情Html地址</param>
        /// <param name="ImgPath">图片保存地址 如：</param>
        /// <param name="ProID">项目ID</param>
        /// <param name="ProName">项目名字</param>
        /// <returns></returns>
        public static void GetKinpanDetails(string ProDetailsUrl, string ImgPath,string ProID,string ProName)
        {
            try
            {
                //string detailsUrl = ProDetailsUrl.ToString().Trim(); 

                //detailsUrl = WebUrl + detailsUrl;

                //string strHtml = HtmlCodeRequest(detailsUrl).Trim();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(ProDetailsUrl);

                doc.OptionOutputAsXml = true;
                //轮播图
                HtmlNode lbtnode = doc.DocumentNode.SelectSingleNode(".//*[@class=\"list-h\"]");

                if (lbtnode != null)
                {
                    MessageLog.AddLog("轮播图开始=》");
                    try
                    {
                        ////*[@id="test"]/li[1]
                        HtmlNodeCollection lbtList = lbtnode.SelectNodes(".//li");
                        foreach (var item in lbtList)
                        {
                            HtmlNodeCollection lablaImg = item.SelectNodes(".//img");
                            HtmlAttribute ImgUrl = lablaImg[0].Attributes["bigpic"];
                            string sImgUrl = ImgUrl.Value.Trim() ?? "";
                            HtmlAttribute Imgtitle = lablaImg[0].Attributes["title"];
                            //   string sImgtitle = Imgtitle.Value ?? "";
                            string sImgtitle = Imgtitle.Value.Trim().Length > 0 ? Imgtitle.Value.Trim() : "";

                            var fName = sImgUrl.Split('/').Last();

                            string tempImgPath = ImgPath + "\\" + fName;

                            List<string> lStr = new List<string>();
                            lStr.Add(tempImgPath);
                            lStr.Add(sImgUrl);

                            //while ((startDownload.ThreadState != System.Threading.ThreadState.Stopped) && (startDownload.ThreadState != System.Threading.ThreadState.Aborted))
                            //{
                            //    Thread.Sleep(5000);
                            //}
                            //startDownload.Abort();


                            //数据保存

                            t_KinpanImgDetail mKinpanImgDetail = new t_KinpanImgDetail();
                            mKinpanImgDetail.ProID = ProID;
                            mKinpanImgDetail.ProImgSavaPath = tempImgPath;
                            mKinpanImgDetail.ProImgTitle = sImgtitle;
                            mKinpanImgDetail.ProImgType = 0;
                            mKinpanImgDetail.ProImgUrl = sImgUrl;

                            if (BllImgDetails.AddOrUpdate(mKinpanImgDetail))
                            {
                                MessageLog.AddLog("添加成功，轮播图图片下载地址（" + sImgUrl + "）；轮播图图片保存地址（" + tempImgPath + "）。");
                                Thread startDownload = new Thread(new ParameterizedThreadStart(DownLoadimg));
                                startDownload.Start(lStr);
                            }
                            else
                            {
                                MessageLog.AddLog("添加失败，轮播图图片下载地址（" + sImgUrl + "）；轮播图图片保存地址（" + tempImgPath + "）。");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageLog.AddLog("轮播图出错=》" + ex);
                    }

                    MessageLog.AddLog("轮播图结束=》");

                }

                //项目详情
                HtmlNode xmxqnode = doc.DocumentNode.SelectSingleNode(".//div[@class=\"QXCon2_c1xmxq\"]");

                if (xmxqnode != null)
                {
                    MessageLog.AddLog("开始保存项目信息详情");
                    //项目信息详情
                    HtmlNodeCollection xmxxxq = xmxqnode.SelectNodes(".//div[@class=\"QXCon2c_con\"]/ul/li");
                    string szhi = "";
                    foreach (var item in xmxxxq)
                    {
                        szhi += item.InnerHtml.Trim() + " ; ";
                        //数据保存
                    }


                    List<string> lxqStr = new List<string>();
                    lxqStr.Add(ImgPath);
                    lxqStr.Add(szhi);
                    lxqStr.Add("项目信息详情");


                    //描述
                    //string sms = "";
                    //HtmlNodeCollection ms = xmxqnode.SelectNodes(".//div[@class=\"QXCon2_clpic\"]/p");
                    //foreach (var item in ms)
                    //{
                    //    sms += item.InnerHtml.Trim();
                    //}

                    //图片   //描述
                    //*[@id="js_detail"]
                    HtmlNodeCollection picDetail = xmxqnode.SelectNodes(".//*[@id=\"js_detail\"]");
                    if (picDetail[0].InnerHtml.Trim().Length == 0)
                    {
                        picDetail = xmxqnode.SelectNodes(".//div[@class=\"QXCon2_clpic\"]");
                    }


                    string sms = ""; //string(.)


                    foreach (var picImg in picDetail)
                    {
                        HtmlNodeCollection mss = picImg.SelectNodes(".//p"); //只可能有一个

                        if (mss.Count > 0)
                        {
                            foreach (var item in mss)
                            {
                                sms += item.InnerText.Trim() + "、";
                            }
                        }

                        List<string> lmsStr = new List<string>();
                        lmsStr.Add(ImgPath);
                        lmsStr.Add(sms);
                        lmsStr.Add("描述");

                        Thread startMsinfoLog = new Thread(new ParameterizedThreadStart(InfoLog));
                        startMsinfoLog.Start(lmsStr);


                        HtmlNodeCollection lablaImg = picImg.SelectNodes(".//img");

                        foreach (var PicImgs in lablaImg)
                        {
                            HtmlAttribute ImgUrl = PicImgs.Attributes["src"];
                           // string sImgUrl = ImgUrl.Value.Trim() ?? "";
                            string sImgUrl = ImgUrl.Value.Trim() ?? "";
                            HtmlAttribute Imgtitle = PicImgs.Attributes["title"];
                            // string sImgtitle = Imgtitle.Value.Trim() ?? "";
                            string sImgtitle = "";
                            try
                            {
                                sImgtitle = Imgtitle.Value.Trim().Length > 0 ? Imgtitle.Value.Trim() : "";

                            }
                            catch (Exception)
                            {

                            }
                            var fName = sImgUrl.Split('/').Last();

                            string tempImgPath = ImgPath + "\\" + fName;

                            List<string> lStr = new List<string>();
                            lStr.Add(tempImgPath);
                            lStr.Add(sImgUrl);


                            t_KinpanImgDetail mKinpanImgDetail = new t_KinpanImgDetail();
                            mKinpanImgDetail.ProID = ProID;
                            mKinpanImgDetail.ProImgSavaPath = tempImgPath;
                            mKinpanImgDetail.ProImgTitle = sImgtitle;
                            mKinpanImgDetail.ProImgType = 1;
                            mKinpanImgDetail.ProImgUrl = sImgUrl;

                            if (BllImgDetails.AddOrUpdate(mKinpanImgDetail))
                            {
                                MessageLog.AddLog("添加成功，图片下载地址（" + sImgUrl + "）；图片保存地址（" + tempImgPath + "）。");
                                Thread startDownload = new Thread(new ParameterizedThreadStart(DownLoadimg));
                                startDownload.Start(lStr);
                            }
                            else
                            {
                                MessageLog.AddLog("添加失败，图片下载地址（" + sImgUrl + "）；图片保存地址（" + tempImgPath + "）。");
                            }


                        }


                        Thread startinfoLog = new Thread(new ParameterizedThreadStart(InfoLog));
                        startinfoLog.Start(lxqStr);


                    }

                    t_KinpanDetails mKinpanDetails = new t_KinpanDetails();
                    mKinpanDetails.ProID = ProID;
                    mKinpanDetails.ProName = ProName;
                    mKinpanDetails.ProTextDtails = szhi;
                    mKinpanDetails.ProDescription = sms ;
                    mKinpanDetails.CreationAt = System.DateTime.Now;

                    if (BllDetails.AddOrUpdate(mKinpanDetails))
                    {
                        MessageLog.AddLog("成功数据==》"+ JsonConvert.SerializeObject(mKinpanDetails));
                        MessageLog.AddLog("项目详情添加成功");
                    }
                    else
                    {
                        MessageLog.AddLog("失败数据==》" + JsonConvert.SerializeObject(mKinpanDetails));
                        MessageLog.AddLog("项目详情添加失败");
                    }
                    MessageLog.AddLog("添加项目详情==>");

                }
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogJson("GetKinpanDetails()==>错误：", ex.ToString());
            }

        }

        /// <summary>
        /// 下载图片   可写到公用类里
        /// </summary>
        /// <param name="obj">第一个：保存地址；第二个：下载地址</param>
        /// <returns></returns>
        public static void DownLoadimg(object obj)
        {
            var temps = (List<string>)obj;
            string simgAllpath = temps[0].ToString(); //保存地址
            string sdownUrl = temps[1].ToString(); //下载地址

            //初始化清空垃圾
            System.GC.Collect();

            if (!string.IsNullOrEmpty(sdownUrl))
            {
                try
                {
                    MessageLog.AddLog("开始下载图片,图片来源：" + sdownUrl + ";图片保存到：" + simgAllpath + "");
                    if (!sdownUrl.Contains("http"))
                    {
                        sdownUrl = WebUrl + sdownUrl;
                    }
                    System.Net.ServicePointManager.DefaultConnectionLimit = 200;

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sdownUrl);
                    request.Timeout = 10000;
                    request.UserAgent = "User-Agent:Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705";
                    //request.UserAgent = "User-Agent:Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.101 Safari/537.36'";
                    //是否允许302
                    request.AllowAutoRedirect = true;
                    request.KeepAlive = false;
                    WebResponse response = request.GetResponse();
                    Stream reader = response.GetResponseStream();
                    //文件名
                    //string aFirstName = Guid.NewGuid().ToString();
                    //扩展名
                    //string aLastName = url.Substring(url.LastIndexOf(".") + 1, (url.Length - url.LastIndexOf(".") - 1));

                    string tempPath = simgAllpath.Substring(0, simgAllpath.LastIndexOf('\\'));

                    if (!Directory.Exists(tempPath))
                        Directory.CreateDirectory(tempPath);

                    if (!File.Exists(simgAllpath))
                    {
                        FileStream writer = new FileStream(simgAllpath, FileMode.OpenOrCreate, FileAccess.Write);
                        byte[] buff = new byte[512];
                        //实际读取的字节数
                        int c = 0;
                        while ((c = reader.Read(buff, 0, buff.Length)) > 0)
                        {
                            writer.Write(buff, 0, c);
                        }
                        //释放所有
                        writer.Close();
                        writer.Dispose();
                        reader.Close();
                        reader.Dispose();
                        reader = null;
                        //取消请求
                        request.Abort();
                        request = null;
                        response.Close();
                        response = null;
                    }
                  
                }
                catch (Exception ex)
                {

                    //test
                    //WebClient wc = new WebClient();
                    //wc.DownloadFile(sdownUrl, simgAllpath);


                    MessageLog.AddErrorLogJson("DownLoadimg()==>下载路径：" + sdownUrl + "==> 保存路径：" + simgAllpath + "==>错误：", ex.ToString());
                    //  return "错误：地址" + url;
                }
            }
            //  return "错误：地址为空";
        }

        /// <summary>
        /// 保存信息   可写到公用类里
        /// </summary>
        /// <param name="obj">第一个：保存地址；第二个：保存内容；第三个：描述</param>
        public static void InfoLog(object obj)
        {
            var temps = (List<string>)obj;
            string savapath = temps[0].ToString(); //保存地址
            string content = temps[1].ToString(); //保存内容
            string description = temps[2].ToString(); //描述


            if (string.IsNullOrEmpty(content))
                return;

            string path = savapath;
            //string path = savapath +"//"+ DateTime.Now.ToString("yyyyMMdd") + ".txt";
            //string directory = GetMapPath("//log");
            //if (string.IsNullOrEmpty(directory))
            //{
            //    return;
            //}
            try
            {

                if (!System.IO.Directory.Exists(savapath))
                    System.IO.Directory.CreateDirectory(savapath);
                path = path + "//" + description + ".txt";
                if (!System.IO.File.Exists(path))
                {
                    using (System.IO.StreamWriter sw = System.IO.File.CreateText(path))
                    {
                        sw.WriteLine(content);
                        sw.WriteLine();
                        sw.Close();
                    }
                }
                else
                {
                    System.IO.FileInfo fileinfo = new System.IO.FileInfo(path);
                    using (System.IO.StreamWriter sw = fileinfo.AppendText())
                    {
                        sw.WriteLine(content);
                        sw.WriteLine();
                        sw.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogJson("InfoLog()==>错误：", ex.ToString());
            }
           // ClearLog();
        }


        /// <summary>
        /// 释放当前
        /// </summary>
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
        /// <summary> 获取远程HTML内容</summary>   
        /// <param name="url">远程网页地址URL</param>   
        /// <returns>成功返回远程HTML的代码内容</returns>   
        private static string GetWebContent(string strUrl)
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
                MessageLog.AddErrorLogJson("GetWebContent()==>错误：", ex.ToString());
                return "";
            }
            return str;
        }


        /// <summary> 获取远程HTML内容</summary>   
        /// <param name="url">远程网页地址URL</param>   
        /// <returns>成功返回远程HTML的代码内容</returns>   
        public static string HtmlCodeRequest(string Url)
        {
            if (string.IsNullOrEmpty(Url))
            {
                return "";
            }
            try
            {
                //创建一个请求
                HttpWebRequest httprequst = (HttpWebRequest)WebRequest.Create(Url);
                //不建立持久性链接
                httprequst.KeepAlive = true;
                //设置请求的方法
                httprequst.Method = "GET";
                //设置标头值
                // httprequst.UserAgent = "User-Agent:Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705";
                httprequst.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
              
                httprequst.Connection = "keep-alive";
                httprequst.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                httprequst.UserAgent = "User-Agent:Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.101 Safari/537.36'";
                //httprequst.Accept = "*/*";
                httprequst.Headers.Add("Accept-Language", "zh-cn,en-us;q=0.5");
                httprequst.ServicePoint.Expect100Continue = false;
                httprequst.Timeout = 10000; //等待10秒
                httprequst.AllowAutoRedirect = true;//是否允许302
                ServicePointManager.DefaultConnectionLimit = 200;
                //获取响应
                HttpWebResponse webRes = (HttpWebResponse)httprequst.GetResponse();
                //获取响应的文本流
                string content = string.Empty;
                using (System.IO.Stream stream = webRes.GetResponseStream())
                {
                    using (System.IO.StreamReader reader = new StreamReader(stream, System.Text.Encoding.GetEncoding("utf-8")))
                    {
                        content = reader.ReadToEnd();
                    }
                }
                //取消请求
                httprequst.Abort();
                //返回数据内容
                return content;
            }
            catch (Exception ex)
            {
                MessageLog.AddErrorLogJson("HtmlCodeRequest()==>错误：", ex.ToString());
                return "";
            }
        }


        /// <summary>
        /// 将object对象进行序列化
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string ToJson(object t)
        {
            return JsonConvert.SerializeObject(t, Formatting.Indented,new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
        }


        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }



}