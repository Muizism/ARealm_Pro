using UnityEngine;
using UnityEngine.UI;

public class WebCamScript : MonoBehaviour
{
    private WebCamTexture webCamTexture;

    void Start()
    {
        // Check if any webcam is available
        if (WebCamTexture.devices.Length > 0)
        {
            Debug.Log("Camera device found");

            // Use the first available webcam
            webCamTexture = new WebCamTexture();

            // Optionally, you can select a specific camera by name:
            // webCamTexture = new WebCamTexture(WebCamTexture.devices[0].name);

            // Start the webcam
            webCamTexture.Play();

            // If you have a RawImage in your canvas to display the webcam feed:
            // RawImage rawImage = GetComponent<RawImage>();
            // rawImage.texture = webCamTexture;
        }
        else
        {
            Debug.LogError("No camera device found");
        }
    }
}
