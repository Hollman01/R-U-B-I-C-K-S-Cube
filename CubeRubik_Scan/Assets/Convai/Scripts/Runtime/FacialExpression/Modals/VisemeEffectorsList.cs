using System.Collections.Generic;
using UnityEngine;

namespace Convai.Scripts.Facial_Expression.Modals
{
    [CreateAssetMenu(fileName = "Convai Viseme Effectors", menuName = "Convai/Expression/Visemes Skin Effector", order = 0)]
    public class VisemeEffectorsList : ScriptableObject
    {
        [SerializeField] public List<BlendShapesIndexEffector> sil;
        [SerializeField] public List<BlendShapesIndexEffector> pp;
        [SerializeField] public List<BlendShapesIndexEffector> ff;
        [SerializeField] public List<BlendShapesIndexEffector> th;
        [SerializeField] public List<BlendShapesIndexEffector> dd;
        [SerializeField] public List<BlendShapesIndexEffector> kk;
        [SerializeField] public List<BlendShapesIndexEffector> ch;
        [SerializeField] public List<BlendShapesIndexEffector> ss;
        [SerializeField] public List<BlendShapesIndexEffector> nn;
        [SerializeField] public List<BlendShapesIndexEffector> rr;
        [SerializeField] public List<BlendShapesIndexEffector> aa;
        [SerializeField] public List<BlendShapesIndexEffector> e;
        [SerializeField] public List<BlendShapesIndexEffector> ih;
        [SerializeField] public List<BlendShapesIndexEffector> oh;
        [SerializeField] public List<BlendShapesIndexEffector> ou;
    }
}
