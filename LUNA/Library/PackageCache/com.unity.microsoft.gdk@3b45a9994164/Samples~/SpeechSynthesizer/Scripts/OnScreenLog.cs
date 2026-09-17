using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GdkSample_SpeechSynthesizer
{
    public class OnScreenLog : MonoBehaviour
    {
        [SerializeField]
        private ScrollRect logScrollRect;
        public GameObject logUIPrefab;
        private string logStringToDisplay;
        private int totalLogPrinted;
        public List<GameObject> createdUILogGO = new List<GameObject>();
        void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            logStringToDisplay = logString;
            string newString = "[" + type + "] : " + logStringToDisplay;
            if (type == LogType.Exception)
            {
                newString = stackTrace;
            }

            PrintToUILog(newString);
        }

        public void PrintToUILog(string message)
        {
            var tempObject = Instantiate(logUIPrefab);
            tempObject.SetActive(true);
            createdUILogGO.Add(tempObject);
            tempObject.GetComponent<Text>().text = System.DateTime.Now.ToString() + "\n " + " " + message;
            totalLogPrinted++;
            tempObject.name = "Log" + totalLogPrinted;
            tempObject.transform.SetParent(logScrollRect.content.gameObject.transform, false);
            logScrollRect.normalizedPosition = new Vector2(0, 0);
            logScrollRect.verticalNormalizedPosition = 0;
        }
    }
}