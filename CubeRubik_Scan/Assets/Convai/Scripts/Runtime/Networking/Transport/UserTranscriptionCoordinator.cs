using System;
using Assets.Convai.Scripts.Server;

namespace Convai.Scripts.Networking.Transport
{
    internal sealed class UserTranscriptionCoordinator
    {
        private readonly IConvaiPlayerEvents _playerEvents;
        private readonly Action<System.Action> _scheduleOnMainThread;
        private string _asrFinalText = string.Empty;
        private bool _awaitingProcessedFinal;
        private bool _completionDispatched;
        private string _processedFinalText = string.Empty;
        private bool _receivedAsrFinal;
        private bool _receivedProcessedFinal;
        private bool _sessionActive;

        private string _sessionId = string.Empty;
        private bool _stopPending;

        public UserTranscriptionCoordinator(IConvaiPlayerEvents playerEvents, Action<System.Action> scheduleOnMainThread)
        {
            _playerEvents = playerEvents ?? throw new ArgumentNullException(nameof(playerEvents));
            _scheduleOnMainThread = scheduleOnMainThread ?? throw new ArgumentNullException(nameof(scheduleOnMainThread));
        }

        public void HandleStart()
        {
            if (_sessionActive && !_stopPending)
            {
                return;
            }

            if (_stopPending && !_completionDispatched)
            {
                CompleteSession();
            }

            StartNewSession();
        }

        public void HandleInterim(string interimText)
        {
            EnsureSession();

            string safeText = interimText ?? string.Empty;
            Dispatch(() => _playerEvents.OnUserTranscriptionReceived(safeText, TranscriptionPhase.Interim));
        }

        public void HandleAsrFinal(string finalText)
        {
            EnsureSession();

            _asrFinalText = finalText ?? string.Empty;
            _receivedAsrFinal = _asrFinalText.Length > 0;

            Dispatch(() => _playerEvents.OnUserTranscriptionReceived(_asrFinalText, TranscriptionPhase.AsrFinal));

            if (_stopPending && !_awaitingProcessedFinal)
            {
                CompleteSession();
            }
        }

        public void HandleProcessedFinal(string cleanedText)
        {
            EnsureSession();

            _processedFinalText = cleanedText ?? string.Empty;
            _receivedProcessedFinal = _processedFinalText.Length > 0;

            Dispatch(() => _playerEvents.OnUserTranscriptionReceived(_processedFinalText, TranscriptionPhase.ProcessedFinal));

            if (_stopPending)
            {
                _awaitingProcessedFinal = false;
                CompleteSession();
            }
        }

        public void HandleStop()
        {
            if (string.IsNullOrEmpty(_sessionId))
            {
                return;
            }

            _stopPending = true;
            _sessionActive = false;
            _awaitingProcessedFinal = !_receivedProcessedFinal;

            if (!_awaitingProcessedFinal)
            {
                CompleteSession();
            }
        }

        public void Reset()
        {
            _sessionId = string.Empty;
            _sessionActive = false;
            _receivedAsrFinal = false;
            _receivedProcessedFinal = false;
            _stopPending = false;
            _awaitingProcessedFinal = false;
            _completionDispatched = false;
            _asrFinalText = string.Empty;
            _processedFinalText = string.Empty;
        }

        private void StartNewSession()
        {
            Reset();

            _sessionId = Guid.NewGuid().ToString("N");
            _sessionActive = true;

            Dispatch(() => _playerEvents.OnUserStartedSpeaking(_sessionId));
            Dispatch(() => _playerEvents.OnUserTranscriptionReceived(string.Empty, TranscriptionPhase.Listening));
        }

        private void EnsureSession()
        {
            if (string.IsNullOrEmpty(_sessionId))
            {
                StartNewSession();
            }
        }

        private void CompleteSession()
        {
            if (_completionDispatched || string.IsNullOrEmpty(_sessionId))
            {
                Reset();
                return;
            }

            string finalTranscript = _processedFinalText.Length > 0
                ? _processedFinalText
                : _asrFinalText;
            bool producedFinal = finalTranscript.Length > 0;

            Dispatch(() => _playerEvents.OnUserTranscriptionReceived(finalTranscript, TranscriptionPhase.Completed));
            Dispatch(() => _playerEvents.OnUserStoppedSpeaking(_sessionId, producedFinal));

            _completionDispatched = true;
            Reset();
        }

        private void Dispatch(System.Action action)
        {
            void SafeInvoke()
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception)
                {
                    // Intentionally left blank to avoid bubbling exceptions from user callbacks.
                }
            }

            _scheduleOnMainThread(SafeInvoke);
        }
    }
}
