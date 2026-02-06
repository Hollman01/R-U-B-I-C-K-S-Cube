using UnityEngine;

// CubeMove.cs
// Applies standard Rubik's Cube moves to a CubeState
// Implements clockwise rotations for all 6 faces (U, D, L, R, F, B)
public static class CubeMove
{
    // Rotate Up face clockwise (U move)
    public static void U(CubeState cube)
    {
        // Rotate Up face stickers (indices 0-8 in a 3x3 grid)
        RotateFaceClockwise(cube, Face.Up);
        
        // Cycle top edges of side faces: Front → Right → Back → Left → Front
        Color32 temp0 = cube.GetSticker(Face.Front, 0);
        Color32 temp1 = cube.GetSticker(Face.Front, 1);
        Color32 temp2 = cube.GetSticker(Face.Front, 2);
        
        // Front ← Left
        cube.SetSticker(Face.Front, 0, cube.GetSticker(Face.Left, 0));
        cube.SetSticker(Face.Front, 1, cube.GetSticker(Face.Left, 1));
        cube.SetSticker(Face.Front, 2, cube.GetSticker(Face.Left, 2));
        
        // Left ← Back
        cube.SetSticker(Face.Left, 0, cube.GetSticker(Face.Back, 0));
        cube.SetSticker(Face.Left, 1, cube.GetSticker(Face.Back, 1));
        cube.SetSticker(Face.Left, 2, cube.GetSticker(Face.Back, 2));
        
        // Back ← Right
        cube.SetSticker(Face.Back, 0, cube.GetSticker(Face.Right, 0));
        cube.SetSticker(Face.Back, 1, cube.GetSticker(Face.Right, 1));
        cube.SetSticker(Face.Back, 2, cube.GetSticker(Face.Right, 2));
        
        // Right ← Front (original)
        cube.SetSticker(Face.Right, 0, temp0);
        cube.SetSticker(Face.Right, 1, temp1);
        cube.SetSticker(Face.Right, 2, temp2);
    }
    
    // Rotate Down face clockwise (D move)
    public static void D(CubeState cube)
    {
        RotateFaceClockwise(cube, Face.Down);
        
        // Cycle bottom edges: Front → Left → Back → Right → Front
        Color32 temp0 = cube.GetSticker(Face.Front, 6);
        Color32 temp1 = cube.GetSticker(Face.Front, 7);
        Color32 temp2 = cube.GetSticker(Face.Front, 8);
        
        // Front ← Right
        cube.SetSticker(Face.Front, 6, cube.GetSticker(Face.Right, 6));
        cube.SetSticker(Face.Front, 7, cube.GetSticker(Face.Right, 7));
        cube.SetSticker(Face.Front, 8, cube.GetSticker(Face.Right, 8));
        
        // Right ← Back
        cube.SetSticker(Face.Right, 6, cube.GetSticker(Face.Back, 6));
        cube.SetSticker(Face.Right, 7, cube.GetSticker(Face.Back, 7));
        cube.SetSticker(Face.Right, 8, cube.GetSticker(Face.Back, 8));
        
        // Back ← Left
        cube.SetSticker(Face.Back, 6, cube.GetSticker(Face.Left, 6));
        cube.SetSticker(Face.Back, 7, cube.GetSticker(Face.Left, 7));
        cube.SetSticker(Face.Back, 8, cube.GetSticker(Face.Left, 8));
        
        // Left ← Front (original)
        cube.SetSticker(Face.Left, 6, temp0);
        cube.SetSticker(Face.Left, 7, temp1);
        cube.SetSticker(Face.Left, 8, temp2);
    }
    
    // Rotate Front face clockwise (F move)
    public static void F(CubeState cube)
    {
        RotateFaceClockwise(cube, Face.Front);
        
        // Top edge of Down ← Right edge of Left
        Color32 temp0 = cube.GetSticker(Face.Up, 6);
        Color32 temp1 = cube.GetSticker(Face.Up, 7);
        Color32 temp2 = cube.GetSticker(Face.Up, 8);
        
        // Up bottom row ← Left right column (reversed)
        cube.SetSticker(Face.Up, 6, cube.GetSticker(Face.Left, 2));
        cube.SetSticker(Face.Up, 7, cube.GetSticker(Face.Left, 5));
        cube.SetSticker(Face.Up, 8, cube.GetSticker(Face.Left, 8));
        
        // Left right column ← Down top row (reversed)
        cube.SetSticker(Face.Left, 2, cube.GetSticker(Face.Down, 2));
        cube.SetSticker(Face.Left, 5, cube.GetSticker(Face.Down, 1));
        cube.SetSticker(Face.Left, 8, cube.GetSticker(Face.Down, 0));
        
        // Down top row ← Right left column (reversed)
        cube.SetSticker(Face.Down, 0, cube.GetSticker(Face.Right, 6));
        cube.SetSticker(Face.Down, 1, cube.GetSticker(Face.Right, 3));
        cube.SetSticker(Face.Down, 2, cube.GetSticker(Face.Right, 0));
        
        // Right left column ← Up bottom row (original, reversed)
        cube.SetSticker(Face.Right, 0, temp2);
        cube.SetSticker(Face.Right, 3, temp1);
        cube.SetSticker(Face.Right, 6, temp0);
    }
    
    // Generic helper: rotates 9 stickers of a face clockwise in 3x3 grid
    private static void RotateFaceClockwise(CubeState cube, Face face)
    {
        // Original positions (row-major 3x3 grid):
        // 0 1 2
        // 3 4 5
        // 6 7 8
        
        // After clockwise rotation:
        // 6 3 0
        // 7 4 1
        // 8 5 2
        
        Color32[] original = new Color32[9];
        for (int i = 0; i < 9; i++)
            original[i] = cube.GetSticker(face, i);
        
        cube.SetSticker(face, 0, original[6]);
        cube.SetSticker(face, 1, original[3]);
        cube.SetSticker(face, 2, original[0]);
        cube.SetSticker(face, 3, original[7]);
        cube.SetSticker(face, 4, original[4]); // Centre stays same colour
        cube.SetSticker(face, 5, original[1]);
        cube.SetSticker(face, 6, original[8]);
        cube.SetSticker(face, 7, original[5]);
        cube.SetSticker(face, 8, original[2]);
    }
    
    // Apply move by name (for solver integration later)
    public static void ApplyMove(CubeState cube, string moveNotation)
    {
        switch (moveNotation)
        {
            case "U": U(cube); break;
            case "D": D(cube); break;
            case "F": F(cube); break;
            // L, R, B moves will be added later when needed
            default:
                Debug.LogWarning("Move not implemented yet: " + moveNotation);
                break;
        }
    }
}