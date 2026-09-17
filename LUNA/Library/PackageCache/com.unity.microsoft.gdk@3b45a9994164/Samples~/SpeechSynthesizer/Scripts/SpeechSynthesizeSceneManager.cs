using System;
using System.Collections.Generic;
using Unity.XGamingRuntime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GdkSample_SpeechSynthesizer
{
    public class SpeechSynthesizeSceneManager : MonoBehaviour
    {
        public GameObject m_EditEndFocusGOInitialized;
        public GameObject m_EditEndFocusGONotInitialized;

        public GameObject m_VoiceInformationDash;
        public List<GameObject> m_VoiceInformationUIDashGO = new List<GameObject>();
        public ScrollRect m_VoiceInformationUIScrollRect;

        public AudioSource m_AudioSource;

        public InputField m_InputFieldButton;
        public Button m_SpeechSynthesizerCreateButton;
        public Button m_PlayEnteredTextButton;
        public Button m_PlayFromSSMLButton;
        public Button m_CloseSpeechSynthButton;
        public Text m_UserText;
        public Text m_SsmlText;

        public bool m_SpeechSynthesizeCreated;

        [SerializeField]
        private EventSystem m_EventSystem;

        private void LateUpdate()
        {
            if (m_PlayEnteredTextButton != null)
                m_PlayEnteredTextButton.interactable = m_SpeechSynthesizeCreated;

            if (m_PlayFromSSMLButton != null)
                m_PlayFromSSMLButton.interactable = m_SpeechSynthesizeCreated;

            if (m_CloseSpeechSynthButton != null)
            {
                if (m_CloseSpeechSynthButton.IsActive() && !m_SpeechSynthesizeCreated && m_SpeechSynthesizerCreateButton != null)
                    m_SpeechSynthesizerCreateButton.Select();

                m_CloseSpeechSynthButton.interactable = m_SpeechSynthesizeCreated;
            }

            if (m_InputFieldButton != null)
                m_InputFieldButton.interactable = m_SpeechSynthesizeCreated;
        }

        public void CreateVoiceInformationDash(List<XSpeechSynthesizerVoiceInformation> voiceInformationList, SpeechSynthesizerManager speechSynthesizerManager)
        {
            foreach (var item in voiceInformationList)
            {
                GameObject tempObject = Instantiate(m_VoiceInformationDash);
                m_VoiceInformationUIDashGO.Add(tempObject);
                tempObject.transform.SetParent(m_VoiceInformationUIScrollRect.content.gameObject.transform, false);
                tempObject.SetActive(true);

                StoreVoiceInformation storeVoiceInformation = tempObject.gameObject.GetComponent<StoreVoiceInformation>();
                storeVoiceInformation.SpeechSynthesizerManager = speechSynthesizerManager;
                storeVoiceInformation.Description.text = item.Description;
                storeVoiceInformation.DisplayName.text = item.DisplayName;
                storeVoiceInformation.Gender.text = item.Gender.ToString();
                storeVoiceInformation.VoiceId.text = item.VoiceId;
                storeVoiceInformation.Language.text = item.Language;
                storeVoiceInformation.VoiceInformation = item;
            }
            m_VoiceInformationUIScrollRect.verticalNormalizedPosition = 0;
        }

        public string SsmlUI(string voice)
        {
            //https://docs.microsoft.com/en-us/dotnet/api/system.speech.synthesis.speechsynthesizer.speakssml?view=netframework-4.8

            DateTime dt = DateTime.Now;

            string language = voice;

            string str = "<speak version=\"1.0\"\n";
            str += " xmlns=\"http://www.w3.org/2001/10/synthesis\"\n";
            str += " xml:lang=\"" + language + "\"\n >";
            str += "<say-as type=\"date:mdy\"> " + dt.Date + "</say-as>\n";
            str += "</speak>";

            m_SsmlText.text = str;
            return str;
        }

        public void DestroyDash()
        {
            foreach (var item in m_VoiceInformationUIDashGO)
            {
                Destroy(item);
            }
            m_VoiceInformationUIDashGO.Clear();
        }

        public void OnEditEndSelectFocusObject()
        {
            if (m_EditEndFocusGOInitialized && m_EditEndFocusGONotInitialized)
            {
                if (m_SpeechSynthesizeCreated)
                    EventSystem.current.SetSelectedGameObject(m_EditEndFocusGOInitialized);
                else
                    EventSystem.current.SetSelectedGameObject(m_EditEndFocusGONotInitialized);
            }
        }
    }
}