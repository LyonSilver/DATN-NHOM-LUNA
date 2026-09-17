using System;
using System.Collections.Generic;
using System.Threading;
using Unity.XGamingRuntime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GdkSample_LeaderboardSample
{
    public class LeaderboardSceneManager : MonoBehaviour
    {
        public class RowUIData
        {
            public XUserHandle userObject;
            public GameObject gameObject;
            public Transform objTransform;
            public Text Column1;
            public Text Column2;
            public Text Column3;
            public bool done;
        }

        public class RowData
        {
            public uint Rank;
            public string GamerTag;
            public string ColumnValues;
            public string StatName;
            public string StatType;
            public string StatValue;
        }

        public enum RowType
        {
            Leaderboard,
            Stat
        }

        public GameObject BusyCube;
        public GameObject LeaderboardDash;
        public List<GameObject> LeaderboardUIDashGO = new List<GameObject>();
        public ScrollRect LeaderboardUIScrollRect;

        [NonSerialized]
        public List<RowUIData> RowUIDataList = new List<RowUIData>();
        public Text AboutSceneText;

        public Button StatsMazeButton;
        public Button StatsCaveButton;
        public Button LeaderboardMazeButton;
        public Button LeaderboardCaveButton;
        public Button PlayGameButton;
        public Text TextSandboxId;
        public Text TextScid;

        [NonSerialized]
        public List<RowData> RowDataList = new List<RowData>();

        private UserManager m_UserManager;

        public bool _UserSignedIn = false;
        private GameObject m_PreviousFocusGO = null;

        public void Start()
        {
            AboutSceneText.text = k_AboutSceneText;

            if (!GDKGameRuntime.TryInitialize())
            {
                return;
            }

            TextSandboxId.text = GDKGameRuntime.GameConfigSandbox;
            TextScid.text = GDKGameRuntime.GameConfigScid;

            if (m_UserManager == null)
            {
                m_UserManager = new UserManager();
                m_UserManager.leaderboardSceneManager = this;
            }

            UpdateIntefaceButtons();

            m_UserManager.UsersChanged += UserManager_UsersChanged;

            if (Mouse.current == null || !Mouse.current.enabled)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        public void Update()
        {
            if (m_UserManager != null)
                m_UserManager.Update();

            if (m_LeaderboardRowsAvailable.WaitOne(0))
            {
                RowUIData newRowData = CreateRowDash();
                RowUIDataList.Add(newRowData);
                switch (m_RowType)
                {
                    case RowType.Leaderboard:
                        FormatLeaderboardHeader(newRowData);
                        FormatLeaderboardRows();
                        break;

                    case RowType.Stat:
                        FormatStatHeader(newRowData);
                        FormatStatRows();
                        break;
                }

                LeaderboardUIScrollRect.verticalNormalizedPosition = 0;

                m_RequestInProgress = false;

                UpdateIntefaceButtons();
            }

            if (leaderboardContext != null)
            {
                leaderboardContext.Update();

                if (leaderboardContext.FailedToRequestData)
                {
                    Debug.LogError("Failed to request data, please try again...");

                    leaderboardContext.FailedToRequestData = false;
                    m_RequestInProgress = false;

                    UpdateIntefaceButtons();
                    m_LeaderboardRowsAvailable.Release(1);
                    ClearLeaderboardData();
                    leaderboardContext = null;
                }
            }
        }

        public void UpdateIntefaceButtons()
        {
            LeaderboardMazeButton.interactable = _UserSignedIn && !m_RequestInProgress;
            LeaderboardCaveButton.interactable = _UserSignedIn && !m_RequestInProgress;
            StatsMazeButton.interactable = _UserSignedIn && !m_RequestInProgress;
            StatsCaveButton.interactable = _UserSignedIn && !m_RequestInProgress;
            PlayGameButton.interactable = _UserSignedIn && !m_RequestInProgress;

            BusyCube.SetActive(m_RequestInProgress);

            if (m_PreviousFocusGO)
            {
                EventSystem.current.SetSelectedGameObject(m_PreviousFocusGO);
                m_PreviousFocusGO = null;
            }
        }

        public void AddUserWithUI()
        {
            // We attempt to add the first user as the default one, the others need to be explicitely selected
            if (m_UserManager.UserDataList.Count == 0)
                m_UserManager.AddDefaultUserSilently(AddUserCompleted);
            else
                m_UserManager.AddUserWithUI(AddUserCompleted);
        }

        public void PlayGame()
        {
            var result = ExploreGameSimulation.PlayGame();
            var json = result.ToEventJson();

            Debug.Log("Sending event json: " + json);

            UserManager.UserData currentUserData = m_UserManager.UserDataList[0];
            int hr = SDK.XBL.XblEventsWriteInGameEvent(
                currentUserData.m_context,
                "AreaExplored",
                json,
                "{}");
            if (HR.FAILED(hr))
            {
                Debug.Log($"XblEventsWriteInGameEvent() Failed with: 0x{hr:X8}");
            }
        }

        public void GetMazeLeaderboard()
        {
            if (m_RequestInProgress)
                return;

            m_RequestInProgress = true;
            m_RowType = RowType.Leaderboard;
            ClearLeaderboardData();

            UserManager.UserData currentUserData = m_UserManager.UserDataList[0];
            leaderboardContext = new LeaderboardsContext(currentUserData.userXUID, currentUserData.m_context);
            leaderboardContext.QueryLeaderboards("MostTraveledMaze", "AreaExplored.Environment.Maze", XblSocialGroupType.None, null, LeaderboardDataAvailable);

            UpdateIntefaceButtons();

            m_PreviousFocusGO = LeaderboardMazeButton.gameObject;
        }

        public void GetCaveLeaderboard()
        {
            if (m_RequestInProgress)
                return;

            m_RequestInProgress = true;
            m_RowType = RowType.Leaderboard;
            ClearLeaderboardData();

            UserManager.UserData currentUserData = m_UserManager.UserDataList[0];
            leaderboardContext = new LeaderboardsContext(currentUserData.userXUID, currentUserData.m_context);
            leaderboardContext.QueryLeaderboards("MostTraveledCave", "AreaExplored.Environment.Cave", XblSocialGroupType.None, null, LeaderboardDataAvailable);

            UpdateIntefaceButtons();

            m_PreviousFocusGO = LeaderboardCaveButton.gameObject;
        }

        public void GetMazeStats()
        {
            if (m_RequestInProgress)
                return;

            m_RequestInProgress = true;
            m_RowType = RowType.Stat;
            ClearLeaderboardData();

            UserManager.UserData currentUserData = m_UserManager.UserDataList[0];
            leaderboardContext = new LeaderboardsContext(currentUserData.userXUID, currentUserData.m_context);
            leaderboardContext.QueryStatistics("AreaExplored.Environment.Maze", StatDataAvailable);

            UpdateIntefaceButtons();

            m_PreviousFocusGO = StatsMazeButton.gameObject;
        }

        public void GetCaveStats()
        {
            if (m_RequestInProgress)
                return;

            m_RequestInProgress = true;
            m_RowType = RowType.Stat;
            ClearLeaderboardData();

            UserManager.UserData currentUserData = m_UserManager.UserDataList[0];
            leaderboardContext = new LeaderboardsContext(currentUserData.userXUID, currentUserData.m_context);
            leaderboardContext.QueryStatistics("AreaExplored.Environment.Cave", StatDataAvailable);

            UpdateIntefaceButtons();

            m_PreviousFocusGO = StatsCaveButton.gameObject;
        }

        private const string k_AboutSceneText = "This demo shows how to use leaderboards and stats.\nSelect the Login User button to get started.";
        private const string k_NowUseButtonsText = "Now use the Play Game button to simulate a game and generate some stats.\nUse either the Stats or Leaderboards to get and display data rows below.";

        private Semaphore m_LeaderboardRowsAvailable = new Semaphore(0, 1);
        private RowType m_RowType;
        private bool m_RequestInProgress = false;
        private string ColumnNames;
        private LeaderboardsContext leaderboardContext;

        private void UserManager_UsersChanged(object sender, XUserChangeEvent e)
        {
        }

        private void FormatLeaderboardHeader(RowUIData uiData)
        {
            // all other columns by default have the correct text in them for the header
            uiData.Column3.text = ColumnNames;
        }

        private void FormatLeaderboardRows()
        {
            int numRows = RowDataList.Count;
            for (int i = 0; i < numRows; ++i)
            {
                RowUIData newRowData = CreateRowDash();
                UpdateLeaderboardRowDash(newRowData, RowDataList[i]);

                RowUIDataList.Add(newRowData);
            }
        }

        private void FormatStatHeader(RowUIData uiData)
        {
            uiData.Column1.text = "Stat Name";
            uiData.Column2.text = "Stat Type";
            uiData.Column3.text = "Value";
        }

        private void FormatStatRows()
        {
            int numRows = RowDataList.Count;
            for (int i = 0; i < numRows; ++i)
            {
                RowUIData newRowData = CreateRowDash();
                UpdateStatRowDash(newRowData, RowDataList[i]);

                RowUIDataList.Add(newRowData);
            }
        }

        public void ClearLeaderboardData()
        {
            foreach (var row in RowUIDataList)
            {
                Destroy(row.gameObject);
            }
            LeaderboardUIDashGO.Clear();
            RowUIDataList.Clear();
            RowDataList.Clear();
        }

        public void LeaderboardDataAvailable(int hresult, uint pageNumber, XblLeaderboardResult xblLeaderboardResult)
        {
            ColumnNames = "";

            foreach (var column in xblLeaderboardResult.Columns)
            {
                ColumnNames += "| " + column.StatName + " |";
            }

            foreach (var row in xblLeaderboardResult.Rows)
            {
                RowData rowData = new RowData();
                rowData.Rank = row.Rank;
                rowData.GamerTag = row.Gamertag;
                rowData.ColumnValues = "";
                foreach (var column in row.ColumnValues)
                {
                    rowData.ColumnValues += "| " + column + " |";
                }

                if (row.Rank > RowDataList.Count)
                    RowDataList.Add(rowData);
                else
                    RowDataList.Insert((int)row.Rank, rowData);
            }

            if (!xblLeaderboardResult.HasNext)
            {
                m_LeaderboardRowsAvailable.Release(1);
            }
        }

        public void StatDataAvailable(int hresult, XblUserStatisticsResult xblStatResult)
        {
            ColumnNames = "Value";

            foreach (var serviceConfigStat in xblStatResult.ServiceConfigStatistics)
            {
                foreach (var stat in serviceConfigStat.Statistics)
                {
                    RowData rowData = new RowData();
                    rowData.StatName = stat.StatisticName;
                    rowData.StatType = stat.StatisticType;
                    rowData.StatValue = stat.Value;

                    RowDataList.Add(rowData);
                }
            }

            m_LeaderboardRowsAvailable.Release(1);
        }

        private void AddUserCompleted(UserManager.UserOpResult result)
        {
            switch (result)
            {
                case UserManager.UserOpResult.Success:
                    {
                        _UserSignedIn = true;
                        AboutSceneText.text = k_NowUseButtonsText;

                        UpdateIntefaceButtons();
                        break;
                    }
                case UserManager.UserOpResult.NoDefaultUser:
                    {
                        m_UserManager.AddUserWithUI(AddUserCompleted);
                        break;
                    }
                case UserManager.UserOpResult.UnknownError:
                    {
                        Debug.Log("Error adding user.");
                        break;
                    }
                default:
                    break;
            }
        }

        private RowUIData CreateRowDash()
        {
            RowUIData rowUiData = new RowUIData();

            GameObject tempObject = Instantiate(LeaderboardDash);
            LeaderboardUIDashGO.Add(tempObject);
            tempObject.transform.SetParent(LeaderboardUIScrollRect.content.gameObject.transform, false);
            tempObject.SetActive(true);

            rowUiData.gameObject = tempObject;

            rowUiData.objTransform = rowUiData.gameObject.transform;
            rowUiData.Column1 = rowUiData.gameObject.GetComponent<RowValues>().Column1;
            rowUiData.Column2 = rowUiData.gameObject.GetComponent<RowValues>().Column2;
            rowUiData.Column3 = rowUiData.gameObject.GetComponent<RowValues>().Column3;

            return rowUiData;
        }

        private void UpdateLeaderboardRowDash(RowUIData currentRowInformation, RowData currentRowData)
        {
            currentRowInformation.Column1.text = currentRowData.Rank.ToString();
            currentRowInformation.Column2.text = currentRowData.GamerTag;
            currentRowInformation.Column3.text = currentRowData.ColumnValues;
        }

        private void UpdateStatRowDash(RowUIData currentRowInformation, RowData currentRowData)
        {
            currentRowInformation.Column1.text = currentRowData.StatName;
            currentRowInformation.Column2.text = currentRowData.StatType;
            currentRowInformation.Column3.text = currentRowData.StatValue;
        }
    }
}