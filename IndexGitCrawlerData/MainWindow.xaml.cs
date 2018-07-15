using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IndexGitCrawlerData
{
    /// <summary>
    /// MainWindow.xaml 
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Configuration.Initialize("./Config.xml");
            DateTime dt = DateTime.Now;
            string dtStr = dt.ToString("yyyy_MM_dd HH_mm_ss");
            Logger.LogFilePath = System.IO.Path.Combine(Configuration.LogDir, dtStr + @".log");
            InitializeComponent();
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            MessageBox.Show("change!!");
        }
        private void Index_Click(object sender, RoutedEventArgs e)
        {

            string repositoryPathText = repositoryPath.Text.Trim().Replace("'", "''");
            string downloadURLText = downloadURL.Text.Trim().Replace("'", "''");
            string repositoryContentText = repositoryContent.Text.Trim().Replace("'", "''");
            string[] topicsList = new string[] { topicsList1.Text.Trim().Replace("'", "''"),
                topicsList2.Text.Trim().Replace("'", "''"),
                topicsList3.Text.Trim().Replace("'", "''"),
                topicsList4.Text.Trim().Replace("'", "''") };
            if (repositoryPathText == "" &&
                downloadURLText == "" &&
                repositoryContentText == "" &&
                topicsList[0] == "" &&
                topicsList[1] == "" &&
                topicsList[2] == "" &&
                topicsList[3] == "")
            {
                MessageBox.Show("Please input the index information");
            }
            else
            {
                DateTime d = DateTime.Now;
                DBCrawlerGitDetailDataModelData.ModelList = GetExistReadmeData(repositoryPathText, downloadURLText, repositoryContentText, topicsList);
                Window showIndexDatawindow = new ShowIndexData();
                showIndexDatawindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                showIndexDatawindow.Show();
            }
        }
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            repositoryPath.Text = "";
            downloadURL.Text = "";
            repositoryContent.Text = "";
            topicsList1.Text = "";
            topicsList2.Text = "";
            topicsList3.Text = "";
            topicsList4.Text = "";
        }

        private List<DBCrawlerGitDetailDataModel> GetExistReadmeData(string repositoryPath, string downloadURL, string repositoryContent, string[] topicsList)
        {
            List<DBCrawlerGitDetailDataModel> readmeDBList = new List<DBCrawlerGitDetailDataModel>();
            HashSet<string> repositoryPathFromDBSet = new HashSet<string>();
            SqlConnection Connection = new SqlConnection("Server=" + Configuration.DBserver + ";DataBase=" + Configuration.DBName + ";uid=" + Configuration.DBUID + ";pwd=" + Configuration.DBPWD);
            Connection.Open();
            string MainTableFullName = "[" + Configuration.DBName + "].[dbo].[" + Configuration.TableName + "]";

            string dbQueryContentSql;
            string dbQueryContentStartSql = "SELECT TOP " +
                (Configuration.IndexLineCountMax - readmeDBList.Count);
            string dbQueryContentColumnSql = MainTableFullName + ".repositoryPath," +
                MainTableFullName + ".downloadURL," +
                MainTableFullName + ".impressionCount," +
                MainTableFullName + ".clickCount," +
                MainTableFullName + ".repositoryContent," +
                MainTableFullName + ".topicsList," +
                MainTableFullName + ".readmeContent" +
                " FROM " +
                MainTableFullName + 
                " WHERE ";
            string queryCondition = "";
            if (repositoryPath != "")
            {
                queryCondition += MainTableFullName + ".repositoryPath like '%" + repositoryPath + "%' and ";
            }
            if (downloadURL != "")
            {
                queryCondition += "downloadURL like '%" + downloadURL + "%' and ";
            }
            for (int i = 0; i < topicsList.Length; i++)
            {
                if (topicsList[i] != "")
                {
                    queryCondition += "topicsList like '%" + topicsList[i] + "%' and ";
                }
            }
            repositoryContent = repositoryContent.Replace("\t", " ").Trim();
            if (repositoryContent != "")
            {
                string[] repositoryContentSplits = repositoryContent.Split(' ');
                try
                {
                    for (int len = repositoryContentSplits.Length; len > 0; len--)
                    {
                        string queryConditionCur = queryCondition;
                        dbQueryContentStartSql = "SELECT TOP " +
                    (Configuration.IndexLineCountMax - readmeDBList.Count);
                        string[] repositoryContentIndexList = new string[repositoryContentSplits.Length - len + 1];
                        for (int index = 0; index < repositoryContentIndexList.Length; index++)
                        {
                            for (int cur = 0; cur < len; cur++)
                            {
                                if (cur > 0)
                                {
                                    repositoryContentIndexList[index] += " ";
                                }
                                repositoryContentIndexList[index] += repositoryContentSplits[index + cur];
                            }
                        }
                        queryConditionCur += "(";
                        foreach (string repositoryContentSplitElement in repositoryContentIndexList)
                        {
                            queryConditionCur += "repositoryContent like '%" + repositoryContentSplitElement + "%' or ";
                        }
                        dbQueryContentSql = dbQueryContentStartSql + dbQueryContentColumnSql + queryConditionCur.Remove(queryConditionCur.Length - 4, 4) + ") ORDER BY " + Configuration.GetFromDBRankDefaultColumn;

                        ExecuteReaderByText(ref readmeDBList, ref repositoryPathFromDBSet, ref Connection, dbQueryContentSql);
                        if (readmeDBList.Count >= Configuration.IndexLineCountMax)
                        {
                            Connection.Close();
                            return readmeDBList;
                        }

                    }
                    Connection.Close();
                    return readmeDBList;

                }
                catch (Exception ex)
                {
                    Connection.Close();
                    Logger.WriteLog("get exist readme data error: " + ex.Message);
                    return readmeDBList;
                }
            }
            else
            {
                dbQueryContentSql = dbQueryContentStartSql + dbQueryContentColumnSql + queryCondition.Remove(queryCondition.Length - 5, 5) + " ORDER BY " + Configuration.GetFromDBRankDefaultColumn;
                try
                {
                    ExecuteReaderByText(ref readmeDBList, ref repositoryPathFromDBSet, ref Connection, dbQueryContentSql);
                    Connection.Close();
                    return readmeDBList;
                }
                catch (Exception ex)
                {
                    Connection.Close();
                    Logger.WriteLog("get exist readme data error: " + ex.Message);
                    return readmeDBList;
                }
            }
        }

        private void ExecuteReaderByText(ref List<DBCrawlerGitDetailDataModel> readmeDBList, ref HashSet<string> repositoryPathFromDBSet, ref SqlConnection Connection, string dbQueryContentSql)
        {

            SqlCommand cmdContent = new SqlCommand(dbQueryContentSql, Connection);
            cmdContent.CommandType = CommandType.Text;

            SqlDataReader contentReader = cmdContent.ExecuteReader();
            while (contentReader.Read())
            {
                string repositoryPathFromDB = contentReader["repositoryPath"].ToString();
                if (repositoryPathFromDBSet.Contains(repositoryPathFromDB))
                {
                    continue;
                }
                string downloadURLFromDB = contentReader["downloadURL"].ToString();
                int impressionCountFromDB = Convert.ToInt32(contentReader["impressionCount"].ToString());
                int clickCountFromDB = Convert.ToInt32(contentReader["clickCount"].ToString());
                string repositoryContentFromDB = contentReader["repositoryContent"].ToString();
                List<string> topicsListFromDB = new List<string>(contentReader["topicsList"].ToString().Split(';'));
                string readmeFileContent = contentReader["readmeContent"].ToString();
                DBCrawlerGitDetailDataModel model = new DBCrawlerGitDetailDataModel(repositoryPathFromDB,
                    downloadURLFromDB,
                    impressionCountFromDB,
                    clickCountFromDB,
                    repositoryContentFromDB,
                    topicsListFromDB,
                    readmeFileContent);
                readmeDBList.Add(model);
                repositoryPathFromDBSet.Add(repositoryPathFromDB);
                if (readmeDBList.Count >= Configuration.IndexLineCountMax)
                {
                    contentReader.Close();
                    return;
                }
            }
            contentReader.Close();
        }
    }
}
