using System.Collections.Generic;
using System.Diagnostics;
using RSJWYFamework.Runtime.Module;

namespace RSJWYFamework.Runtime.AsyncOperation
{
    /// <summary>
    /// 异步系统
    /// 参考Yooasset文件管理
    /// </summary>
    public static class RAsyncOperationSystem
    {
        /// <summary>
        /// 异步操作列表
        /// </summary>
        private static readonly List<RAsyncOperationBase> _operations = new(1000);

        /// <summary>
        /// 新加入的异步操作列表
        /// </summary>
        private static readonly List<RAsyncOperationBase> _newList = new(1000);

        // 计时器相关
        private static Stopwatch _watch;
        private static long _frameTime;

        /// <summary>
        /// 异步操作的最小时间片段
        /// </summary>
        public static long MaxTimeSlice { set; get; } = long.MaxValue;

        /// <summary>
        /// 处理器是否繁忙
        /// </summary>
        public static bool IsBusy
        {
            get { return _watch.ElapsedMilliseconds - _frameTime >= MaxTimeSlice; }
        }


        /// <summary>
        /// 销毁异步操作系统
        /// </summary>
        static void DestroyAll()
        {
            _operations.Clear();
            _newList.Clear();
            _watch = null;
            _frameTime = 0;
            MaxTimeSlice = long.MaxValue;
        }

        /// <summary>
        /// 销毁指定名称的任务
        /// </summary>
        public static void ClearAsyncOperation(string AsyncOperationName)
        {
            // 终止临时队列里的任务
            foreach (var operation in _newList)
            {
                if (operation._AsyncOperationName == AsyncOperationName)
                {
                    operation.SetAbort();
                }
            }

            // 终止正在进行的任务
            foreach (var operation in _operations)
            {
                if (operation._AsyncOperationName == AsyncOperationName)
                {
                    operation.SetAbort();
                }
            }
        }

        /// <summary>
        /// 开始处理异步操作类
        /// </summary>
        public static void StartOperation(string AsyncOperationName, RAsyncOperationBase operation)
        {
            _newList.Add(operation);
            operation._AsyncOperationName=AsyncOperationName;
            operation.SetStart();
        }

        public static void Init()
        {
            _watch = Stopwatch.StartNew();
        }

        public static void Close()
        {
            DestroyAll();
        }

        /// <summary>
        /// 更新异步操作系统
        /// </summary>
        public static void Update(float time, float deltaTime)
        {
            _frameTime = _watch.ElapsedMilliseconds;

            // 添加新增的异步操作
            if (_newList.Count > 0)
            {
                bool sorting = false;
                foreach (var operation in _newList)
                {
                    if (operation.Priority > 0)
                    {
                        sorting = true;
                        break;
                    }
                }
                
                _operations.AddRange(_newList);
                _newList.Clear();

                // 重新排序优先级
                if (sorting)
                    _operations.Sort();
            }

            // 更新进行中的异步操作
            for (int i = 0; i < _operations.Count; i++)
            {
                //繁忙时跳过
                if (IsBusy)
                    break;
                //操作是否是完成的
                var operation = _operations[i];
                if (operation.IsFinish)
                    continue;
                //操作未结束时，每帧刷新
                if (operation.IsDone == false)
                    operation.InternalOnUpdate(time,deltaTime);
                //操作结束设置操作完成
                if (operation.IsDone)
                    operation.SetFinish();
            }

            // 移除已经完成的异步操作
            for (int i = _operations.Count - 1; i >= 0; i--)
            {
                var operation = _operations[i];
                if (operation.IsFinish)
                    _operations.RemoveAt(i);
            }
        }

        public static void UpdatePerSecond(float time)
        {
            for (int i = 0; i < _operations.Count; i++)
            {
                _operations[i].InternalOnUpdatePerSecond(time);
            }
        }
    }
}