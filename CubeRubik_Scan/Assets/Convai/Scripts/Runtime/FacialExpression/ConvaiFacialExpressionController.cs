// using System;
// using Convai.Scripts.Facial_Expression.Modals;
// using UnityEngine;

// namespace Convai.Scripts.Facial_Expression
// {
//     [Serializable]
//     public class ConvaiFacialExpressionController
//     {
//         [field: SerializeField] public FacialExpressionData FacialExpressionData { get; private set; }
//         [field: SerializeField] public float WeightBlendingPower { get; private set; }
//         // private ConvaiLipsyncApplicationBase _applicationBase;
//         public void Initialize(ConvaiNPC convaiNPC)
//         {
//             // _applicationBase = ConvaiLipsyncApplicationFactory.CreateLipsyncApplicationBase(convaiNPC, this);
//         }

//         public void LateUpdate()
//         {
//             _applicationBase.ApplyFrame();
//         }

//         public void Reset()
//         {
//             _applicationBase.ResetFrame();
//         }
//     }
// }


