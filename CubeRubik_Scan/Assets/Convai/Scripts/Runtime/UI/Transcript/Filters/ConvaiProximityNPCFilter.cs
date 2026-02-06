using System.Collections.Generic;
using System.Linq;
using Convai.Scripts.Extensions;

namespace Convai.Scripts.TranscriptUI.Filters
{
    public class ConvaiProximityNPCFilter : ConvaiTranscriptFilterBase
    {
        private void FixedUpdate()
        {
            List<string> insideCollider = NPCInsideColliderList
                .Select(x => x.CharacterID)
                .ToList();

            List<string> insideVisionCone = NPCInsideColliderList
                .FindAll(x => TranscriptHandler.transform.LookingAtTarget(x.transform, TranscriptHandler.ConvaiPlayer.VisionConeAngle))
                .Select(x => x.CharacterID)
                .ToList();

            insideVisionCone.ForEach(x => TranscriptHandler.AddCharacterChatId(x));

            insideCollider
                .Except(insideVisionCone)
                .ToList()
                .ForEach(x => TranscriptHandler.RemoveCharacterChatId(x));
        }
    }
}
