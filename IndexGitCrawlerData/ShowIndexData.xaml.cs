using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IndexGitCrawlerData
{
    /// <summary>
    /// ShowIndexData.xaml 的交互逻辑
    /// </summary>
    public partial class ShowIndexData : Window
    {
        int pageIndex;
        int pageCount;
        private BackgroundWorker m_BackgroundWorker;// 申明后台对象
        Thread newWindowThread;
        public ShowIndexData()
        {
            InitializeComponent();
            pageIndex = 0;
            pageCount = DBCrawlerGitDetailDataModelData.ModelList.Count;
            Initialize();
            DBCrawlerGitDetailDataModelData.GetDownloadResposeComplete = false;
            m_BackgroundWorker = new BackgroundWorker(); // 实例化后台对象
            m_BackgroundWorker.WorkerReportsProgress = true; // 设置可以通告进度
            m_BackgroundWorker.WorkerSupportsCancellation = true; // 设置可以取消
            m_BackgroundWorker.DoWork += new DoWorkEventHandler(ShowCostInfoAndCreateDownLoadWindow);
            m_BackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(UpdateTimeCost);
            m_BackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(EmptyTimeCostInfo);
        }
        private void Initialize()
        {
            if (pageCount == 0)
            {
                pageIndexContent.Text = "0/0";
            }
            else
            {
                DBCrawlerGitDetailDataModelData.CurrentModel = DBCrawlerGitDetailDataModelData.ModelList[pageIndex];
                repositoryPath.Text = DBCrawlerGitDetailDataModelData.CurrentModel.repositoryPath;
                downloadURL.Text = DBCrawlerGitDetailDataModelData.CurrentModel.downloadURL;
                repositoryContent.Text = DBCrawlerGitDetailDataModelData.CurrentModel.repositoryContent;
                string topicsListStr = "";
                foreach (string topic in DBCrawlerGitDetailDataModelData.CurrentModel.topicsList)
                {
                    topicsListStr += topic + "\t";
                }
                topicsList.Text = topicsListStr;
                impressionCount.Text = DBCrawlerGitDetailDataModelData.CurrentModel.impressionCount + "";
                clickCount.Text = DBCrawlerGitDetailDataModelData.CurrentModel.clickCount + "";
                readmeFileContent.Text = DBCrawlerGitDetailDataModelData.CurrentModel.readmeFileContent;
                pageIndexContent.Text = (pageIndex + 1) + "/" + pageCount;
            }

        }
        private void Index_Left(object sender, RoutedEventArgs e)
        {
            if (pageIndex == 0)
            {
                pageIndex = pageCount - 1;
            }
            else
            {
                pageIndex--;
            }
            Initialize();
        }
        private void Index_Right(object sender, RoutedEventArgs e)
        {
            if (pageIndex == pageCount - 1)
            {
                pageIndex = 0;
            }
            else
            {
                pageIndex++;
            }
            Initialize();
        }
        private void Download_Zip(object sender, RoutedEventArgs e)
        {
            DBCrawlerGitDetailDataModelData.GetDownloadResposeComplete = false;
            downloadInfo.Text = "getting information, please wait!!!";
            try
            {
                m_BackgroundWorker.RunWorkerAsync(this);
            }
            catch (Exception ex)
            {
                Logger.WriteLog("Download_Zip Click error: " + ex.Message);
                MessageBox.Show("get download information failed, please copy the downloadURL to the address bar to download");
            }
        }
        void ShowCostInfoAndCreateDownLoadWindow(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            int i = 0;
            newWindowThread = new Thread(ShowDownloadZipWindow);
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.Start();
            while (!DBCrawlerGitDetailDataModelData.GetDownloadResposeComplete && i < Configuration.DownloadWaitTime)
            {
                //if (bw.CancellationPending)
                //{
                //    e.Cancel = true;
                //    break;
                //}
                bw.ReportProgress(i++);

                Thread.Sleep(1000);
            }
            if (i >= Configuration.DownloadWaitTime)
            {
                try
                {
                    if (newWindowThread != null)
                    {
                        newWindowThread.Abort();
                    }
                    MessageBox.Show("get download information time out, please copy the downloadURL to the address bar to download");
                }
                catch (Exception ex)
                {
                    Logger.WriteLog("time out and newWindowThread Abort error: " + ex.Message);
                    MessageBox.Show("get download information time out, please copy the downloadURL to the address bar to download");
                }
            }
        }
        void ShowDownloadZipWindow()
        {
            try
            {
                Window downlaodZipWindow = new DownloadZip();
                downlaodZipWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                downlaodZipWindow.Show();
                System.Windows.Threading.Dispatcher.Run();
            }
            catch (Exception ex)
            {
                Logger.WriteLog("Show DownloadZipWindow error: " + ex.Message);
                if (!DBCrawlerGitDetailDataModelData.GetDownloadResposeComplete)
                {
                    MessageBox.Show("get download information failed, please copy the downloadURL to the address bar to download");
                }
            }
        }
        void UpdateTimeCost(object sender, ProgressChangedEventArgs e)
        {
            int progress = e.ProgressPercentage;
            timeCost.Text = string.Format("{0}", progress);
        }
        void EmptyTimeCostInfo(object sender, RunWorkerCompletedEventArgs e)
        {
            //if (e.Error != null)
            //{
            //    MessageBox.Show("Error");
            //}
            //else if (e.Cancelled)
            //{
            //    MessageBox.Show("Canceled");
            //}
            //else
            //{
            //    MessageBox.Show("Completed");
            //}
            downloadInfo.Text = "";
            timeCost.Text = "";
        }

        private void Copy_RepositoryPath(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(repositoryPath.Text);
        }

        private void Copy_DownloadURL(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(downloadURL.Text);
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            DBCrawlerGitDetailDataModelData.GetDownloadResposeComplete = true;
            if (newWindowThread != null)
            {
                try
                {
                    newWindowThread.Abort();
                }
                catch (Exception ex)
                {
                    Logger.WriteLog("MainWindow_Closed newWindowThread Abort error: " + ex.Message);
                }
            }
        }
    }
}
