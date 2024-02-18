using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Networking;
using System.Collections;
using TMPro;  // Import for TextMeshPro
using System.Linq;

using Firebase.Database;

public class CaptureAndSendImageScript01 : MonoBehaviour
{
    public ARCameraManager arCameraManager;
    public Button tapButton;
    public TextMeshProUGUI debugText;          // Debug Text using TextMeshPro
    public TextMeshProUGUI captureDebugText;   // Capture Debug Text using TextMeshPro
    public TextMeshProUGUI sendDebugText;      // Send Debug Text using TextMeshPro
    public TextMeshProUGUI responseDebugText;  // Response Debug Text using TextMeshPro
    public TextMeshProUGUI userNameText; // Add this for displaying the user's name
    private DatabaseReference databaseReference;
    private string lambdaEndpoint = "https://yopit6ndtj.execute-api.us-east-1.amazonaws.com/default/face";
    private Texture2D capturedImage;

    void Start()
    {
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference; 
        if (arCameraManager == null)
        {
            arCameraManager = FindObjectOfType<ARCameraManager>();
            UpdateDebugText("ARCameraManager not set. Finding in the scene...", debugText);
        }
        else
        {
            UpdateDebugText("ARCameraManager is set.", debugText);
        }

        if (tapButton != null)
        {
            tapButton.onClick.AddListener(OnScreenTapped);
            UpdateDebugText("Tap button listener added.", debugText);
        }
        else
        {
            UpdateDebugText("Error: Tap button not set.", debugText);
        }

        // Initializing text fields for debugging
        InitializeDebugText(captureDebugText, "Capture Debug Initialized");
        InitializeDebugText(sendDebugText, "Send Debug Initialized");
        InitializeDebugText(responseDebugText, "Response Debug Initialized");
    }

    void OnScreenTapped()
    {
        UpdateDebugText("Screen tapped!", debugText);
        StartCoroutine(CaptureAndSendImage());
    }

    IEnumerator CaptureAndSendImage()
    {
        UpdateDebugText("Capturing image...", captureDebugText);
        yield return new WaitForEndOfFrame();

        capturedImage = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        capturedImage.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        capturedImage.Apply();
        UpdateDebugText("Image captured!", captureDebugText);

        yield return StartCoroutine(SendImageToLambda(capturedImage));
    }

    IEnumerator SendImageToLambda(Texture2D image)
    {
        UpdateDebugText("Preparing to send image to Lambda...", sendDebugText);
        byte[] imageBytes = image.EncodeToPNG();

        using (UnityWebRequest request = new UnityWebRequest(lambdaEndpoint, "POST"))
        {
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(imageBytes);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();


            UpdateDebugText("Sending image to Lambda...", sendDebugText);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                /*string userId = request.downloadHandler.text*/;
                string userId = "10YJyEexiTRV9vke1X9daclhoEJ2";
                UpdateDebugText("Lambda Response with UserID: " + userId, responseDebugText);

                // Retrieve and display user's name
                StartCoroutine(GetUserNameFromFirebase(userId));
                string lambdaResponse = request.downloadHandler.text;
                UpdateDebugText("Lambda Response: " + lambdaResponse, responseDebugText);
            }
            else
            {
                // Detailed error logging
                string errorDetails = $"Failed to send image to Lambda. " +
                                      $"StatusCode: {request.responseCode}, " +
                                      $"Error: {request.error}";
                if (request.GetResponseHeaders() != null)
                {
                    errorDetails += $", Headers: {string.Join(", ", request.GetResponseHeaders().Select(kvp => $"{kvp.Key}: {kvp.Value}"))}";
                }
                UpdateDebugText(errorDetails, responseDebugText);
            }
        }
    }
    IEnumerator GetUserNameFromFirebase(string userId)
    {
        var userRef = databaseReference.Child("users").Child(userId);
        var getUserTask = userRef.GetValueAsync();
        yield return new WaitUntil(() => getUserTask.IsCompleted);

        if (getUserTask.Exception != null)
        {
            Debug.LogError("Failed to retrieve user: " + getUserTask.Exception);
        }
        else
        {
            DataSnapshot snapshot = getUserTask.Result;
            string userName = snapshot.Child("username").Value.ToString();
            UpdateDebugText("User Name: " + userName, userNameText);
        }
    }
    void UpdateDebugText(string message, TextMeshProUGUI targetText)  // Corrected parameter type
    {
        if (targetText != null)
        {
            targetText.text += message + "\n";
        }
    }

    void InitializeDebugText(TextMeshProUGUI debugTextField, string message)  // Corrected parameter type
    {
        if (debugTextField != null)
        {
            debugTextField.text = message + "\n";
        }
        else
        {
            UpdateDebugText("Error: Debug text field not set.", debugText);
        }
    }
}
