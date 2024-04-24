using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARCanvasManager : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public GameObject canvasPrefab;

    private GameObject canvasInstance;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Touch touch = Input.GetTouch(0);
            if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
            {
                foreach (var hit in hits)
                {
                    Vector3 hitPoint = hit.pose.position;
                    Vector3 planeNormal = hit.trackable.transform.up;

                    if (IsVerticalPlane(planeNormal))
                    {
                        Pose pose = hit.pose;
                        ShowCanvas(pose.position, pose.rotation);
                        break;
                    }
                }
            }
        }
    }

    bool IsVerticalPlane(Vector3 planeNormal)
    {
        return Vector3.Dot(planeNormal, Vector3.up) < Mathf.Cos(30 * Mathf.Deg2Rad);
    }

    void ShowCanvas(Vector3 position, Quaternion rotation)
    {
        if (canvasInstance == null)
        {
            canvasInstance = Instantiate(canvasPrefab, position, rotation);
            // You can add additional customization here, like setting the parent or adjusting scale.
        }
        else
        {
            canvasInstance.transform.position = position;
            canvasInstance.transform.rotation = rotation;
            canvasInstance.SetActive(true);
        }
    }
}
