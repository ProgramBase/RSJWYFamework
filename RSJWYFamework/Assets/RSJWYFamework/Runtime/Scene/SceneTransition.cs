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
                    progressText.text = $"{value}%";
                    loadingBar.localScale = new Vector3((float)value / 100, 1, 1);
                }

            }
        }

        /// <summary>
        /// 切换到中转
        /// </summary>
        public async UniTask ToTransferScene(float time=0.15f)
        {
            canvasGroup.alpha = 0;
            Progress = 0;
            gameObject.SetActive(true);
            //await DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1, time).AsyncWaitForCompletion();
            canvasGroup.alpha = 1;

            await UniTask.Yield(PlayerLoopTiming.Update);
        }
        /// <summary>
        /// 切换到下一场景
        /// </summary>
        public async UniTask ToNextScene(float time=0.15f)
        {
            canvasGroup.alpha = 1;
            //await DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0, time).AsyncWaitForCompletion();
            gameObject.SetActive(false);
            
            await UniTask.Yield(PlayerLoopTiming.Update);
            canvasGroup.alpha = 0;
            Progress = 0;
        }
        /// <summary>
        /// 更新进度条
        /// </summary>
        /// <param name="progress">进度</param>
        /// <param name="info">进度信息</param>
        public void UpdateProgress(int progress, string info)
        {
            Progress=progress;
            this.info.text = info;
        }
    }
}