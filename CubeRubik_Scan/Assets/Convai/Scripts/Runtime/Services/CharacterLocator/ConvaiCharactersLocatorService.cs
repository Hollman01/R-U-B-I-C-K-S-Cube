using System.Collections.Generic;
using System.Linq;
using Convai.Scripts.Player;
using UnityEngine;

namespace Convai.Scripts.Services.CharacterLocator
{
    public class ConvaiCharacterLocatorService
    {
        private readonly List<ConvaiNPC> _npcList = Object.FindObjectsByType<ConvaiNPC>(FindObjectsSortMode.None).ToList();
        private readonly List<ConvaiPlayer> _playerList = Object.FindObjectsByType<ConvaiPlayer>(FindObjectsSortMode.None).ToList();

        public List<ConvaiNPC> GetNPCList() => _npcList;
        public List<ConvaiPlayer> GetPlayerList() => _playerList;

        public bool GetNPC(string charID, out ConvaiNPC npc)
        {
            npc = _npcList.Find(n => n.CharacterID == charID);
            return npc != null;
        }

        public bool GetPlayer(string apiKey, out ConvaiPlayer player)
        {
            player = _playerList.Find(x => x.APIKey == apiKey);
            return player != null;
        }

        public void AddNPC(ConvaiNPC npc) => _npcList.Add(npc);
        public void AddPlayer(ConvaiPlayer player) => _playerList.Add(player);

        public void RemoveNPC(ConvaiNPC npc) => _npcList.Remove(npc);
        public void RemovePlayer(ConvaiPlayer player) => _playerList.Remove(player);
    }
}
