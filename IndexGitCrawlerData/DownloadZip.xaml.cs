using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
    /// DownloadZip.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadZip : Window
    {
        HttpWebResponse response;
        public DownloadZip()
        {
            InitializeComponent();
            string fileName = GetFileNameAndResponse(DBCrawlerGitDetailDataModelData.CurrentModel.downloadURL);
            downloadPath.Text = System.IO.Path.Combine(Environment.CurrentDirectory, fileName);
            DBCrawlerGitDetailDataModelData.GetDownloadResposeComplete = true;
        }

        private void Download_Zip(object sender, RoutedEventArgs e)
        {
            try
            {
                DownloadFile(downloadPath.Text);
                MessageBox.Show("download success!!!");
            }catch (Exception ex)
            {
                Logger.WriteLog("Download Zip error: " + ex.Message);
                MessageBox.Show("download failed, please copy the downloadURL to the address bar to download");
            }
        }
        private string GetFileNameAndResponse(string url)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            response = request.GetResponse() as HttpWebResponse;
            string fileName = response.Headers["Content-Disposition"];//attachment;filename=FileName.txt
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = response.ResponseUri.Segments[response.ResponseUri.Segments.Length - 1];
            }
            else
            {
                fileName = fileName.Remove(0, fileName.IndexOf("filename=") + 9);
            }
            //string filePath = System.IO.Path.Combine(path, fileName);
            return fileName;
        }
        private void DownloadFile(string filePath)
        {
            using (Stream responseStream = response.GetResponseStream())
            {
                long totalLength = response.ContentLength;
                //using (Stream stream = new FileStream(filePath, overwrite ? FileMode.Create : FileMode.CreateNew))
                using (Stream stream = new FileStream(filePath, FileMode.CreateNew))
                {
                    byte[] bArr = new byte[1024];
                    int size;
                    while ((size = responseStream.Read(bArr, 0, bArr.Length)) > 0)
                    {
                        stream.Write(bArr, 0, size);
                    }
                }
            }
        }
    }
}
