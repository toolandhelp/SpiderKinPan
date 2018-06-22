using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Kinpan.CommonLib;
using Kinpan.Model;
using Kinpan.BLL;
using Newtonsoft.Json;

namespace Kinpan.Console
{
    class Program
    {
        private static bll_KinpanWard Bllward = new bll_KinpanWard();
        private static bll_KinpanDetails BllDetails = new bll_KinpanDetails();
        private static bll_KinpanImgDetail BllImgDetails = new bll_KinpanImgDetail();
        private static bll_KinpanProList BllProList = new bll_KinpanProList();
        static void Main(string[] args)
        {
            
            WebClient web = new WebClient();
            decimal rate = Run();
        }


        /// <summary>
        /// 开始跑
        /// </summary>
        /// <returns></returns>
        private static decimal Run()
        {
            MessageLog.AddLog("Run() ===> 开始" );
            decimal rate = 0M;
            try
            {
                // list_test
                //string WebList = "http://www.kinpan.com/kpaward/bs_dm112";
                //string ListHtmlTest = GetWebContent(WebList).Trim();

                //List<t_KinpanProList> dtAllProListTest = GetKinpanAllLists(ListHtmlTest);
                //return rate;


                string WebUrl = "http://www.kinpan.com/";

                string html = GetWebContent(WebUrl).Trim();
                List<t_KinpanWard> listWard = GetKinpanMenusLists(html);

                if (listWard == null)
                    rate = 0M;
                else
                {
                    //Thread t2 = new Thread(new ParameterizedThreadStart(PrintNumbers));//有参数的委托
                    //t2.Start(10);
                    for (int i = 0; i < listWard.Count; i++)
                    {
                        if (listWard[i].Url.ToString().Length > 0)
                        {
                            string WebUrlList = WebUrl + listWard[i].Url.ToString().TrimStart('/'); ;

                            string ListHtml = GetWebContent(WebUrlList).Trim();

                            List<t_KinpanProList> dtAllProList = GetKinpanAllLists(ListHtml);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageLog.AddLog("Run() ===>" + ex);
                rate = 0M;
            }
            return rate;
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
                MessageLog.AddLog("GetWebContent() ===>" + ex);
                return "";
            }
            return str;
        }

        /// <summary>
        /// 获取Kinpan菜单List
        /// </summary>
        /// <param name="strHtml"></param>
        /// <returns></returns>
        private static List<t_KinpanWard> GetKinpanMenusLists(string strHtml)
        {
            List<t_KinpanWard> lKinpanWard = new List<t_KinpanWard>();

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
                        MessageLog.AddLog("添加成功= 共("+ thisCount + ")第（"+rid+"）=>" + JsonConvert.SerializeObject(mKinpanWard));
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
                MessageLog.AddLog("GetKinpanMenusLists() ===>" + ex);

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

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(strHtml);

            doc.OptionOutputAsXml = true;
            HtmlNode node = doc.DocumentNode.SelectSingleNode(".//div[@class=\"SCon_pic03\"]");
            if (node == null)
            {
                return null;
            }

            HtmlNodeCollection ProList = node.SelectNodes(".//*[@class=\"con_list_wrap\"]/li");

            //翻页
            HtmlNodeCollection fy = node.SelectNodes(".//div[@class=\"SCon_picfy\"]/p");
            string yeCount = fy[0].InnerText.TrimStart('共');
            yeCount = yeCount.Substring(0, 5);
            string Count = Regex.Replace(yeCount, @"[^0-9]+", "");
            int iCount = int.Parse(Count);
            //当前curpage
            HtmlNodeCollection thisYe = node.SelectNodes(".//a[@class=\"curpage\"]");
            string sthisYe= thisYe[0].InnerText.Trim();
            sthisYe = Regex.Replace(sthisYe, @"[^0-9]+", "");
            int ithisYe = int.Parse(sthisYe);

            //当前search url
            HtmlNodeCollection url = node.SelectNodes(".//div[@class=\"SCon_picfy\"]/p/a");
            HtmlAttribute urltemp = url[0].Attributes["href"];
            string Surltemp = urltemp.Value.Trim() ?? "";

            //Surltemp = Regex.Replace(Surltemp, @"\\d + ", "");
            //Regex r = new Regex("\\d+");
            //var ms = r.Matches(Surltemp);
            //if (ms.Count > 0)
            //    ms.OfType<Match>().Last();
            Surltemp = Surltemp.Remove(Surltemp.Length - 1, 1);//移除最后数字 (十位数有问题）

           // return lKinpanProList;
            if (iCount != ithisYe)
            {
                string WebUrl = "http://www.kinpan.com/" + urltemp + (ithisYe + 1);
                string ListHtml = GetWebContent(WebUrl).Trim();
                GetKinpanAllLists(ListHtml);
            }


            int thisCount = ProList.Count;
            int rid = 0;
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
                mKinpanProList.ProName = sName;
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
                mKinpanProList.ProImgSavePath = FileUtils.SaveFile(simgurl, sName+"\\" + fName.Last());

                if (BllProList.AddOrUpdate(mKinpanProList))
                {
                    MessageLog.AddLog("添加成功= 共(" + thisCount + ")第（" + rid + "）=>" + JsonConvert.SerializeObject(mKinpanProList));
                }
                else
                {
                    MessageLog.AddLog("失败--- 共(" + thisCount + ")第（" + rid + "）----" + JsonConvert.SerializeObject(mKinpanProList));
                }

                //详情
                string WebUrl = "http://www.kinpan.com/" + sProUrl;
                string ListHtml = GetWebContent(WebUrl).Trim();
                GetKinpanDatail(ListHtml);

                rid++;

            }


            return lKinpanProList;
        }


        public static List<t_KinpanDetails> GetKinpanDatail(string strHtml)
        {
            List<t_KinpanDetails> lKinpanDetails = new List<t_KinpanDetails>();


            return lKinpanDetails;
        }
    }
}
