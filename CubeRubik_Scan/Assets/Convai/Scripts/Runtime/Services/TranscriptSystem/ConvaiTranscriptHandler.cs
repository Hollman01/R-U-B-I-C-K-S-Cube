using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Convai.Scripts.Configuration;
using Convai.Scripts.Player;
using UnityEngine;

namespace Convai.Scripts.Services.TranscriptSystem
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class ConvaiTranscriptHandler : MonoBehaviour
    {
        public List<string> visibleCharacterChatIds = new();
        [SerializeField] private ConvaiTranscriptStyleData styleData;
        [SerializeField] private ConvaiConfigurationDataSO convaiConfigurationDataSO;
        [field: SerializeField] public ConvaiPlayer ConvaiPlayer { get; private set; }
        [field: SerializeField] public bool DestroyUiOnDisable { get; set; }
        [field: SerializeField] public float FadeDuration { get; set; } = 0.5f;
        private readonly List<ConvaiTranscriptUIBase> _createdUIs = new();

        private ConvaiTranscriptUIBase _activeUI;
        public bool IsSettingsPanelOpened { get; set; }
        public bool IsPreviewing { get; private set; }

        private int ActiveStyleIndex => convaiConfigurationDataSO.ActiveTranscriptStyleIndex;


        private void Awake()
        {
            ConvaiConfigurationDataSO.GetData(out convaiConfigurationDataSO);

            ConvaiServices.UISystem.OnPreviewStyle += index =>
            {
                StartCoroutine(PreviewStyle(index));
            };

            ConvaiServices.UISystem.OnSettingsClosed += () =>
            {
                if (IsPreviewing)
                {
                    return;
                }

                if (!convaiConfigurationDataSO.TranscriptSystemEnabled)
                {
                    DestroyUIElements();
                    return;
                }

                if (_createdUIs.Count == 0)
                {
                    GenerateUI();
                }

                SetStyle(ActiveStyleIndex);
            };
        }

        private void OnEnable()
        {
            if (_createdUIs.Count == 0)
            {
                GenerateUI();
            }

            if (!convaiConfigurationDataSO.TranscriptSystemEnabled)
            {
                return;
            }

            SetStyle(ActiveStyleIndex);
        }


        private void OnDisable()
        {
            if (!DestroyUiOnDisable)
            {
                return;
            }

            DestroyUIElements();
        }

        public event Action<string, bool> VisibleCharacterIDChanged = delegate { };

        public void DestroyUIElements()
        {
            for (int i = _createdUIs.Count - 1; i >= 0; i--)
            {
                _createdUIs[i].OnDeactivate();
                Destroy(_createdUIs[i].gameObject);
            }

            _createdUIs.Clear();
        }

        public void GenerateUI(bool destroyExistingUi = true)
        {
            if (destroyExistingUi)
            {
                DestroyUIElements();
            }

            foreach (ConvaiTranscriptUIBase ui in from style in styleData.styleList select Instantiate(style.prefab, transform))
            {
                ui.gameObject.SetActive(false);
                ui.Initialize(this);
                _createdUIs.Add(ui);
            }
        }

        public void SetStyle(ConvaiTranscriptUIBase ui)
        {
            if (_activeUI != null && _activeUI == ui)
            {
                return;
            }

            if (_activeUI != null)
            {
                _activeUI.OnDeactivate();
            }

            _activeUI = ui;
            for (int i = 0; i < _createdUIs.Count; i++)
            {
                _createdUIs[i].gameObject.SetActive(i == ActiveStyleIndex);
            }

            _activeUI.OnActivate();
        }

        public void SetStyle(int index)
        {
            if (index < 0 || index >= _createdUIs.Count)
            {
                throw new InvalidDataException("Invalid style: Index out of range.");
            }

            SetStyle(_createdUIs[index]);
        }

        public ConvaiTranscriptUIBase GetActiveUI() => _activeUI;
        public int GetActiveStyleIndex() => ActiveStyleIndex;

        public void AddCharacterChatId(string chatId)
        {
            if (visibleCharacterChatIds.Contains(chatId))
            {
                return;
            }

            visibleCharacterChatIds.Add(chatId);
            VisibleCharacterIDChanged(chatId, true);
        }

        public void RemoveCharacterChatId(string chatId)
        {
            if (!visibleCharacterChatIds.Contains(chatId))
            {
                return;
            }

            visibleCharacterChatIds.Remove(chatId);
            VisibleCharacterIDChanged(chatId, false);
        }


        public void RemoveAllCharacterChatIds()
        {
            visibleCharacterChatIds.ForEach(x => VisibleCharacterIDChanged(x, false));
            visibleCharacterChatIds.Clear();
        }

        public void RemoveCharacterIDAt(int index)
        {
            if (index < 0 || index >= visibleCharacterChatIds.Count)
            {
                return;
            }

            string chatId = visibleCharacterChatIds[index];
            visibleCharacterChatIds.RemoveAt(index);
            VisibleCharacterIDChanged?.Invoke(chatId, false);
        }

        private IEnumerator PreviewStyle(int index)
        {
            if (index < 0 || index >= _createdUIs.Count)
            {
                throw new InvalidDataException("Invalid style: Index out of range.");
            }

            IsPreviewing = true;
            _activeUI?.gameObject.SetActive(false);
            _createdUIs[index].gameObject.SetActive(true);
            yield return _createdUIs[index].Preview();
            _createdUIs[index].gameObject.SetActive(false);
            _activeUI?.gameObject.SetActive(true);
            IsPreviewing = false;
        }
    }
}
