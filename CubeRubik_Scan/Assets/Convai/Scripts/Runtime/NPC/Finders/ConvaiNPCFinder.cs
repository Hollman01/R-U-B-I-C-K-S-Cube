using Convai.Scripts;
using Convai.Scripts.Player;
using JetBrains.Annotations;
using UnityEngine;

public class ConvaiNPCFinder : MonoBehaviour, IConvaiNPCFinder
{
    [SerializeField] private ConvaiNPC _npc;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public bool GetNPC([CanBeNull] out ConvaiNPC npc)
    {
        npc = _npc;
        return true;
    }
}
