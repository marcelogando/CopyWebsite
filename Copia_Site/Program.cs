using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

namespace Copia_Site
{
    class Program
    {
        static void Main(string[] args)
        {
            String Url = "https://www.templatemonster.com/demo/49340.html";
            String RootUrl = "https://www.templatemonster.com/demo/";
            String dataToPost = String.Empty;
            String appFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\";
            String FolderCss = appFolder + @"css\";
            String FolderJs = appFolder + @"js\";
            String FolderImg = appFolder + @"img\";
            var request = (HttpWebRequest)WebRequest.Create(Url);
            var data = Encoding.ASCII.GetBytes(dataToPost);

            if (!Directory.Exists(FolderCss))
            {
                Directory.CreateDirectory(FolderCss);
            }

            if (!Directory.Exists(FolderJs))
            {
                Directory.CreateDirectory(FolderJs);
            }

            if (!Directory.Exists(FolderImg))
            {
                Directory.CreateDirectory(FolderImg);
            }

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            //String HTMLResponse = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("Windows-1252")).ReadToEnd();
            StringBuilder sbHTML = new StringBuilder();

            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                while (!sr.EndOfStream)
                {
                    String row = sr.ReadLine();
                    int i = 0;

                    String UrlCss = String.Empty;
                    String UrlJs = String.Empty;
                    String UrlImg = String.Empty;

                    String NewUrlCss = String.Empty;
                    String NewUrlJs = String.Empty;
                    String NewUrlImg = String.Empty;

                    try
                    {
                        if (row.Contains("rel=\"stylesheet\"") && row.Contains(".css"))
                        {
                            MatchCollection MatchCss = Regex.Matches(row, "href=\".*?\"");
                            UrlCss = MatchCss[i].Value.Replace("href=\"", String.Empty);

                            while (i < MatchCss.Count - 1 && !UrlCss.Contains(".css"))
                            {
                                i++;
                                UrlCss = MatchCss[i].Value.Replace("href=\"", String.Empty);
                            }

                            UrlCss = UrlCss.Substring(0, UrlCss.Length - 1);
                        }

                        if (!UrlCss.Equals(String.Empty) && UrlCss.Contains(".css"))
                        {
                            if (!UrlCss.Contains("http"))
                            {
                                int idxBar = UrlCss.IndexOf("/");
                                NewUrlCss = RootUrl + UrlCss.Substring(idxBar, UrlCss.Length - idxBar);
                            }
                            else
                            {
                                NewUrlCss = UrlCss;
                            }

                            Uri uri = new Uri(NewUrlCss);
                            string filename = System.IO.Path.GetFileName(uri.LocalPath);
                            SaveFile(GetFileString(NewUrlCss), appFolder + @"css\" + filename);
                        }

                        if (row.Contains("<script") && row.Contains("src=\"") && row.Contains(".js"))
                        {
                            MatchCollection MatchJs = Regex.Matches(row, "src=\".*?\"");
                            UrlJs = MatchJs[i].Value.Replace("src=\"", String.Empty);

                            while (i < MatchJs.Count - 1 && !UrlJs.Contains(".js"))
                            {
                                i++;
                                UrlJs = MatchJs[i].Value.Replace("href=\"", String.Empty);
                            }

                            UrlJs = UrlJs.Substring(0, UrlJs.Length - 1);
                        }

                        if (!UrlJs.Equals(String.Empty) && UrlJs.Contains(".js"))
                        {
                            if (!UrlJs.Contains("http"))
                            {
                                int idxBar = UrlJs.IndexOf("/");
                                NewUrlJs = RootUrl + UrlJs.Substring(idxBar, UrlJs.Length - idxBar);
                            }
                            else
                            {
                                NewUrlJs = UrlJs;
                            }

                            Uri uri = new Uri(NewUrlJs);
                            string filename = System.IO.Path.GetFileName(uri.LocalPath);
                            SaveFile(GetFileString(NewUrlJs), FolderJs + filename);
                        }

                        if (row.Contains("<img") && row.Contains("src=\"") && Regex.IsMatch(row, @"<img([\w\W]+?)/>"))
                        {
                            MatchCollection MatchImg = Regex.Matches(row, "src=\".*?\"");
                            UrlImg = MatchImg[i].Value.Replace("src=\"", String.Empty);

                            while (i < MatchImg.Count - 1 && !Regex.IsMatch(UrlImg, ".*?gif|png|jpg|jpeg|bmp"))
                            {
                                i++;
                                UrlImg = MatchImg[i].Value.Replace("src=\"", String.Empty);
                            }

                            UrlImg = UrlImg.Substring(0, UrlImg.Length - 1);
                        }

                        if (!UrlImg.Equals(String.Empty))
                        {
                            if (!UrlImg.Contains("http"))
                            {
                                int idxBar = UrlImg.IndexOf("/");
                                NewUrlImg = RootUrl + UrlImg.Substring(idxBar, UrlImg.Length - idxBar);
                            }
                            else
                            {
                                NewUrlImg = UrlImg;
                            }

                            Uri uri = new Uri(UrlImg);
                            string filename = System.IO.Path.GetFileName(uri.LocalPath);
                            DownloadImage(NewUrlImg, FolderImg + filename);

                            row = row.Replace(UrlImg, FolderImg + filename);
                        }
                    }
                    catch
                    {

                    }

                    sbHTML.Append(row);
                }

                using (StreamWriter swHTML = new StreamWriter(appFolder + "page.html"))
                {
                    swHTML.Write(sbHTML.ToString());
                }
            }
        }

        static void DownloadImage(string ImageUrl, string ImagePath)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(ImageUrl, ImagePath);
            }
        }

        static string GetFileString(String UrlFile)
        {
            WebClient client = new WebClient();
            return client.DownloadString(UrlFile);
        }

        static void SaveFile(string FileString, string SavePath)
        {
            using (StreamWriter sw = new StreamWriter(SavePath, false, Encoding.GetEncoding("Windows-1252")))
            {
                sw.Write(FileString);
            } 
        }
    }
}
