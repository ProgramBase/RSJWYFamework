using System.Collections.Generic;
using System.Diagnostics;
using RSJWYFamework.Runtime.Module;

namespace RSJWYFamework.Runtime.AsyncOperation
{
    /// <summary>
    /// 异步系统
    /// 参考Yooasset文件管理
    /// </summary>
    public class RAsyncOperationSystem:IModule,ILife
    {
        /// <summary>
        /// 异步操作列表
        /// </summary>
        private readonly List<RAsyncOperationBase> _operations = new(1000);

        /// <summary>
        /// 新加入的异步操作列表
        /// </summary>
        private readonly List<RAsyncOperationBase> _newList = new(1000);

        // 计时器相关
        private Stopwatch _watch;
        private long _frameTime;

        /// <summary>
        /// 异步操作的最小时间片段
        /// </summary>
        public long MaxTimeSlice { set; get; } = long.MaxValue;

        /// <summary>
        /// 处理器是否繁忙
        /// </summary>
        public bool IsBusy
        {
            get { return _watch.ElapsedMilliseconds - _frameTime >= MaxTimeSlice; }
        }


        /// <summary>
        /// 销毁异步操作系统
        /// </summary>
        public void DestroyAll()
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
        public void ClearAsyncOperation(string AsyncOperationName)
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
        public void StartOperation(string AsyncOperationName, RAsyncOperationBase operation)
        {
            _newList.Add(operation);
            operation._AsyncOperationName=AsyncOperationName;
            operation.SetStart();
        }

        public void Init()
        {
            _watch = Stopwatch.StartNew();
        }

        public void Close()
        {
            DestroyAll();
        }

        /// <summary>
        /// 更新异步操作系统
        /// </summary>
        public void Update(float time, float deltaTime)
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

        public void UpdatePerSecond(float time)
        {
            for (int i = 0; i < _operations.Count; i++)
            {
                _operations[i].InternalOnUpdatePerSecond(time);
            }
        }

        public void FixedUpdate()
        {
            
        }

        public void LateUpdate()
        {
        }

        public uint Priority()
        {
            return 0;
        }
    }
}