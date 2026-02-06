using System.Collections.Generic;
using Convai.Scripts.Extensions;
using Convai.Scripts.LoggerSystem;
using Convai.Scripts.Player;
using UnityEngine;

namespace Convai.Scripts
{
    [RequireComponent(typeof(SphereCollider), typeof(ConvaiPlayer))]
    public class ConvaiProximityNPCFinder : MonoBehaviour, IConvaiNPCFinder
    {
        private readonly List<ConvaiNPC> _npcInProximityList = new();
        private ConvaiPlayer _convaiPlayer;

        private void Awake() => _convaiPlayer = GetComponent<ConvaiPlayer>();

        private void OnDrawGizmos()
        {
            foreach (ConvaiNPC convaiNPC in _npcInProximityList)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, convaiNPC.transform.position);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out ConvaiNPC convaiNPC))
            {
                ConvaiUnityLogger.DebugLog($"{convaiNPC.name} is now inside the proximity NPC area", LogCategory.Character);
                _npcInProximityList.Add(convaiNPC);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out ConvaiNPC convaiNPC))
            {
                ConvaiUnityLogger.DebugLog($"{convaiNPC.name} is now outside the proximity NPC area", LogCategory.Character);
                _npcInProximityList.Remove(convaiNPC);
            }
        }

        public bool GetNPC(out ConvaiNPC npc)
        {
            npc = null;
            if (_npcInProximityList.Count == 0)
            {
                return false;
            }

            float nearestDistance = float.MaxValue;

            // Get the vision cone from the player script
            List<ConvaiNPC> withInVisionCone =
                _npcInProximityList.FindAll(x => transform.LookingAtTarget(x.transform, _convaiPlayer.VisionConeAngle));

            foreach (ConvaiNPC convaiNPC in withInVisionCone)
            {
                float distance = Vector2.Distance(transform.position, convaiNPC.transform.position);
                if (Mathf.Approximately(distance, nearestDistance))
                {
                    if (npc == null || transform.GetDotProduct(convaiNPC.transform) <= transform.GetDotProduct(npc.transform))
                    {
                        continue;
                    }

                    npc = convaiNPC;
                    nearestDistance = distance;
                }
                else if (distance < nearestDistance)
                {
                    npc = convaiNPC;
                    nearestDistance = distance;
                }
            }

            return npc != null;
        }
    }
}
