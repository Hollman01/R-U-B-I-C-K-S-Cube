using UnityEngine;

namespace Convai.Scripts.Scriptable_Objects
{
    [CreateAssetMenu(fileName = "ConvaiKeyBindings", menuName = "Convai/Key Bindings")]
    public class ConvaiKeyBindings : ScriptableObject
    {
        [SerializeField] private KeyCode talkKey = KeyCode.T;
        [SerializeField] private KeyCode openSettingsKey = KeyCode.F10;

        public KeyCode TalkKey => talkKey;
        public KeyCode OpenSettingsKey => openSettingsKey;


        public static bool GetBinding(out ConvaiKeyBindings binding)
        {
            binding = Resources.Load<ConvaiKeyBindings>(nameof(ConvaiKeyBindings));
            return binding != null;
        }
    }
}
