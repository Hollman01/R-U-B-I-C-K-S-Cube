using System.Collections;
using Convai.Scripts.TranscriptUI;
using UnityEngine;

namespace Convai.Scripts.Services.TranscriptSystem
{
    public class ConvaiTranscriptUIBase : MonoBehaviour
    {
        protected FadeCanvas FadeCanvas;
        protected CanvasGroup FadeCanvasGroup;
        protected ConvaiTranscriptHandler TranscriptHandler;


        private void Awake()
        {
            FadeCanvas = GetComponent<FadeCanvas>();
            FadeCanvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            ConvaiServices.TranscriptService.SetCharacterMessageSubscriptionState(OnCharacterMessage, true);
            ConvaiServices.TranscriptService.SetPlayerMessageSubscriptionState(OnPlayerMessage, true);
            ConvaiServices.TranscriptService.SetInteractionIDCreatedState(OnInteractionIDCreated, true);
            ConvaiServices.UISystem.OnSettingsOpened += OnSettingsOpened;
            ConvaiServices.UISystem.OnSettingsClosed += OnSettingsClosed;
        }

        private void OnDisable()
        {
            ConvaiServices.TranscriptService.SetCharacterMessageSubscriptionState(OnCharacterMessage, false);
            ConvaiServices.TranscriptService.SetPlayerMessageSubscriptionState(OnPlayerMessage, false);
            ConvaiServices.TranscriptService.SetInteractionIDCreatedState(OnInteractionIDCreated, false);
            ConvaiServices.UISystem.OnSettingsOpened -= OnSettingsOpened;
            ConvaiServices.UISystem.OnSettingsClosed -= OnSettingsClosed;
        }

        public virtual void ResetChat() { }
        public virtual void Initialize(ConvaiTranscriptHandler convaiTranscriptHandler) => TranscriptHandler = convaiTranscriptHandler;

        public virtual void OnActivate() => TranscriptHandler.VisibleCharacterIDChanged += TranscriptHandler_OnVisibleCharacterIDChanged;

        public virtual void OnDeactivate() => TranscriptHandler.VisibleCharacterIDChanged -= TranscriptHandler_OnVisibleCharacterIDChanged;

        protected virtual void OnCharacterMessage(ConvaiTranscriptData transcript) { }
        protected virtual void OnPlayerMessage(ConvaiTranscriptData transcript) { }
        protected virtual void OnInteractionIDCreated(string characterId, string interactionID) { }

        protected void HandleFading()
        {
            if (TranscriptHandler.IsSettingsPanelOpened || TranscriptHandler.IsPreviewing)
            {
                return;
            }

            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            switch (TranscriptHandler.visibleCharacterChatIds.Count)
            {
                case 0:
                    FadeCanvas.StartFadeOut(FadeCanvasGroup, TranscriptHandler.FadeDuration);
                    break;
                case 1:
                    FadeCanvas.StartFadeIn(FadeCanvasGroup, TranscriptHandler.FadeDuration);
                    break;
            }
        }

        private void TranscriptHandler_OnVisibleCharacterIDChanged(string id, bool newState) => OnVisibleCharacterIDChanged(id, newState);

        protected virtual void OnVisibleCharacterIDChanged(string id, bool newState) { }


        private void OnSettingsClosed()
        {
            TranscriptHandler.IsSettingsPanelOpened = false;
            HandleFading();
        }

        private void OnSettingsOpened()
        {
            TranscriptHandler.IsSettingsPanelOpened = true;
            FadeCanvas.StartFadeOut(FadeCanvasGroup, TranscriptHandler.FadeDuration);
        }

        public IEnumerator Preview()
        {
            FadeCanvas.StartFadeInFadeOutWithGap(FadeCanvasGroup, TranscriptHandler.FadeDuration, TranscriptHandler.FadeDuration, 1f);
            yield return new WaitForSeconds(TranscriptHandler.FadeDuration * 2 + 1f);
        }
    }
}
