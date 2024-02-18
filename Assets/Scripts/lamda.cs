using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Collections;

public class LambdaScript : MonoBehaviour
{
    // Lambda endpoint URL
    private string lambdaEndpoint = "https://yopit6ndtj.execute-api.us-east-1.amazonaws.com/default/face";

    // Image path
    public string imagePath = "your_image_path_here"; // Provide the path to your image file

    private bool isError = false;

    void Start()
    {
        StartCoroutine(SendImageToLambda());
    }

    IEnumerator SendImageToLambda()
    {
        byte[] imageBytes = File.ReadAllBytes(imagePath);


        // Create a UnityWebRequest
        using (UnityWebRequest request = new UnityWebRequest(lambdaEndpoint, "POST"))
        {
            // Set the upload handler
            request.uploadHandler = new UploadHandlerRaw(imageBytes);
     

            // Set the download handler
            request.downloadHandler = new DownloadHandlerBuffer();

            // Send the request
            yield return request.SendWebRequest();

            // Check if the request was successful
            if (request.result == UnityWebRequest.Result.Success)
            {
                // Read and display the Lambda response
                string lambdaResponse = request.downloadHandler.text;
                Debug.Log("Lambda Response: " + lambdaResponse);
            }
            else
            {
                isError = true;
                Debug.LogError("Failed to send image to Lambda. StatusCode: " + request.responseCode);
            }
        }

        if (isError)
        {
            Debug.LogError("Error occurred during image upload.");
            // Handle error as needed
        }
    }
}
