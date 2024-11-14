//关注 微信公众号：【呆呆敲代码的小Y】，一起学习、获取更多游戏开发相关资源。

using DG.Tweening;
using TMPro;
using UnityEngine;

namespace RSJWYFamework.Runtime.Tips
{
    public class TipsItem : MonoBehaviour
    {
        [SerializeField] 
        private TextMeshProUGUI tipsTMP;

        public void SetTips(string _tips)
        {
            tipsTMP.text = _tips;
            gameObject.SetActive(true);
        }

        private void OnEnable()
        {
            transform.DOLocalMoveY(100,2f).OnComplete(() =>
            {
                Destroy(gameObject);
            });
        }
    }
}