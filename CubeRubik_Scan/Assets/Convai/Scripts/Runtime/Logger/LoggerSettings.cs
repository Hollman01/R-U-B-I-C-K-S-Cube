using UnityEngine;

namespace Convai.Scripts.LoggerSystem
{
    [CreateAssetMenu(fileName = "LoggerSettings", menuName = "Convai/LoggerSettings")]
    public class LoggerSettings : ScriptableObject
    {
        public ConvaiUnityLogger.LogLevel LipSync;
        public ConvaiUnityLogger.LogLevel Character;
        public ConvaiUnityLogger.LogLevel Actions;
        public ConvaiUnityLogger.LogLevel UI;
        public ConvaiUnityLogger.LogLevel SDK;
        public ConvaiUnityLogger.LogLevel Editor;
        public ConvaiUnityLogger.LogLevel RestAPI;
        public ConvaiUnityLogger.LogLevel Player;
    }

    #region LogCategory enum

    public enum LogCategory
    {
        Character,
        LipSync,
        Actions,
        Editor,
        UI,
        SDK,
        REST,
        Player
    }

    #endregion
}
