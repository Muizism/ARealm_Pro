using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class detectFaces : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI username;
    [SerializeField] private TextMeshProUGUI interests;
    [SerializeField] private TextMeshProUGUI age;
    [SerializeField] private TextMeshProUGUI type;
    [SerializeField] private GameObject data;
    [SerializeField] private GameObject cancelButton;
    [SerializeField] public Text error;


    private string awsLambdaEndpoint = "https://yopit6ndtj.execute-api.us-east-1.amazonaws.com/default/face";
    private void Start()
    {
        // Ensure that the cancel button is active at the start
        cancelButton.SetActive(true);
    }

    void Update()
    {
        // Check if there is any touch input
        if (Input.touchCount > 0)
        {
            // Get the first touch
            Touch touch = Input.GetTouch(0);

            // Check if the touch phase is began (finger just touched the screen)
            if (touch.phase == TouchPhase.Began)
            {
                // Capture the screen when the touch begins
                Debug.Log("Screen tapped, capturing screen...");
                StartCoroutine(CaptureAndSendScreenshot());
                data.SetActive(true);
            }
        }
    }
    public void OnCancelButtonClick()
    {
        // Deactivate the data object and the cancel button itself
        data.SetActive(false);
        cancelButton.SetActive(false);
    }
    private IEnumerator CaptureAndSendScreenshot()
    {
        yield return new WaitForEndOfFrame();

        Camera camera = Camera.main;
        int width = Screen.width;
        int height = Screen.height;
        RenderTexture rt = new RenderTexture(width, height, 24);
        camera.targetTexture = rt;

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = rt;

        camera.Render();

        Texture2D image = new Texture2D(width, height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply();

        camera.targetTexture = null;
        RenderTexture.active = currentRT;

        byte[] imageBytes = image.EncodeToPNG();
        Destroy(image);

        Debug.Log("Image captured and encoded to PNG format.");
        Debug.Log("Sending image to AWS Lambda...");
        yield return StartCoroutine(SendImageToAWSLambda(imageBytes));
    }

    private IEnumerator SendImageToAWSLambda(byte[] imageBytes)
    {
        using (UnityWebRequest www = new UnityWebRequest(awsLambdaEndpoint, "POST"))
        {
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(imageBytes);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/octet-stream");

            Debug.Log("Sending image data...");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string responseJson = www.downloadHandler.text;
                Debug.Log("Image sent successfully!");
                Debug.Log($"Response from AWS Lambda: {responseJson}");

                // Deserialize the response JSON
                Schedule schedule = JsonUtility.FromJson<Schedule>(responseJson);
                StartCoroutine(UpdateUIWithDelay(schedule, 5.0f));
                // Update UI elements with response data
                username.text = schedule.Username;
                age.text = schedule.Age;
                type.text = schedule.Type;
                interests.text = string.Join(", ", schedule.Interests);
                error.text = responseJson;
                Debug.Log(responseJson);

                Debug.Log("UI updated with response data!");
            }
            else if (www.responseCode == 404)
            {
                Debug.Log("No face found.");
            }
            else
            {
                Debug.LogError($"Error sending image to AWS Lambda: {www.error}");
            }
        }
    }
    private IEnumerator UpdateUIWithDelay(Schedule schedule, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Update UI elements with response data
        username.text = schedule.Username;
        age.text = schedule.Age;
        type.text = schedule.Type;
        interests.text = string.Join(", ", schedule.Interests);
        error.text = JsonUtility.ToJson(schedule, true);

        Debug.Log("UI updated with response data after delay!");
    }
    [Serializable]
    public class ImagePayload
    {
        public bool isBase64Encoded;
        public string body;
    }

    [Serializable]
    public class Schedule
    {
        public string Username;
        public string Age;
        public string Type;
        public List<string> Interests;
    }
}
