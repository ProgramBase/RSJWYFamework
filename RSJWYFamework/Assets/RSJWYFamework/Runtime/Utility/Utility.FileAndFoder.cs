using System.IO;
using RSJWYFamework.Runtime.ExceptionLogManager;
using RSJWYFamework.Runtime.Main;

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
            /// <summary>
            /// 创建文件，如果路径文件夹不存在，则创建
            /// </summary>
            /// <param name="FolderORFilePath">文件或者文件夹路径</param>
            public static void CheckDirectoryAndFileCreate(string FolderORFilePath)
            {
                // 检查路径是否包含文件名
                if (!File.Exists(FolderORFilePath) || !Directory.Exists(FolderORFilePath))
                {
                    //如果不包含有效的文件夹或者文件
                    //提取目录，检测是否存在并创建
                    string directoryPath = Path.GetDirectoryName(FolderORFilePath);
                    CheckDirectoryExistsAndCreate(directoryPath);
                    //检测文件是否存在并创建
                    CheckFileExistsAndCreate(FolderORFilePath);
                }
            }
            /// <summary>
            /// 清空文件夹
            /// </summary>
            /// <param name="directoryPath"></param>
            /// <exception cref="DirectoryNotFoundException"></exception>
            public static void ClearDirectory(string directoryPath)
            {
                // 验证路径是否指向一个文件夹
                if (!Directory.Exists(directoryPath))
                {
                    throw new RSJWYException(RSJWYFameworkEnum.Utility,$"The directory '{directoryPath}' does not exist.");
                }

                // 获取文件夹内的所有文件（包括子文件夹中的文件）
                string[] files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);

                // 删除所有文件
                foreach (string file in files)
                {
                    File.Delete(file);
                }

                // 获取文件夹内的所有子文件夹
                string[] directories = Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories);

                // 删除所有子文件夹
                foreach (string dir in directories)
                {
                    Directory.Delete(dir, true); // true 表示递归删除所有子目录和文件
                }
            }
        }
    }
}