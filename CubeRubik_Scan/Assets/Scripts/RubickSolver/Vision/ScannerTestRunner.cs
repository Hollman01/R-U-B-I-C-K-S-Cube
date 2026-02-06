using UnityEngine;

// ScannerTestRunner.cs
// Runs cube scanner self-test in Editor (no device needed)
public class ScannerTestRunner : MonoBehaviour
{
    void Start()
    {
        // Find or create CubeScanner
        CubeScanner scanner = FindObjectOfType<CubeScanner>();
        if (scanner == null)
        {
            GameObject scannerObj = new GameObject("CubeScanner");
            scanner = scannerObj.AddComponent<CubeScanner>();
        }
        
        scanner.RunSelfTest();
    }
}