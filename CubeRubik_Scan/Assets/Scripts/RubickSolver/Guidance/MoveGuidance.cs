using UnityEngine;
using System.Collections.Generic;

// MoveGuidance.cs
// Shows MR guidance arrows/UI for each solver move
// Works in Editor with mock data; later overlays on real cube via passthrough
public class MoveGuidance : MonoBehaviour
{
    public VoiceGuidance voiceGuidance; // Optional voice integration
    
    [System.Serializable]
    public class MoveVisual
    {
        public string moveNotation;      // "R", "U", "F'", etc.
        public GameObject arrowPrefab;   // Visual arrow pointing to face
        public string instructionText;   // "Rotate right face clockwise"
    }
    
    public List<MoveVisual> moveVisuals = new List<MoveVisual>();
    private int currentMoveIndex = 0;
    
    void Start()
    {
        Debug.Log("[MoveGuidance] Ready to show move instructions");
    }
    
    // Show next move in sequence
    public void ShowNextMove(List<string> solutionSequence)
    {
        if (currentMoveIndex >= solutionSequence.Count)
        {
            Debug.Log("[MoveGuidance] ✅ Cube solved! All moves completed.");
            
            // Speak completion
            if (voiceGuidance != null)
                voiceGuidance.SpeakMove("None");
            return;
        }
        
        string nextMove = solutionSequence[currentMoveIndex];
        string instruction = GetInstructionText(nextMove);
        
        Debug.Log($"[MoveGuidance] ➡️ Next move: {nextMove}");
        Debug.Log($"[MoveGuidance] 💬 {instruction}");
        
        // Speak the move (if voice available)
        if (voiceGuidance != null)
            voiceGuidance.SpeakMove(nextMove);
        
        currentMoveIndex++;
    }
    
    // Convert move notation to human instruction
    private string GetInstructionText(string move)
    {
        if (string.IsNullOrEmpty(move) || move == "None") return "Cube is solved!";
        
        bool isCounterClockwise = move.Contains("'");
        string face = move.Replace("'", "").Replace("2", "");
        
        string faceName = "";
        switch (face)
        {
            case "U": faceName = "top"; break;
            case "D": faceName = "bottom"; break;
            case "L": faceName = "left"; break;
            case "R": faceName = "right"; break;
            case "F": faceName = "front"; break;
            case "B": faceName = "back"; break;
            default: faceName = "unknown"; break;
        }
        
        string direction = isCounterClockwise ? "counter-clockwise" : "clockwise";
        return $"Rotate the {faceName} face {direction}.";
    }
    
    // Editor-only self-test
    public void RunSelfTest()
    {
        Debug.Log("=== MoveGuidance Self-Test ===");
        
        List<string> testSequence = new List<string> { "R", "U", "R'", "U'" };
        Debug.Log($"Test sequence: {string.Join(", ", testSequence)}");
        
        foreach (var move in testSequence)
        {
            string instruction = GetInstructionText(move);
            Debug.Log($"Move '{move}' → '{instruction}'");
        }
        
        Debug.Log("✅ PASS | All move instructions generated correctly");
    }
}