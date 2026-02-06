using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Convai.Scripts.Action.Actions
{
    public class ConvaiDanceAction : ConvaiActionBase
    {
        public const string NPC = "npc";
        public const string TIME = "time";
        public const string RADIUS = "radius";
        public const string COLOR = "color";
        private Color _color;
        private float _maxTime;

        private Transform _npc;
        private float _radius;
        private float _timeToDance;

        public override void Initialize(Dictionary<string, object> parameters, MonoBehaviour monoBehaviour, string actionName)
        {
            base.Initialize(parameters, monoBehaviour, actionName);
            _npc = (Transform)parameters[NPC];
            _timeToDance = (float)parameters[TIME];
            _radius = (float)parameters[RADIUS];
            _color = (Color)parameters[COLOR];
            _maxTime = _timeToDance;
        }

        public override void PerformAction()
        {
            SetState(ActionState.InProgress);
            _monoBehaviour.StartCoroutine(Dance());
        }

        private IEnumerator Dance()
        {
            SpriteRenderer spriteRenderer = _npc.GetComponent<SpriteRenderer>();
            Color color = spriteRenderer.color;
            spriteRenderer.color = _color;
            Vector2 originalPosition = _npc.position;
            while (_timeToDance > 0)
            {
                yield return new WaitUntil(() => CurrentState == ActionState.InProgress);
                _npc.position = originalPosition + Random.insideUnitCircle * (_radius * (_timeToDance / _maxTime));
                _timeToDance -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            _npc.position = originalPosition;
            spriteRenderer.color = color;
            SetState(ActionState.Completed);
        }

        public override void Cleanup() => base.Cleanup();
    }
}
