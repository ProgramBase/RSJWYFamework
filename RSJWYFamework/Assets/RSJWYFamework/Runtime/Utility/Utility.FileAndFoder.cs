using System.IO;

namespace RSJWYFamework.Runtime.Utility
{
    public static partial class Utility
    {
        /// <summary>
        /// 文件文件夹工具
        /// </summary>
        public static class FileAndFoder
        {
            /// <summary>
            /// 检测文件目录是否存在并创建
            /// </summary>
            /// <param name="folderPathName">文件夹路径</param>
            public static void CheckDirectoryExistsAndCreate(string folderPathName)
            {
                if (!Directory.Exists(folderPathName))
                {
                    Directory.CreateDirectory(folderPathName);
                }
            }

            /// <summary>
            /// 检测文件是否存在并创建
            /// </summary>
            /// <param name="fileName">文件夹路径</param>
            public static void CheckFileExistsAndCreate(string fileName)
            {
                if (!File.Exists(fileName))
                {
                    File.Create(fileName);
                }
            }
            /// <summary>
            /// 创建文件，如果路径文件夹不存在，则创建
            /// </summary>
            /// <param name="folderPathName">文件路径</param>
            /// <param name="fileName">文件名</param>
            public static void CheckDirectoryAndFileCreate(string folderPathName,string fileName)
            {
                CheckDirectoryExistsAndCreate(folderPathName);
                CheckFileExistsAndCreate($"{folderPathName}/{fileName}");
            }
        }
    }
}