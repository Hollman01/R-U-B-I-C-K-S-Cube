using System.Collections;
using Convai.Scripts;
using UnityEngine;

/// <summary>
/// Defines the UI positioning modes for XR interactions.
/// </summary>
public enum UIPositioningMode
{
    PlayerFacing,  // UI positioned relative to the NPC and player interaction
    CameraFront    // UI positioned in front of the camera at a fixed distance
}

/// <summary>
/// Handles the UI transformation for XR interactions, adjusting the UI position based on the player's camera distance from an NPC.
/// Also uses raycasting to determine the NPC the player is looking at.
/// </summary>
public class XRNPCUIPositionHandler : MonoBehaviour
{
    [Header("UI Positioning")]
    [SerializeField] private UIPositioningMode _positioningMode = UIPositioningMode.PlayerFacing;
    
    [Header("Player Facing Settings")]
    [SerializeField] private float _lerpSpeed;
    [SerializeField] private Vector3 _offset;
    [SerializeField] private float _cameraDistanceThreshold;
    [SerializeField] private float _raycastMaxDistance = 10f;
    
    [Header("Camera Front Settings")]
    [SerializeField] private float _cameraFrontDistance = 1f;
    [SerializeField] private Vector3 _cameraLeftOffset = new Vector3(-0.5f, 0f, 0f);
    [SerializeField] private bool _forStartOnly = false;
    [SerializeField] private float _forStartOnlyDurationSeconds = 5f;
    private Camera _playerCamera;
    private ConvaiNPC _currentNPC;
    private float _forStartOnlyEndTime;
    private bool _forStartOnlyInitialized;

    private void OnEnable()
    {
        //if (_currentNPC == null)
        //{
        //    _currentNPC = FindAnyObjectByType<ConvaiNPC>();
        //}
    }

    private void Start()
    {
        _playerCamera = Camera.main;
        if (_forStartOnly)
        {
            _forStartOnlyEndTime = Time.time + _forStartOnlyDurationSeconds;
            _forStartOnlyInitialized = true;
        }
    }

    private void LateUpdate()
    {
        if (_forStartOnly)
        {
            if (!_forStartOnlyInitialized)
            {
                _forStartOnlyEndTime = Time.time + _forStartOnlyDurationSeconds;
                _forStartOnlyInitialized = true;
            }
            if (Time.time > _forStartOnlyEndTime)
            {
                return;
            }
        }
        if (_positioningMode == UIPositioningMode.PlayerFacing)
        {
            RaycastForNPC();

            if (_currentNPC != null)
            {
                UpdateUIPosition();
                FaceCamera();
            }
        }
        else if (_positioningMode == UIPositioningMode.CameraFront)
        {
            UpdateCameraFrontPosition();
            FaceCamera();
        }
    }

    /// <summary>
    /// Performs a raycast from the center of the camera to find an NPC.
    /// If a new NPC is hit, updates the reference.
    /// </summary>
    private void RaycastForNPC()
    {
        if (_playerCamera == null)
            return;

        Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, _raycastMaxDistance))
        {
            ConvaiNPC foundNPC = hit.transform.GetComponent<ConvaiNPC>();
            if (foundNPC != null && foundNPC != _currentNPC)
            {
                _currentNPC = foundNPC;
                SetUIPosition(); // Snap UI instantly to new NPC
            }
        }
    }

    private void SetUIPosition()
    {
        if (_currentNPC == null)
            return;

        Transform npcTransform = _currentNPC.transform;
        Vector3 targetPosition = CalculateTargetPosition(npcTransform);
        transform.position = targetPosition;
    }

    private void UpdateUIPosition()
    {
        if (_currentNPC == null)
            return;

        Transform npcTransform = _currentNPC.transform;
        Vector3 targetPosition = CalculateTargetPosition(npcTransform);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * _lerpSpeed);
    }

    /// <summary>
    /// Updates UI position for camera front mode, positioning it to the left of the camera at a fixed distance.
    /// </summary>
    private void UpdateCameraFrontPosition()
    {
        if (_playerCamera == null)
            return;

        Vector3 targetPosition = CalculateCameraFrontPosition();
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * _lerpSpeed);
    }

    private Vector3 CalculateTargetPosition(Transform npcTransform)
    {
        Vector3 leftOffset = new Vector3(-_offset.x, _offset.y, _offset.z);
        Vector3 rightOffset = new Vector3(_offset.x, _offset.y, _offset.z);

        Vector3 leftOffsetPosition = npcTransform.position + npcTransform.TransformDirection(leftOffset);
        Vector3 rightOffsetPosition = npcTransform.position + npcTransform.TransformDirection(rightOffset);

        float distanceToLeftOffset = Vector3.Distance(leftOffsetPosition, _playerCamera.transform.position);
        float distanceToRightOffset = Vector3.Distance(rightOffsetPosition, _playerCamera.transform.position);

        Vector3 dynamicOffset = DetermineDynamicOffset(distanceToLeftOffset, distanceToRightOffset);

        return npcTransform.position + npcTransform.TransformDirection(dynamicOffset);
    }

    /// <summary>
    /// Calculates the target position for camera front mode, positioning the UI to the left of the camera.
    /// </summary>
    private Vector3 CalculateCameraFrontPosition()
    {
        if (_playerCamera == null)
            return transform.position;

        // Get camera's forward direction
        Vector3 cameraForward = _playerCamera.transform.forward;
        Vector3 cameraRight = _playerCamera.transform.right;

        // Position UI in front of camera at specified distance
        Vector3 frontPosition = _playerCamera.transform.position + cameraForward * _cameraFrontDistance;

        // Apply left offset relative to camera's orientation
        Vector3 leftPosition = frontPosition + cameraRight * _cameraLeftOffset.x;
        leftPosition += _playerCamera.transform.up * _cameraLeftOffset.y;
        leftPosition += cameraForward * _cameraLeftOffset.z;

        return leftPosition;
    }

    private Vector3 DetermineDynamicOffset(float distanceToLeftOffset, float distanceToRightOffset)
    {
        Vector3 leftOffset = new Vector3(-_offset.x, _offset.y, _offset.z);
        Vector3 rightOffset = new Vector3(_offset.x, _offset.y, _offset.z);

        float threshold = 0.5f;

        if (distanceToLeftOffset < _cameraDistanceThreshold && distanceToRightOffset < _cameraDistanceThreshold)
        {
            float difference = Mathf.Abs(distanceToLeftOffset - distanceToRightOffset);
            return difference > threshold
                ? (distanceToLeftOffset > distanceToRightOffset ? leftOffset : rightOffset)
                : leftOffset;
        }
        else
        {
            return distanceToLeftOffset >= _cameraDistanceThreshold ? leftOffset : rightOffset;
        }
    }

    private void FaceCamera()
    {
        Vector3 direction = transform.position - _playerCamera.transform.position;
        transform.rotation = Quaternion.LookRotation(direction);
    }

   
}
