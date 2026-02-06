using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Convai.RestAPI;
using Convai.RestAPI.Internal;
using Convai.Scripts;
using Convai.Scripts.Configuration;
using Convai.Scripts.LoggerSystem;
using Convai.Scripts.Networking.Transport;
using Convai.Scripts.Player;
using Convai.Scripts.RTVI.Inbound;
using LiveKit;
using LiveKit.Proto;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using RoomOptions = LiveKit.RoomOptions;

public class ConvaiRoomManager : MonoBehaviour
{
    private const float HEARTBEAT_INTERVAL_SECONDS = 10f;
    [field: SerializeField] public ConvaiConnectionType ConnectionType { get; private set; } = ConvaiConnectionType.Audio;
    [field: SerializeField] public ConvaiLLMProvider LLMProvider { get; private set; } = ConvaiLLMProvider.Dynamic;
    [field: SerializeField] public string CoreServerURL { get; private set; }
    [field: SerializeField] public bool HasRoomDetails { get; private set; }
    [field: SerializeField] public bool IsConnectedToRoom { get; private set; }
    [SerializeField] public UnityEvent OnRoomConnectionSuccessful;
    [SerializeField] public UnityEvent OnRoomConnectionFailed;
    [SerializeField] public UnityEvent<bool> OnMicMuteChanged;

    // Bot lifecycle events
    [SerializeField] public UnityEvent OnBotReady; // Fired when bot is ready and can be talked with
    [SerializeField] public UnityEvent OnBotStartedSpeaking; // Fired when bot starts speaking
    [SerializeField] public UnityEvent OnBotStoppedSpeaking; // Fired when bot stops speaking

    // User speaking events
    [SerializeField] public UnityEvent OnUserStartedSpeaking; // Fired when user starts speaking
    [SerializeField] public UnityEvent OnUserStoppedSpeaking; // Fired when user stops speaking

    // Bot processing events
    [SerializeField] public UnityEvent OnBotLLMStarted; // Fired when LLM starts processing
    [SerializeField] public UnityEvent OnBotLLMStopped; // Fired when LLM stops processing
    [SerializeField] public UnityEvent OnBotTTSStarted; // Fired when TTS starts processing
    [SerializeField] public UnityEvent OnBotTTSStopped; // Fired when TTS stops processing

    // Room participant events
    [SerializeField] public UnityEvent<Participant> OnParticipantConnected; // Fired when a participant joins
    [SerializeField] public UnityEvent<Participant> OnParticipantDisconnected; // Fired when a participant leaves

    // Room connection state events
    [SerializeField] public UnityEvent OnRoomDisconnected; // Fired when room disconnects
    [SerializeField] public UnityEvent OnRoomReconnecting; // Fired when room is reconnecting
    [SerializeField] public UnityEvent OnRoomReconnected; // Fired when room reconnects

    // Track events
    [SerializeField] public UnityEvent<IRemoteTrack, RemoteTrackPublication, RemoteParticipant> OnTrackSubscribed; // Fired when a track is subscribed
    [SerializeField] public UnityEvent<IRemoteTrack, RemoteTrackPublication, RemoteParticipant> OnTrackUnsubscribed; // Fired when a track is unsubscribed

    [SerializeField] private ConvaiVisionPublisher visionPublisher;

    // Private state
    private ConvaiConfigurationDataSO _configuration;
    private LocalAudioTrack _currentAudioTrack;
    private Coroutine _heartbeatCoroutine;
    private MicrophoneSource _microphoneSource;

    // Public references & state
    public static ConvaiRoomManager Instance { get; private set; }
    public Room Room { get; private set; }
    public ConvaiPlayer Player { get; set; }
    public bool IsMicMuted { get; private set; }
    public RTVIHandler RTVIHandler { get; set; }
    public Dictionary<ConvaiNPC, ConvaiRealtimeNPCData> NpcToParticipantMap { get; private set; }
    public List<ConvaiNPC> NpcList { get; private set; }

