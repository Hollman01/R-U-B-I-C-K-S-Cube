using System;
using UnityEngine;

namespace Convai.Scripts.Action.Models
{
    [Serializable]
    public class ConvaiInteractableItem
    {
        public ItemType itemType = ItemType.Character;
        public GameObject gameObject;
        public string itemName = string.Empty;
        public string description = string.Empty;
    }

    public enum ItemType
    {
        Object, Character
    }
}
