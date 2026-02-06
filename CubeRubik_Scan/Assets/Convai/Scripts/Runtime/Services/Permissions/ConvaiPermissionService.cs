using System;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace Convai.Scripts.Services.Permissions
{
    public class ConvaiPermissionService
    {
        public bool HasMicrophonePermission()
        {
#if UNITY_ANDROID
            return Permission.HasUserAuthorizedPermission(Permission.Microphone);
#elif UNITY_IOS
        return Application.HasUserAuthorization(UserAuthorization.Microphone);
#else
            return true;
#endif
        }

        public void RequestMicrophonePermission(Action<bool> callback)
        {
#if UNITY_ANDROID
            PermissionCallbacks permissionCallbacks = new();
            permissionCallbacks.PermissionGranted += str => callback(true);
            permissionCallbacks.PermissionDenied += str => callback(false);
            // permissionCallbacks.PermissionDeniedAndDontAskAgain += (str) => callback(false);
            Permission.RequestUserPermission(Permission.Microphone, permissionCallbacks);
#elif UNITY_IOS
            MonoBehaviour monoBehaviour = new GameObject("ConvaiPermissionService").AddComponent<MonoBehaviour>();
            monoBehaviour.StartCoroutine(RequestPermission());
            IEnumerator RequestPermission() {
                yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
                callback(Application.HasUserAuthorization(UserAuthorization.Microphone));
                Object.Destroy(monoBehaviour.gameObject);
            }
#endif
        }
    }
}
