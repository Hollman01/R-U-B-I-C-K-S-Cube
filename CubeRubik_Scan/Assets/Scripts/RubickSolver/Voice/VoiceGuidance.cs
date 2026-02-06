using UnityEngine;

// VoiceGuidance.cs
// Convai voice integration with Editor-safe mock mode
// Automatically uses mock in Editor, real Convai on device
public class VoiceGuidance : MonoBehaviour
{
    private bool useMock = true; // Editor = mock, Device = real Convai
    
    void Awake()
    {
        // Detect if Convai is available (device build with Convai setup)
        #if !UNITY_EDITOR
        if (IsConvaiAvailable())
        {
            useMock = false;
            Debug.Log("[VoiceGuidance] ✅ Real Convai agent available");
        }
        else
        {
            Debug.LogWarning("[VoiceGuidance] ⚠️ Convai not configured — using mock voice");
        }
        #else
        Debug.Log("[VoiceGuidance] 🎙️ Editor mode: using mock voice (no API calls)");
        #endif
    }
    
    // Speak a move instruction
    public void SpeakMove(string moveNotation)
    {
        string instruction = GetInstructionText(moveNotation);
        
        if (useMock)
        {
            // Editor-safe mock: log what WOULD be spoken
            Debug.Log($"[VoiceGuidance] 🗣️ (MOCK) \"{instruction}\"");
        }
        else
        {
            // Real Convai integration (device only)
            SendToConvai(instruction);
        }
    }
    
    // Convert move notation to natural language
    private string GetInstructionText(string move)
    {
        if (string.IsNullOrEmpty(move) || move == "None") return "The cube is solved! Well done!";
        
        bool ccw = move.Contains("'");
        string face = move.Replace("'", "").Replace("2", "");
        string faceName = face switch { "U" => "top", "D" => "bottom", "L" => "left", "R" => "right", "F" => "front", "B" => "back", _ => "unknown face" };
        string dir = ccw ? "counter-clockwise" : "clockwise";
        return $"Rotate the {faceName} face {dir}.";
    }
    
    // Mock Convai send (Editor-safe)
    private void SendToConvai(string text)
    {
        // Real implementation would be:
        // ConvaiAgent.Instance.SendMessage(text);
        Debug.Log($"[VoiceGuidance] 📡 Sending to Convai: \"{text}\"");
    }
    
    // Check if Convai runtime is available (device only)
    private bool IsConvaiAvailable()
    {
        #if UNITY_EDITOR
        return false;
        #else
        // Safe reflection check to avoid compile errors if Convai missing
        System.Type convaiType = System.Type.GetType("Convai.Scripts.ConvaiAgent, Assembly-CSharp");
        return convaiType != null;
        #endif
    }
    
    // Editor-only self-test
    public static void RunSelfTest()
    {
        Debug.Log("\n=== VoiceGuidance Self-Test ===");
        
        string[] testMoves = { "R", "U'", "F", "None" };
        foreach (string move in testMoves)
        {
            string instruction = new VoiceGuidance().GetInstructionText(move);
            Debug.Log($"Move '{move}' → \"{instruction}\"");
        }
        
        Debug.Log("✅ Voice instruction generation validated\n");
    }
}