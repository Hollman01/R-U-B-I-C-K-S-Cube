using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Convai.Scripts.Action.Actions
{
    public class ConvaiWaitForSecondsAction : ConvaiActionBase
    {
        public const string TIME = "time";
        private float _maxTime;

        public override void Initialize(Dictionary<string, object> parameters, MonoBehaviour monoBehaviour, string actionName)
        {
            base.Initialize(parameters, monoBehaviour, actionName);
            _maxTime = (float)parameters["time"];
        }

        public override void PerformAction()
        {
            SetState(ActionState.InProgress);
            _monoBehaviour.StartCoroutine(DelayCoroutine());
        }

        private IEnumerator DelayCoroutine()
        {
            while (_maxTime > 0)
            {
                _maxTime -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            SetState(ActionState.Completed);
        }
    }
}
