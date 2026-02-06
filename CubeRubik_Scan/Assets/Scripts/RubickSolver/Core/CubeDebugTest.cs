using UnityEngine;

// CubeDebugTest.cs
// Temporary script to verify cube logic works correctly
// Attach to an empty GameObject in SampleScene
public class CubeDebugTest : MonoBehaviour
{
    void Start()
    {
        // Create a solved cube
        CubeState cube = new CubeState();
        
        // Log front face top-left sticker BEFORE U move
        Color32 before = cube.GetSticker(Face.Front, 0);
        Debug.Log($"Before U move - Front top-left sticker: {ColorToName(before)}");
        
        // Apply U move (Up face clockwise)
        CubeMove.U(cube);
        
        // Log same sticker AFTER U move
        Color32 after = cube.GetSticker(Face.Front, 0);
        Debug.Log($"After U move - Front top-left sticker: {ColorToName(after)}");
    }
    
    // Helper to convert colour to readable name
    private string ColorToName(Color32 colour)
    {
        if (colour.Equals(CubeState.GREEN)) return "Green";
        if (colour.Equals(CubeState.RED)) return "Red";
        if (colour.Equals(CubeState.BLUE)) return "Blue";
        if (colour.Equals(CubeState.ORANGE)) return "Orange";
        if (colour.Equals(CubeState.WHITE)) return "White";
        if (colour.Equals(CubeState.YELLOW)) return "Yellow";
        return "Unknown";
    }
}