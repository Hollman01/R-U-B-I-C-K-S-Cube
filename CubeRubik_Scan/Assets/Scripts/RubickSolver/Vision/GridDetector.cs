using UnityEngine;

// GridDetector.cs
// Detects a 3x3 sticker grid inside a rectangular region
// Works with any Texture2D — testable in Editor with mock textures
public static class GridDetector
{
    // Sample colours from a 3x3 grid inside a region of the texture
    // regionRect: area containing the cube face (e.g. Rect(0.3f, 0.3f, 0.4f, 0.4f) = centre 40%)
    // Returns 9 classified Face colours in row-major order:
    // [0][1][2]
    // [3][4][5]
    // [6][7][8]
    public static Face[] SampleGrid(Texture2D texture, Rect regionRect)
    {
        Face[] stickers = new Face[9];
        
        // Convert normalised rect (0-1) to pixel coordinates
        int startX = Mathf.RoundToInt(regionRect.x * texture.width);
        int startY = Mathf.RoundToInt(regionRect.y * texture.height);
        int regionWidth = Mathf.RoundToInt(regionRect.width * texture.width);
        int regionHeight = Mathf.RoundToInt(regionRect.height * texture.height);
        
        // Subdivide region into 3×3 grid cells
        int cellWidth = regionWidth / 3;
        int cellHeight = regionHeight / 3;
        
        int index = 0;
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                // Sample centre of each cell (avoid edges/noise)
                int sampleX = startX + col * cellWidth + cellWidth / 2;
                int sampleY = startY + row * cellHeight + cellHeight / 2;
                
                // Clamp to texture bounds (safety)
                sampleX = Mathf.Clamp(sampleX, 0, texture.width - 1);
                sampleY = Mathf.Clamp(sampleY, 0, texture.height - 1);
                
                // Get pixel colour
                Color32 pixel = texture.GetPixel(sampleX, sampleY);
                
                // Classify colour → Face
                stickers[index] = ColourDetector.ClassifyColour(pixel);
                index++;
            }
        }
        
        return stickers;
    }
    
    // Editor-only self-test using a generated test texture
    public static void RunSelfTest()
    {
        Debug.Log("=== GridDetector Self-Test ===");
        
        // Create a 300x300 test texture with a simulated Rubik's face
        Texture2D testTex = new Texture2D(300, 300, TextureFormat.RGBA32, false);
        
        // Fill with white background
        for (int y = 0; y < testTex.height; y++)
            for (int x = 0; x < testTex.width; x++)
                testTex.SetPixel(x, y, Color.white);
        
        // Draw a simulated 3×3 green face (Front) in the centre
        int cellSize = 80;
        int margin = (testTex.width - cellSize * 3) / 2;
        
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                // Fill each sticker cell with green
                for (int dy = 0; dy < cellSize; dy++)
                {
                    for (int dx = 0; dx < cellSize; dx++)
                    {
                        int px = margin + col * cellSize + dx;
                        int py = margin + row * cellSize + dy;
                        testTex.SetPixel(px, py, Color.green);
                    }
                }
            }
        }
        
        testTex.Apply();
        
        // Sample the central 80% region (where our fake cube face lives)
        Rect testRegion = new Rect(0.1f, 0.1f, 0.8f, 0.8f);
        Face[] result = SampleGrid(testTex, testRegion);
        
        // Verify all 9 stickers classified as Front (Green)
        bool allGreen = true;
        for (int i = 0; i < 9; i++)
        {
            if (result[i] != Face.Front)
                allGreen = false;
        }
        
        Debug.Log(allGreen ? "✅ PASS | All 9 stickers detected as Green (Front)" : "❌ FAIL | Not all stickers classified as Front");
        
        // Cleanup
        Object.DestroyImmediate(testTex);
    }
}