using UnityEngine;

// GridTestRunner.cs
// Runs grid detection self-test in Editor (no device needed)
public class GridTestRunner : MonoBehaviour
{
    void Start()
    {
        GridDetector.RunSelfTest();
    }
}