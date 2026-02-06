using Convai.Scripts.Services;
using TMPro;
using UnityEngine;

namespace Convai.Scripts.TranscriptUI
{
    public class ConvaiMessageUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI senderUI;
        [SerializeField] private TextMeshProUGUI messageUI;
        private string _interactionID;
        private string _message;
        public string Identifier { get; set; }
        public bool IsCompleted { get; set; } = false;
        public void SetSender(string sender) => senderUI.text = sender;
        public void SetSenderColor(Color nameTagColor) => senderUI.color = nameTagColor;

        public void SetMessage(string message)
        {
            messageUI.text = message;
            _message = message;
        }

        public void AppendMessage(string message) => SetMessage(messageUI.text + message);
        public void SetInteractionID(string interactionID) => _interactionID = interactionID;

        public bool SendFeedback(bool isPositiveFeedback)
        {
            if (string.IsNullOrEmpty(_interactionID))
            {
                return false;
            }

            if (!ConvaiServices.CharacterLocatorService.GetNPC(Identifier, out ConvaiNPC npc))
            {
                return false;
            }

            // npc.SendResponseFeedback(isPositiveFeedback, _interactionID, _message);
            return true;
        }

        public void SetSenderUIActive(bool isActive) => senderUI.gameObject.SetActive(isActive);
        public void SetMessageUIActive(bool isActive) => messageUI.gameObject.SetActive(isActive);
    }
}
