using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
public class placewall : MonoBehaviour
{
    [SerializeField]
    private Canvas canvasPrefab; // Reference to your Canvas prefab
    private ARRaycastManager arRaycastManager;
    private ARPlaneManager arPlaneManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool hasPlacedCanvas = false;

    private void Awake()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
        arPlaneManager = GetComponent<ARPlaneManager>();
        canvasPrefab.gameObject.SetActive(false); // Ensure the Canvas prefab is initially inactive
    }

    private void OnEnable()
    {
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();
        EnhancedTouch.Touch.onFingerDown += FingerDown;
    }
    private void OnDisable()
    {
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();
        EnhancedTouch.Touch.onFingerDown -= FingerDown;
    }


    private void FingerDown(Finger finger)
    {
        if (finger.index != 0 || hasPlacedCanvas) return;

        Vector2 touchPosition = finger.screenPosition;
        if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.Planes))
        {
            foreach (var hit in hits)
            {
                // Check if the detected plane is vertical
                if (arPlaneManager.GetPlane(hit.trackableId).alignment == PlaneAlignment.Vertical)
                {
                    Pose placementPose = hit.pose;

                    // Instead of instantiating, we enable and position the Canvas
                    canvasPrefab.gameObject.SetActive(true);
                    canvasPrefab.transform.position = placementPose.position;
                    canvasPrefab.transform.rotation = placementPose.rotation;

                    // Optionally adjust the Canvas to face the camera directly
                    if (Camera.main != null)
                    {
                        Vector3 cameraPosition = Camera.main.transform.position;
                        cameraPosition.y = canvasPrefab.transform.position.y; // Optional: Adjust if you want the canvas to tilt slightly upward/downward
                        canvasPrefab.transform.LookAt(cameraPosition);
                        canvasPrefab.transform.Rotate(0, 180f, 0); // Canvas should rotate to face the opposite direction
                    }

                    hasPlacedCanvas = true; // Prevents multiple canvases from being placed
                    break;
                }
            }
        }
    }
}