    // Private connection/session variables
    private string Token { get; set; }
    private string RoomName { get; set; }
    private string SessionID { get; set; }
    private string RoomURL { get; set; }
    private string CharacterSessionID { get; set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator Start()
    {
        Room = new Room();
        if (!ConvaiConfigurationDataSO.GetData(out _configuration))
        {
            ConvaiUnityLogger.Error("Convai Configuration Data not found", LogCategory.SDK);
            OnRoomConnectionFailed?.Invoke();
            yield break;
        }

        ConvaiPlayer playerObject = FindAnyObjectByType<ConvaiPlayer>();
        if (playerObject == null)
        {
            ConvaiUnityLogger.Error("ConvaiPlayer not found in the scene", LogCategory.SDK);
            OnRoomConnectionFailed?.Invoke();
            yield break;
        }

        Player = playerObject;

        NpcToParticipantMap = new Dictionary<ConvaiNPC, ConvaiRealtimeNPCData>();
        NpcList = FindObjectsByType<ConvaiNPC>(FindObjectsSortMode.None).ToList();
        NpcList.ForEach(npc => NpcToParticipantMap[npc] = new ConvaiRealtimeNPCData());
        if (NpcList.Count == 0)
        {
            ConvaiUnityLogger.Error("No ConvaiNPCs found in the scene", LogCategory.SDK);
            OnRoomConnectionFailed?.Invoke();
            yield break;
        }

        HasRoomDetails = false;

        // Per-character session resume: use NPC flag to decide
        string storedSessionId = null;
        ConvaiNPC activeNpc = NpcList[0];
        if (activeNpc.EnableSessionResume)
        {
            storedSessionId = _configuration.GetCharacterSessionId(activeNpc.CharacterID);
            if (!string.IsNullOrEmpty(storedSessionId))
            {
                ConvaiUnityLogger.Info(
                    $"Session resumption enabled for NPC {activeNpc.CharacterName}. Using stored session ID: {storedSessionId}",
                    LogCategory.SDK);
            }
        }

        ConvaiRoomRequest roomRequest = new(
            _configuration.APIKey,
            activeNpc.CharacterID,
            "livekit",
            ConnectionTypeToString(ConnectionType), // Convert enum to string for API
            LLMProviderToString(LLMProvider), // Convert enum to string for API
            CoreServerURL,
            storedSessionId
        );

        Task<bool> connectTask = ConnectToConvai(roomRequest, activeNpc.CharacterID, activeNpc.EnableSessionResume);
        yield return new WaitUntil(() => connectTask.IsCompleted);
        bool connected = connectTask.Result;
        HasRoomDetails = connected;
        IsConnectedToRoom = connected;
        if (!connected)
        {
            ConvaiUnityLogger.Error("Failed to connect to Convai and Convai Realtime room", LogCategory.SDK);
            OnRoomConnectionFailed?.Invoke();
            yield break;
        }

        ConvaiUnityLogger.Info("Connected to Convai and Convai Realtime room successfully", LogCategory.SDK);

        InitializeVisionPublishingIfNeeded();
        
        OnRoomConnectionSuccessful?.Invoke();
    }


    // Starts periodic heartbeat to keep connection healthy
    private void StartHeartbeat()
    {
        StopHeartbeat();
        _heartbeatCoroutine = StartCoroutine(HeartbeatRoutine());
    }

    // Stops heartbeat coroutine if running
    private void StopHeartbeat()
    {
        if (_heartbeatCoroutine != null)
        {
            StopCoroutine(_heartbeatCoroutine);
            _heartbeatCoroutine = null;
        }
    }

    // Sends periodic heartbeats via room data channel
    private IEnumerator HeartbeatRoutine()
    {
        WaitForSeconds wait = new(HEARTBEAT_INTERVAL_SECONDS);
        while (Room is { ConnectionState: ConnectionState.ConnConnected } && _heartbeatCoroutine != null)
        {
            try
            {
                object payload = new { type = "heartbeat", ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() };
                string json = JsonConvert.SerializeObject(payload);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                Room.LocalParticipant?.PublishData(bytes);
            }
            catch (Exception ex)
            {
                ConvaiUnityLogger.DebugLog($"Heartbeat send failed: {ex.Message}", LogCategory.SDK);
            }

            yield return wait;
        }
    }

    /// <summary>
    ///     Connects to a Convai room using the specified room request and character ID, with optional session resume.
    ///     Attempts to establish a connection to the Convai backend and join the room for the given character.
    /// </summary>
    /// <param name="roomRequest">The request object containing room connection parameters.</param>
    /// <param name="characterId">The unique identifier of the character to connect as.</param>
    /// <param name="enableSessionResume">If true, attempts to resume a previous session if possible.</param>
    /// <returns>Returns true if the connection to the Convai room was successful; otherwise, false.</returns>
    public async Task<bool> ConnectToConvai(ConvaiRoomRequest roomRequest, string characterId, bool enableSessionResume)
    {
        // Attempt 1: use provided session id if present
        (bool success, RoomDetails details, string error) attemptWithSessionId = await TryGetRoomDetailsAsync(roomRequest);
        if (attemptWithSessionId.success)
        {
            ApplyRoomDetails(attemptWithSessionId.details);
            if (enableSessionResume && !string.IsNullOrEmpty(CharacterSessionID))
            {
                _configuration.StoreCharacterSessionId(characterId, CharacterSessionID);
                ConvaiUnityLogger.Info($"Stored character session ID for character {characterId}: {CharacterSessionID}",
                    LogCategory.SDK);
            }

            // Now connect to Convai Realtime room and initialize services
            return await ConnectToRoomAndInitialize();
        }

        // If the error indicates an invalid session id, clear and retry once without it
        bool invalidSession = enableSessionResume &&
                              !string.IsNullOrEmpty(roomRequest.CharacterSessionId) &&
                              !string.IsNullOrEmpty(attemptWithSessionId.error) &&
                              attemptWithSessionId.error.Contains("Invalid character_session_id");
        if (invalidSession)
        {
            ConvaiUnityLogger.Warn("Invalid stored character_session_id. Clearing and retrying without session.",
                LogCategory.SDK);
            _configuration.ClearCharacterSessionId(characterId);
            roomRequest.CharacterSessionId = null;

            (bool success, RoomDetails details, string error) attemptWithoutSessionId = await TryGetRoomDetailsAsync(roomRequest);
            if (attemptWithoutSessionId.success)
            {
                ApplyRoomDetails(attemptWithoutSessionId.details);

                // Now connect to Convai Realtime room and initialize services
                return await ConnectToRoomAndInitialize();
            }

            ConvaiUnityLogger.Error($"Error: {attemptWithoutSessionId.error}", LogCategory.SDK);
            return false;
        }

        ConvaiUnityLogger.Error($"Error: {attemptWithSessionId.error}", LogCategory.SDK);
        return false;
    }

    private static async Task<(bool success, RoomDetails details, string error)> TryGetRoomDetailsAsync(ConvaiRoomRequest roomRequest)
    {
        ConvaiUnityLogger.DebugLog($"Connecting to Convai with request: {JsonConvert.SerializeObject(roomRequest)}",
            LogCategory.SDK);
        ConvaiREST.GetRoomConnectionOperation operation = new(roomRequest);
        while (!operation.IsCompleted)
        {
            ConvaiUnityLogger.Info("Waiting for room connection...", LogCategory.SDK);
            await Task.Delay(100);
        }

        if (operation.WasSuccess)
        {
            return (true, operation.Result, null);
        }

        return (false, null, operation.ErrorMessage);
    }

    private void ApplyRoomDetails(RoomDetails roomDetails)
    {
        string result = JsonConvert.SerializeObject(roomDetails);
        ConvaiUnityLogger.Info($"🔑 ROOM DETAILS RECEIVED: {result}", LogCategory.SDK);
        Token = roomDetails.Token;
        RoomName = roomDetails.RoomName;
        SessionID = roomDetails.SessionId;
        RoomURL = roomDetails.RoomURL;
        CharacterSessionID = roomDetails.CharacterSessionId;
        ConvaiUnityLogger.Info(
            $"Token: {Token}; Room Name: {RoomName}; Room URL: {RoomURL}; Session ID: {SessionID}; Character Session ID: {CharacterSessionID}",
            LogCategory.SDK);
    }

    /// <summary>
    ///     Connects to the Convai Realtime room and initializes required services (heartbeat and RTVI handler).
    /// </summary>
    /// <returns>True if connection and initialization was successful, false otherwise.</returns>
    private async Task<bool> ConnectToRoomAndInitialize()
    {
        // Now connect to Convai Realtime room
        bool roomConnected = await ConnectToRoom();
        if (roomConnected)
        {
            ConvaiUnityLogger.Info("Connected to Convai and Convai Realtime room successfully", LogCategory.SDK);
            StartHeartbeat();
            RTVIHandler = new RTVIHandler(Room, this);
            return true;
        }

        return false;
    }

    private async void InitializeVisionPublishingIfNeeded()
    {
        if (ConnectionType != ConvaiConnectionType.Video)
        {
            return;
        }

        if (Room == null)
        {
            ConvaiUnityLogger.Warn("Video connection requested but Room is not yet available.", LogCategory.SDK);
            return;
        }

        if (visionPublisher == null)
        {
            ConvaiUnityLogger.Warn("ConvaiVisionPublisher is not assigned; video frames will not be published.", LogCategory.SDK);
            return;
        }

        try
        {
            visionPublisher.BindRoom(Room);

            ConvaiVisionPublisher.VideoPreset preset =
                LLMProvider == ConvaiLLMProvider.GeminiLive
                    ? ConvaiVisionPublisher.VideoPreset.GeminiLive
                    : ConvaiVisionPublisher.VideoPreset.Snapshot;

            bool presetApplied = await visionPublisher.ApplyPresetAsync(preset);
            bool started = await visionPublisher.EnsurePublishingAsync(Room);

            if (!presetApplied)
            {
                ConvaiUnityLogger.Warn($"Failed to apply default video preset {preset}.", LogCategory.SDK);
            }

            if (!started)
            {
                ConvaiUnityLogger.Warn("Failed to start video publishing after room connection.", LogCategory.SDK);
            }
        }
        catch (Exception ex)
        {
            ConvaiUnityLogger.Error($"Error while initializing vision publishing: {ex.Message}", LogCategory.SDK);
        }
    }

    private async Task<bool> ConnectToRoom()
    {
        ConvaiUnityLogger.DebugLog("Connecting to Room...", LogCategory.SDK);

        // Event subscriptions
        Room.TrackSubscribed += OnTrackSubscribedInternal;
        Room.TrackUnsubscribed += OnTrackUnsubscribedInternal;

        // Voice Activity Detection and interruption events
        Room.ActiveSpeakersChanged += OnActiveSpeakersChanged;
        Room.TrackMuted += OnTrackMuted;
        Room.TrackUnmuted += OnTrackUnMuted;

        // Core connectivity events
        Room.ParticipantConnected += OnParticipantConnectedInternal;
        Room.ParticipantDisconnected += OnParticipantDisconnectedInternal;
        Room.Connected += OnRoomConnected;
        Room.Disconnected += OnRoomDisconnectedInternal;
        Room.Reconnecting += OnRoomReconnectingInternal;
        Room.Reconnected += OnRoomReconnectedInternal;
        Room.TrackPublished += OnTrackPublished;

        // Configure room options for optimal audio
        RoomOptions options = new()
        {
            AutoSubscribe = true, // Automatically subscribe to tracks
            Dynacast = true, // Enable dynamic casting
            AdaptiveStream = true // Enable adaptive streaming
        };

        (bool success, string error) = await TryConnectConvaiRealtimeRoomAsync(options);
        if (success)
        {
            ConvaiUnityLogger.Info("Connected to room: " + RoomName, LogCategory.SDK);
            ConvaiUnityLogger.Info("Session ID: " + SessionID, LogCategory.SDK);
            ConvaiUnityLogger.Info($"Room connection state: {Room.ConnectionState}", LogCategory.SDK);
            return true;
        }

        ConvaiUnityLogger.Error($"Failed to connect to Convai Realtime: {error}", LogCategory.SDK);
        return false;
    }

    private async Task<(bool success, string error)> TryConnectConvaiRealtimeRoomAsync(RoomOptions options)
    {
        ConvaiUnityLogger.Info($"Connecting to room with URL: {RoomURL}", LogCategory.SDK);
        ConnectInstruction connect = Room.Connect(RoomURL, Token, options);
        while (!connect.IsDone)
        {
            ConvaiUnityLogger.Info("Waiting for room connection...", LogCategory.SDK);
            await Task.Delay(100);
        }

        if (!connect.IsError)
        {
            return (true, null);
        }

        return (false, connect.ToString());
    }

    public void DisconnectFromRoom()
    {
        ConvaiUnityLogger.Info("Disconnecting from room...", LogCategory.SDK);

        if (visionPublisher != null)
        {
            visionPublisher.BindRoom(null);
            _ = visionPublisher.StopPublishingAsync();
        }

        StopHeartbeat();

        if (Room != null)
        {
            Room.TrackSubscribed -= OnTrackSubscribedInternal;
            Room.TrackUnsubscribed -= OnTrackUnsubscribedInternal;
            Room.ActiveSpeakersChanged -= OnActiveSpeakersChanged;
            Room.TrackMuted -= OnTrackMuted;
            Room.TrackUnmuted -= OnTrackUnMuted;

            Room.ParticipantConnected -= OnParticipantConnectedInternal;
            Room.ParticipantDisconnected -= OnParticipantDisconnectedInternal;
            Room.Connected -= OnRoomConnected;
            Room.Disconnected -= OnRoomDisconnectedInternal;
            Room.Reconnecting -= OnRoomReconnectingInternal;
            Room.Reconnected -= OnRoomReconnectedInternal;
            Room.TrackPublished -= OnTrackPublished;

            Room.Disconnect();
        }

        _microphoneSource = null;
        _currentAudioTrack = null;
        RTVIHandler = null;

        foreach (KeyValuePair<ConvaiNPC, ConvaiRealtimeNPCData> data in NpcToParticipantMap)
        {
            data.Value.AudioStream?.Dispose();
            data.Value.AudioStream = null;
            data.Value.AudioSource = null;

            // Also clean up any AudioSource components on NPCs
            if (data.Key.TryGetComponent(out AudioSource audioSource))
            {
                audioSource.Stop();
                audioSource.clip = null;
                Destroy(audioSource);
                ConvaiUnityLogger.Info($"Cleaned up AudioSource on NPC: {data.Key.name}", LogCategory.SDK);
            }
        }

        Room = null;
        Token = null;
        RoomName = null;
        SessionID = null;
        CharacterSessionID = null;

        IsMicMuted = false;

        ConvaiUnityLogger.Info("Disconnected from room", LogCategory.SDK);
    }

    public async void StartListening(int microphoneIndex = 0)
    {
        try
        {
            ConvaiUnityLogger.Info($"StartListening called with microphone index: {microphoneIndex}", LogCategory.SDK);

            if (Room == null)
            {
                ConvaiUnityLogger.Error("Room is null - not connected to room", LogCategory.SDK);
                return;
            }

            if (Room.LocalParticipant == null)
            {
                ConvaiUnityLogger.Error("LocalParticipant is null - not connected to room", LogCategory.SDK);
                return;
            }

            ConvaiUnityLogger.Info($"Room connection state: {Room.ConnectionState}", LogCategory.SDK);
            ConvaiUnityLogger.Info($"Available microphones: {string.Join(", ", Microphone.devices)}", LogCategory.SDK);

            if (microphoneIndex >= Microphone.devices.Length)
            {
                ConvaiUnityLogger.Error(
                    $"Invalid microphone index: {microphoneIndex}, available devices: {Microphone.devices.Length}",
                    LogCategory.SDK);
                return;
            }

            // Check microphone permission
            if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                ConvaiUnityLogger.Info("Requesting microphone permission...", LogCategory.SDK);
                AsyncOperation permission = Application.RequestUserAuthorization(UserAuthorization.Microphone);

                // Wait for permission response
                while (!permission.isDone)
                {
                    await Task.Delay(100);
                }

                if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
                {
                    ConvaiUnityLogger.Error("Microphone permission denied", LogCategory.SDK);
                    return;
                }
            }

            if (_microphoneSource != null)
            {
                ConvaiUnityLogger.Warn("Already listening - stopping previous microphone", LogCategory.SDK);
                await StopListeningAsync();
            }

            try
            {
                MicrophoneSource microphoneSource = new(Microphone.devices[microphoneIndex], Player.gameObject);

                // Start microphone BEFORE creating and publishing track
                microphoneSource.Start();
                if (IsMicMuted)
                {
                    microphoneSource.SetMute(true);
                }

                ConvaiUnityLogger.Info($"Microphone started: {Microphone.devices[microphoneIndex]}", LogCategory.SDK);

                LocalAudioTrack localAudioTrack =
                    LocalAudioTrack.CreateAudioTrack("player-microphone", microphoneSource, Room);
                TrackPublishOptions options = new()
                {
                    AudioEncoding = new AudioEncoding { MaxBitrate = 64000 }, Source = TrackSource.SourceMicrophone
                };
                PublishTrackInstruction publish = Room.LocalParticipant.PublishTrack(localAudioTrack, options);

                while (!publish.IsDone)
                {
                    ConvaiUnityLogger.Info("Waiting for track to be published...", LogCategory.SDK);
                    await Task.Delay(100);
                }

                if (!publish.IsError)
                {
                    ConvaiUnityLogger.Info("Track published successfully", LogCategory.SDK);
                    _microphoneSource = microphoneSource;
                    _currentAudioTrack = localAudioTrack;
                }
                else
                {
                    ConvaiUnityLogger.Error($"Failed to publish track: {publish}", LogCategory.SDK);
                    microphoneSource.Stop();
                }
            }
            catch (Exception ex)
            {
                ConvaiUnityLogger.Error($"Exception in StartListening: {ex.Message}", LogCategory.SDK);
            }
        }
        catch (Exception e)
        {
            ConvaiUnityLogger.Error($"Exception in StartListening: {e.Message}",
                LogCategory.SDK);
        }
    }

