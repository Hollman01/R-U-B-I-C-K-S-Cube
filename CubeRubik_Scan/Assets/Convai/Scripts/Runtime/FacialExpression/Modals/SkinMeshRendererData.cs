using System;
using UnityEngine;

namespace Convai.Scripts.Facial_Expression.Modals
{
    [Serializable]
    public class SkinMeshRendererData
    {
        public SkinnedMeshRenderer renderer;
        public VisemeEffectorsList visemeEffectorsList;

        [Tooltip("Lower and Upper bound of the Blend Shape weight, Ex: 0-1, or 0-100")]
        public Vector2 weightBounds = new(0, 1);
    }
}
