using UnityEngine;

// ColourDetector.cs
// Classifies a colour into Rubik's Cube faces using HSV
// Testable in Editor with a test texture (no device build needed)
public static class ColourDetector
{
    // Classify pixel colour into Rubik's Cube faces using HSV
public static Face ClassifyColour(Color32 colour)
{
    // Convert to HSV for robust colour detection (lighting invariant)
    Color.RGBToHSV(colour, out float h, out float s, out float v);
    
    // White: low saturation OR high value + low saturation
    if (s < 0.25f || (v > 0.9f && s < 0.4f))
        return Face.Up; // White
    
    // Yellow: hue ~50-70° (0.14-0.19) with medium-high saturation
    if (h > 0.14f && h < 0.19f && s > 0.4f)
        return Face.Down; // Yellow
    
    // Red: hue near 0° or 360° (0.0-0.05 OR 0.95-1.0)
    if (((h < 0.05f || h > 0.95f) && s > 0.5f) || (h > 0.9f && h < 1.0f && s > 0.6f))
        return Face.Right; // Red
    
    // Green: hue ~100-140° (0.28-0.39)
    if (h > 0.28f && h < 0.39f && s > 0.4f)
        return Face.Front; // Green
    
    // Blue: hue ~210-270° (0.58-0.75)
    if (h > 0.58f && h < 0.75f && s > 0.4f)
        return Face.Back; // Blue
    
    // Orange: hue ~10-40° (0.03-0.11)
    if (h > 0.03f && h < 0.11f && s > 0.5f)
        return Face.Left; // Orange
    
    // Fallback: brightest channel wins
    if (colour.r > colour.g && colour.r > colour.b) return Face.Right;   // Red
    if (colour.g > colour.r && colour.g > colour.b) return Face.Front;   // Green
    if (colour.b > colour.r && colour.b > colour.g) return Face.Back;    // Blue
    
    return Face.Up; // Default to White
}
    
    // Test the classifier with known colours (Editor-safe)
    public static void RunSelfTest()
    {
        Debug.Log("=== ColourDetector Self-Test ===");
        
        TestColour("Pure Red", new Color32(255, 0, 0, 255), Face.Right);
        TestColour("Pure Green", new Color32(0, 255, 0, 255), Face.Front);
        TestColour("Pure Blue", new Color32(0, 0, 255, 255), Face.Back);
        TestColour("Pure White", new Color32(255, 255, 255, 255), Face.Up);
        TestColour("Yellow", new Color32(255, 255, 0, 255), Face.Down);
        TestColour("Orange", new Color32(255, 165, 0, 255), Face.Left);
    }
    
    private static void TestColour(string name, Color32 colour, Face expected)
    {
        Face result = ClassifyColour(colour);
        string status = (result == expected) ? "✅ PASS" : "❌ FAIL";
        Debug.Log($"{status} | {name}: {result} (expected {expected})");
    }
}
