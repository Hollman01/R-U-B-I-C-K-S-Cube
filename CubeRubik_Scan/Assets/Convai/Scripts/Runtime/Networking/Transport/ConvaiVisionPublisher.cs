using System;
using System.Threading.Tasks;
using LiveKit;
using LiveKit.Proto;
using Convai.Scripts.LoggerSystem;
using UnityEngine;

namespace Convai.Scripts.Networking.Transport
{
    /// <summary>
    /// Publishes a Unity camera feed to LiveKit using the Convai realtime SDK.
    /// Wraps RenderTexture capture, TextureVideoSource lifecycle, and publish options.
    /// </summary>
    [DisallowMultipleComponent]
    public class ConvaiVisionPublisher : MonoBehaviour
    {
        [Serializable]
        public struct CaptureProfile
        {
            public int Width;
            public int Height;
            public int Fps;
            public int MaxBitrate;

            public bool IsValid =>
                Width > 0 &&
                Height > 0 &&
                Fps > 0 &&
                MaxBitrate > 0;
        }

        public enum VideoPreset
        {
            Snapshot,
            GeminiLive
        }

        protected const string LogPrefix = "[ConvaiVisionPublisher]";
        protected const int ParticipantPollIntervalMs = 50;
        protected const int ParticipantTimeoutMs = 10000;

        [Header("Camera Binding")]
        [SerializeField] private Camera captureCamera;
        [SerializeField] private string trackName = "convai-video";
        [SerializeField] private TrackSource trackSource = TrackSource.SourceCamera;

        [Header("Encoding Defaults")]
        [SerializeField] private CaptureProfile defaultProfile = new()
        {
            Width = 1280,
            Height = 720,
            Fps = 15,
            MaxBitrate = 1_000_000
        };

        [SerializeField] private CaptureProfile snapshotProfile = new()
        {
            Width = 640,
            Height = 360,
            Fps = 15,
            MaxBitrate = 400_000
        };

        [SerializeField] private CaptureProfile geminiLiveProfile = new()
        {
            Width = 1280,
            Height = 720,
            Fps = 15,
            MaxBitrate = 1_200_000
        };

        [SerializeField] private bool simulcast = true;
        [SerializeField] private VideoCodec videoCodec = VideoCodec.Vp8;

        [Header("Runtime")]
        [SerializeField] private bool autoStartWhenBound;
        [SerializeField] private bool verboseLogging;

        protected Room _boundRoom;
        protected RenderTexture _renderTexture;
        protected RenderTexture _cameraRenderTexture;
        protected TextureVideoSource _videoSource;
        protected LocalVideoTrack _localVideoTrack;
        protected Coroutine _pumpCoroutine;

        protected CaptureProfile _currentProfile;
        protected bool _currentSimulcast;
        protected VideoCodec _currentCodec;
        protected TrackSource _currentTrackSource;
        protected string _currentTrackName;

        protected bool _isPublishing;
        protected bool _isStarting;
        protected bool _isStopping;
        protected double _nextCaptureTimestamp;
        protected double _captureInterval = 1d / 15d;

        protected static readonly Vector2 FlipScale = new Vector2(1f, -1f);
        protected static readonly Vector2 FlipOffset = new Vector2(0f, 1f);

        public bool HasCamera => captureCamera != null;
        public bool IsPublishing => _isPublishing;
        public bool IsMuted => _videoSource != null && _videoSource.Muted;
        public CaptureProfile CurrentProfile => _currentProfile;

        private void Awake()
        {
            _currentProfile = SanitizeProfile(defaultProfile);
            _currentSimulcast = simulcast;
            _currentCodec = videoCodec;
            _currentTrackSource = trackSource;
            _currentTrackName = string.IsNullOrWhiteSpace(trackName) ? "convai-video" : trackName.Trim();
            ResetCaptureTiming();
            RefreshCaptureInterval();
        }

        private void OnEnable()
        {
            if (autoStartWhenBound && _boundRoom != null)
            {
                _ = EnsurePublishingAsync(_boundRoom);
            }
        }

        private void OnDisable()
        {
            _ = StopPublishingAsync();
        }

        private void OnDestroy()
        {
            _ = StopPublishingAsync();
        }

        public void BindRoom(Room room)
        {
            _boundRoom = room;
            if (autoStartWhenBound && room != null)
            {
                _ = EnsurePublishingAsync(room);
            }
        }

