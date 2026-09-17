using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GdkSample_LeaderboardSample
{
    public class OnScreenLog : MonoBehaviour
    {
        [SerializeField]
        private ScrollRect logScrollRect;

        public GameObject logUIPrefab;
        private string logStringToDisplay;
        private int totalLogPrinted;
        public List<GameObject> createdUILogGO = new List<GameObject>();

        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
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
