using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using System.Collections;
using TMPro;
using System.Net.Http;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine.Networking;
using System;

public class CaptureAndSendImageScript02 : MonoBehaviour
{
    public ARCameraManager arCameraManager;
    public Button tapButton;
    public Button cancelButton;
    public Button showButton;
    public TextMeshProUGUI debugText;
    public TextMeshProUGUI captureDebugText;
    public TextMeshProUGUI sendDebugText;
    public TextMeshProUGUI responseDebugText;
    public TextMeshProUGUI responseDebug01Text;
    public TextMeshProUGUI userNameText;
    public TextMeshProUGUI ageText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI interestsText;
    public TextMeshProUGUI additionalText;

    private DatabaseReference databaseReference;
    private string lambdaEndpoint = "https://yopit6ndtj.execute-api.us-east-1.amazonaws.com/default/face";
    private string lambdaEndpointForTextDetection = "https://nwt9wmn64g.execute-api.us-east-1.amazonaws.com/default/ImageRecognition";
    private Texture2D capturedImage;
    string userId = "EDLcjHKt1FNsma50KlCjr2hmVA32";

    void Start()
    {
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        arCameraManager = FindObjectOfType<ARCameraManager>();
        tapButton.onClick.AddListener(OnScreenTapped);
        cancelButton.onClick.AddListener(CancelButtonClicked);
        showButton.onClick.AddListener(ShowButtonClicked);
    }

    void OnScreenTapped()
    {
        StartCoroutine(CaptureAndSendImage());
    }

    IEnumerator CaptureAndSendImage()
    {
        yield return new WaitForEndOfFrame();
        capturedImage = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        capturedImage.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        capturedImage.Apply();
        captureDebugText.text = "Image captured.";
        Debug.Log("Image captured.");
      /*  yield return StartCoroutine(SendImageToLambdaForTextDetection(capturedImage));*/
        yield return StartCoroutine(sending_face(capturedImage));
    }

   private IEnumerator sending_face(Texture2D image)
    {
        byte[] imageBytes = image.GetRawTextureData();
        UnityWebRequest request = new UnityWebRequest(lambdaEndpoint, "POST")
        {
            uploadHandler = new UploadHandlerRaw(imageBytes),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/octet-stream");
        sendDebugText.text = "Sending image...";
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            responseDebugText.text = "Response: " + request.downloadHandler.text;
            Debug.Log("Lambda response: " + request.downloadHandler.text);
        }
        else
        {
            responseDebugText.text = "Failed to send image: " + request.error;
            Debug.LogError("Failed to send image: " + request.error);
        }
    }

   

    void CancelButtonClicked()
    {
        cancelButton.gameObject.SetActive(false);
        showButton.gameObject.SetActive(false);
        additionalText.gameObject.SetActive(false);
        userNameText.gameObject.SetActive(false);
        ageText.gameObject.SetActive(false);
        typeText.gameObject.SetActive(false);
        interestsText.gameObject.SetActive(false);
        Debug.Log("Cancel button clicked, UI elements hidden.");
    }

    void ShowButtonClicked()
    {
        Debug.Log("Show button clicked.");
    }
}
