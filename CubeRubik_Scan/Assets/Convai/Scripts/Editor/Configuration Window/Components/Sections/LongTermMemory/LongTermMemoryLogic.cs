using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Convai.RestAPI;
using Convai.RestAPI.Internal;
using Convai.Scripts.Configuration;
using Convai.Scripts.LoggerSystem;
using UnityEngine.UIElements;

namespace Convai.Editor.Configuration_Window.Components.Sections.LongTermMemory
{
    public class LongTermMemoryLogic
    {
        private readonly List<string> _selectedSpeakerList = new();
        private readonly ConvaiLongTermMemorySection _ui;

        public LongTermMemoryLogic(ConvaiLongTermMemorySection section)
        {
            _ui = section;
            if (ConvaiConfigurationWindowEditor.IsApiKeySet)
            {
                RefreshSpeakerList();
            }

            ConvaiConfigurationWindowEditor.OnAPIKeySet += RefreshSpeakerList;
            _ui.RefreshButton.clicked += RefreshSpeakerList;
            _ui.DeleteButton.clicked += DeleteSelectedSpeakerIDs;
        }

        private async void DeleteSelectedSpeakerIDs()
        {
            if (_selectedSpeakerList.Count == 0)
            {
                return;
            }

            if (!ConvaiConfigurationDataSO.GetData(out ConvaiConfigurationDataSO dataSO))
            {
                ConvaiUnityLogger.Error("ConvaiConfigurationDataSO not found in Resources folder.", LogCategory.Editor);
                return;
            }

            if (string.IsNullOrEmpty(dataSO.APIKey))
            {
                ConvaiUnityLogger.Warn("API Key is null", LogCategory.REST);
                return;
            }

            _ui.DeleteButton.SetEnabled(false);

            // Create a list to hold all delete operations
            List<ConvaiREST.DeleteSpeakerIDOperation> deleteOperations = new();

            // Start all delete operations simultaneously
            foreach (string id in _selectedSpeakerList)
            {
                ConvaiREST.DeleteSpeakerIDOperation operation = new(new DeleteSpeakerIDModel(dataSO.APIKey, id));
                deleteOperations.Add(operation);
                ConvaiUnityLogger.DebugLog($"Started delete operation for Speaker ID: {id}", LogCategory.REST);
            }

            // Wait for all operations to complete simultaneously
            while (deleteOperations.Any(op => !op.IsCompleted))
            {
                await Task.Delay(100);
            }

            // Check results after all operations are complete
            bool allSuccessful = true;
            foreach (ConvaiREST.DeleteSpeakerIDOperation operation in deleteOperations)
            {
                if (operation.WasSuccess)
                {
                    ConvaiUnityLogger.DebugLog("Speaker ID deleted successfully.", LogCategory.REST);
                }
                else
                {
                    allSuccessful = false;
                    ConvaiUnityLogger.Error($"Failed to delete speaker ID: {operation.ErrorMessage}", LogCategory.REST);
                }
            }

            // Log overall result
            if (allSuccessful)
            {
                ConvaiUnityLogger.DebugLog($"All {deleteOperations.Count} speaker IDs deleted successfully.", LogCategory.REST);
            }
            else
            {
                ConvaiUnityLogger.Warn("Some speaker ID deletions failed. Check logs for details.", LogCategory.REST);
            }

            // Refresh the speaker list after all operations are complete
            RefreshSpeakerList();

            // Re-enable the delete button
            _ui.DeleteButton.SetEnabled(true);
        }

        private async void RefreshSpeakerList()
        {
            if (!ConvaiConfigurationDataSO.GetData(out ConvaiConfigurationDataSO dataSO))
            {
                ConvaiUnityLogger.Error("ConvaiConfigurationDataSO not found in Resources folder.", LogCategory.Editor);
                return;
            }

            if (string.IsNullOrEmpty(dataSO.APIKey))
            {
                ConvaiUnityLogger.Warn("API Key is null", LogCategory.REST);
                return;
            }

            ConvaiREST.GetSpeakerIDListOperation operation = new(new ConvaiModel(dataSO.APIKey));
            while (!operation.IsCompleted)
            {
                await Task.Delay(100);
            }

            if (operation.WasSuccess)
            {
                GenerateSpeakerList(operation.Result);
            }
            else
            {
                ConvaiUnityLogger.Error(operation.ErrorMessage, LogCategory.REST);
            }
        }

        private void GenerateSpeakerList(List<SpeakerIDDetails> list)
        {
            _ui.IDContainer.Clear();
            _selectedSpeakerList.Clear();
            if (list.Count == 0)
            {
                _ui.TableTitle.text = "No Speaker ID Found";
                _ui.DeleteButton.style.display = DisplayStyle.None;
                _ui.IDContainer.style.display = DisplayStyle.None;
                return;
            }

            _ui.TableTitle.text = "Speaker ID List";
            _ui.DeleteButton.style.display = DisplayStyle.Flex;
            _ui.IDContainer.style.display = DisplayStyle.Flex;
            _ui.DeleteButton.SetEnabled(false);
            foreach (LTMItemUI item in from sid in list select new LTMItemUI(sid.Name, sid.ID, OnSpeakerSelected))
            {
                _ui.IDContainer.Add(item);
            }
        }

        private void OnSpeakerSelected(bool selected, string id)
        {
            if (selected)
            {
                _selectedSpeakerList.Add(id);
            }
            else
            {
                _selectedSpeakerList.Remove(id);
            }

            _ui.DeleteButton.SetEnabled(_selectedSpeakerList.Count > 0);
        }
    }
}
