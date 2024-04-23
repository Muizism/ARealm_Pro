using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class FaceDetectionController : MonoBehaviour
{
    public GameObject arObjectPrefab;
    private ARFaceManager arFaceManager;
    private GameObject instantiatedObject;

    void Start()
    {
        arFaceManager = GetComponent<ARFaceManager>();
        arFaceManager.facesChanged += OnFacesChanged;
    }

    void OnFacesChanged(ARFacesChangedEventArgs eventArgs)
    {
        // Check if there are any faces detected
        if (eventArgs.added.Count > 0)
        {
            // If there are faces detected, instantiate the AR object above the first detected face
            ARFace firstDetectedFace = eventArgs.added[0];
            Vector3 headPosition = firstDetectedFace.transform.position;
            Vector3 offset = new Vector3(0f, 0.2f, 0f); // Offset to position the object above the head
            Vector3 objectPosition = headPosition + offset;

            // Instantiate AR object at the calculated position
            if (instantiatedObject == null)
            {
                instantiatedObject = Instantiate(arObjectPrefab, objectPosition, Quaternion.identity);
            }
        }
        else
        {
            // If no faces detected, destroy the instantiated AR object
            Destroy(instantiatedObject);
            instantiatedObject = null;
        }
    }
}