        public CaptureProfile CreateProfile(int? width, int? height, int? fps, int? maxBitrate)
        {
            CaptureProfile profile = _currentProfile;
            if (width.HasValue)
            {
                profile.Width = width.Value;
            }

            if (height.HasValue)
            {
                profile.Height = height.Value;
            }

            if (fps.HasValue)
            {
                profile.Fps = fps.Value;
            }

            if (maxBitrate.HasValue)
            {
                profile.MaxBitrate = maxBitrate.Value;
            }

            return profile;
        }

        public bool TryResolvePreset(string value, out VideoPreset preset)
        {
            preset = VideoPreset.Snapshot;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            switch (value.Trim().ToLowerInvariant())
            {
                case "snapshot":
                case "snap":
                    preset = VideoPreset.Snapshot;
                    return true;
                case "gemini_live":
                case "gemini":
                case "gemini-live":
                    preset = VideoPreset.GeminiLive;
                    return true;
                default:
                    return false;
            }
        }

        public bool TryResolveCodec(string value, out VideoCodec codec)
        {
            codec = _currentCodec;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            switch (value.Trim().ToLowerInvariant())
            {
                case "vp8":
                    codec = VideoCodec.Vp8;
                    return true;
                case "vp9":
                    codec = VideoCodec.Vp9;
                    return true;
                case "h264":
                    codec = VideoCodec.H264;
                    return true;
                case "av1":
                    codec = VideoCodec.Av1;
                    return true;
                default:
                    return false;
            }
        }

        public bool TryResolveTrackSource(string value, out TrackSource source)
        {
            source = _currentTrackSource;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            switch (value.Trim().ToLowerInvariant())
            {
                case "camera":
                case "webcam":
                    source = TrackSource.SourceCamera;
                    return true;
                case "screen":
                case "screenshare":
                case "display":
                    source = TrackSource.SourceScreenshare;
                    return true;
                default:
                    return false;
            }
        }

        public virtual async Task<bool> EnsurePublishingAsync(Room room)
        {
            if (room == null)
            {
                ConvaiUnityLogger.Warn($"{LogPrefix} Cannot start publishing without a room reference.", LogCategory.SDK);
                return false;
            }

            if (!HasCamera)
            {
                ConvaiUnityLogger.Warn($"{LogPrefix} Capture camera is not assigned.", LogCategory.SDK);
                return false;
            }

            _boundRoom = room;

            if (_isPublishing)
            {
                return true;
            }

            if (_isStarting)
            {
                while (_isStarting)
                {
                    await Task.Delay(ParticipantPollIntervalMs);
                }

                return _isPublishing;
            }

            _isStarting = true;
            try
            {
                if (!await WaitForLocalParticipantAsync(room))
                {
                    ConvaiUnityLogger.Warn($"{LogPrefix} Timed out waiting for LocalParticipant before publishing video.", LogCategory.SDK);
                    return false;
                }

                PrepareRenderTexture();
                if (_renderTexture == null)
                {
                    ConvaiUnityLogger.Error($"{LogPrefix} Failed to create RenderTexture for video capture.", LogCategory.SDK);
                    return false;
                }

                _videoSource = new TextureVideoSource(_renderTexture);
                _localVideoTrack = LocalVideoTrack.CreateVideoTrack(_currentTrackName, _videoSource, room);

                TrackPublishOptions options = BuildTrackOptions();
                PublishTrackInstruction publishInstruction = room.LocalParticipant.PublishTrack(_localVideoTrack, options);
                while (!publishInstruction.IsDone)
                {
                    await Task.Delay(ParticipantPollIntervalMs);
                }

                if (publishInstruction.IsError)
                {
                    ConvaiUnityLogger.Error($"{LogPrefix} LiveKit rejected the video track publish request.", LogCategory.SDK);
                    CleanupVideoSource();
                    ReleaseRenderTexture();
                    return false;
                }

                _videoSource.Start();
                _pumpCoroutine = StartCoroutine(_videoSource.Update());
                _isPublishing = true;
                LogVerbose($"Started publishing {_currentTrackName} ({_currentProfile.Width}x{_currentProfile.Height}@{_currentProfile.Fps})");

                if (Application.isPlaying)
                {
                    CaptureFrameFromCamera();
                    _nextCaptureTimestamp = Time.unscaledTime + _captureInterval;
                }

                return true;
            }
            catch (Exception ex)
            {
                ConvaiUnityLogger.Error($"{LogPrefix} Exception while starting video publishing: {ex.Message}", LogCategory.SDK);
                CleanupVideoSource();
                ReleaseRenderTexture();
                return false;
            }
            finally
            {
                _isStarting = false;
            }
        }

