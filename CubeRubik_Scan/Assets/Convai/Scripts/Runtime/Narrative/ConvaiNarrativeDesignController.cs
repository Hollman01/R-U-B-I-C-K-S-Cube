using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Convai.Scripts.NarrativeDesign
{
    [Serializable]
    public class ConvaiNarrativeSection
    {
        [field: SerializeField] public string SectionID { get; private set; }
        [field: SerializeField] public UnityEvent OnSectionStart { get; private set; }
        [field: SerializeField] public UnityEvent OnSectionEnd { get; private set; }
    }

    [Serializable]
    public class ConvaiNarrativeDesignTemplateKey
    {
        [field: SerializeField] public string Key { get; private set; }
        [field: SerializeField] public string Value { get; private set; }
    }

    [Serializable]
    public class ConvaiNarrativeDesignController
    {
        [field: SerializeField] public List<ConvaiNarrativeSection> Sections { get; private set; }
        // [field: SerializeField] public List<ConvaiNarrativeDesignTemplateKey> TemplateKeys { get; private set; }

        private string _currentSectionID = string.Empty;

        public void OnNarrativeDesignSectionReceived(string sectionID)
        {
            Debug.Log($"[Unity Engine] [Narrative Design] OnNarrativeDesignSectionReceived: {sectionID}");
            if (string.IsNullOrEmpty(sectionID))
            {
                return;
            }

            if (_currentSectionID == sectionID)
            {
                return;
            }

            if (_currentSectionID != string.Empty)
            {
                Sections.Find(section => section.SectionID == _currentSectionID)?.OnSectionEnd.Invoke();
            }

            _currentSectionID = sectionID;
            Sections.Find(section => section.SectionID == _currentSectionID)?.OnSectionStart.Invoke();
        }

        // public Dictionary<string, string> GetTemplateKeys()
        // {
        //     return TemplateKeys.ToDictionary(key => key.Key, key => key.Value);
        // }
    }
}
