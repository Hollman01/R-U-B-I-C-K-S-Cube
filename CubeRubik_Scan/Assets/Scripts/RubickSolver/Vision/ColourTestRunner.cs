using UnityEngine;

// ColourTestRunner.cs
// Runs colour detection self-test in Editor (no device needed)
public class ColourTestRunner : MonoBehaviour
{
    void Start()
    {
        ColourDetector.RunSelfTest();
    }
}