    public void StopListening() =>
        // Use async version but don't await to maintain synchronous interface
        _ = StopListeningAsync();

    private async Task StopListeningAsync()
    {
        if (_microphoneSource != null || _currentAudioTrack != null)
        {
            ConvaiUnityLogger.Info("Stopping microphone listening and unpublishing track", LogCategory.SDK);

            // First unpublish the track from Convai Realtime
            if (_currentAudioTrack != null && Room?.LocalParticipant != null)
            {
                try
                {
                    ConvaiUnityLogger.Info($"Unpublishing track with SID: {_currentAudioTrack.Sid}", LogCategory.SDK);
                    UnpublishTrackInstruction
                        unpublish = Room.LocalParticipant.UnpublishTrack(_currentAudioTrack, true);

                    // Wait for unpublish to complete
                    while (!unpublish.IsDone)
                    {
                        await Task.Delay(50);
                    }

                    if (!unpublish.IsError)
                    {
                        ConvaiUnityLogger.Info("Track unpublished successfully", LogCategory.SDK);
                    }
                    else
                    {
                        ConvaiUnityLogger.Error("Failed to unpublish track", LogCategory.SDK);
                    }
                }
                catch (Exception ex)
                {
                    ConvaiUnityLogger.Error($"Exception while unpublishing track: {ex.Message}", LogCategory.SDK);
                }

                _currentAudioTrack = null;
            }

            // Then stop the microphone source
            if (_microphoneSource != null)
            {
                try
                {
                    _microphoneSource.Stop();
                    ConvaiUnityLogger.Info("Microphone source stopped", LogCategory.SDK);
                }
                catch (Exception ex)
                {
                    ConvaiUnityLogger.Error($"Exception while stopping microphone: {ex.Message}", LogCategory.SDK);
                }

                _microphoneSource = null;
            }
        }
    }

