// using System.Collections.Generic;
// using Convai.Scripts.Facial_Expression.Modals;
// using Service;
// using UnityEngine;

// namespace Convai.Scripts.Facial_Expression.Types
// {
//     public class ConvaiVisemeLipsync : ConvaiLipsyncApplicationBase
//     {
//         private Viseme _currentViseme;
//         public ConvaiVisemeLipsync(ConvaiNPC convaiNPC, ConvaiFacialExpressionController lipsyncController) : base(convaiNPC, lipsyncController) { _currentViseme = default; }

//         public override void ApplyFrame()
//         {
//             if(_currentViseme == null) return;
//             UpdateJawBoneRotation(new Vector3(0.0f, 0.0f, -90.0f));
//             UpdateTongueBoneRotation(new Vector3(0.0f, 0.0f, -5.0f));

//             if (HasHeadSkinnedMeshRenderer)
//                 UpdateMeshRenderer(ExpressionData.Head);
//             if (HasTeethSkinnedMeshRenderer)
//                 UpdateMeshRenderer(ExpressionData.Teeth);
//             if (HasTongueSkinnedMeshRenderer)
//                 UpdateMeshRenderer(ExpressionData.Tongue);

//             UpdateJawBoneRotation(new Vector3(0.0f, 0.0f, -90.0f - CalculateBoneEffect(ExpressionData.JawBoneEffector) * 30f));
//             UpdateTongueBoneRotation(new Vector3(0.0f, 0.0f, CalculateBoneEffect(ExpressionData.TongueBoneEffector) * 80f - 5f));
//         }

//         public override void ResetFrame()
//         {
//             _currentViseme = new Viseme();
//         }

//         protected override void SetVisemeFrame(Viseme newViseme)
//         {
//             _currentViseme = newViseme;
//         }

//         private float CalculateBoneEffect(VisemeBoneEffectorList boneEffectorList)
//         {
//             if (boneEffectorList is null) return 0;
//             return (
//                        boneEffectorList.sil * _currentViseme.Sil +
//                        boneEffectorList.pp * _currentViseme.Pp +
//                        boneEffectorList.ff * _currentViseme.Ff +
//                        boneEffectorList.th * _currentViseme.Th +
//                        boneEffectorList.dd * _currentViseme.Dd +
//                        boneEffectorList.kk * _currentViseme.Kk +
//                        boneEffectorList.ch * _currentViseme.Ch +
//                        boneEffectorList.ss * _currentViseme.Ss +
//                        boneEffectorList.nn * _currentViseme.Nn +
//                        boneEffectorList.rr * _currentViseme.Rr +
//                        boneEffectorList.aa * _currentViseme.Aa +
//                        boneEffectorList.e * _currentViseme.E +
//                        boneEffectorList.ih * _currentViseme.Ih +
//                        boneEffectorList.oh * _currentViseme.Oh +
//                        boneEffectorList.ou * _currentViseme.Ou
//                    )
//                    / boneEffectorList.Total;
//         }

//         private void UpdateMeshRenderer(SkinMeshRendererData data)
//         {
//             VisemeEffectorsList effectorsList = data.visemeEffectorsList;
//             SkinnedMeshRenderer skinnedMesh = data.renderer;
//             Vector2 bounds = data.weightBounds;
//             if (effectorsList == null) return;
//             Dictionary<int, float> finalModifiedValuesDictionary = new();
//             CalculateBlendShapeEffect(effectorsList.pp, ref finalModifiedValuesDictionary, _currentViseme.Pp);
//             CalculateBlendShapeEffect(effectorsList.ff, ref finalModifiedValuesDictionary, _currentViseme.Ff);
//             CalculateBlendShapeEffect(effectorsList.th, ref finalModifiedValuesDictionary, _currentViseme.Th);
//             CalculateBlendShapeEffect(effectorsList.dd, ref finalModifiedValuesDictionary, _currentViseme.Dd);
//             CalculateBlendShapeEffect(effectorsList.kk, ref finalModifiedValuesDictionary, _currentViseme.Kk);
//             CalculateBlendShapeEffect(effectorsList.ch, ref finalModifiedValuesDictionary, _currentViseme.Ch);
//             CalculateBlendShapeEffect(effectorsList.ss, ref finalModifiedValuesDictionary, _currentViseme.Ss);
//             CalculateBlendShapeEffect(effectorsList.nn, ref finalModifiedValuesDictionary, _currentViseme.Nn);
//             CalculateBlendShapeEffect(effectorsList.rr, ref finalModifiedValuesDictionary, _currentViseme.Rr);
//             CalculateBlendShapeEffect(effectorsList.aa, ref finalModifiedValuesDictionary, _currentViseme.Aa);
//             CalculateBlendShapeEffect(effectorsList.e, ref finalModifiedValuesDictionary, _currentViseme.E);
//             CalculateBlendShapeEffect(effectorsList.ih, ref finalModifiedValuesDictionary, _currentViseme.Ih);
//             CalculateBlendShapeEffect(effectorsList.oh, ref finalModifiedValuesDictionary, _currentViseme.Oh);
//             CalculateBlendShapeEffect(effectorsList.ou, ref finalModifiedValuesDictionary, _currentViseme.Ou);
//             foreach (KeyValuePair<int, float> keyValuePair in finalModifiedValuesDictionary)
//                 SetBlendShapeWeightInterpolate(skinnedMesh, keyValuePair.Key, keyValuePair.Value * bounds.y - bounds.x, WeightBlendingPower);
//         }

//         private static void CalculateBlendShapeEffect(List<BlendShapesIndexEffector> effectors, ref Dictionary<int, float> dictionary, float value)
//         {
//             foreach (BlendShapesIndexEffector blendShapesIndexEffector in effectors)
//                 if (dictionary.ContainsKey(blendShapesIndexEffector.index))
//                     dictionary[blendShapesIndexEffector.index] += value * blendShapesIndexEffector.effectPercentage;
//                 else
//                     dictionary[blendShapesIndexEffector.index] = value * blendShapesIndexEffector.effectPercentage;
//         }

//         public void SetBlendShapeWeightInterpolate(SkinnedMeshRenderer renderer, int index, float value, float weight)
//         {
//             renderer.SetBlendShapeWeight(index, Mathf.Lerp(renderer.GetBlendShapeWeight(index), value, weight));
//         }


//     }
// }


