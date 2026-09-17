using Unity.XGamingRuntime;
using UnityEngine;

namespace GdkSample_LeaderboardSample
{
    public delegate void LeaderboardGetResultAvailable(int hresult, uint pageNumber, XblLeaderboardResult xblLeaderboardResult);

    public delegate void StatGetResultAvailable(int hresult, XblUserStatisticsResult xblStatResult);

    public class LeaderboardsContext
    {
        public const uint MaxItemsPerQuery = 15; // Deliberately set low in this sample to demonstrate querying multiple pages

        public uint Page { get; private set; }

        private bool RequestInProgress { get; set; }
        public bool FailedToRequestData { get; set; }

        private ulong Xuid { get; }
        private XblContextHandle ContextHandle { get; }

        private LeaderboardGetResultAvailable LeaderboardCompletionRoutine { get; set; }
        private StatGetResultAvailable StatCompletionRoutine { get; set; }

        public LeaderboardsContext(ulong xuid, XblContextHandle contextHandle)
        {
            Page = 0;
            Xuid = xuid;
            ContextHandle = contextHandle;
        }

        private const float LeaderboardGetNextDelay = 500; // Basic throttling to avoid Http429TooManyRequests error
        private float LeaderboardGetNextTime = 0;
        private XblLeaderboardResult LastXblLeaderboardResult = null;

        public void Update()
        {
            if (ContextHandle==null)
                return;

            if (RequestInProgress && LastXblLeaderboardResult!=null)
            {
                float realTimeSinceStartupMilliSeconds = Time.realtimeSinceStartup * 1000;
                if (realTimeSinceStartupMilliSeconds >= LeaderboardGetNextTime)
                {
                    SDK.XBL.XblLeaderboardResultGetNextAsync(ContextHandle, LastXblLeaderboardResult, MaxItemsPerQuery, ProcessLeaderboardResult);
                    LastXblLeaderboardResult = null;
                }
            }
        }

        public void QueryLeaderboards(
            string leaderboardName,
            string statName,
            XblSocialGroupType queryGroupType,
            string[] additionalColumns,
            LeaderboardGetResultAvailable completionRoutine
            )
        {
            if (RequestInProgress)
            {
                completionRoutine(-1, 0, null);
                return;
            }

            RequestInProgress = true;
            LeaderboardCompletionRoutine = completionRoutine;

            XblLeaderboardQuery query;
            int hresult = XblLeaderboardQuery.Create(
                Xuid,
                GDKGameRuntime.GameConfigScid,
                leaderboardName,
                statName,
                XblSocialGroupType.None,
                additionalColumns,
                XblLeaderboardSortOrder.Descending,
                MaxItemsPerQuery,
                0,
                0,
                null,
                XblLeaderboardQueryType.UserStatBacked,
                out query
                );

            if (HR.FAILED(hresult))
            {
                completionRoutine(hresult, 0, null);
            }

            SDK.XBL.XblLeaderboardGetLeaderboardAsync(
                ContextHandle,
                query,
                ProcessLeaderboardResult);
        }

        public void QueryStatistics(
        string statName,
        StatGetResultAvailable completionRoutine
        )
        {
            if (RequestInProgress)
            {
                completionRoutine(-1, null);
                return;
            }

            RequestInProgress = true;
            StatCompletionRoutine = completionRoutine;
            SDK.XBL.XblUserStatisticsGetSingleUserStatisticAsync(
                ContextHandle,
                Xuid,
                GDKGameRuntime.GameConfigScid,
                statName,
                ProcessStatsResult);
        }

        private void ScheduleLeaderboardGetNext(XblLeaderboardResult xblLeaderboardResult)
        {
            float realTimeSinceStartupMilliSeconds = Time.realtimeSinceStartup * 1000;
            LeaderboardGetNextTime = realTimeSinceStartupMilliSeconds + LeaderboardGetNextDelay;

            LastXblLeaderboardResult = xblLeaderboardResult;
        }

        public void ProcessLeaderboardResult(int hr, XblLeaderboardResult xblLeaderboardResult)
        {
            if (HR.SUCCEEDED(hr))
            {
                LeaderboardCompletionRoutine(hr, Page, xblLeaderboardResult);
                Page++;

                if (xblLeaderboardResult.HasNext)
                {
                    ScheduleLeaderboardGetNext(xblLeaderboardResult);
                }
                else
                {
                    RequestInProgress = false;
                    LeaderboardCompletionRoutine = null;
                    LastXblLeaderboardResult = null;
                }
            }
            else
            {
                FailedToRequestData = true;
                RequestInProgress = false;
                LeaderboardCompletionRoutine = null;
                LastXblLeaderboardResult = null;

                UnityEngine.Debug.LogError($"ProcessLeaderboardResult Failed with: hResult=0x{hr:X} {HR.NameOf(hr)}, XblGetErrorCondition={SDK.XBL.XblGetErrorCondition(hr)}");
            }
        }

        public void ProcessStatsResult(int hr, XblUserStatisticsResult result)
        {
            if (HR.SUCCEEDED(hr))
            {
                StatCompletionRoutine(hr, result);

                RequestInProgress = false;
                StatCompletionRoutine = null;
            }
            else
            {
                FailedToRequestData = true;
                RequestInProgress = false;
                StatCompletionRoutine = null;
                UnityEngine.Debug.LogError($"ProcessStatsResult Failed with: hResult=0x{hr:X} {HR.NameOf(hr)}, XblGetErrorCondition={{SDK.XBL.XblGetErrorCondition(hr)}}");
            }
        }
    }
}