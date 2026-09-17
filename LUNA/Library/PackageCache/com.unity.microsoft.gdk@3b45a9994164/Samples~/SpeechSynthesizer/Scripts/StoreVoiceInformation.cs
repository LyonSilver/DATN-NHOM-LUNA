using System;
using Unity.XGamingRuntime;
using UnityEngine;
using UnityEngine.UI;

namespace GdkSample_SpeechSynthesizer
{
    public class StoreVoiceInformation : MonoBehaviour
    {
        public Text Description;
        public Text DisplayName;
        public Text Gender;
        public Text VoiceId;
        public Text Language;
        [NonSerialized]
        public XSpeechSynthesizerVoiceInformation VoiceInformation;
        [NonSerialized]
        public SpeechSynthesizerManager SpeechSynthesizerManager;

        private void Start()
        {
            gameObject.GetComponent<Button>().onClick.AddListener(SetCustomVoiceButton);
        }

        private void SetCustomVoiceButton()
        {
            SpeechSynthesizerManager.SpeechSynthesizerSetCustomVoice(this);
        }
    }
}
