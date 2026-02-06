using System;
using UnityEngine;

// CubeState.cs
// Represents a full 3x3x3 Rubik's Cube state (54 stickers)
// Uses standard colour scheme: White=Up, Yellow=Down, Green=Front, Blue=Back, Red=Right, Orange=Left
[Serializable]
public class CubeState
{
    // 6 faces × 9 stickers = 54 total stickers
    // Index order: [0]=Up, [1]=Down, [2]=Left, [3]=Right, [4]=Front, [5]=Back
    private Color32[][] stickers = new Color32[6][];

    // Standard Rubik's Cube colours
    public static readonly Color32 WHITE  = new Color32(255, 255, 255, 255);
    public static readonly Color32 YELLOW = new Color32(255, 165, 0, 255);   // Orange-yellow for visibility
    public static readonly Color32 GREEN  = new Color32(0, 255, 0, 255);
    public static readonly Color32 BLUE   = new Color32(0, 0, 255, 255);
    public static readonly Color32 RED    = new Color32(255, 0, 0, 255);
    public static readonly Color32 ORANGE = new Color32(255, 165, 0, 255);   // Orange

    // Constructor: creates a solved cube
    public CubeState()
    {
        // Initialise each face with 9 stickers of the same colour
        stickers[(int)Face.Up]    = CreateUniformFace(WHITE);
        stickers[(int)Face.Down]  = CreateUniformFace(YELLOW);
        stickers[(int)Face.Left]  = CreateUniformFace(ORANGE);
        stickers[(int)Face.Right] = CreateUniformFace(RED);
        stickers[(int)Face.Front] = CreateUniformFace(GREEN);
        stickers[(int)Face.Back]  = CreateUniformFace(BLUE);
    }

    // Helper: creates an array of 9 identical colours
    private Color32[] CreateUniformFace(Color32 colour)
    {
        Color32[] face = new Color32[9];
        for (int i = 0; i < 9; i++) face[i] = colour;
        return face;
    }

    // Get sticker colour at specific face and position (0-8 grid index)
    public Color32 GetSticker(Face face, int index)
    {
        if (index < 0 || index >= 9)
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be 0-8");
        return stickers[(int)face][index];
    }

    // Set sticker colour (used during scanning)
    public void SetSticker(Face face, int index, Color32 colour)
    {
        if (index < 0 || index >= 9)
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be 0-8");
        stickers[(int)face][index] = colour;
    }

    // Returns a deep copy of this cube state (for solver safety)
    public CubeState Clone()
    {
        CubeState copy = new CubeState();
        for (int f = 0; f < 6; f++)
        {
            for (int i = 0; i < 9; i++)
            {
                copy.stickers[f][i] = this.stickers[f][i];
            }
        }
        return copy;
    }
}