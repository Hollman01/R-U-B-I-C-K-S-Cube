using UnityEngine;

// SolverTestRunner.cs
// Runs solver self-test in Editor (no device needed)
public class SolverTestRunner : MonoBehaviour
{
    void Start()
    {
        CubeSolver.RunSelfTest();
    }
}