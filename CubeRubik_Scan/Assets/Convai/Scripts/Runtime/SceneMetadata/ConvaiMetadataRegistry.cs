using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Convai.Scripts.SceneMetadata
{
    /// <summary>
    ///     Static registry that efficiently tracks all ConvaiObjectMetadata components in the scene.
    ///     Provides high-performance metadata collection without expensive FindObjectsOfType calls.
    /// </summary>
    public static class ConvaiMetadataRegistry
    {
        private static readonly HashSet<ConvaiObjectMetadata> _registeredMetadata = new();
        private static readonly object _lock = new();

        /// <summary>
        ///     Event fired when metadata is registered
        /// </summary>
        public static Action<ConvaiObjectMetadata> OnMetadataRegistered;

        /// <summary>
        ///     Event fired when metadata is unregistered
        /// </summary>
        public static Action<ConvaiObjectMetadata> OnMetadataUnregistered;

        /// <summary>
        ///     Total number of registered metadata components
        /// </summary>
        public static int Count
        {
            get
            {
                lock (_lock)
                {
                    return _registeredMetadata.Count;
                }
            }
        }

        /// <summary>
        ///     Registers a metadata component with the registry
        /// </summary>
        /// <param name="metadata">The metadata component to register</param>
        public static void RegisterMetadata(ConvaiObjectMetadata metadata)
        {
            if (metadata == null)
            {
                Debug.LogWarning("[ConvaiMetadataRegistry] Attempted to register null metadata");
                return;
            }

            lock (_lock)
            {
                if (_registeredMetadata.Add(metadata))
                {
                    Debug.Log($"[ConvaiMetadataRegistry] Registered metadata for '{metadata.ObjectName}' on {metadata.gameObject.name}", metadata);
                    OnMetadataRegistered?.Invoke(metadata);
                }
            }
        }

        /// <summary>
        ///     Unregisters a metadata component from the registry
        /// </summary>
        /// <param name="metadata">The metadata component to unregister</param>
        public static void UnregisterMetadata(ConvaiObjectMetadata metadata)
        {
            if (metadata == null)
            {
                return;
            }

            lock (_lock)
            {
                if (_registeredMetadata.Remove(metadata))
                {
                    Debug.Log($"[ConvaiMetadataRegistry] Unregistered metadata for '{metadata.ObjectName}' on {metadata.gameObject.name}", metadata);
                    OnMetadataUnregistered?.Invoke(metadata);
                }
            }
        }

        /// <summary>
        ///     Gets all registered metadata components
        /// </summary>
        /// <returns>Array of all registered metadata components</returns>
        public static ConvaiObjectMetadata[] GetAllMetadata()
        {
            lock (_lock)
            {
                return _registeredMetadata.ToArray();
            }
        }

        /// <summary>
        ///     Gets all valid metadata components (those that should be included and have valid data)
        /// </summary>
        /// <returns>Array of valid metadata components</returns>
        public static ConvaiObjectMetadata[] GetValidMetadata()
        {
            lock (_lock)
            {
                return _registeredMetadata
                    .Where(m => m != null && m.IncludeInMetadata && m.IsValid)
                    .ToArray();
            }
        }

        /// <summary>
        ///     Gets all metadata as SceneMetadata objects for RTVI messaging
        /// </summary>
        /// <returns>List of SceneMetadata objects</returns>
        public static List<RTVI.Outbound.SceneMetadata> GetSceneMetadataList()
        {
            ConvaiObjectMetadata[] validMetadata = GetValidMetadata();
            List<RTVI.Outbound.SceneMetadata> sceneMetadataList = new(validMetadata.Length);

            foreach (ConvaiObjectMetadata metadata in validMetadata)
            {
                try
                {
                    sceneMetadataList.Add(metadata.ToSceneMetadata());
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ConvaiMetadataRegistry] Failed to convert metadata for '{metadata.ObjectName}': {ex.Message}", metadata);
                }
            }

            return sceneMetadataList;
        }

        /// <summary>
        ///     Clears all registered metadata (useful for testing or scene cleanup)
        /// </summary>
        public static void Clear()
        {
            lock (_lock)
            {
                int count = _registeredMetadata.Count;
                _registeredMetadata.Clear();
                Debug.Log($"[ConvaiMetadataRegistry] Cleared {count} registered metadata components");
            }
        }

        /// <summary>
        ///     Gets metadata statistics for debugging
        /// </summary>
        /// <returns>Dictionary with registry statistics</returns>
        public static Dictionary<string, object> GetStatistics()
        {
            lock (_lock)
            {
                ConvaiObjectMetadata[] validMetadata = _registeredMetadata.Where(m => m != null && m.IncludeInMetadata && m.IsValid).ToArray();
                ConvaiObjectMetadata[] invalidMetadata = _registeredMetadata.Where(m => m != null && (!m.IncludeInMetadata || !m.IsValid)).ToArray();
                ConvaiObjectMetadata[] nullMetadata = _registeredMetadata.Where(m => m == null).ToArray();

                return new Dictionary<string, object>
                {
                    ["TotalRegistered"] = _registeredMetadata.Count,
                    ["ValidMetadata"] = validMetadata.Length,
                    ["InvalidMetadata"] = invalidMetadata.Length,
                    ["NullReferences"] = nullMetadata.Length,
                    ["ValidNames"] = validMetadata.Select(m => m.ObjectName).ToArray(),
                    ["InvalidReasons"] = invalidMetadata.Select(m =>
                        !m.IncludeInMetadata ? "Excluded" : "Invalid data").ToArray()
                };
            }
        }

        /// <summary>
        ///     Validates the registry and removes any null references
        /// </summary>
        /// <returns>Number of null references removed</returns>
        public static int CleanupNullReferences()
        {
            lock (_lock)
            {
                ConvaiObjectMetadata[] nullRefs = _registeredMetadata.Where(m => m == null).ToArray();
                foreach (ConvaiObjectMetadata nullRef in nullRefs)
                {
                    _registeredMetadata.Remove(nullRef);
                }

                if (nullRefs.Length > 0)
                {
                    Debug.Log($"[ConvaiMetadataRegistry] Cleaned up {nullRefs.Length} null references");
                }

                return nullRefs.Length;
            }
        }
    }
}
