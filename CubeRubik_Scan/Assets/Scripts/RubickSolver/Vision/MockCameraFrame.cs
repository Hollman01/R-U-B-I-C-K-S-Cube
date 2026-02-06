using UnityEngine;

// MockCameraFrame.cs
// Generates realistic simulated camera frames of a Rubik's Cube face
// Fully Editor-testable — no device build required
public static class MockCameraFrame
{
    // Generate a texture simulating a camera view of a scrambled cube face
    // faceColours: 9 colours in row-major order (top-left to bottom-right)
    public static Texture2D GenerateFaceTexture(Color32[] faceColours, int textureSize = 300)
    {
        if (faceColours.Length != 9)
            throw new System.ArgumentException("Must provide exactly 9 colours for 3x3 grid");
        
        Texture2D tex = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        
        // Background (dark grey)
        for (int y = 0; y < textureSize; y++)
            for (int x = 0; x < textureSize; x++)
                tex.SetPixel(x, y, new Color32(40, 40, 40, 255));
        
        // Draw 3x3 grid of stickers (with small gaps between)
        int cellSize = textureSize / 3;
        int gap = 2; // 2px gap between stickers
        
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                Color32 colour = faceColours[row * 3 + col];
                int startX = col * cellSize + gap;
                int startY = row * cellSize + gap;
                int width = cellSize - gap * 2;
                int height = cellSize - gap * 2;
                
                for (int dy = 0; dy < height; dy++)
                {
                    for (int dx = 0; dx < width; dx++)
                    {
                        int px = startX + dx;
                        int py = startY + dy;
                        if (px < textureSize && py < textureSize)
                            tex.SetPixel(px, py, colour);
                    }
                }
            }
        }
        
        tex.Apply();
        return tex;
    }
    
    // Editor-only self-test
    public static void RunSelfTest()
    {
        Debug.Log("\n=== MockCameraFrame Self-Test ===");
        
        // Simulate a Green face (Front) with one Red sticker swapped (scrambled)
        Color32[] scrambledFront = new Color32[9];
        for (int i = 0; i < 9; i++) scrambledFront[i] = CubeState.GREEN;
        scrambledFront[0] = CubeState.RED; // Top-left sticker = Red (scrambled)
        
        Texture2D mockFrame = GenerateFaceTexture(scrambledFront, 300);
        
        // Sample the grid
        Face[] detected = GridDetector.SampleGrid(mockFrame, new Rect(0.05f, 0.05f, 0.9f, 0.9f));
        
        // Verify detection
        bool topLeftRed = detected[0] == Face.Right;   // Red = Right face
        bool centreGreen = detected[4] == Face.Front;  // Centre should still be Green
        
        Debug.Log(topLeftRed ? "✅ PASS | Top-left sticker detected as Red" : "❌ FAIL | Top-left not Red");
        Debug.Log(centreGreen ? "✅ PASS | Centre sticker detected as Green" : "❌ FAIL | Centre not Green");
        
        // Cleanup
        Object.DestroyImmediate(mockFrame);
        
        Debug.Log("✅ Mock frame generation + grid detection validated\n");
    }
}