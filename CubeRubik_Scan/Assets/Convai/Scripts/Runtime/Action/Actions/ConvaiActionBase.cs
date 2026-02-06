using System;
using System.Collections.Generic;
using UnityEngine;

namespace Convai.Scripts.Action.Actions
{
    public abstract class ConvaiActionBase
    {
        #region ActionState enum

        public enum ActionState
        {
            NotStarted,
            InProgress,
            Paused,
            Completed,
            Failed,
            Interrupted
        }

        #endregion

        protected MonoBehaviour _monoBehaviour;

        protected Dictionary<string, object> _parameters;
        public string ActionName { get; private set; }

        public ActionState CurrentState { get; protected set; } = ActionState.NotStarted;

        public event Action<ActionState> OnActionStateChanged;


        public virtual void Initialize(Dictionary<string, object> parameters, MonoBehaviour monoBehaviour, string actionName)
        {
            ActionName = actionName;
            _monoBehaviour = monoBehaviour;
            _parameters = parameters;
            CurrentState = ActionState.NotStarted;
        }


        public abstract void PerformAction();

        public virtual void InterruptAction()
        {
            CurrentState = ActionState.Interrupted;
            OnActionStateChanged?.Invoke(CurrentState);
        }


        public virtual void Pause() => SetState(ActionState.Paused);

        public virtual void Resume() => SetState(ActionState.InProgress);

        protected void SetState(ActionState newState)
        {
            CurrentState = newState;
            OnActionStateChanged?.Invoke(CurrentState);

            if (CurrentState == ActionState.Completed || CurrentState == ActionState.Failed)
            {
                Cleanup();
            }
        }

        public virtual void Cleanup() => OnActionStateChanged = null;
    }
}
