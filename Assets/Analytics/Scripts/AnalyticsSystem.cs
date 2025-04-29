using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProjectCore.Analytics
{
    [CreateAssetMenu( fileName ="AnalyticSystem", menuName ="Analytics/ Analytic System")]
    public class AnalyticsSystem : ScriptableObject
    {
        
        public List<AnalyticsLogger> AnalyticsLoggers= new List<AnalyticsLogger>();

        #region UI Interaction Analytics
        public void LogUIInteractionEvents(string screen, string element, string type, string action, Dictionary<string, object> additionalData)
        {

            for(int i=0;i<AnalyticsLoggers.Count;i++)
            {
                AnalyticsLoggers[i].UIInteraction(screen, element, type, action, additionalData);
            }

        }

        #endregion

        #region EconomyEvents

        public void LogEconomySinkEvent(string placement, string type, string name, int amount, Dictionary<string, object> additionalData)
        {
            for (int i = 0; i < AnalyticsLoggers.Count; i++)
            {
                AnalyticsLoggers[i].EconomySink(placement, type, name, amount, additionalData);
            }
        }

        public void LogEconomySource(string placement, string type, string name, int amount, Dictionary<string, object> additionalData)
        {
            for (int i = 0; i < AnalyticsLoggers.Count; i++)
            {
                AnalyticsLoggers[i].EconomySource(placement, type, name, amount, additionalData);
            }
        }




        #endregion


        #region LevelAnalytics


        public virtual void LevelComplete(int levelNo, string levelMode, string levelTime)
        {
            for (int i = 0; i < AnalyticsLoggers.Count; i++)
            {
                AnalyticsLoggers[i].LevelComplete(levelNo, levelMode, levelTime);
            }
        }

        public virtual void LevelFailed(int levelNo, string levelMode, string levelTime)
        {
            for (int i = 0; i < AnalyticsLoggers.Count; i++)
            {
                AnalyticsLoggers[i].LevelFailed(levelNo, levelMode, levelTime);
            }
        }

        public virtual void LevelPaused(int levelNo, string levelMode, string levelTime)
        {
            for (int i = 0; i < AnalyticsLoggers.Count; i++)
            {
                AnalyticsLoggers[i].LevelPaused(levelNo, levelMode, levelTime);
            }

        }

        public virtual void LevelRestart(int levelNo, string levelMode, string levelTime)
        {
            for (int i = 0; i < AnalyticsLoggers.Count; i++)
            {
                AnalyticsLoggers[i].LevelRestart(levelNo, levelMode, levelTime);
            }
        }

        public virtual void LevelResumed(int levelNo, string levelMode, string levelTime)
        {
            for (int i = 0; i < AnalyticsLoggers.Count; i++)
            {
                AnalyticsLoggers[i].LevelResumed(levelNo, levelMode, levelTime);
            }

        }

        public virtual void LevelStart(int levelNo, string levelMode)
        {
            for (int i = 0; i < AnalyticsLoggers.Count; i++)
            {
                AnalyticsLoggers[i].LevelStart(levelNo, levelMode);
            }
        }


        #endregion


        



    }
}