        public async Task StopPublishingAsync()
        {
            if (!_isPublishing && !_isStarting && !_isStopping && _localVideoTrack == null)
            {
                return;
            }

            if (_isStopping)
            {
                while (_isStopping)
                {
                    await Task.Delay(ParticipantPollIntervalMs);
                }

                return;
            }

            _isStopping = true;
            try
            {
                if (_pumpCoroutine != null)
                {
                    StopCoroutine(_pumpCoroutine);
                    _pumpCoroutine = null;
                }

                _videoSource?.Stop();

                if (_localVideoTrack != null && _boundRoom?.LocalParticipant != null)
                {
                    try
                    {
                        UnpublishTrackInstruction unpublish = _boundRoom.LocalParticipant.UnpublishTrack(_localVideoTrack, true);
                        while (!unpublish.IsDone)
                        {
                            await Task.Delay(ParticipantPollIntervalMs);
                        }
                    }
                    catch (Exception ex)
                    {
                        ConvaiUnityLogger.DebugLog($"{LogPrefix} Unpublish threw: {ex.Message}", LogCategory.SDK);
                    }
                }

                CleanupVideoSource();
                _localVideoTrack = null;
                _isPublishing = false;
                LogVerbose("Stopped publishing video track.");
            }
            finally
            {
                ReleaseRenderTexture();
                _isStopping = false;
            }
        }

        public async Task<bool> ApplyPresetAsync(VideoPreset preset)
        {
            _currentProfile = preset switch
            {
                VideoPreset.GeminiLive => SanitizeProfile(geminiLiveProfile),
                _ => SanitizeProfile(snapshotProfile)
            };

            LogVerbose($"Applied preset {preset} ({_currentProfile.Width}x{_currentProfile.Height}@{_currentProfile.Fps})");
            RefreshCaptureInterval();

            if (!_isPublishing)
            {
                return true;
            }

            return await RestartWithCurrentSettingsAsync();
        }

        public async Task<bool> ApplyCustomProfileAsync(
            CaptureProfile profile,
            bool? simulcastOverride = null,
            string codecOverride = null,
            string trackSourceOverride = null,
            string trackNameOverride = null)
        {
            _currentProfile = SanitizeProfile(profile);

            if (simulcastOverride.HasValue)
            {
                _currentSimulcast = simulcastOverride.Value;
            }

            if (!string.IsNullOrWhiteSpace(codecOverride) && TryResolveCodec(codecOverride, out VideoCodec parsedCodec))
            {
                _currentCodec = parsedCodec;
            }

            if (!string.IsNullOrWhiteSpace(trackSourceOverride) && TryResolveTrackSource(trackSourceOverride, out TrackSource parsedSource))
            {
                _currentTrackSource = parsedSource;
            }

            if (!string.IsNullOrWhiteSpace(trackNameOverride))
            {
                _currentTrackName = trackNameOverride.Trim();
            }

            LogVerbose($"Applied custom profile {_currentProfile.Width}x{_currentProfile.Height}@{_currentProfile.Fps} bitrate {_currentProfile.MaxBitrate} simulcast {_currentSimulcast} codec {_currentCodec}");
            RefreshCaptureInterval();

            if (!_isPublishing)
            {
                return true;
            }

            return await RestartWithCurrentSettingsAsync();
        }

        public void SetMuted(bool muted)
        {
            if (_videoSource == null)
            {
                return;
            }

            _videoSource.SetMute(muted);
            LogVerbose(muted ? "Video track muted." : "Video track unmuted.");
        }

        private async Task<bool> RestartWithCurrentSettingsAsync()
        {
            await StopPublishingAsync();
            if (_boundRoom == null)
            {
                return false;
            }

            return await EnsurePublishingAsync(_boundRoom);
        }

        protected virtual async Task<bool> WaitForLocalParticipantAsync(Room room)
        {
            int waited = 0;
            while (room.LocalParticipant == null && waited < ParticipantTimeoutMs)
            {
                await Task.Delay(ParticipantPollIntervalMs);
                waited += ParticipantPollIntervalMs;
            }

            return room.LocalParticipant != null;
        }

        protected virtual void PrepareRenderTexture()
        {
            ReleaseRenderTexture();

            CaptureProfile profile = _currentProfile;
            if (!profile.IsValid)
            {
                profile = SanitizeProfile(defaultProfile);
                _currentProfile = profile;
            }

            _cameraRenderTexture = new RenderTexture(profile.Width, profile.Height, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 1,
                useMipMap = false,
                autoGenerateMips = false,
                name = $"ConvaiVision_Capture_{profile.Width}x{profile.Height}"
            };

            _cameraRenderTexture.Create();

            _renderTexture = new RenderTexture(profile.Width, profile.Height, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 1,
                useMipMap = false,
                autoGenerateMips = false,
                name = $"ConvaiVision_{profile.Width}x{profile.Height}"
            };

            _renderTexture.Create();
            ResetCaptureTiming();
            RefreshCaptureInterval();
        }

