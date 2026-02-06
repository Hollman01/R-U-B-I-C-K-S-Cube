using UnityEngine;

// CubeScanner.cs
// Reconstructs a full CubeState from 6 scanned faces
// Works with mock data in Editor; later connects to CameraCapture on device
public class CubeScanner : MonoBehaviour
{
    private CubeState scannedState;
    private int facesScanned = 0;
    
    void Awake()
    {
        // Initialize here (runs before Start) to support immediate testing
        scannedState = new CubeState();
        Debug.Log("[CubeScanner] Ready to scan faces");
    }
    
    // Scan one face and assign it to the correct cube face
    public void ScanFace(Face faceToAssign, Texture2D texture, Rect region)
    {
        if (texture == null)
        {
            Debug.LogError("[CubeScanner] Texture is null");
            return;
        }
        
        // Sample 3×3 grid colours
        Face[] stickerColours = GridDetector.SampleGrid(texture, region);
        
        // Map scanned colours to CubeState stickers
        for (int i = 0; i < 9; i++)
        {
            Color32 colour = GetStandardColour(stickerColours[i]);
            scannedState.SetSticker(faceToAssign, i, colour);
        }
        
        facesScanned++;
        Debug.Log($"[CubeScanner] ✅ Scanned face {faceToAssign} ({facesScanned}/6)");
        
        if (facesScanned >= 6)
        {
            LogCubeState();
        }
    }
    
    // Helper: map Face enum to its standard Rubik's colour
    private Color32 GetStandardColour(Face face)
    {
        switch (face)
        {
            case Face.Up:    return CubeState.WHITE;
            case Face.Down:  return CubeState.YELLOW;
            case Face.Left:  return CubeState.ORANGE;
            case Face.Right: return CubeState.RED;
            case Face.Front: return CubeState.GREEN;
            case Face.Back:  return CubeState.BLUE;
            default:         return CubeState.WHITE;
        }
    }
    
    // Log the entire cube state for debugging
    private void LogCubeState()
    {
        Debug.Log("=== FULL CUBE STATE SCANNED ===");
        Debug.Log($"Up face centre:    {ColourName(scannedState.GetSticker(Face.Up, 4))}");
        Debug.Log($"Down face centre:  {ColourName(scannedState.GetSticker(Face.Down, 4))}");
        Debug.Log($"Front face centre: {ColourName(scannedState.GetSticker(Face.Front, 4))}");
        Debug.Log($"Back face centre:  {ColourName(scannedState.GetSticker(Face.Back, 4))}");
        Debug.Log($"Left face centre:  {ColourName(scannedState.GetSticker(Face.Left, 4))}");
        Debug.Log($"Right face centre: {ColourName(scannedState.GetSticker(Face.Right, 4))}");
        Debug.Log("Ready for solver!");
    }
    
    private string ColourName(Color32 colour)
    {
        if (colour.Equals(CubeState.WHITE)) return "White";
        if (colour.Equals(CubeState.YELLOW)) return "Yellow";
        if (colour.Equals(CubeState.GREEN)) return "Green";
        if (colour.Equals(CubeState.BLUE)) return "Blue";
        if (colour.Equals(CubeState.RED)) return "Red";
        if (colour.Equals(CubeState.ORANGE)) return "Orange";
        return "Unknown";
    }
    
    // Editor-only self-test with mock textures
    public void RunSelfTest()
    {
        Debug.Log("=== CubeScanner Self-Test ===");
        
        // Create a simple 100x100 green texture for Front face test
        Texture2D greenTex = CreateSolidTexture(100, 100, Color.green);
        
        // Scan Front face (centre region of texture)
        ScanFace(Face.Front, greenTex, new Rect(0.1f, 0.1f, 0.8f, 0.8f));
        
        // Verify centre sticker is green
        Color32 centre = scannedState.GetSticker(Face.Front, 4);
        bool isGreen = centre.Equals(CubeState.GREEN);
        Debug.Log(isGreen ? "✅ PASS | Front face centre is Green" : "❌ FAIL | Front face centre not Green");
        
        // Cleanup
        Object.DestroyImmediate(greenTex);
    }
    
    // Helper: create a solid-colour test texture
    private Texture2D CreateSolidTexture(int width, int height, Color colour)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                tex.SetPixel(x, y, colour);
        tex.Apply();
        return tex;
    }
}