    public void SetMicMuted(bool mute)
    {
        IsMicMuted = mute;
        if (_microphoneSource != null)
        {
            try
            {
                _microphoneSource.SetMute(mute);
            }
            catch (Exception ex)
            {
                ConvaiUnityLogger.DebugLog($"Failed to set mic mute: {ex.Message}", LogCategory.SDK);
            }
        }

        OnMicMuteChanged?.Invoke(IsMicMuted);
    }

    public void ToggleMicMute() => SetMicMuted(!IsMicMuted);

    public bool SetNpcAudioMuted(ConvaiNPC npc, bool mute)
    {
        if (npc == null)
        {
            ConvaiUnityLogger.DebugLog("Attempted to set mute on a null NPC", LogCategory.SDK);
            return false;
        }

        if (NpcToParticipantMap == null || !NpcToParticipantMap.TryGetValue(npc, out ConvaiRealtimeNPCData npcData))
        {
            ConvaiUnityLogger.DebugLog($"NPC data not found when trying to set mute for {npc.name}", LogCategory.SDK);
            return false;
        }

        npcData.IsMuted = mute;

        if (npcData.AudioSource != null)
        {
            npcData.AudioSource.mute = mute;
            ConvaiUnityLogger.DebugLog($"Set mute={mute} for NPC audio source {npc.name}", LogCategory.SDK);
            return true;
        }

        ConvaiUnityLogger.DebugLog(
            $"Audio source not yet available for NPC {npc.name}. Stored mute state for future streams.",
            LogCategory.SDK);
        return false;
    }