        protected virtual void ReleaseRenderTexture()
        {
            ResetCaptureTiming();

            if (captureCamera != null && captureCamera.targetTexture == _cameraRenderTexture)
            {
                captureCamera.targetTexture = null;
            }

            if (_renderTexture != null)
            {
                if (_renderTexture.IsCreated())
                {
                    _renderTexture.Release();
                }

                Destroy(_renderTexture);
                _renderTexture = null;
            }

            if (_cameraRenderTexture != null)
            {
                if (_cameraRenderTexture.IsCreated())
                {
                    _cameraRenderTexture.Release();
                }

                Destroy(_cameraRenderTexture);
                _cameraRenderTexture = null;
            }
        }

        protected virtual TrackPublishOptions BuildTrackOptions()
        {
            uint maxBitrate = (uint)Math.Max(1, _currentProfile.MaxBitrate);
            uint maxFramerate = (uint)Math.Max(1, _currentProfile.Fps);

            return new TrackPublishOptions
            {
                VideoEncoding = new VideoEncoding
                {
                    MaxBitrate = maxBitrate,
                    MaxFramerate = maxFramerate
                },
                VideoCodec = _currentCodec,
                Simulcast = _currentSimulcast,
                Source = _currentTrackSource
            };
        }

        protected virtual void CleanupVideoSource()
        {
            if (_pumpCoroutine != null)
            {
                StopCoroutine(_pumpCoroutine);
                _pumpCoroutine = null;
            }

            if (_videoSource != null)
            {
                _videoSource.Stop();
                _videoSource.Dispose();
                _videoSource = null;
            }
        }

        protected virtual void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (!_isPublishing || captureCamera == null || _renderTexture == null || _cameraRenderTexture == null)
            {
                return;
            }

            if (!captureCamera.gameObject.activeInHierarchy)
            {
                return;
            }

            double now = Time.unscaledTime;
            if (now < _nextCaptureTimestamp)
            {
                return;
            }

            _nextCaptureTimestamp = now + _captureInterval;

            CaptureFrameFromCamera();
        }

        protected virtual void CaptureFrameFromCamera()
        {
            if (captureCamera == null || _cameraRenderTexture == null || _renderTexture == null)
            {
                return;
            }

            RenderTexture previousTarget = captureCamera.targetTexture;
            bool wasEnabled = captureCamera.enabled;
            bool previousForceIntoRenderTexture = captureCamera.forceIntoRenderTexture;

            try
            {
                if (!wasEnabled)
                {
                    captureCamera.enabled = true;
                }

                captureCamera.forceIntoRenderTexture = true;
                captureCamera.targetTexture = _cameraRenderTexture;
                captureCamera.Render();
                Graphics.Blit(_cameraRenderTexture, _renderTexture, FlipScale, FlipOffset);
            }
            catch (Exception ex)
            {
                ConvaiUnityLogger.DebugLog($"{LogPrefix} Capture frame failed: {ex.Message}", LogCategory.SDK);
            }
            finally
            {
                captureCamera.targetTexture = previousTarget;
                captureCamera.forceIntoRenderTexture = previousForceIntoRenderTexture;

                if (!wasEnabled)
                {
                    captureCamera.enabled = false;
                }
            }
        }

        protected virtual void ResetCaptureTiming()
        {
            _nextCaptureTimestamp = Time.unscaledTime;
        }

        protected virtual void RefreshCaptureInterval()
        {
            int fps = Mathf.Clamp(_currentProfile.Fps, 1, 120);
            _captureInterval = 1d / fps;
        }

        private static CaptureProfile SanitizeProfile(CaptureProfile profile)
        {
            profile.Width = Mathf.Clamp(profile.Width, 16, 4096);
            profile.Height = Mathf.Clamp(profile.Height, 16, 4096);
            profile.Fps = Mathf.Clamp(profile.Fps, 1, 120);
            profile.MaxBitrate = Mathf.Max(profile.MaxBitrate, 64_000);
            return profile;
        }

        protected virtual void LogVerbose(string message)
        {
            if (verboseLogging)
            {
                ConvaiUnityLogger.DebugLog($"{LogPrefix} {message}", LogCategory.SDK);
            }
        }
    }
}


