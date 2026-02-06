using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Convai.Scripts.Action.Actions
{
    public class ConvaiThrowAction : ConvaiActionBase
    {
        public const string NPC = "npc";
        public const string TIME = "time";

        private Transform _npc;
        private float _time;

        public override void Initialize(Dictionary<string, object> parameters, MonoBehaviour monoBehaviour, string actionName)
        {
            base.Initialize(parameters, monoBehaviour, actionName);
            _npc = (Transform)_parameters[NPC];
            _time = (float)_parameters[TIME];
        }

        public override void PerformAction()
        {
            SetState(ActionState.InProgress);
            _monoBehaviour.StartCoroutine(Throw());
        }

        private IEnumerator Throw()
        {
            Vector3 originalScale = _npc.localScale;
            while (_time > 0)
            {
                yield return new WaitUntil(() => CurrentState == ActionState.InProgress);
                _npc.localScale = Vector3.one + originalScale * Mathf.Sin(_time);
                _time -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            _npc.localScale = originalScale;
            SetState(ActionState.Completed);
        }

        public override void Cleanup()
        {
            base.Cleanup();
            _npc = null;
            _time = -1f;
        }
    }
}
