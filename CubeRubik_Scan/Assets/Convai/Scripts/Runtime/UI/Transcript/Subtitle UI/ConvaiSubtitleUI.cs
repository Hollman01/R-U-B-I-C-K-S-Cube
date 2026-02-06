using Convai.Scripts.Services;
using Convai.Scripts.Services.TranscriptSystem;
using Convai.Scripts.TranscriptUI.Filters;
using UnityEngine;

namespace Convai.Scripts.TranscriptUI.Subtitle_UI
{
    public class ConvaiSubtitleUI : ConvaiTranscriptUIBase
    {
        [SerializeField] private ConvaiMessageUI convaiMessageUI;
        [SerializeField] private string defaultText;
        [SerializeField] private ConvaiFeedbackHandler feedbackContainer;
        private ConvaiSingleNPCFilter _filter;

        public override void OnActivate()
        {
            ResetMessageUI();
            _filter = TranscriptHandler.gameObject.AddComponent<ConvaiSingleNPCFilter>();
            base.OnActivate();
            FadeCanvas.StartFadeIn(FadeCanvasGroup, TranscriptHandler.FadeDuration);
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            Destroy(_filter);
        }

        protected override void OnPlayerMessage(ConvaiTranscriptData transcript) => UpdateMessage(transcript, true);

        protected override void OnCharacterMessage(ConvaiTranscriptData transcript) => UpdateMessage(transcript);

        private void UpdateMessage(ConvaiTranscriptData transcript, bool fromPlayer = false)
        {
            if (!TranscriptHandler.visibleCharacterChatIds.Contains(transcript.Identifier) && !fromPlayer)
            {
                if (transcript.Identifier == convaiMessageUI.Identifier && transcript.IsLastChunk)
                {
                    ResetMessageUI();
                }

                return;
            }

            if (transcript.IsLastChunk && !fromPlayer)
            {
                ResetMessageUI();
            }
            else
            {
                if (string.IsNullOrEmpty(transcript.Message))
                {
                    return;
                }

                feedbackContainer.gameObject.SetActive(!fromPlayer);
                convaiMessageUI.Identifier = transcript.Identifier;
                convaiMessageUI.SetSenderUIActive(true);
                convaiMessageUI.SetSender(transcript.Name);
                convaiMessageUI.SetMessage(transcript.Message);
                if (ConvaiServices.CharacterLocatorService.GetNPC(transcript.Identifier, out ConvaiNPC npc))
                {
                    //convaiMessageUI.SetSenderColor(npc.TranscriptMetaData.nameTagColor);
                }
            }
        }

        private void ResetMessageUI()
        {
            convaiMessageUI.SetMessage(defaultText);
            convaiMessageUI.SetSenderUIActive(false);
            feedbackContainer.gameObject.SetActive(false);
            feedbackContainer.ResetState();
        }

        protected override void OnInteractionIDCreated(string characterId, string interactionID) => convaiMessageUI.SetInteractionID(interactionID);

        protected override void OnVisibleCharacterIDChanged(string id, bool newState)
        {
            if (!newState && (convaiMessageUI.Identifier ?? string.Empty).Equals(id))
            {
                ResetMessageUI();
            }
        }
    }
}
