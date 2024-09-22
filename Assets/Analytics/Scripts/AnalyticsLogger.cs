using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCore.Analytics
{ 
[CreateAssetMenu(fileName ="AnalyticsLogger",menuName ="Analytics/Analytics Logger")]
public class AnalyticsLogger : ScriptableObject,ILevelAnalytics,IEconomyAnalytics,IUIInteractionAnalytics
{

        #region EconomyEvents

        public virtual void EconomySink(string placement, string type, string name, int amount, Dictionary<string, object> additionalData)
        {
            Debug.LogError( $"EconomySink : placement : {placement}, type : {type}, name : {name}, amount : {amount} " );
        }

        public virtual void EconomySource(string placement, string type, string name, int amount, Dictionary<string, object> additionalData)
        {
            Debug.LogError($"EconomySource: placement : {placement}, type : {type}, name : {name}, amount : {amount} ");

        }




        #endregion


        #region LevelAnalytics


        public virtual void LevelComplete(int levelNo, string levelMode, string levelTime)
        {
            Debug.LogError( $"LevelComplete: levelNo: {levelNo}, levelMode: {levelMode}, levelTime: {levelTime}");
        }

        public virtual void LevelFailed(int levelNo, string levelMode, string levelTime)
        {
            Debug.LogError($"LevelFailed: levelNo: {levelNo}, levelMode: {levelMode}, levelTime: {levelTime}");

        }

        public virtual void LevelPaused(int levelNo, string levelMode, string levelTime)
        {
            Debug.LogError($"LevelPaused: levelNo: {levelNo}, levelMode: {levelMode}, levelTime: {levelTime}");

        }

        public virtual void LevelRestart(int levelNo, string levelMode, string levelTime)
        {
            Debug.LogError($"LevelRestart: levelNo: {levelNo}, levelMode: {levelMode}, levelTime: {levelTime}");

        }

        public virtual void LevelResumed(int levelNo, string levelMode, string levelTime)
        {
            Debug.LogError($"LevelResumed: levelNo: {levelNo}, levelMode: {levelMode}, levelTime: {levelTime}");

        }

        public virtual void LevelStart(int levelNo, string levelMode)
        {
            Debug.LogError($"LevelStart: levelNo: {levelNo}, levelMode: {levelMode}");

        }


        #endregion


        #region UIINteraction Analytics
        public virtual void UIInteraction(string screen, string element, string type, string action, Dictionary<string, object> additionalData)
        {
            Debug.LogError($"UIInteraction: screen : {screen}, element : {element}, type : {type}, action : {action} ");

        }


        #endregion
    }
}