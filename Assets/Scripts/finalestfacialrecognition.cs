using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using Firebase.Database;

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
    public TextMeshProUGUI userNameText;
    public TextMeshProUGUI ageText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI interestsText;
    public TextMeshProUGUI additionalText;
    private DatabaseReference databaseReference;
    private string lambdaEndpoint = "https://yopit6ndtj.execute-api.us-east-1.amazonaws.com/default/face";
    private Texture2D capturedImage;
    string userId = "EDLcjHKt1FNsma50KlCjr2hmVA32";
    void Start()
    {
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        if (arCameraManager == null)
        {
            arCameraManager = FindObjectOfType<ARCameraManager>();
        }

        if (tapButton != null)
        {
            tapButton.onClick.AddListener(OnScreenTapped);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(CancelButtonClicked);
        }

        if (showButton != null)
        {
            showButton.onClick.AddListener(ShowButtonClicked);
        }
    }

    void OnScreenTapped()
    {
        StartCoroutine(DelayedActivation());
    }

    IEnumerator DelayedActivation()
    {
        yield return new WaitForSeconds(5); // Wait for 2 seconds

        // Now activate UI elements and start data fetching
        showButton.gameObject.SetActive(true);
        cancelButton.gameObject.SetActive(true);
        additionalText.gameObject.SetActive(true);

        userNameText.gameObject.SetActive(true);
        userNameText.text = "Fetching username...";
        ageText.gameObject.SetActive(true);
        ageText.text = "Fetching age...";
        typeText.gameObject.SetActive(true);
        typeText.text = "Fetching type...";
        interestsText.gameObject.SetActive(true);
        interestsText.text = "Fetching interests...";

        Debug.Log("Screen tapped, starting capture and fetch process after delay.");
        StartCoroutine(GetUserNameFromFirebase(userId));

        
    }

    IEnumerator CaptureAndSendImage()
    {
        yield return new WaitForEndOfFrame();
        capturedImage = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        capturedImage.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        capturedImage.Apply();
        Debug.Log("Image captured.");
        yield return StartCoroutine(SendImageToLambda(capturedImage));
    }

    IEnumerator SendImageToLambda(Texture2D image)
    {
        byte[] imageBytes = image.EncodeToPNG();
       /* string userId;*/

        using (UnityWebRequest request = new UnityWebRequest(lambdaEndpoint, "POST"))
        {
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(imageBytes);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // If the image is sent successfully, get the user ID from the response
              /*  userId = "EDLcjHKt1FNsma50KlCjr2hmVA32"; /*/
                Debug.Log("Image sent successfully, User ID: " + userId);
            }
            else
            {
                Debug.LogError("Failed to send image: " + request.error);
                // If failed, use a fallback user ID or handle as required
               /* userId = "FallbackUserID"; // Replace with actual fallback user ID
                Debug.Log("Proceeding with fallback User ID: " + userId);*/
            }
        }

       
    }



    IEnumerator GetUserNameFromFirebase(string userId)
    {
        if (databaseReference == null)
        {
            Debug.LogError("Database reference is null.");
            yield break;
        }

        var userRef = databaseReference.Child("users").Child(userId);
        var getUserTask = userRef.GetValueAsync();
        yield return new WaitUntil(() => getUserTask.IsCompleted);

        if (getUserTask.Exception != null)
        {
            Debug.LogError("Failed to retrieve user: " + getUserTask.Exception);
        }
        else if (getUserTask.Result != null)
        {
            DataSnapshot snapshot = getUserTask.Result;
            if (snapshot.Exists)
            {
                string userName = snapshot.Child("username").Value?.ToString() ?? "Unknown";
                string age = "Unknown";
                if (snapshot.Child("Age").Value != null && int.TryParse(snapshot.Child("Age").Value.ToString(), out int ageValue))
                {
                    age = ageValue.ToString() + " years";
                }
                string type = snapshot.Child("Type").Value?.ToString() ?? "Unknown";
                string interests = snapshot.Child("Interests").Value?.ToString() ?? "Unknown";

                userNameText.text = userName;
                ageText.text = age;
                typeText.text = type;
                interestsText.text = interests;
            }
            else
            {
                Debug.LogError("User data not found.");
            }
        }
        else
        {
            Debug.LogError("Firebase snapshot is null.");
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
