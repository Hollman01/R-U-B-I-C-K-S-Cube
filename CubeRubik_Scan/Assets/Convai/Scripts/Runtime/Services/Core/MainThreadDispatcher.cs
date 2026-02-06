using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Convai.Scripts.Services.Core
{
    [DefaultExecutionOrder(-10000)]
    [DisallowMultipleComponent]
    public sealed class MainThreadDispatcher : MonoBehaviour
    {
        private static int _mainThreadId;
        private static volatile bool _quitting;
        private readonly ConcurrentQueue<System.Action> _queue = new();
        private int? _maxPerFrame;

        public static MainThreadDispatcher Instance { get; private set; }

        public static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == _mainThreadId;

        public static int PendingCount => Instance?._queue?.Count ?? 0;

        public static int? MaxPerFrame
        {
            get => Instance?._maxPerFrame;
            set
            {
                if (Instance == null)
                {
                    Debug.LogWarning("[MainThreadDispatcher] Attempted to set MaxPerFrame before dispatcher is ready.");
                    return;
                }

                if (value <= 0)
                {
                    Instance._maxPerFrame = null;
                }
                else
                {
                    Instance._maxPerFrame = value;
                }
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            _quitting = false;
        }

        private void Update()
        {
            if (_queue.IsEmpty || _queue == null)
            {
                return;
            }

            int processed = 0;
            while (_queue.TryDequeue(out System.Action action))
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }

                processed++;
                if (_maxPerFrame.HasValue && processed >= _maxPerFrame.Value)
                {
                    break;
                }
            }
        }

        private void OnApplicationQuit()
        {
            _quitting = true;
            _queue.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Bootstrap()
        {
            if (Instance != null)
            {
                return;
            }

            GameObject existing = GameObject.Find(nameof(MainThreadDispatcher));
            if (existing != null)
            {
                Instance = existing.GetComponent<MainThreadDispatcher>();
                if (Instance == null)
                {
                    Instance = existing.AddComponent<MainThreadDispatcher>();
                }
            }
            else
            {
                GameObject go = new(nameof(MainThreadDispatcher));
                Instance = go.AddComponent<MainThreadDispatcher>();
            }

            DontDestroyOnLoad(Instance.gameObject);
            Instance.gameObject.hideFlags = HideFlags.HideInHierarchy;
        }

        public static bool Post(System.Action action)
        {
            if (action == null)
            {
                return false;
            }

            if (_quitting)
            {
                return false;
            }

            if (Instance == null)
            {
                Debug.LogWarning("[MainThreadDispatcher] Post called before Bootstrap; call Bootstrap earlier in startup sequence.");
                return false;
            }

            if (IsMainThread)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }

                return true;
            }

            Instance._queue.Enqueue(action);
            return true;
        }

        public static Task PostAsync(System.Action action)
        {
            if (action == null)
            {
                return Task.CompletedTask;
            }

            TaskCompletionSource<bool> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

            bool enqueued = Post(() =>
            {
                if (_quitting)
                {
                    tcs.TrySetCanceled();
                    return;
                }

                try
                {
                    action();
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            if (!enqueued)
            {
                tcs.TrySetCanceled();
            }

            return tcs.Task;
        }

        public static Task<T> PostAsync<T>(Func<T> func)
        {
            if (func == null)
            {
                return Task.FromResult(default(T));
            }

            TaskCompletionSource<T> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

            bool enqueued = Post(() =>
            {
                if (_quitting)
                {
                    tcs.TrySetCanceled();
                    return;
                }

                try
                {
                    T result = func();
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            if (!enqueued)
            {
                tcs.TrySetCanceled();
            }

            return tcs.Task;
        }

        public static void Clear() => Instance?._queue?.Clear();
    }
}
