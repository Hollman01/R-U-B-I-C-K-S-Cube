using UnityEngine;
using UnityEngine.UI;

namespace Convai.Scripts.TranscriptUI
{
    public class ConvaiFeedbackHandler : MonoBehaviour
    {
        [SerializeField] private ConvaiMessageUI convaiMessageUI;
        [SerializeField] private Button positiveButton;
        [SerializeField] private Button negativeButton;

        [SerializeField] private GameObject positiveButtonFill;
        [SerializeField] private GameObject negativeButtonFill;


        private void OnEnable()
        {
            positiveButton.onClick.AddListener(OnPositiveButtonClick);
            negativeButton.onClick.AddListener(OnNegativeButtonClick);
        }

        private void OnDisable()
        {
            positiveButton.onClick.RemoveListener(OnPositiveButtonClick);
            negativeButton.onClick.RemoveListener(OnNegativeButtonClick);
        }

        public void ResetState()
        {
            positiveButtonFill.SetActive(false);
            negativeButtonFill.SetActive(false);
        }


        private void OnNegativeButtonClick()
        {
            if (convaiMessageUI.SendFeedback(false))
            {
                ToggleFillImage(false);
            }
        }

        private void OnPositiveButtonClick()
        {
            if (convaiMessageUI.SendFeedback(true))
            {
                ToggleFillImage(true);
            }
        }

        private void ToggleFillImage(bool isPositiveFeedback)
        {
            positiveButtonFill.SetActive(isPositiveFeedback);
            negativeButtonFill.SetActive(!isPositiveFeedback);
        }
    }
}
