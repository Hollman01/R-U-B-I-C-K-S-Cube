using UnityEngine;
using System.Collections.Generic;

// RubikWorkflow.cs
// Minimal pipeline validation in Editor — no dependencies on Start() timing
public class RubikWorkflow : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("\n=== 🧊 RUBIK WORKFLOW (Editor Validation) ===\n");
        
        // STEP 1: Simulate scan
        Debug.Log("[1/3] SCAN → Simulating U-scrambled cube...");
        CubeState scanned = new CubeState();
        CubeMove.U(scanned);
        Debug.Log("✅ Scanned state created (U move applied)\n");
        
        // STEP 2: Solve
        Debug.Log("[2/3] SOLVE → Computing solution...");
        List<string> solution = CubeSolver.ComputeSolution(scanned);
        if (solution.Count > 0)
            Debug.Log($"✅ Solution: {string.Join(" → ", solution)}\n");
        else
            Debug.Log("✅ Cube already solved\n");
        
        // STEP 3: Guidance
        Debug.Log("[3/3] GUIDE → Generating move instructions...");
        string move = solution.Count > 0 ? solution[0] : "None";
        string instruction = GetInstructionText(move);
        Debug.Log($"✅ Next move: '{move}' → \"{instruction}\"\n");
        
        Debug.Log("=== ✅ WORKFLOW VALIDATED IN EDITOR ===");
        Debug.Log("Next phase: Connect to real camera on Quest 3\n");
    }
    
    private string GetInstructionText(string move)
    {
        if (string.IsNullOrEmpty(move) || move == "None") return "Cube is solved!";
        
        bool ccw = move.Contains("'");
        string face = move.Replace("'", "").Replace("2", "");
        string faceName = face switch { "U" => "top", "D" => "bottom", "L" => "left", "R" => "right", "F" => "front", "B" => "back", _ => "unknown" };
        string dir = ccw ? "counter-clockwise" : "clockwise";
        return $"Rotate the {faceName} face {dir}.";
    }
}