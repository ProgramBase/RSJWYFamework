using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace RSJWYFamework.Runtime.Scene
{
    internal class SceneTransition : MonoBehaviour
    {
        private static GameObject m_canvas;

        private GameObject m_overlay;

        private void Awake()
        {
            // Create a new, ad-hoc canvas that is not destroyed after loading the new scene
            // to more easily handle the fading code.
            m_canvas = new GameObject("TransitionCanvas");
            var canvas = m_canvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            DontDestroyOnLoad(m_canvas);
        }
        /// <summary>
        /// 获取实例对象
        /// </summary>
        /// <returns></returns>
        public static SceneTransition Instance()
        {
            var fade = new GameObject("Transition");
            fade.AddComponent<SceneTransition>();
            fade.transform.SetParent(m_canvas.transform, false);
            fade.transform.SetAsLastSibling();
            return fade.GetComponent<SceneTransition>();
        }
        
        /// <summary>
        /// 切换到中转
        /// </summary>
        public async UniTask ToTransferScene()
        {
            await UniTask.CompletedTask;
        }
        /// <summary>
        /// 切换到下一场景
        /// </summary>
        public async UniTask ToNextScene()
        {
            await UniTask.CompletedTask;
        }
    }
}