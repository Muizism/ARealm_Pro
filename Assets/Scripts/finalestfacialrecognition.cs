using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class finalfacialrecognition : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomText;
    [SerializeField] private TextMeshProUGUI time830_950Text;
    [SerializeField] private GameObject scheduleCanvas;
    [SerializeField] private GameObject icon;






    private string awsLambdaEndpoint = "https://nwt9wmn64g.execute-api.us-east-1.amazonaws.com/default/ImageRecognition";
  /*  string filePath = "C:\\Users\\Abdul Moiz\\Downloads\\check.jpg";*/


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
                /* StartCoroutine(SendImageFromFile());*/

                // You can also perform other actions here if needed
                scheduleCanvas.SetActive(true);
                /* icon.SetActive(true);*/
            }
        }
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

 /*   private IEnumerator SendImageFromFile()
    {
        Debug.Log("Reading image from file...");
        // Ensure the filePath is correct and accessible
        if (File.Exists(filePath))
        {
            byte[] imageBytes = File.ReadAllBytes(filePath);
            Debug.Log("Image read from file successfully.");

            // Now send this imageBytes array to AWS Lambda
            Debug.Log("Sending image to AWS Lambda...");
            yield return StartCoroutine(SendImageToAWSLambda(imageBytes));
        }
        else
        {
            Debug.LogError($"File not found at path: {filePath}");
        }
    }*/
    public void DisplayScheduleOnCanvas(Schedule schedule)
    {
        roomText.text = $"Room: {schedule.Room}";

        foreach (var timeSlot in schedule.TimeSlots)
        {
            switch (timeSlot.Key)
            {
                case "08:30-09:50":
                    time830_950Text.text = timeSlot.Value;
                    break;



            }

        }
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
                var schedule = JsonUtility.FromJson<Schedule>(www.downloadHandler.text);
                string jsonResponse = www.downloadHandler.text.Trim('{', '}');
                string[] jsonParts = jsonResponse.Split(',');
                string formattedResponse = string.Join("\n\n", jsonParts);
                time830_950Text.text = formattedResponse;
                DisplayScheduleOnCanvas(schedule);
                Debug.Log("Image sent successfully!");
                Debug.Log($"Response from AWS Lambda: {www.downloadHandler.text}");
            }
            else if (www.responseCode == 404)
            {
                Debug.Log("No room information found.");
                roomText.text = ("No room information found");
            }
            else
            {
                Debug.LogError($"Error sending image to AWS Lambda: {www.error}");
            }
        }

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
        public string Room;
        public Dictionary<string, string> TimeSlots = new Dictionary<string, string>();
    }
}



