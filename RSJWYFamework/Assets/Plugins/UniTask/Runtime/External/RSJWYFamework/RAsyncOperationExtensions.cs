using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.AsyncOperation;

namespace Plugins.UniTask.Runtime.External.RSJWYFamework
{
    public class RAsyncOperationExtensions
    {
        public static Cysharp.Threading.Tasks.UniTask ToUniTask(RAsyncOperationBase operation)
        {
            var tcs = new UniTaskCompletionSource();
            return tcs.Task;
        }
    }
}