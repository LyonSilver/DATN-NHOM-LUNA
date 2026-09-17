using System;
using UnityEngine;

namespace GdkSample_LeaderboardSample
{
    [Serializable]
    public struct GameResult
    {
        public string Environment;
        public uint DistanceTraveled;
        public uint GotLostCount;
        public uint ItemsFoundCount;

        public string ToEventJson()
        {
            return JsonUtility.ToJson(this);
        }
    }
}
