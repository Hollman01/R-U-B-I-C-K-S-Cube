using System;
using System.Collections.Generic;
using UnityEngine;

namespace Convai.Scripts.Services.TranscriptSystem
{
    [CreateAssetMenu(fileName = "Convai Transcript Styles", menuName = "Convai/UI/Transcript Styles", order = 0)]
    public class ConvaiTranscriptStyleData : ScriptableObject
    {
        public List<TranscriptStyle> styleList = new();
    }

    [Serializable]
    public class TranscriptStyle
    {
        public string name;
        public ConvaiTranscriptUIBase prefab;
    }
}
