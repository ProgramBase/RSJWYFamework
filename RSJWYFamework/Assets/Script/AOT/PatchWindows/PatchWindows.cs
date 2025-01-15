using System;
using RSJWYFamework.Runtime.Event;
using RSJWYFamework.Runtime.Main;
using UnityEngine;
using UnityEngine.UI;

namespace Script.AOT.PatchWindows
{
    public class PatchWindows : MonoBehaviour
    {
        [SerializeField]
        private GameObject _messageBoxObj;

        [SerializeField] 
        private Text _messageBoxText;
        
        [SerializeField]
        private Slider _slider;
        
        [SerializeField]
        private Text Tips;

        private void Awake()
        {
            _messageBoxObj.SetActive(false);
        }

        private void OnEnable()
        {
            Main.EventModle.BindEventRecord<PatchEventDefine.UpdateProgressEvent>(OnHandleEventMessage);
        }

        private void OnDisable()
        {
            Main.EventModle.UnBindEventRecord<PatchEventDefine.UpdateProgressEvent>(OnHandleEventMessage);
        }

        void OnHandleEventMessage(object sender, RecordEventArgsBase recordEventArgsBase)
        {
            if (recordEventArgsBase is PatchEventDefine.UpdateProgressEvent updateProgressEvent)
            {
                ShowProgress(updateProgressEvent.progress,updateProgressEvent.tips);
            }
            else if (recordEventArgsBase is PatchEventDefine.ShowMessageEvent showMessageEvent)
            {
                ShowMessage(showMessageEvent.tips);
            }
        }
        
        public void ShowMessage(string message)
        {
            _messageBoxText.text = message;
            _messageBoxObj.SetActive(true);
        }

        public void ShowProgress(float progress, string tips)
        {
            if (progress<=0)
            {
                _slider.value = 0;
            }
            else if (progress >= 1)
            {
                _slider.value = 1;
            }
            else
            {
                _slider.value = progress;
            }
            Tips.text = tips;
        }
    }
}
