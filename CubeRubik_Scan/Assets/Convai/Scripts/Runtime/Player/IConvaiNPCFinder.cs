using JetBrains.Annotations;

namespace Convai.Scripts.Player
{
    public interface IConvaiNPCFinder
    {
        bool GetNPC([CanBeNull] out ConvaiNPC npc);
    }
}
