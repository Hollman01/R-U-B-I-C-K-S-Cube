using System.Collections.Generic;
using Convai.Scripts.RTVI.Outbound;
using UnityEngine;

namespace Convai.Scripts.SceneMetadata
{
    /// <summary>
    ///     Collects metadata from all registered ConvaiObjectMetadata components and sends it via RTVI.
    ///     Uses the efficient ConvaiMetadataRegistry instead of expensive FindObjectsOfType calls.
    /// </summary>
    public class ConvaiSceneMetadataCollector : MonoBehaviour
    {
        [Header("Room Manager Configuration")] [Tooltip("Reference to the ConvaiRoomManager that manages the RTVI connection")] [SerializeField]
        private ConvaiRoomManager _roomManager;

        [Header("Collection Settings")] [Tooltip("Whether to automatically collect metadata on Start")] [SerializeField]
        private bool _collectOnStart;

        [Tooltip("Whether to log collection stats")] [SerializeField]
        private bool _logStatistics = true;

        [Header("Debug Info")] [SerializeField]
        private int _lastCollectedCount;

        [SerializeField] private float _lastCollectionTime;

        private void Start()
        {
            // Auto-find ConvaiRoomManager if not assigned
            if (_roomManager == null)
            {
                _roomManager = FindFirstObjectByType<ConvaiRoomManager>();
                if (_roomManager == null)
                {
                    Debug.LogWarning(
                        "[ConvaiSceneMetadataCollector] No ConvaiRoomManager found in scene. Metadata collection will not work until one is assigned.",
                        this);
                    return;
                }
            }

            // Subscribe to room connection successful event to send metadata when room is ready to send metadata
            _roomManager.OnRoomConnectionSuccessful.AddListener(OnRoomConnected);
        }

        private void OnDestroy()
        {
            // Unsubscribe from event to prevent memory leaks
            if (_roomManager != null)
            {
                _roomManager.OnRoomConnectionSuccessful.RemoveListener(OnRoomConnected);
            }
        }

        /// <summary>
        ///     Called when the room connection is successful - sends metadata if configured to collect on start
        /// </summary>
        private void OnRoomConnected()
        {
            if (_collectOnStart)
            {
                CollectAndSendSceneMetadata();
            }
        }

        /// <summary>
        ///     Collects metadata from all registered ConvaiObjectMetadata components and sends via RTVI
        ///     Can be called manually after room connection or will be called automatically if _collectOnStart is enabled
        /// </summary>
        public void CollectAndSendSceneMetadata()
        {
            // Check if room manager is available
            if (_roomManager == null)
            {
                Debug.LogError("[ConvaiSceneMetadataCollector] ConvaiRoomManager is not assigned! Cannot send metadata.", this);
                return;
            }

            // Check if RTVI handler is available (room must be connected)
            if (_roomManager.RTVIHandler == null)
            {
                Debug.LogWarning(
                    "[ConvaiSceneMetadataCollector] RTVIHandler is not available. Make sure the room is connected before sending metadata.", this);
                return;
            }

            float startTime = Time.realtimeSinceStartup;

            // Get metadata from registry (much faster than FindObjectsOfType)
            List<RTVI.Outbound.SceneMetadata> sceneMetadataList = ConvaiMetadataRegistry.GetSceneMetadataList();

            _lastCollectedCount = sceneMetadataList.Count;
            _lastCollectionTime = Time.realtimeSinceStartup - startTime;

            if (_logStatistics)
            {
                Dictionary<string, object> stats = ConvaiMetadataRegistry.GetStatistics();
                Debug.Log($"[ConvaiSceneMetadataCollector] Collected {_lastCollectedCount} metadata objects in {_lastCollectionTime:F4}s. " +
                          $"Registry stats: {stats["TotalRegistered"]} total, {stats["ValidMetadata"]} valid, {stats["InvalidMetadata"]} invalid",
                    this);
            }

            // Send to core service via room manager's RTVI handler
            RTVIUpdateSceneMetadata message = new(sceneMetadataList);
            _roomManager.RTVIHandler.SendData(message);

            Debug.Log($"[ConvaiSceneMetadataCollector] Sent {_lastCollectedCount} metadata objects to RTVI service", this);
        }

        /// <summary>
        ///     Gets the current metadata count without sending
        /// </summary>
        /// <returns>Number of valid metadata objects currently registered</returns>
        public int GetMetadataCount() => ConvaiMetadataRegistry.GetValidMetadata().Length;

        /// <summary>
        ///     Gets all current metadata without sending
        /// </summary>
        /// <returns>List of current scene metadata</returns>
        public List<RTVI.Outbound.SceneMetadata> GetCurrentMetadata() => ConvaiMetadataRegistry.GetSceneMetadataList();

        /// <summary>
        ///     Validates all registered metadata and logs any issues
        /// </summary>
        public void ValidateAllMetadata()
        {
            ConvaiObjectMetadata[] allMetadata = ConvaiMetadataRegistry.GetAllMetadata();
            List<string> validationIssues = new();

            foreach (ConvaiObjectMetadata metadata in allMetadata)
            {
                if (metadata == null)
                {
                    validationIssues.Add("Found null metadata reference");
                    continue;
                }

                List<string> errors = metadata.GetValidationErrors();
                if (errors.Count > 0)
                {
                    validationIssues.Add($"{metadata.gameObject.name}: {string.Join(", ", errors)}");
                }
            }

            if (validationIssues.Count > 0)
            {
                Debug.LogWarning($"[ConvaiSceneMetadataCollector] Found {validationIssues.Count} validation issues:\n" +
                                 string.Join("\n", validationIssues), this);
            }
            else
            {
                Debug.Log($"[ConvaiSceneMetadataCollector] All {allMetadata.Length} metadata objects are valid", this);
            }
        }

        /// <summary>
        ///     Checks if the metadata collection system is properly set up and ready to send data
        /// </summary>
        /// <returns>True if ready to send metadata, false otherwise</returns>
        public bool IsReadyToSendMetadata()
        {
            if (_roomManager == null)
            {
                Debug.LogWarning("[ConvaiSceneMetadataCollector] ConvaiRoomManager is not assigned", this);
                return false;
            }

            if (_roomManager.RTVIHandler == null)
            {
                Debug.LogWarning("[ConvaiSceneMetadataCollector] RTVIHandler is not available (room not connected)", this);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Sets the room manager reference (useful for runtime assignment)
        /// </summary>
        /// <param name="roomManager">The ConvaiRoomManager to use</param>
        public void SetRoomManager(ConvaiRoomManager roomManager)
        {
            _roomManager = roomManager;
            Debug.Log("[ConvaiSceneMetadataCollector] Room manager assigned", this);
        }
    }
}
