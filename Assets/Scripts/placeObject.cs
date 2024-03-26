using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;
using UnityEngine. XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(requiredComponent: typeof(ARRaycastManager),
requiredComponent2: typeof(ARPlaneManager))]
public class placeObject : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;
    private ARRaycastManager aRRaycastManager;
    private ARPlaneManager aRPLaneManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private void Awake()
    {
        aRRaycastManager = GetComponent<ARRaycastManager>();
        aRPLaneManager = GetComponent<ARPlaneManager>();
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

    private void FingerDown(EnhancedTouch.Finger finger)
    {
        if (finger.index != 0) return;
        if (aRRaycastManager.Raycast(screenPoint: finger.currentTouch.
        screenPosition, hitResults: hits, trackableTypes: TrackableType.
        PlaneWithinPolygon)) ;
        foreach (ARRaycastHit hit in hits)
        {
            Pose pose = hit.pose;
            GameObject obj = Instantiate(original: prefab, position: pose.
            position, rotation: pose.rotation);
            if (aRPLaneManager.GetPlane(trackableId: hit.trackableId).alignment == PlaneAlignment.HorizontalUp)
                
            {
                Vector3 position = obj.transform.position;
                Vector3 cameraPosition = Camera.main.transform.position;
                Vector3 direction = cameraPosition - position;
                Vector3 targetRotationEuler = Quaternion.LookRotation
                (forward: direction).eulerAngles;
                Vector3 scaledEuler = Vector3.Scale(a: targetRotationEuler,
                b: obj.transform.up.normalized); // (0, 1, 0)
                Quaternion targetRotation = Quaternion.Euler
                (euler: scaledEuler);
                obj.transform.rotation = obj.transform.rotation
                * targetRotation;
            }

        }
     
    }
}