    public bool MuteNpc(ConvaiNPC npc) => SetNpcAudioMuted(npc, true);

    public bool UnmuteNpc(ConvaiNPC npc) => SetNpcAudioMuted(npc, false);

    public bool IsNpcAudioMuted(ConvaiNPC npc)
    {
        if (npc == null || NpcToParticipantMap == null)
        {
            return false;
        }

        return NpcToParticipantMap.TryGetValue(npc, out ConvaiRealtimeNPCData npcData) && npcData.IsMuted;
    }

    private void OnRoomConnected(Room room)
    {
        ConvaiUnityLogger.Info($"🏠 ROOM CONNECTED: {room.Name}", LogCategory.SDK);
        ConvaiUnityLogger.Info($"   - Room SID: {room.Sid}", LogCategory.SDK);
        ConvaiUnityLogger.Info($"   - Local participant: {room.LocalParticipant?.Identity}", LogCategory.SDK);
        ConvaiUnityLogger.Info("   - Waiting for bot to join...", LogCategory.SDK);
    }

    private void OnRoomDisconnectedInternal(Room room)
    {
        ConvaiUnityLogger.Info($"Room disconnected: {room.Name}", LogCategory.SDK);
        if (visionPublisher != null)
        {
            visionPublisher.BindRoom(null);
            _ = visionPublisher.StopPublishingAsync();
        }
        OnRoomDisconnected?.Invoke();
    }

