public interface ILevelAnalytics 
{
    public void LevelStart(int levelNo, string levelMode);

    public void LevelPaused(int levelNo, string levelMode, string levelTime);

    public void LevelResumed(int levelNo, string levelMode, string levelTime);

    public void LevelFailed(int levelNo, string levelMode, string levelTime);
    public void LevelComplete(int levelNo, string levelMode, string levelTime);

    public void LevelRestart(int levelNo, string levelMode, string levelTime);
}
