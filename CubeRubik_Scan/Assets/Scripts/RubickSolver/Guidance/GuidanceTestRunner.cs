using UnityEngine;

// GuidanceTestRunner.cs
public class GuidanceTestRunner : MonoBehaviour
{
    void Start()
    {
        // Find or create MoveGuidance
        MoveGuidance guidance = FindObjectOfType<MoveGuidance>();
        if (guidance == null)
        {
            GameObject guidanceObj = new GameObject("MoveGuidance");
            guidance = guidanceObj.AddComponent<MoveGuidance>();
        }
        
        guidance.RunSelfTest();
    }
}