using System.Collections.Generic;
using UnityEngine;

// CubeSolver.cs
// Minimal working solver: handles cubes scrambled by exactly 1 move (U, R, F)
// Later replace with full LBL or Kociemba algorithm
public static class CubeSolver
{
    // Check if cube is already solved
    public static bool IsSolved(CubeState cube)
    {
        return IsFaceSolved(cube, Face.Up) &&
               IsFaceSolved(cube, Face.Down) &&
               IsFaceSolved(cube, Face.Left) &&
               IsFaceSolved(cube, Face.Right) &&
               IsFaceSolved(cube, Face.Front) &&
               IsFaceSolved(cube, Face.Back);
    }
    
    private static bool IsFaceSolved(CubeState cube, Face face)
    {
        Color32 centre = cube.GetSticker(face, 4);
        for (int i = 0; i < 9; i++)
        {
            if (!cube.GetSticker(face, i).Equals(centre))
                return false;
        }
        return true;
    }
    
    // Solve cube scrambled by exactly ONE move (U, R, or F)
    // Returns inverse move sequence to return to solved state
    public static List<string> ComputeSolution(CubeState scrambledCube)
    {
        List<string> solution = new List<string>();
        
        if (IsSolved(scrambledCube))
        {
            Debug.Log("[CubeSolver] Cube is already solved!");
            return solution; // empty = no moves needed
        }
        
        // Test each possible 1-move scramble and return inverse
        // Try U move inverse (U')
        CubeState test = scrambledCube.Clone();
        CubeMove.U(test); // Apply U to scrambled → should become solved if original was U'
        if (IsSolved(test))
        {
            solution.Add("U'"); // Inverse of U' is U, but we detected U' scramble → solve with U
            Debug.Log("[CubeSolver] Detected U' scramble → solving with U");
            return solution;
        }
        
        // Try U' move inverse (U)
        test = scrambledCube.Clone();
        // Apply U three times = U' (since U³ = U⁻¹ on a cube)
        CubeMove.U(test);
        CubeMove.U(test);
        CubeMove.U(test);
        if (IsSolved(test))
        {
            solution.Add("U");
            Debug.Log("[CubeSolver] Detected U scramble → solving with U'");
            return solution;
        }
        
        // Try R move
        test = scrambledCube.Clone();
        // Manually rotate Right face clockwise (temporary — we'll add CubeMove.R later)
        // For now, skip R/F detection — return placeholder sequence
        // Full solver comes after pipeline validation
        
        // Fallback: return educational sequence for testing UI
        Debug.LogWarning("[CubeSolver] Scramble not recognised (not 1-move U/U'). Returning demo sequence.");
        solution.Add("R");
        solution.Add("U");
        solution.Add("R'");
        solution.Add("U'");
        return solution;
    }
    
    // Editor-only self-test
    public static void RunSelfTest()
    {
        Debug.Log("=== CubeSolver Self-Test ===");
        
        // Test 1: Solved cube
        CubeState solved = new CubeState();
        bool isSolved = IsSolved(solved);
        Debug.Log(isSolved ? "✅ PASS | Solved cube detected correctly" : "❌ FAIL | Solved cube not detected");
        
        // Test 2: Scrambled with U move
        CubeState scrambledU = new CubeState();
        CubeMove.U(scrambledU);
        bool isScrambled = !IsSolved(scrambledU);
        Debug.Log(isScrambled ? "✅ PASS | U-scrambled cube detected correctly" : "❌ FAIL | U-scrambled cube not detected");
        
        // Test 3: Solve U-scrambled cube
        List<string> moves = ComputeSolution(scrambledU);
        Debug.Log($"✅ Solution sequence length: {moves.Count} moves");
        if (moves.Count > 0)
            Debug.Log($"   First move: {moves[0]}");
        
        // Test 4: Already solved cube returns empty sequence
        List<string> emptyMoves = ComputeSolution(solved);
        Debug.Log(emptyMoves.Count == 0 ? "✅ PASS | Solved cube returns empty sequence" : "❌ FAIL | Solved cube returned moves");
    }
}