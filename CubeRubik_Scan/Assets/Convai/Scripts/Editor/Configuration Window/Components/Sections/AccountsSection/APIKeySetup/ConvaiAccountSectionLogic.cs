using Convai.Editor.Configuration_Window.Components.Sections.AccountsSection.UserAccountInformation;
using UnityEditor;

namespace Convai.Editor.Configuration_Window.Components.Sections.AccountsSection.APIKeySetup
{
    public class ConvaiAccountSectionLogic
    {
        private readonly AccountInformationUI _accountInformationUI;
        private readonly ConvaiAccountSection _ui;

        public ConvaiAccountSectionLogic(ConvaiAccountSection section, AccountInformationUI accountInformationUI)
        {
            _ui = section;
            _accountInformationUI = accountInformationUI;
            APIKeySetupLogic.LoadExistingApiKey(_ui.APIInputField, _ui.UpdateSaveButton);
            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            _ui.ShowHideAPIKeyButton.clicked += TogglePasswordVisibility;
            _ui.UpdateSaveButton.clicked += () => ClickEvent(_ui.APIInputField.value);
        }

        private void TogglePasswordVisibility()
        {
            _ui.APIInputField.isPasswordField = !_ui.APIInputField.isPasswordField;
            _ui.ShowHideAPIKeyButton.text = _ui.APIInputField.isPasswordField ? "Show" : "Hide";
        }

        private void ClickEvent(string apiKey) =>
            APIKeySetupLogic.BeginButtonTask(apiKey, (isSuccessful, shouldShowPage2) =>
            {
                if (isSuccessful)
                {
                    if (shouldShowPage2)
                    {
                        APIKeyReferralWindow.ShowWindow();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Success", "API Key loaded successfully!", "OK");
                    }

                    _ui.APIInputField.isReadOnly = false;
                    _ui.UpdateSaveButton.text = "Update API Key";
                    ConvaiConfigurationWindowEditor.IsApiKeySet = true;
                }
                else
                {
                    ConvaiConfigurationWindowEditor.IsApiKeySet = false;
                }

                _accountInformationUI.GetUserAPIUsageData(isSuccessful);
            });
    }
}
