// using System.Collections.Generic;
// using Convai.Scripts.Action.Actions;
// using Convai.Scripts.Action.Models;
// // using Convai.SDK.CSharp.Actions.Parsers;
// using UnityEngine;

// namespace Convai.Scripts.Action
// {
//     public class ConvaiActionCreatorDemo : MonoBehaviour, IConvaiActionCreator
//     {
//         private List<ConvaiInteractableItem> _interactableItems;

//         public ConvaiActionBase CreateAction(ParsedAction parsedAction, GameObject npcGameobject)
//         {
//             MonoBehaviour monoBehaviour = npcGameobject.GetComponent<MonoBehaviour>();
//             string actionName = parsedAction.Action;
//             switch (actionName)
//             {
//                 case "MoveTo":

//                     if (!GetInteractiveItem(parsedAction, out ConvaiInteractableItem convaiInteractableItem))
//                     {
//                         Debug.Log($"No ConvaiInteractableItem found for Action Name: {parsedAction.Action}");
//                         return new ConvaiEmptyAction();
//                     }
//                     ConvaiActionBase moveTo = new ConvaiMoveToAction();
//                     moveTo.Initialize(new Dictionary<string, object>
//                     {
//                         {ConvaiMoveToAction.NPC, npcGameobject },
//                         {ConvaiMoveToAction.TARGET, convaiInteractableItem.gameObject }
//                     }, monoBehaviour, actionName);
//                     return moveTo;
//                 case "Throw":
//                     ConvaiActionBase throwAction = new ConvaiThrowAction();
//                     throwAction.Initialize(new Dictionary<string, object>
//                     {
//                         {ConvaiThrowAction.NPC, npcGameobject.transform },
//                         {ConvaiThrowAction.TIME, 10f }
//                     }, monoBehaviour, actionName);
//                     return throwAction;
//                 case "Dance":
//                     ConvaiActionBase dance = new ConvaiDanceAction();
//                     dance.Initialize(new Dictionary<string, object>
//                     {
//                         {ConvaiDanceAction.NPC, npcGameobject.transform },
//                         {ConvaiDanceAction.TIME, 5f },
//                         {ConvaiDanceAction.RADIUS, 2f },
//                         {ConvaiDanceAction.COLOR, Color.green }
//                     }, monoBehaviour, actionName);
//                     return dance;
//                 case "WaitForSeconds":
//                     ConvaiActionBase wait = new ConvaiWaitForSecondsAction();
//                     wait.Initialize(new Dictionary<string, object>
//                     {
//                         {ConvaiWaitForSecondsAction.TIME, parsedAction.Number }
//                     }, monoBehaviour, actionName);
//                     return wait;
//                 default:
//                     return new ConvaiEmptyAction();
//             }
//         }

//         private bool GetInteractiveItem(ParsedAction parsedAction, out ConvaiInteractableItem convaiInteractableItem)
//         {
//             convaiInteractableItem = _interactableItems.Find(x => x.itemName == parsedAction.ObjectName!);
//             return convaiInteractableItem != null;
//         }

//         public void SetConvaiInteractableItemList(List<ConvaiInteractableItem> list)
//         {
//             _interactableItems = list;
//         }
//     }
// }


