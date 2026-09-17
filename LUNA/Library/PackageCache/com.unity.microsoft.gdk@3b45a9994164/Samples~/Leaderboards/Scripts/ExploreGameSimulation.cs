using System;

namespace GdkSample_LeaderboardSample
{
    internal class ExploreGameSimulation
    {
        private static readonly string[] Environments = { "Maze", "Cave" };

        public static GameResult PlayGame()
        {
            GameResult result = new GameResult();

            result.DistanceTraveled = CalculateDistance();
            result.GotLostCount = CalculateTimeLost(result.DistanceTraveled);
            result.ItemsFoundCount = CalculateItemsFound(result.DistanceTraveled);
            result.Environment = Environments[UnityEngine.Random.Range(0, 2)];

            return result;
        }

        private static uint CalculateDistance()
        {
            float weight = UnityEngine.Random.Range(0.0f, 4.302f);
            if (weight <= 0.002f)
                return (uint)UnityEngine.Random.Range(0, 601);

            if (weight <= 1.502f)
                return (uint)UnityEngine.Random.Range(601, 1251);

            if (weight <= 4.002f)
                return (uint)UnityEngine.Random.Range(1251, 2201);

            return (uint)UnityEngine.Random.Range(2201, 2601);
        }

        private static uint CalculateTimeLost(uint distance)
        {
            float lostRate;
            do
            {
                lostRate = GenerateExponentialDistRandomNumber(2.2f);
                lostRate *= distance / 100;
            } while (lostRate < 1.0f && distance < 700);

            return (uint)lostRate;
        }

        private static uint CalculateItemsFound(uint distance)
        {
            float itemRate = GenerateExponentialDistRandomNumber(1.0f);

            itemRate *= distance / 100;
            return (uint)itemRate;
        }

        private static float GenerateExponentialDistRandomNumber(float rate)
        {
            float u;
            do
            {
                u = UnityEngine.Random.Range(0f, 1f);
            } while (u == 1f);

            float result = (float)Math.Log((double)(1f - u)) / (0 - rate);

            return result;
        }
    }
}