    private void OnRoomReconnectingInternal(Room room)
    {
        ConvaiUnityLogger.Info($"Room reconnecting: {room.Name}", LogCategory.SDK);
        OnRoomReconnecting?.Invoke();
    }

    private void OnRoomReconnectedInternal(Room room)
    {
        ConvaiUnityLogger.Info($"Room reconnected: {room.Name}", LogCategory.SDK);
        if (ConnectionType == ConvaiConnectionType.Video && visionPublisher != null)
        {
            visionPublisher.BindRoom(room);
            _ = visionPublisher.EnsurePublishingAsync(room);
        }
        OnRoomReconnected?.Invoke();
    }

    private void OnTrackUnsubscribedInternal(IRemoteTrack track, RemoteTrackPublication publication, RemoteParticipant participant)
    {
        ConvaiUnityLogger.Info($"Track unsubscribed: {track.Name} from participant: {participant.Identity}",
            LogCategory.SDK);
        OnTrackUnsubscribed?.Invoke(track, publication, participant);

        if (track is RemoteAudioTrack)
        {
            if (!NpcToParticipantMap.TryGetValue(NpcList[0], out ConvaiRealtimeNPCData npcData))
            {
                ConvaiUnityLogger.DebugLog("NPC data not found for unsubscribed track", LogCategory.SDK);
                return;
            }

            // Hardcoded to use the first NPC for audio playback, will be updated later
            if (NpcList[0].TryGetComponent(out AudioSource audioSource))
            {
                // Stop and clean up the AudioSource
                audioSource.Stop();
                audioSource.clip = null; // Clear any assigned audio clip
                ConvaiUnityLogger.Info($"Stopped and cleaned AudioSource on NPC: {NpcList[0].name}", LogCategory.SDK);

                // Remove the AudioSource component to prevent accumulation
                Destroy(audioSource);
                ConvaiUnityLogger.Info($"Destroyed AudioSource component on NPC: {NpcList[0].name}", LogCategory.SDK);
            }

            // Dispose the audio stream if it exists
            if (npcData.AudioStream != null)
            {
                npcData.AudioStream.Dispose();
                npcData.AudioStream = null;
                ConvaiUnityLogger.Info("Disposed audio stream on track unsubscribe", LogCategory.SDK);
            }

            npcData.AudioSource = null;
        }
    }

    private void OnTrackSubscribedInternal(IRemoteTrack track, RemoteTrackPublication publication, RemoteParticipant participant)
    {
        ConvaiUnityLogger.Info($"Track subscribed: {track.Name} from participant: {participant.Identity}",
            LogCategory.SDK);
        OnTrackSubscribed?.Invoke(track, publication, participant);

        if (track is RemoteAudioTrack audioTrack)
        {
            // Find the NPC associated with this participant
            ConvaiNPC targetNpc = FindNPCByParticipantId(participant.Sid);
            if (targetNpc == null)
            {
                // Fall back to first NPC if no mapping found
                if (NpcList != null && NpcList.Count > 0)
                {
                    targetNpc = NpcList[0];
                    ConvaiUnityLogger.Info($"No NPC mapping found for participant {participant.Identity}, using default NPC: {targetNpc.name}",
                        LogCategory.SDK);
                }
                else
                {
                    ConvaiUnityLogger.DebugLog("No NPCs available for audio track subscription", LogCategory.SDK);
                    return;
                }
            }

            AudioSource audioSource = targetNpc.GetComponent<AudioSource>();

            // Only add AudioSource if it doesn't exist
            if (audioSource == null)
            {
                audioSource = targetNpc.gameObject.AddComponent<AudioSource>();
                ConvaiUnityLogger.Info($"Added new AudioSource to NPC: {targetNpc.name}", LogCategory.SDK);
            }
            else
            {
                // Stop any existing audio before reusing the component
                audioSource.Stop();
                ConvaiUnityLogger.Info($"Reusing existing AudioSource on NPC: {targetNpc.name}", LogCategory.SDK);
            }

            // Configure AudioSource for proper playback
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = 1.0f;
            audioSource.spatialBlend = 0.0f; // 2D audio
            audioSource.priority = 128;

            // Dispose existing audio stream if present
            if (!NpcToParticipantMap.TryGetValue(targetNpc, out ConvaiRealtimeNPCData npcData))
            {
                ConvaiUnityLogger.DebugLog($"NPC data not found for subscribed track from NPC: {targetNpc.name}", LogCategory.SDK);
                return;
            }

            if (npcData.AudioStream != null)
            {
                npcData.AudioStream.Dispose();
                ConvaiUnityLogger.Info("Disposed existing audio stream", LogCategory.SDK);
            }

            npcData.AudioStream = new AudioStream(audioTrack, audioSource);
            npcData.AudioSource = audioSource;
            audioSource.mute = npcData.IsMuted;
            ConvaiUnityLogger.Info($"Audio stream created for track: {track.Name} on NPC: {targetNpc.name}", LogCategory.SDK);
        }
    }


    private void OnTrackUnMuted(TrackPublication publication, Participant participant)
    {
    }

    private void OnTrackMuted(TrackPublication publication, Participant participant)
    {
    }

