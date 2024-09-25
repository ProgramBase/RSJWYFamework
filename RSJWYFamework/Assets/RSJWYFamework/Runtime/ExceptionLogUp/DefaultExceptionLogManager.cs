using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Logger;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Module;
using UnityEngine;

namespace RSJWYFamework.Runtime.ExceptionLogUp
{
    /// <summary>
    /// 日志回传处理程序
    /// </summary>
    public class DefaultExceptionLogManager:IModule
    {
        /// <summary>
        /// 上传日志队列
        /// </summary>
        private ConcurrentQueue<ExceptionLogInfo> _UpLoadExpInfo;
        /// <summary>
        /// 保存本地日志队列
        /// </summary>
        private ConcurrentQueue<ExceptionLogInfo> _SaveLogFileExpInfo;


        public void Init()
        {
            _UpLoadExpInfo = new();
            _SaveLogFileExpInfo = new();
            Application.logMessageReceivedThreaded += UnityLogMessageReceivedThreadedEvent;
            CheckQueue().Forget();
        }


        public void Close()
        {
            Application.logMessageReceivedThreaded -= UnityLogMessageReceivedThreadedEvent;
        }

        public void Update(float time, float deltaTime)
        {
            
        }

        public void UpdatePerSecond(float time)
        {
            
        }

        async UniTaskVoid CheckQueue()
        {
            await UniTask.SwitchToThreadPool();
            while (true)
            {
                //每1秒检查一次队列
                await UniTask.WaitForSeconds(1);
                try
                {
                    if (_UpLoadExpInfo.Count < 1) continue;
                    _UpLoadExpInfo.TryDequeue(out var _d);
                    using HttpClient client = new HttpClient();
                    // 目标URL
                    string url = $"回传日志网址";
                    // 创建请求内容
                    var postData = new Dictionary<string, string>
                    {
                        { "ProjectName", $"{Application.companyName}" },
                        { "AppName", $"{Application.productName}" },
                        { "AppVersion", $"{Application.version}" },
                        { "ResourceInfo", $"{Utility.Utility.GetTimeStamp()}" },
                        { "ERRTime", $"{Utility.Utility.GetTimeStamp()}" },
                        { "ERRType", $"{_d.Type}" },
                        { "ERRLog", $"{_d.LogString}" },
                        { "ERRStackTrace", $"{_d.StackTrace}" }
                    };
                    var content = new FormUrlEncodedContent(postData);

                    // 发送POST请求
                    HttpResponseMessage response = await client.PostAsync(url, content);

                    // 确保HTTP成功状态值
                    //response.EnsureSuccessStatusCode();

                    // 读取并输出响应内容
                    //string responseBody = await response.Content.ReadAsStringAsync();
                    //RSJWYLogger.Log(RSJWYFameworkEnum.ExceptionLogManager, responseBody);
                }
                catch (HttpRequestException e)
                {
                    RSJWYLogger.Warning
                        (RSJWYFameworkEnum.ExceptionLogManager, $"提交日志时出错，写入本地和上传，准备汇报，错误日志\n{e}");
                    await UniTask.WaitForSeconds(1);
                }
            }
        }

        public void UnityLogMessageReceivedThreadedEvent(string log, string stacktrace, LogType type)
        {
            if (type==LogType.Log)return;
            var _log = new ExceptionLogInfo
            {
                Type = type,
                LogString = log,
                StackTrace = stacktrace
            };
            
            _UpLoadExpInfo.Enqueue(_log);
            _SaveLogFileExpInfo.Enqueue(_log);
        }
    }
}
