using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCore.Analytics
{

    [CreateAssetMenu(fileName = "FirebaseLogger", menuName = "Analytics/ Analytics Firebase Logger")]
    public class FirebaseAnalyticsLogger :AnalyticsLogger
    {

       
        #region EconomyEvents

        public void EconomySink(string placement, string type, string name, int amount, Dictionary<string, object> additionalData)
        {
            //TODO Log Firebase Analytics Here

            base.EconomySink(placement, type, name, amount,additionalData);
            //Debug.LogError($"EconomySink : placement : {placement}, type : {type}, name : {name}, amount : {amount} ");
        }

        public void EconomySource(string placement, string type, string name, int amount, Dictionary<string, object> additionalData)
        {
            base.EconomySource(placement, type, name, amount, additionalData);
        }




        #endregion


        #region LevelAnalytics


        public virtual void LevelComplete(int levelNo, string levelMode, string levelTime)
        {
            base.LevelComplete(levelNo, levelMode, levelTime);
        }

        public virtual void LevelFailed(int levelNo, string levelMode, string levelTime)
        {
            base.LevelFailed(levelNo, levelMode, levelTime);
        }

        public virtual void LevelPaused(int levelNo, string levelMode, string levelTime)
        {
            base.LevelPaused(levelNo, levelMode, levelTime);
        }

        public virtual void LevelRestart(int levelNo, string levelMode, string levelTime)
        {
            base.LevelRestart(levelNo, levelMode, levelTime);
        }

        public virtual void LevelResumed(int levelNo, string levelMode, string levelTime)
        {
            base.LevelResumed (levelNo, levelMode, levelTime);
        }

        public virtual void LevelStart(int levelNo, string levelMode)
        {
            base.LevelStart(levelNo, levelMode);
        }


        #endregion


        #region UIINteraction Analytics
        public virtual void UIInteraction(string screen, string element, string type, string action, Dictionary<string, object> additionalData)
        {
            base.UIInteraction(screen, element, type, action, additionalData);
        }


        #endregion
    
    
    }
}