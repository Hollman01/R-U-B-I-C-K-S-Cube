using System;
using System.Threading.Tasks;
using LiveKit;
using LiveKit.Proto;
using Convai.Scripts.LoggerSystem;
using Convai.Scripts.Networking.Transport;
using UnityEngine;
using Meta.XR;

public class ConvaiVisionPublisherMeta : ConvaiVisionPublisher
{
    [Header("Passthrough Capture")]
    [SerializeField] private PassthroughCameraAccess passthroughCameraAccess;
    [SerializeField] private bool autoStartPassthrough = true;
    [SerializeField] private int passthroughPollIntervalMs = 100;
    [SerializeField] private int passthroughInitTimeoutMs = 50000;

    protected Texture _passthroughTexture;
    protected bool _passthroughWarningLogged;

    private async void Start()
    {
        if (autoStartPassthrough)
        {
            await EnsurePassthroughReadyAsync();
        }
    }

    public override async Task<bool> EnsurePublishingAsync(Room room)
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
            if (!await EnsurePassthroughReadyAsync())
            {
                return false;
            }

            if (!await WaitForLocalParticipantAsync(room))
            {
                ConvaiUnityLogger.Warn($"{LogPrefix} Timed out waiting for LocalParticipant before publishing video.", LogCategory.SDK);
                return false;
            }

            _videoSource = new TextureVideoSource(_passthroughTexture);
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

    protected virtual async Task<bool> EnsurePassthroughReadyAsync()
    {
        if (passthroughCameraAccess == null)
        {
            if (!_passthroughWarningLogged)
            {
                ConvaiUnityLogger.Error($"{LogPrefix} PassthroughCameraAccess reference is not assigned.", LogCategory.SDK);
                _passthroughWarningLogged = true;
            }
            return false;
        }

        if (!await EnsurePassthroughPermissionAsync())
        {
            return false;
        }

        if (!passthroughCameraAccess.IsPlaying)
        {
            int waited = 0;
            while (!passthroughCameraAccess.IsPlaying && waited < passthroughInitTimeoutMs)
            {
                await Task.Delay(passthroughPollIntervalMs);
                waited += passthroughPollIntervalMs;
            }

            if (!passthroughCameraAccess.IsPlaying)
            {
                ConvaiUnityLogger.Error($"{LogPrefix} Passthrough camera did not start playing within timeout.", LogCategory.SDK);
                return false;
            }
        }

        Texture resolvedTexture = null;
        int textureWaited = 0;
        while (resolvedTexture == null && textureWaited < passthroughInitTimeoutMs)
        {
            try
            {
                resolvedTexture = passthroughCameraAccess.GetTexture();
            }
            catch (Exception ex)
            {
                if (!_passthroughWarningLogged)
                {
                    ConvaiUnityLogger.DebugLog($"{LogPrefix} Passthrough texture not ready: {ex.Message}", LogCategory.SDK);
                    _passthroughWarningLogged = true;
                }
            }

            if (resolvedTexture == null)
            {
                await Task.Delay(passthroughPollIntervalMs);
                textureWaited += passthroughPollIntervalMs;
            }
        }

        if (resolvedTexture == null)
        {
            ConvaiUnityLogger.Error($"{LogPrefix} Passthrough camera texture did not become available within timeout.", LogCategory.SDK);
            return false;
        }

        _passthroughTexture = resolvedTexture;
        _passthroughWarningLogged = false;
        return true;
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    protected virtual async Task<bool> EnsurePassthroughPermissionAsync()
    {
        bool HasPermission()
        {
            return OVRPermissionsRequester.IsPermissionGranted(OVRPermissionsRequester.Permission.Scene) &&
                   OVRPermissionsRequester.IsPermissionGranted(OVRPermissionsRequester.Permission.PassthroughCameraAccess);
        }

        if (!HasPermission())
        {
            OVRPermissionsRequester.Request(new[]
            {
                OVRPermissionsRequester.Permission.Scene,
                OVRPermissionsRequester.Permission.PassthroughCameraAccess
            });
        }

        int waited = 0;
        while (!HasPermission() && waited < passthroughInitTimeoutMs)
        {
            await Task.Delay(passthroughPollIntervalMs);
            waited += passthroughPollIntervalMs;
        }

        if (!HasPermission())
        {
            ConvaiUnityLogger.Error($"{LogPrefix} Passthrough permissions were not granted by the user.", LogCategory.SDK);
            return false;
        }

        return true;
    }
#else
    protected virtual Task<bool> EnsurePassthroughPermissionAsync()
    {
        return Task.FromResult(true);
    }
#endif
}
