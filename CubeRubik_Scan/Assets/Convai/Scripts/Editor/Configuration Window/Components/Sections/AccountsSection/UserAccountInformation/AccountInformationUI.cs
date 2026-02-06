using System;
using System.Globalization;
using System.Threading.Tasks;
using Convai.RestAPI;
using Convai.Scripts.Configuration;
using Convai.Scripts.LoggerSystem;
using UnityEngine.UIElements;

namespace Convai.Editor.Configuration_Window.Components.Sections.AccountsSection.UserAccountInformation
{
    public class AccountInformationUI
    {
        private readonly ConvaiAccountSection _ui;

        public AccountInformationUI(ConvaiAccountSection section)
        {
            _ui = section;
            SetupApiKeyField();
        }

        private void SetupApiKeyField()
        {
            TextField apiKeyField = _ui.APIInputField;
            if (apiKeyField != null && ConvaiConfigurationDataSO.GetData(out ConvaiConfigurationDataSO dataSO) &&
                !string.IsNullOrEmpty(dataSO.APIKey))
            {
                apiKeyField.value = dataSO.APIKey;
                apiKeyField.isReadOnly = false;
                ConvaiConfigurationWindowEditor.IsApiKeySet = true;
                GetUserAPIUsageData();
            }
            else
            {
                ConvaiConfigurationWindowEditor.IsApiKeySet = false;
                SetInvalidApiKeyUI();
            }
        }

        public async void GetUserAPIUsageData(bool validApiKey = true)
        {
            if (!validApiKey)
            {
                SetInvalidApiKeyUI();
                return;
            }

            try
            {
                if (ConvaiConfigurationDataSO.GetData(out ConvaiConfigurationDataSO dataSO))
                {
                    if (string.IsNullOrEmpty(dataSO.APIKey))
                    {
                        // ConvaiLogger.Warn("API Key is null", ConvaiLogger.LogCategory.GRPC);
                        SetInvalidApiKeyUI();
                        return;
                    }
                }

                ConvaiModel model = new(dataSO.APIKey);
                ConvaiREST.GetAPIUsageDetailsOperation operation = new(model);
                while (!operation.IsCompleted)
                {
                    // Wait for operation to complete
                    await Task.Delay(100);
                }

                if (operation.WasSuccess)
                {
                    UpdateUIWithUsageData(operation.Result.Usage);
                }
                else
                {
                    ConvaiUnityLogger.Warn("Failed to parse usage data.", LogCategory.REST);
                    SetInvalidApiKeyUI();
                }
            }
            catch (Exception ex)
            {
                ConvaiUnityLogger.Exception($"Error fetching API usage data: {ex.Message}", LogCategory.REST);
                SetInvalidApiKeyUI();
            }
        }

        private void UpdateUIWithUsageData(UserUsageData.UsageData usageData)
        {
            _ui.PlanName.text = usageData.PlanName;
            _ui.ExpiryDate.text = GetFormattedDate(usageData.ExpiryTs.ToString(CultureInfo.CurrentCulture));
            _ui.DailyInteractionUI.ProgressBar.value = CalculateUsagePercentage(usageData.DailyUsage, usageData.DailyLimit);
            _ui.MonthlyInteractionUI.ProgressBar.value = CalculateUsagePercentage(usageData.MonthlyUsage, usageData.MonthlyLimit);
            _ui.DailyInteractionUI.InteractionLiteral.text = FormatUsageLabel(usageData.DailyUsage, usageData.DailyLimit);
            _ui.MonthlyInteractionUI.InteractionLiteral.text = FormatUsageLabel(usageData.MonthlyUsage, usageData.MonthlyLimit);
        }

        private void SetInvalidApiKeyUI()
        {
            _ui.PlanName.text = "Invalid API Key";
            _ui.ExpiryDate.text = "Invalid API Key";
            _ui.DailyInteractionUI.ProgressBar.value = 0;
            _ui.MonthlyInteractionUI.ProgressBar.value = 0;
            _ui.DailyInteractionUI.InteractionLiteral.text = "0/0 interactions";
            _ui.MonthlyInteractionUI.InteractionLiteral.text = "0/0 interactions";
        }

        private static float CalculateUsagePercentage(int usage, int limit) => limit > 0 ? (float)usage / limit * 100 : 0;

        private static string FormatUsageLabel(int usage, int limit) => $"{usage}/{limit} interactions";

        private static string GetFormattedDate(string dateString)
        {
            if (string.IsNullOrEmpty(dateString))
            {
                return dateString;
            }

            if (!DateTime.TryParseExact(dateString, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                return dateString;
            }

            string daySuffix = GetDaySuffix(date.Day);
            return date.ToString($"MMMM dd'{daySuffix}' yyyy");

            // ConvaiUnityLogger.Warn($"Failed to parse date: {dateString}", LogCategory.GRPC);
        }

        private static string GetDaySuffix(int day) =>
            (day % 10, day / 10) switch
            {
                (1, 1) or (2, 1) or (3, 1) => "th",
                (1, _) => "st",
                (2, _) => "nd",
                (3, _) => "rd",
                _ => "th"
            };
    }
}
