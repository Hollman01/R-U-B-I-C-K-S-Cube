using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Convai.Scripts.Action.Actions
{
    public class ConvaiMoveToAction : ConvaiActionBase
    {
        public const string NPC = "npc";
        public const string TARGET = "target";

        private GameObject npc;
        private GameObject target;

        public override void Initialize(Dictionary<string, object> parameters, MonoBehaviour monoBehaviour, string actionName)
        {
            base.Initialize(parameters, monoBehaviour, actionName);
            npc = (GameObject)_parameters[NPC];
            target = (GameObject)_parameters[TARGET];
        }

        public override void PerformAction()
        {
            SetState(ActionState.InProgress);
            _monoBehaviour.StartCoroutine(MoveTo());
        }

        private IEnumerator MoveTo()
        {
            while (Vector2.Distance(npc.transform.position, target.transform.position) > 0.1)
            {
                yield return new WaitUntil(() => CurrentState == ActionState.InProgress);
                Vector2 dir = (target.transform.position - npc.transform.position).normalized;
                npc.transform.Translate(dir * 5 * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }

            npc.transform.position = target.transform.position;
            SetState(ActionState.Completed);
        }
    }
}
