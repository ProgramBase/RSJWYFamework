using Cysharp.Threading.Tasks;
using RSJWYFamework.Runtime.Event;
using UnityEngine;
using YooAsset;

namespace RSJWYFamework.Runtime.Tips
{
    public class TipsManager : MonoBehaviour
    {
        // Start is called before the first frame update
        private AssetHandle tipsAH;

        private async void Awake()
        {
            tipsAH= YooAssets.LoadAssetSync<GameObject>("Prefab_TipsItem");
            await tipsAH.ToUniTask();
        }

        private void OnEnable()
        {
            Main.Main.EventModle.BindEvent<TipsInfoEventArgs>(TipsEvent);
        } 
        private void OnDisable()
        {
            Main.Main.EventModle.UnBindEvent<TipsInfoEventArgs>(TipsEvent);
        }
        private void TipsEvent(object sender, EventArgsBase e)
        {
            if (e is TipsInfoEventArgs args)
            {
                LoadItem(args.tips).Forget();
            }
        }

        async UniTask LoadItem(string str)
        {
            var TipsIO = tipsAH.InstantiateAsync(transform);
            await TipsIO.ToUniTask();
            //TipsIO.Result.GetComponent<TipsItem>().SetTips(str);
        }

        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
