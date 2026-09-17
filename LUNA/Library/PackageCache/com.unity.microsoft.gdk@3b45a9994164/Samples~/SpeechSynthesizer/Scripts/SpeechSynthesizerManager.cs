using System;
using System.Collections.Generic;
using Unity.XGamingRuntime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GdkSample_SpeechSynthesizer
{
    public class SpeechSynthesizerManager : MonoBehaviour
    {
        private enum State
        {
            XSpeechSynthesizerCloseHandle,              //Closes the speech synthesizer and releases allocated system resources.
            XSpeechSynthesizerCloseStreamHandle,        //Closes the speech synthesizer stream and releases allocated system resources.
            XSpeechSynthesizerCreate,                   //Creates a speech synthesizer.
            XSpeechSynthesizerCreateStreamFromSsml,     //Creates a speech synthesis stream from the specified SSML.
            XSpeechSynthesizerCreateStreamFromText,     //Creates a speech synthesis stream from the specified plain text.
            XSpeechSynthesizerEnumerateInstalledVoices, //Enumerates the installed voices, and calls the method pointed to by callback for each voice.
            XSpeechSynthesizerGetStreamData,            //Retrieves the data from a speech synthesis stream.
            XSpeechSynthesizerGetStreamDataSize,        //Gets the size of the data buffer from a speech synthesis stream.
            XSpeechSynthesizerInstalledVoicesCallback,  //The client-implemented callback function that receives information about a voice when XSpeechSynthesizerEnumerateInstalledVoices is called.
            XSpeechSynthesizerSetCustomVoice,           //Specifies that the speech synthesizer is to use the specified custom voice.
            XSpeechSynthesizerSetDefaultVoice,          //Specifies that the speech synthesizer is to use the system’s default voice.
            WaitForNextTask,
            Play,
            Error,
            Idle,
            End
        }

        private State m_State = State.Idle;
        private XSpeechSynthesizerHandle m_XSpeechSynthesizerHandle;
        private XSpeechSynthesizerStreamHandle m_XSpeechSynthesizerStreamHandle;
        private XSpeechSynthesizerVoiceInformation[] m_XSpeechSynthesizerVoiceInformation;
        private AudioClip m_SpeechSynthesizerClip;
        private SpeechSynthesizeSceneManager m_SpeechSynthesizeSceneManager;

        [NonSerialized]
        public List<XSpeechSynthesizerVoiceInformation> m_VoiceInformation = new List<XSpeechSynthesizerVoiceInformation>();

        private void Start()
        {
            GDKGameRuntime.TryInitialize();
#if UNITY_6000_5_OR_NEWER
            m_SpeechSynthesizeSceneManager = FindAnyObjectByType<SpeechSynthesizeSceneManager>();
#else
            m_SpeechSynthesizeSceneManager = FindFirstObjectByType<SpeechSynthesizeSceneManager>();
#endif

            if (Mouse.current == null || !Mouse.current.enabled)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        private void OnDestroy()
        {
            if (m_XSpeechSynthesizerHandle != null)
            {
                SDK.XSpeechSynthesizerCloseHandle(m_XSpeechSynthesizerHandle);
                m_XSpeechSynthesizerHandle = null;
            }

            if (m_XSpeechSynthesizerStreamHandle != null)
            {
                SDK.XSpeechSynthesizerCloseStreamHandle(m_XSpeechSynthesizerStreamHandle);
                m_XSpeechSynthesizerStreamHandle = null;
            }
        }

        public void Update()
        {
            switch (m_State)
            {
                case State.XSpeechSynthesizerCloseHandle:
                    m_State = State.WaitForNextTask;
                    SpeechSynthesizerCloseHandle();
                    break;

                case State.XSpeechSynthesizerCloseStreamHandle:
                    m_State = State.WaitForNextTask;
                    SpeechSynthesizerCloseStreamHandle();
                    break;

                case State.XSpeechSynthesizerCreate:
                    m_State = State.WaitForNextTask;
                    SpeechSynthesizerCreate();
                    break;

                case State.XSpeechSynthesizerCreateStreamFromSsml:
                    m_State = State.WaitForNextTask;
                    SpeechSynthesizerCreateStreamFromSsml();
                    break;

                case State.XSpeechSynthesizerCreateStreamFromText:
                    m_State = State.WaitForNextTask;
                    SpeechSynthesizerCreateStreamFromText();
                    break;

                case State.XSpeechSynthesizerEnumerateInstalledVoices:
                    m_State = State.WaitForNextTask;
                    SpeechSynthesizerEnumerateInstalledVoices();
                    break;

                case State.XSpeechSynthesizerGetStreamData:
                    m_State = State.WaitForNextTask;
                    SpeechSynthesizerGetStreamData();
                    break;

                case State.XSpeechSynthesizerGetStreamDataSize:
                    //used in SpeechSynthesizerGetStreamData
                    break;

                case State.XSpeechSynthesizerInstalledVoicesCallback:
                    //used in XSpeechSynthesizerEnumerateInstalledVoices
                    break;

                case State.XSpeechSynthesizerSetCustomVoice:
                    break;

                case State.XSpeechSynthesizerSetDefaultVoice:
                    m_State = State.WaitForNextTask;
                    SpeechSynthesizerSetDefaultVoice();
                    break;

                case State.WaitForNextTask:
                    break;

                case State.Play:
                    m_State = State.WaitForNextTask;
                    break;

                case State.Error:
                    m_State = State.Idle;
                    break;

                case State.Idle:
                    m_State = State.Idle;
                    break;

                case State.End:
                    m_State = State.Idle;
                    break;
            }
        }

        private void SpeechSynthesizerCloseHandle()
        {
            if (m_SpeechSynthesizeSceneManager.m_SpeechSynthesizeCreated == true && m_XSpeechSynthesizerStreamHandle != null)
            {
                int hr = SDK.XSpeechSynthesizerCloseHandle(m_XSpeechSynthesizerHandle);
                if (hr != 0)
                {
                    Debug.Log("XSpeechSynthesizerCloseHandle failed, hresult: " + hr);
                    m_State = State.Error;
                }
            }

            m_State = State.End;
            ClearSpeechSynthesizer();
        }

        private void SpeechSynthesizerCloseStreamHandle()
        {
            // When CloseSpeechSynthesizer is called, a stream might not have been initialized yet.
            // But we still need to close the handle if it was initialized.
            m_State = State.XSpeechSynthesizerCloseHandle;

            if (m_SpeechSynthesizeSceneManager.m_SpeechSynthesizeCreated == false || m_XSpeechSynthesizerStreamHandle == null)
            {
                return;
            }

            int hr = SDK.XSpeechSynthesizerCloseStreamHandle(m_XSpeechSynthesizerStreamHandle);
            if (hr != 0)
            {
                Debug.Log("XSpeechSynthesizerCloseStreamHandle failed, hresult: " + hr);
                m_State = State.Error;
            }
        }

        public void CreateSpeechSynthesizer()
        {
            Debug.Log("Button Click: 'Create Speech Synthesizer'");
            m_State = State.XSpeechSynthesizerCreate;
        }

        private void SpeechSynthesizerCreate()
        {
            if (!m_SpeechSynthesizeSceneManager.m_SpeechSynthesizeCreated)
            {
                int hr = SDK.XSpeechSynthesizerCreate(out m_XSpeechSynthesizerHandle);
                if (hr == 0)
                {
                    m_SpeechSynthesizeSceneManager.m_SpeechSynthesizeCreated = true;
                    Debug.Log("XSpeechSynthesizer Created");
                }
                else
                {
                    Debug.Log($"XSpeechSynthesizerCreate failed, hresult: {hr} ({HR.NameOf(hr)})");
                    m_State = State.Error;
                }
                SpeechSynthesizerSetDefaultVoice();

                m_State = State.XSpeechSynthesizerEnumerateInstalledVoices;
            }
        }

        private void SpeechSynthesizerCreateStreamFromSsml()
        {
            if (m_SpeechSynthesizeSceneManager.m_SpeechSynthesizeCreated)
            {
                int hr = SDK.XSpeechSynthesizerCreateStreamFromSsml(
                m_XSpeechSynthesizerHandle,
                m_SpeechSynthesizeSceneManager.m_SsmlText.text,
                out m_XSpeechSynthesizerStreamHandle);
                if (hr != 0)
                {
                    Debug.Log("XSpeechSynthesizerCreateStreamFromSsml failed, hresult: " + hr);
                    m_State = State.Error;
                }

                m_State = State.XSpeechSynthesizerGetStreamData;
            }
        }

        private void SpeechSynthesizerCreateStreamFromText()
        {
            if (m_SpeechSynthesizeSceneManager.m_SpeechSynthesizeCreated)
            {
                if (m_SpeechSynthesizeSceneManager.m_UserText.text.Length > 0)
                {
                    int hr = SDK.XSpeechSynthesizerCreateStreamFromText(
                    m_XSpeechSynthesizerHandle,
                    m_SpeechSynthesizeSceneManager.m_UserText.text,
                    out m_XSpeechSynthesizerStreamHandle);
                    if (hr != 0)
                    {
                        Debug.Log("XSpeechSynthesizerCreateStreamFromText failed, hresult: " + hr);
                        m_State = State.Error;
                    }

                    m_State = State.XSpeechSynthesizerGetStreamData;
                }
                else
                {
                    Debug.Log("Enter Text first");
                }
            }
        }

        private void SpeechSynthesizerEnumerateInstalledVoices()
        {
            int hr = SDK.XSpeechSynthesizerEnumerateInstalledVoices(out m_XSpeechSynthesizerVoiceInformation);
            if (hr == 0)
            {
                SpeechSynthesizerInstalledVoices(m_XSpeechSynthesizerVoiceInformation);
            }
            else
            {
                Debug.Log("XSpeechSynthesizerEnumerateInstalledVoices failed, hresult: " + hr);
                m_State = State.Error;
            }

            m_State = State.WaitForNextTask;
        }

        private void SpeechSynthesizerInstalledVoices(XSpeechSynthesizerVoiceInformation[] m_VoiceInformation)
        {
            m_XSpeechSynthesizerVoiceInformation = m_VoiceInformation;
            PopulateVoiceInformation();
        }

        private void PopulateVoiceInformation()
        {
            for (int i = 0; i < m_XSpeechSynthesizerVoiceInformation.Length; i++)
            {
                m_VoiceInformation.Add(m_XSpeechSynthesizerVoiceInformation[i]);
            }

            if (m_VoiceInformation.Count > 0)
                m_SpeechSynthesizeSceneManager.CreateVoiceInformationDash(m_VoiceInformation, this);
        }

        private void SpeechSynthesizerGetStreamData()
        {
            int hr = SDK.XSpeechSynthesizerGetStreamData(m_XSpeechSynthesizerStreamHandle, out byte[] buffer);
            if (hr != 0)
            {
                Debug.Log("XSpeechSynthesizerGetStreamData failed, hresult: " + hr);
                m_State = State.Error;
            }

            PlaySpeechSynthesizerAudio(buffer);
        }

        public void SpeechSynthesizerSetCustomVoice(StoreVoiceInformation voiceInformation)
        {
            int hr = SDK.XSpeechSynthesizerSetCustomVoice(m_XSpeechSynthesizerHandle, voiceInformation.VoiceInformation.VoiceId);
            if (hr == 0)
            {
                m_SpeechSynthesizeSceneManager.SsmlUI(voiceInformation.VoiceInformation.Language);
            }
            else
            {
                Debug.Log("XSpeechSynthesizerSetCustomVoice failed, hresult: " + hr);

                m_State = State.Error;
            }
        }

        private void SpeechSynthesizerSetDefaultVoice()
        {
            int hr = SDK.XSpeechSynthesizerSetDefaultVoice(m_XSpeechSynthesizerHandle);
            if (hr != 0)
            {
                Debug.Log("XSpeechSynthesizerSetDefaultVoice failed, hresult: " + hr);
                m_State = State.Error;
            }
            else
            {
                m_SpeechSynthesizeSceneManager.SsmlUI("en-US");
            }
        }

        public void CreateSpeechFromText()
        {
            m_State = State.XSpeechSynthesizerCreateStreamFromText;
        }

        public void CreateSpeechFromSsml()
        {
            m_State = State.XSpeechSynthesizerCreateStreamFromSsml;
        }

        private void PlaySpeechSynthesizerAudio(byte[] buffer)
        {
            float[] convertedAudioBytes = ConvertByteToFloat(buffer);
            float[] samples = new float[convertedAudioBytes.Length * 4];
            Buffer.BlockCopy(convertedAudioBytes, 0, samples, 0, samples.Length);
            m_SpeechSynthesizerClip = AudioClip.Create("SpeechSynthesizerAudio", samples.Length, buffer[22], BitConverter.ToInt32(buffer, 24), false);
            m_SpeechSynthesizerClip.SetData(samples, 0);
            m_SpeechSynthesizeSceneManager.m_AudioSource.clip = m_SpeechSynthesizerClip;

            if (m_SpeechSynthesizerClip != null)
            {
                m_SpeechSynthesizeSceneManager.m_AudioSource.Play();
            }
            else
            {
                Debug.Log("Created SpeechSynthesizer Clip is null");
            }

            m_State = State.WaitForNextTask;
        }

        public void CloseSpeechSynthesizer()
        {
            m_State = State.XSpeechSynthesizerCloseStreamHandle;
        }

        public void ClearSpeechSynthesizer()
        {
            Debug.Log("XSpeechSynthesizer Closed");
            m_SpeechSynthesizeSceneManager.DestroyDash();
            m_SpeechSynthesizeSceneManager.m_SpeechSynthesizeCreated = false;
            m_XSpeechSynthesizerHandle = null;
            m_XSpeechSynthesizerStreamHandle = null;
            m_VoiceInformation.Clear();
            m_SpeechSynthesizeSceneManager.m_SsmlText.text = "";
        }

        private static float[] ConvertByteToFloat(byte[] array)
        {
            float[] results = new float[array.Length / 2];

            for (int i = 43; i < results.Length; i++)
            {
                results[i] = (float)(BitConverter.ToInt16(array, i * 2) / 32768.0);
            }

            return results;
        }
    }
}