    internal void HandleVideoControl(VideoControlPayload payload)
    {
        if (payload == null)
        {
            ConvaiUnityLogger.DebugLog("Received null video control payload.", LogCategory.SDK);
            return;
        }

        if (ConnectionType != ConvaiConnectionType.Video)
        {
            ConvaiUnityLogger.DebugLog("Ignoring video control message because connection type is not video.", LogCategory.SDK);
            return;
        }

        if (visionPublisher == null)
        {
            ConvaiUnityLogger.Warn("Video control requested but ConvaiVisionPublisher is not assigned.", LogCategory.SDK);
            return;
        }

        _ = HandleVideoControlAsync(payload);
    }

    private async Task HandleVideoControlAsync(VideoControlPayload payload)
    {
        try
        {
            if (Room == null)
            {
                ConvaiUnityLogger.Warn("Video control received but room is not connected.", LogCategory.SDK);
                return;
            }

            visionPublisher.BindRoom(Room);

            string action = payload.Action?.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(action))
            {
                ConvaiUnityLogger.DebugLog("Video control message is missing an action.", LogCategory.SDK);
                return;
            }

            switch (action)
            {
                case "start":
                    await visionPublisher.EnsurePublishingAsync(Room);
                    break;
                case "stop":
                    await visionPublisher.StopPublishingAsync();
                    break;
                case "pause":
                    visionPublisher.SetMuted(true);
                    break;
                case "resume":
                    visionPublisher.SetMuted(false);
                    break;
                case "set_preset":
                    if (!visionPublisher.TryResolvePreset(payload.Preset, out ConvaiVisionPublisher.VideoPreset preset))
                    {
                        ConvaiUnityLogger.Warn($"Unknown video preset '{payload.Preset}'.", LogCategory.SDK);
                        return;
                    }

                    if (!await visionPublisher.ApplyPresetAsync(preset))
                    {
                        ConvaiUnityLogger.Warn($"Failed to apply video preset '{payload.Preset}'.", LogCategory.SDK);
                    }
                    else if (!visionPublisher.IsPublishing)
                    {
                        await visionPublisher.EnsurePublishingAsync(Room);
                    }

                    break;
                case "set_custom":
                {
                    ConvaiVisionPublisher.CaptureProfile profile = visionPublisher.CreateProfile(
                        payload.Width,
                        payload.Height,
                        payload.Fps,
                        payload.MaxBitrate);

                    bool success = await visionPublisher.ApplyCustomProfileAsync(
                        profile,
                        payload.Simulcast,
                        payload.Codec,
                        payload.TrackSource,
                        payload.TrackName);

                    if (!success)
                    {
                        ConvaiUnityLogger.Warn("Failed to apply custom video profile.", LogCategory.SDK);
                    }
                    else if (!visionPublisher.IsPublishing)
                    {
                        await visionPublisher.EnsurePublishingAsync(Room);
                    }

                    break;
                }
                default:
                    ConvaiUnityLogger.DebugLog($"Unhandled video control action '{payload.Action}'.", LogCategory.SDK);
                    break;
            }
        }
        catch (Exception ex)
        {
            ConvaiUnityLogger.Error($"Exception while handling video control: {ex.Message}", LogCategory.SDK);
        }
    }

    private void OnActiveSpeakersChanged(List<Participant> speakers)
    {
    }

    private void OnParticipantConnectedInternal(Participant participant)
    {
        ConvaiUnityLogger.Info($"🎉 PARTICIPANT CONNECTED: {participant.Identity} (Name: {participant.Name})",
            LogCategory.SDK);
        OnParticipantConnected?.Invoke(participant);
        ConvaiUnityLogger.Info($"   - Participant SID: {participant.Sid}", LogCategory.SDK);
        ConvaiUnityLogger.Info($"   - Total remote participants now: {Room?.RemoteParticipants?.Count}",
            LogCategory.SDK);

        // Map the participant to the first available NPC (current single-NPC setup)
        if (NpcList != null && NpcList.Count > 0 && NpcToParticipantMap != null)
        {
            ConvaiNPC targetNpc = NpcList[0]; // Using first NPC for now
            if (NpcToParticipantMap.TryGetValue(targetNpc, out ConvaiRealtimeNPCData npcData))
            {
                npcData.ParticipantId = participant.Sid;
                ConvaiUnityLogger.Info($"   - Mapped participant {participant.Identity} (SID: {participant.Sid}) to NPC: {targetNpc.CharacterName}",
                    LogCategory.SDK);
            }
            else
            {
                ConvaiUnityLogger.DebugLog($"   - Failed to find NPC data for mapping participant {participant.Identity}",
                    LogCategory.SDK);
            }
        }
        else
        {
            ConvaiUnityLogger.DebugLog("   - Cannot map participant: NpcList or NpcToParticipantMap is null/empty",
                LogCategory.SDK);
        }
    }

    private void OnParticipantDisconnectedInternal(Participant participant)
    {
        OnParticipantDisconnected?.Invoke(participant);
        ConvaiUnityLogger.Info($"Participant disconnected: {participant.Identity}", LogCategory.SDK);
    }

    private void OnTrackPublished(RemoteTrackPublication publication, RemoteParticipant participant) =>
        ConvaiUnityLogger.Info($"Track published by {participant.Identity}: {publication.Name}", LogCategory.SDK);

    #region Session Management Methods

    /// <summary>
    ///     Gets the stored session ID for a specific character
    /// </summary>
    /// <param name="characterId">The character ID</param>
    /// <returns>The stored session ID, or null if not found</returns>
    public string GetStoredSessionId(string characterId) => _configuration?.GetCharacterSessionId(characterId);

    /// <summary>
    ///     Clears the stored session ID for a specific character
    /// </summary>
    /// <param name="characterId">The character ID</param>
    public void ClearStoredSessionId(string characterId)
    {
        _configuration?.ClearCharacterSessionId(characterId);
        ConvaiUnityLogger.Info($"Cleared stored session ID for character: {characterId}", LogCategory.SDK);
    }

    /// <summary>
    ///     Clears all stored session IDs
    /// </summary>
    public void ClearAllStoredSessionIds()
    {
        _configuration?.ClearAllCharacterSessionIds();
        ConvaiUnityLogger.Info("Cleared all stored session IDs", LogCategory.SDK);
    }

    /// <summary>
    ///     Gets the current character session ID from the active connection
    /// </summary>
    /// <returns>The current character session ID, or null if not connected</returns>
    public string GetCurrentCharacterSessionId() => CharacterSessionID;

    /// <summary>
    ///     Finds an NPC by participant ID
    /// </summary>
    /// <param name="participantId">The Convai Realtime participant ID to search for</param>
    /// <returns>The ConvaiNPC associated with the participant, or null if not found</returns>
    public ConvaiNPC FindNPCByParticipantId(string participantId)
    {
        if (string.IsNullOrEmpty(participantId) || NpcToParticipantMap == null)
        {
            return null;
        }

        foreach (KeyValuePair<ConvaiNPC, ConvaiRealtimeNPCData> kvp in NpcToParticipantMap)
        {
            if (kvp.Value.ParticipantId == participantId)
            {
                return kvp.Key;
            }
        }

        return null;
    }

    #endregion

    #region RTVI Event Handlers (called by RTVIHandler)

    /// <summary>
    ///     Called by RTVIHandler when bot-ready event is received from server.
    ///     This indicates the bot is ready and can be talked with.
    /// </summary>
    public void OnBotReadyReceived()
    {
        ConvaiUnityLogger.Info("🤖 Bot is ready and can be talked with", LogCategory.SDK);
        OnBotReady?.Invoke();
    }

    /// <summary>
    ///     Called by RTVIHandler when bot-started-speaking event is received.
    /// </summary>
    public void OnBotStartedSpeakingReceived()
    {
        OnBotStartedSpeaking?.Invoke();
    }

    /// <summary>
    ///     Called by RTVIHandler when bot-stopped-speaking event is received.
    /// </summary>
    public void OnBotStoppedSpeakingReceived()
    {
        OnBotStoppedSpeaking?.Invoke();
    }

    /// <summary>
    ///     Called by RTVIHandler when user-started-speaking event is received.
    /// </summary>
    public void OnUserStartedSpeakingReceived()
    {
        OnUserStartedSpeaking?.Invoke();
    }

    /// <summary>
    ///     Called by RTVIHandler when user-stopped-speaking event is received.
    /// </summary>
    public void OnUserStoppedSpeakingReceived()
    {
        OnUserStoppedSpeaking?.Invoke();
    }

    /// <summary>
    ///     Called by RTVIHandler when bot-llm-started event is received.
    /// </summary>
    public void OnBotLLMStartedReceived()
    {
        OnBotLLMStarted?.Invoke();
    }

    /// <summary>
    ///     Called by RTVIHandler when bot-llm-stopped event is received.
    /// </summary>
    public void OnBotLLMStoppedReceived()
    {
        OnBotLLMStopped?.Invoke();
    }

    /// <summary>
    ///     Called by RTVIHandler when bot-tts-started event is received.
    /// </summary>
    public void OnBotTTSStartedReceived()
    {
        OnBotTTSStarted?.Invoke();
    }

    /// <summary>
    ///     Called by RTVIHandler when bot-tts-stopped event is received.
    /// </summary>
    public void OnBotTTSStoppedReceived()
    {
        OnBotTTSStopped?.Invoke();
    }

    #endregion

    #region Enum Conversion Helpers

    /// <summary>
    ///     Converts ConvaiConnectionType enum to the string value expected by the API.
    /// </summary>
    /// <param name="connectionType">The connection type enum value.</param>
    /// <returns>The lowercase string representation ("audio" or "video").</returns>
    private static string ConnectionTypeToString(ConvaiConnectionType connectionType)
    {
        return connectionType switch
        {
            ConvaiConnectionType.Audio => "audio",
            ConvaiConnectionType.Video => "video",
            _ => "audio" // Default fallback
        };
    }

    /// <summary>
    ///     Converts ConvaiLLMProvider enum to the string value expected by the API.
    /// </summary>
    /// <param name="llmProvider">The LLM provider enum value.</param>
    /// <returns>The string representation expected by the backend API.</returns>
    private static string LLMProviderToString(ConvaiLLMProvider llmProvider)
    {
        return llmProvider switch
        {
            ConvaiLLMProvider.Dynamic => "dynamic",
            ConvaiLLMProvider.GeminiLive => "gemini-live",
            ConvaiLLMProvider.GeminiBaml => "gemini-baml",
            _ => "dynamic" // Default fallback
        };
    }

    #endregion
}

public class ConvaiRealtimeNPCData
{
    public string ParticipantId { get; set; } = string.Empty;
    public AudioStream AudioStream { get; set; }
    public AudioSource AudioSource { get; set; }
    public bool IsMuted { get; set; }
}
