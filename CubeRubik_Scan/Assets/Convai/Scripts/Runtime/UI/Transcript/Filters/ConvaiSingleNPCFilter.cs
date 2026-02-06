using System.Collections.Generic;
using Convai.Scripts.Extensions;
using UnityEngine;

namespace Convai.Scripts.TranscriptUI.Filters
{
    public class ConvaiSingleNPCFilter : ConvaiTranscriptFilterBase
    {
        private void FixedUpdate()
        {
            ConvaiNPC npc = null;
            if (NPCInsideColliderList.Count == 0)
            {
                return;
            }

            float nearestDistance = float.MaxValue;
            Transform viewer = TranscriptHandler.transform;
            float visionConeAngle = TranscriptHandler.ConvaiPlayer.VisionConeAngle;
            List<ConvaiNPC> withInVisionCone = NPCInsideColliderList.FindAll(x => viewer.LookingAtTarget(x.transform, visionConeAngle));
            foreach (ConvaiNPC convaiNPC in withInVisionCone)
            {
                float distance = Vector2.Distance(transform.position, convaiNPC.transform.position);
                if (Mathf.Approximately(distance, nearestDistance))
                {
                    if (npc == null)
                    {
                        continue;
                    }

                    if (viewer.GetDotProduct(convaiNPC.transform) > viewer.GetDotProduct(npc.transform))
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

            if (npc == null)
            {
                TranscriptHandler.RemoveCharacterIDAt(0);
                return;
            }


            if (TranscriptHandler.visibleCharacterChatIds.Count > 0 && TranscriptHandler.visibleCharacterChatIds[0] == npc.CharacterID)
            {
                return;
            }

            TranscriptHandler.RemoveCharacterIDAt(0);
            TranscriptHandler.AddCharacterChatId(npc.CharacterID);
        }
    }
}
