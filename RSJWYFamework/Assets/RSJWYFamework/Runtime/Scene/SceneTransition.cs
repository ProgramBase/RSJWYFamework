using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace RSJWYFamework.Runtime.Scene
{
    internal class SceneTransition : MonoBehaviour
    {
        [SerializeField]
        CanvasGroup canvasGroup;

        [SerializeField]
        Transform loadingBar;
        
        [SerializeField]
        Text info;

        [SerializeField]
        Text progressText;

        int progress = 0;
        
        int Progress
        {
            get
            {
                return progress;
            }
            set
            {
                if (value>=100)
                {
                    progress = 100;
                }
                else if (value<=0)
                {
                    progress = 0;
                }
                else
                {
                    progress = value;
                }

            }
        }

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 切换到中转
        /// </summary>
        public async UniTask ToTransferScene(float time=0.15f)
        {
            canvasGroup.alpha = 0;
            gameObject.SetActive(true);
            await DOTween.To(
                () => canvasGroup.alpha, x => canvasGroup.alpha = x, 1, time)
                .AsyncWaitForCompletion();
            canvasGroup.alpha = 1;
        }
        /// <summary>
        /// 切换到下一场景
        /// </summary>
        public async UniTask ToNextScene(float time=0.15f)
        {
            canvasGroup.alpha = 1;
            gameObject.SetActive(true);
            await DOTween.To(
                    () => canvasGroup.alpha, x => canvasGroup.alpha = x, 0, time)
                .AsyncWaitForCompletion();
            canvasGroup.alpha = 0;
            gameObject.SetActive(false);
        }
    }
}