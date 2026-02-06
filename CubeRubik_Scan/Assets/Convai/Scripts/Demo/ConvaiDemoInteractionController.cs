using Convai.Scripts.Scriptable_Objects;
using Convai.Scripts.Services;
using UnityEngine;

namespace Convai.Scripts
{
    public class ConvaiDemoInteractionController : MonoBehaviour
    {
        [SerializeField] private ConvaiKeyBindings keyBindings;

        private void Awake()
        {
            if (keyBindings == null)
            {
                ConvaiKeyBindings.GetBinding(out keyBindings);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(keyBindings.OpenSettingsKey))
            {
                ConvaiServices.UISystem.ShowSettings();
            }
        }
    }
}
