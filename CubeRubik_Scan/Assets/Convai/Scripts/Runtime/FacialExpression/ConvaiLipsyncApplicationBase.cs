// using Convai.Scripts.Facial_Expression.Modals;
// using Convai.SDK.CSharp;
// using Convai.SDK.CSharp.Facial_Expression.Lipsync.Processors;
// using Service;
// using UnityEngine;

// namespace Convai.Scripts.Facial_Expression
// {
//     public abstract class ConvaiLipsyncApplicationBase
//     {
//         private ILipsyncDataProcessor Processor { get; set; }
//         protected FacialExpressionData ExpressionData { get; private set; }
//         protected float WeightBlendingPower;
//         private ConvaiNPC _convaiNPC;

//         protected ConvaiLipsyncApplicationBase(ConvaiNPC convaiNPC, ConvaiFacialExpressionController lipsyncController)
//         {
//             InitializeField(convaiNPC, lipsyncController);
//             PerformNullCheck();
//             SubscribeToEvents();
//         }

//         private void SubscribeToEvents()
//         {
//             Processor.UpdateVisemeFrameAction += SetVisemeFrame;
//             Processor.UpdateARKitBlendShapesFrameAction += SetARKitBlendShapeFrame;
//             _convaiNPC.ConvaiEventManager.CharacterResponseEvent += ConvaiEventManager_CharacterResponseEvent;
//         }

//         private void PerformNullCheck()
//         {
//             HasHeadSkinnedMeshRenderer = ExpressionData.Head.renderer != null;
//             HasTeethSkinnedMeshRenderer = ExpressionData.Teeth.renderer != null;
//             HasTongueSkinnedMeshRenderer = ExpressionData.Tongue.renderer != null;
//             HasJawBone = ExpressionData.JawBone != null;
//             HasTongueBone = ExpressionData.TongueBone != null;
//         }

//         private void InitializeField(ConvaiNPC convaiNPC, ConvaiFacialExpressionController lipsyncController)
//         {
//             _convaiNPC = convaiNPC;
//             Processor = _convaiNPC.Brain.LipsyncDataProcessor;
//             ExpressionData = lipsyncController.FacialExpressionData;
//             WeightBlendingPower = lipsyncController.WeightBlendingPower;
//         }

//         ~ConvaiLipsyncApplicationBase()
//         {
//             _convaiNPC.ConvaiEventManager.CharacterResponseEvent += ConvaiEventManager_CharacterResponseEvent;
//             Processor.UpdateVisemeFrameAction -= SetVisemeFrame;
//             Processor.UpdateARKitBlendShapesFrameAction -= SetARKitBlendShapeFrame;
//         }

//         private void ConvaiEventManager_CharacterResponseEvent(CharacterResponseEventArgs args)
//         {
//             if (args.EventState == CharacterResponseEventArgs.State.Start)
//                 ResetFrame();
//         }

//         protected virtual void SetVisemeFrame(Viseme newViseme) { }
//         protected virtual void SetARKitBlendShapeFrame(ARKitBlendShapes frame) { }
//         public abstract void ApplyFrame();
//         public abstract void ResetFrame();
//         protected void UpdateTongueBoneRotation(Vector3 newRotation)
//         {
//             if (!HasTongueBone) return;
//             ExpressionData.TongueBone.transform.localEulerAngles = newRotation;
//         }
//         protected void UpdateJawBoneRotation(Vector3 newRotation)
//         {
//             if (!HasJawBone) return;
//             ExpressionData.JawBone.transform.localEulerAngles = newRotation;
//         }

//         #region Null States of References

//         protected bool HasHeadSkinnedMeshRenderer { get; private set; }
//         protected bool HasTeethSkinnedMeshRenderer { get; private set; }
//         protected bool HasTongueSkinnedMeshRenderer { get; private set; }
//         private bool HasJawBone { get; set; }
//         private bool HasTongueBone { get; set; }

//         #endregion
//     }
// }


