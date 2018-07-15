using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace IndexGitCrawlerData
{
    public static class Configuration
    {
        public static string DBserver { get; set; }
        public static string DBName { get; set; }
        public static string DBUID { get; set; }
        public static string DBPWD { set; get; }
        public static string TableName { get; set; }
        public static int IndexLineCountMax { get; set; }
        public static string GetFromDBRankDefaultColumn { get; set; }
        public static int DownloadWaitTime { get; set; }
        public static string LogDir { get; set; }

        //public static Dictionary<string, string> JobList;
        public static void Initialize(string configFilePath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(configFilePath);
            XmlNode xn = xmlDoc.SelectSingleNode("Configuration");


            XmlNodeList xnl1 = xn.ChildNodes;

            foreach (XmlNode xn1 in xnl1)
            {
                XmlElement xe1 = (XmlElement)xn1;

                switch (xe1.Name)
                {
                    case "DBserver":
                        DBserver = xe1.InnerText;
                        break;
                    case "DBName":
                        DBName = xe1.InnerText;
                        break;
                    case "DBUID":
                        DBUID = xe1.InnerText;
                        break;
                    case "DBPWD":
                        DBPWD = xe1.InnerText;
                        break;
                    case "TableName":
                        TableName = xe1.InnerText;
                        break;
                    case "IndexLineCountMax":
                        IndexLineCountMax = Convert.ToInt32(xe1.InnerText);
                        break;
                    case "GetFromDBRankDefaultColumn":
                        GetFromDBRankDefaultColumn = xe1.InnerText;
                        break;
                    case "DownloadWaitTime":
                        DownloadWaitTime = Convert.ToInt16(xe1.InnerText);
                        break;
                    case "LogDir":
                        LogDir = xe1.InnerText;
                        CreateDirectory(LogDir);
                        break;
                }
            }
        }
        private static void CreateDirectory(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }
    }
}