using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndexGitCrawlerData
{
    class DBCrawlerGitDetailDataModel : IComparable
    {
        public string repositoryPath { get; set; }
        public string downloadURL { get; set; }
        public int impressionCount { get; set; }
        public int clickCount { get; set; }
        public string repositoryContent { get; set; }
        public List<string> topicsList { get; set; }
        public string readmeFileContent { get; set; }
        public DBCrawlerGitDetailDataModel(
            string repositoryPath,
            string downloadURL,
            int impressionCount,
            int clickCount,
            string repositoryContent,
            List<string> topicsList,
            string readmeFileContent)
        {
            this.repositoryPath = repositoryPath;
            this.downloadURL = downloadURL;
            this.impressionCount = impressionCount;
            this.clickCount = clickCount;
            this.repositoryContent = repositoryContent;
            this.topicsList = topicsList;
            this.readmeFileContent = readmeFileContent;
        }

        public int CompareTo(object obj)
        {
            DBCrawlerGitDetailDataModel compareModel = (DBCrawlerGitDetailDataModel)obj;
            if (DBCrawlerGitDetailDataModelData.IndexRepositoryContentList == null || DBCrawlerGitDetailDataModelData.IndexRepositoryContentList.Count == 0)
            {
                return compareByCount(compareModel);
            }
            else
            {
                int maxLen = DBCrawlerGitDetailDataModelData.IndexRepositoryContentList.Count;
                for (int len = maxLen; len > 0; len--)
                {
                    List<string> IndexRepositoryContentListCurrent = new List<string>();
                    for (int startIndex = 0; startIndex <= maxLen - len; startIndex++)
                    {
                        string indexRepositoryContentCurrentElement = "";
                        for (int curIndex = 0; curIndex < len; curIndex++)
                        {
                            if (curIndex > 0)
                            {
                                indexRepositoryContentCurrentElement += " ";
                            }
                            indexRepositoryContentCurrentElement += DBCrawlerGitDetailDataModelData.IndexRepositoryContentList[startIndex + curIndex];
                        }
                        IndexRepositoryContentListCurrent.Add(indexRepositoryContentCurrentElement);
                    }
                    int thisContainConut = 0;
                    int compareContainConut = 0;
                    foreach (string indexRepositoryContentElement in IndexRepositoryContentListCurrent)
                    {
                        if (this.repositoryContent.Contains(indexRepositoryContentElement))
                        {
                            thisContainConut++;
                        }
                        if (compareModel.repositoryContent.Contains(indexRepositoryContentElement))
                        {
                            compareContainConut++;
                        }
                    }
                    if (compareContainConut != thisContainConut)
                    {
                        return compareContainConut - thisContainConut;
                    }
                    if (compareContainConut > 0)
                    {
                        return compareByCount(compareModel);
                    }

                }
                return compareByCount(compareModel);
            }
        }
        private int compareByCount(DBCrawlerGitDetailDataModel compareModel)
        {
            if (this.impressionCount > compareModel.impressionCount)
            {
                return -1;
            }
            else if (this.impressionCount < compareModel.impressionCount)
            {
                return 1;
            }
            else
            {
                if (this.clickCount > compareModel.clickCount)
                {
                    return -1;
                }
                else if (this.clickCount < compareModel.clickCount)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }

    static class DBCrawlerGitDetailDataModelData
    {
        public static List<DBCrawlerGitDetailDataModel> ModelList { get; set; }
        public static DBCrawlerGitDetailDataModel CurrentModel { get; set; }
        public static bool GetDownloadResposeComplete { get; set; }
        public static List<string> IndexRepositoryContentList { get; set; }
    }
}
