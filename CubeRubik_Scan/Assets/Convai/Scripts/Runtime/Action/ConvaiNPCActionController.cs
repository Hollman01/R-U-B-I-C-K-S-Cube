// using System.Collections.Generic;
// using System.Threading;
// using Convai.Scripts.Action.Actions;
// using Convai.Scripts.Action.Models;
// using Convai.SDK.CSharp.Actions.Parsers;
// using Convai.SDK.CSharp.Logger;
// using Cysharp.Threading.Tasks;
// using Service;
// using UnityEngine;
// using UnityEngine.Events;
// using static Convai.Scripts.Action.Actions.ConvaiActionBase;

// namespace Convai.Scripts.Action {

//     [System.Serializable]
//     public class ConvaiNPCActionController
//     {
//         [field: SerializeField] public List<string> Actions { get; set; } = new();
//         [field: SerializeField] public List<ConvaiInteractableItem> ConvaiInteractableItems { get; set; } = new();
//         [SerializeField] UnityEvent<string, ActionState> onCurrentActionStateChange;
//         public ConvaiActionBase CurrentAction { get; private set; }
//         private Queue<ParsedAction> _actionsToDo;
//         private ConvaiNPC _convaiNPC;
//         private IConvaiActionCreator _convaiActionCreator;
//         private CancellationTokenSource _performTaskSource;

//         public void Initialize(ConvaiNPC convaiNPC)
//         {
//             _convaiNPC = convaiNPC;
//             _convaiActionCreator = _convaiNPC.GetComponent<IConvaiActionCreator>();
//             if (_convaiActionCreator == null)
//             {
//                 ConvaiLogger.Log($"Initialize of ConvaiNPCActionController for NPC ID: {_convaiNPC.CharacterID} failed as no IConvaiActionCreator implemented component is attached to this Game Object");
//                 return;
//             }
//             _convaiActionCreator.SetConvaiInteractableItemList(ConvaiInteractableItems);
//             _actionsToDo = new Queue<ParsedAction>();
//             _convaiNPC.ConvaiEventManager.CharacterResponseActionEvent += ConvaiEventManager_CharacterResponseActionEvent;
//             _performTaskSource = new CancellationTokenSource();
//             PerformActions(_performTaskSource.Token);
//         }

//         public void Reset()
//         {
//             _actionsToDo = new Queue<ParsedAction>();
//             _convaiNPC = null;
//             _convaiActionCreator = null;
//             CurrentAction = null;
//             _performTaskSource?.Cancel();
//             _performTaskSource = null;
//         }

//         private void ConvaiEventManager_CharacterResponseActionEvent(List<ParsedAction> actionList) {
//             foreach ( ParsedAction t in actionList ) {
//                 ConvaiLogger.Log($"Adding Action To NPC Character [{_convaiNPC.CharacterID}] : {t}");
//                 _actionsToDo.Enqueue(t);
//             }
//         }

//         public ActionConfig CreateActionConfig()
//         {
//             ActionConfig actionConfig = new();
//             foreach (string actionName in Actions)
//             {
//                 actionConfig.Actions.Add(actionName);
//             }
//             foreach (ConvaiInteractableItem item in ConvaiInteractableItems)
//             {
//                 switch (item.itemType)
//                 {
//                     case ItemType.Object:
//                         Debug.Log($"Adding Object: {item.itemName} for NPC: {_convaiNPC.CharacterID}");
//                         actionConfig.Objects.Add(new ActionConfig.Types.Object
//                         {
//                             Name = item.itemName,
//                             Description = item.description
//                         });
//                         break;
//                     case ItemType.Character:
//                         Debug.Log($"Adding Character: {item.itemName} for NPC: {_convaiNPC.CharacterID}");
//                         actionConfig.Characters.Add(new ActionConfig.Types.Character
//                         {
//                             Name = item.itemName,
//                             Bio = item.description,
//                         });
//                         break;
//                 }
//             }
//             return actionConfig;
//         }


//         private async void PerformActions(CancellationToken cancellationToken)
//         {
//             while (true)
//             {
//                 while (_actionsToDo.Count == 0)
//                     await UniTask.WaitForEndOfFrame(_convaiNPC, cancellationToken);
//                 ParsedAction parsedAction = _actionsToDo.Dequeue();
//                 if (!GetActionName(parsedAction, out string actionName) || parsedAction.ActionType == ParsedAction.Type.None)
//                 {
//                     Debug.Log($"No Action found with name: {actionName} for NPC with ID: {_convaiNPC.CharacterID} or parsed action is of type none");
//                     continue;
//                 }
//                 CurrentAction = _convaiActionCreator.CreateAction(parsedAction, _convaiNPC.gameObject);
//                 CurrentAction.OnActionStateChanged += CurrentAction_OnActionStateChanged;
//                 CurrentAction.PerformAction();
//                 await UniTask.WaitUntil(CurrentActionIsCompleted, PlayerLoopTiming.Update, cancellationToken);
//             }
//         }

//         private bool GetActionName(ParsedAction parsedAction, out string actionName)
//         {
//             actionName = Actions.Find(x => x == parsedAction.Action);
//             return !string.IsNullOrEmpty(actionName);
//         }

//         private void CurrentAction_OnActionStateChanged(ActionState state)
//         {
//             Debug.Assert(CurrentAction != null, "Current Action should not be null during this check");
//             onCurrentActionStateChange?.Invoke(CurrentAction.ActionName, state);
//         }

//         public bool CurrentActionIsCompleted()
//         {
//             Debug.Assert(CurrentAction != null, "Current Action should not be null during this check");
//             return CurrentAction.CurrentState is ActionState.Completed or ActionState.Failed;
//         }
//     }

// }


