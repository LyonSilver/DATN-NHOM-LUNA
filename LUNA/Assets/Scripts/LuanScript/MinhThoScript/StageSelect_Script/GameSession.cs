public enum Difficulty { Easy, Normal, Hard }

public static class GameSession
{
    public static StageData CurrentStage;
    public static Difficulty CurrentDifficulty = Difficulty.Normal;
}