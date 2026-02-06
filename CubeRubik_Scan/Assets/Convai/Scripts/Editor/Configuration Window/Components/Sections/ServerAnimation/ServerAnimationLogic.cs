using System.Collections.Generic;
using System.Threading.Tasks;
using Convai.RestAPI;
using Convai.RestAPI.Internal;
using Convai.Scripts.Configuration;
using Convai.Scripts.LoggerSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Convai.Editor.Configuration_Window.Components.Sections.ServerAnimation
{
    public class ServerAnimationLogic
    {
        private readonly List<ConvaiServerAnimationItem> _animations = new();
        private readonly Dictionary<string, Texture2D> _animationThumbnails = new();
        private readonly List<ServerAnimationItemResponse> _selectedAnimations = new();
        private readonly ConvaiServerAnimationSection _ui;
        private int _currentPage = 1;
        private ServerAnimationListResponse _response;

        public ServerAnimationLogic(ConvaiServerAnimationSection section)
        {
            _ui = section;
            if (ConvaiConfigurationWindowEditor.IsApiKeySet)
            {
                RefreshAnimationList();
            }
            else
            {
                ConvaiConfigurationWindowEditor.OnAPIKeySet += RefreshAnimationList;
            }

            _ui.RefreshButton.clicked += RefreshAnimationList;
            _ui.ImportButton.clicked += ImportButtonOnClicked;
            _ui.PreviousButton.clicked += PreviousButtonOnClicked;
            _ui.NextButton.clicked += NextButtonOnClicked;
        }

        ~ServerAnimationLogic()
        {
            ConvaiConfigurationWindowEditor.OnAPIKeySet -= RefreshAnimationList;
            _ui.RefreshButton.clicked -= RefreshAnimationList;
            _ui.ImportButton.clicked -= ImportButtonOnClicked;
            _ui.PreviousButton.clicked -= PreviousButtonOnClicked;
            _ui.NextButton.clicked -= NextButtonOnClicked;
        }

        private void NextButtonOnClicked()
        {
            _currentPage++;
            RefreshAnimationList();
        }

        private void PreviousButtonOnClicked()
        {
            _currentPage--;
            RefreshAnimationList();
        }

        private void ImportButtonOnClicked()
        {
            DisableButtons();
            _animations.ForEach(x => x.CanBeSelected = false);
            ServerAnimationService.ImportAnimations(_selectedAnimations, OnImportComplete, s =>
            {
                ConvaiUnityLogger.Error(s, LogCategory.REST);
                EditorUtility.DisplayDialog("Import Failed", s, "Ok");
                OnImportComplete();
            });
        }

        private void DisableButtons()
        {
            _ui.RefreshButton.SetEnabled(false);
            _ui.NextButton.SetEnabled(false);
            _ui.PreviousButton.SetEnabled(false);
            _ui.ImportButton.SetEnabled(false);
        }

        private void OnImportComplete()
        {
            _ui.RefreshButton.SetEnabled(true);
            _ui.NextButton.SetEnabled(_currentPage != _response.TotalPages);
            _ui.PreviousButton.SetEnabled(_currentPage != 1);
            _ui.ImportButton.SetEnabled(false);
            _selectedAnimations.Clear();
            _animations.ForEach(x =>
            {
                x.CanBeSelected = true;
                x.IsSelected = false;
            });
        }


        private async void RefreshAnimationList()
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

            _ui.AnimationContainer.contentContainer.Clear();
            _selectedAnimations.Clear();
            DisableButtons();
            ConvaiREST.GetAnimationListOperation operation = new(new GetAnimationListModel(dataSO.APIKey, _currentPage, "success"));
            while (!operation.IsCompleted)
            {
                // Wait for operation to complete
                await Task.Delay(100);
            }

            if (operation.WasSuccess)
            {
                OnAnimationListReceived(operation.Result);
            }
            else
            {
                OnError(operation.ErrorMessage);
            }
        }

        private void OnError(string obj) => ConvaiUnityLogger.Error(obj, LogCategory.REST);

        private void OnAnimationListReceived(ServerAnimationListResponse obj)
        {
            Debug.Log($"Received Animation List {obj.CurrentPage} {obj.TotalPages}");
            _response = obj;
            _currentPage = obj.CurrentPage;
            _animations.Clear();
            _ui.RefreshButton.SetEnabled(true);
            _ui.NextButton.SetEnabled(_currentPage != obj.TotalPages);
            _ui.PreviousButton.SetEnabled(_currentPage != 1);
            foreach (ServerAnimationItemResponse animation in obj.Animations)
            {
                ConvaiServerAnimationItem item = new(OnAnimationSelected, animation) { Name = { text = animation.AnimationName } };
                _animations.Add(item);
                _ui.AnimationContainer.contentContainer.Add(item);
                if (_animationThumbnails.TryGetValue(animation.AnimationID, out Texture2D thumbnail))
                {
                    item.Thumbnail.style.backgroundImage = new StyleBackground(thumbnail);
                }
                else
                {
                    if (string.IsNullOrEmpty(animation.ThumbnailURL))
                    {
                        continue;
                    }

                    ConvaiREST.DownloadFile(animation.ThumbnailURL, bytes =>
                    {
                        Texture2D texture = new(256, 256);
                        texture.LoadImage(bytes);
                        _animationThumbnails.Add(animation.AnimationID, texture);
                        item.Thumbnail.style.backgroundImage = new StyleBackground(texture);
                    }, OnError);
                }
            }
        }

        private void OnAnimationSelected(bool isSelected, ServerAnimationItemResponse id)
        {
            if (isSelected)
            {
                _selectedAnimations.Add(id);
            }
            else
            {
                _selectedAnimations.Remove(id);
            }

            _ui.ImportButton.SetEnabled(_selectedAnimations.Count > 0);
        }
    }
}
