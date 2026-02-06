using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;

// CameraCapture.cs
// Minimal passthrough frame detection for Quest 3
// Works ONLY on-device (not in Editor Play mode)
[RequireComponent(typeof(ARCameraManager))]
public class CameraCapture : MonoBehaviour
{
    private ARCameraManager arCameraManager;
    private int frameCounter = 0;

    void Start()
    {
        arCameraManager = GetComponent<ARCameraManager>();
        
        if (arCameraManager == null)
        {
            Debug.LogError("[CameraCapture] Missing ARCameraManager. Attach to Main Camera.");
            enabled = false;
            return;
        }

        Debug.Log("[CameraCapture] Ready. Frames work ONLY after Android build to Quest 3.");
    }

    void Update()
    {
        // Editor warning only
        if (Application.isEditor)
        {
            if (Time.frameCount % 120 == 0)
                Debug.Log("[CameraCapture] ⚠️ Passthrough works ONLY on Quest 3 device after build.");
            return;
        }

        // Acquire latest CPU-accessible camera image
        if (arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            // Simply log that we received a frame + its size
            frameCounter++;
            if (frameCounter % 30 == 0)
            {
                Debug.Log($"[CameraCapture] 📷 Frame {frameCounter}: {image.width}x{image.height} format={image.format}");
            }
            
            image.Dispose(); // Critical: release native resources
        }
    }
}