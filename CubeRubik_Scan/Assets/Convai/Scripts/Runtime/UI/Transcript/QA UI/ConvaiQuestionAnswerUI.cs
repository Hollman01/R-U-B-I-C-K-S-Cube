using Convai.Scripts.Services;
using Convai.Scripts.Services.TranscriptSystem;
using Convai.Scripts.TranscriptUI.Filters;
using UnityEngine;

namespace Convai.Scripts.TranscriptUI.QA_UI
{
    public class ConvaiQuestionAnswerUI : ConvaiTranscriptUIBase
    {
        [SerializeField] private ConvaiMessageUI characterMessageUI;
        [SerializeField] private ConvaiMessageUI playerMessageUI;
        [SerializeField] private GameObject feedbackContainer;
        private ConvaiSingleNPCFilter _filter;

        public override void OnActivate()
        {
            base.OnActivate();
            _filter = TranscriptHandler.gameObject.AddComponent<ConvaiSingleNPCFilter>();
            ResetMessageUI();
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            Destroy(_filter);
        }

        protected override void OnCharacterMessage(ConvaiTranscriptData transcript)
        {
            if (!TranscriptHandler.visibleCharacterChatIds.Contains(transcript.Identifier))
            {
                if (transcript.Identifier == characterMessageUI.Identifier && transcript.IsLastChunk)
                {
                    ResetMessageUI();
                }

                return;
            }

            feedbackContainer.SetActive(true);
            characterMessageUI.SetMessage(transcript.Message);
            characterMessageUI.SetSender(transcript.Name);
            characterMessageUI.Identifier = transcript.Identifier;
            if (ConvaiServices.CharacterLocatorService.GetNPC(transcript.Identifier, out ConvaiNPC npc))
            {
                //characterMessageUI.SetSenderColor(npc.TranscriptMetaData.nameTagColor);
            }

            if (!transcript.IsLastChunk)
            {
                return;
            }

            ResetMessageUI();
        }


        protected override void OnPlayerMessage(ConvaiTranscriptData transcript)
        {
            playerMessageUI.SetMessage(transcript.Message);
            playerMessageUI.SetSender(transcript.Name);
            playerMessageUI.SetSenderColor(Color.white);
        }

        protected override void OnInteractionIDCreated(string characterId, string interactionID) =>
            characterMessageUI.SetInteractionID(interactionID);

        protected override void OnVisibleCharacterIDChanged(string id, bool newState) => HandleFading();

        private void ResetMessageUI()
        {
            characterMessageUI.SetMessage(string.Empty);
            characterMessageUI.SetSender(string.Empty);
            playerMessageUI.SetMessage(string.Empty);
            playerMessageUI.SetSender(string.Empty);
            feedbackContainer.SetActive(false);
        }
    }
}
