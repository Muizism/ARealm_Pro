using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Extensions;
using Firebase.Auth;
using System.Collections;
using TMPro;
using UnityEngine.Networking;
using System.IO;

public class UserProfileManager01 : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField ageInput;
    public TMP_InputField typeInput;
    public TMP_InputField genderInput;
    public TMP_InputField interestsInput;
    public RawImage profileImage;
    public GameObject loader;
    public TMP_Text statusText;
    public Button uploadImageButton;

    private DatabaseReference databaseReference;
    private FirebaseStorage storage;
    private Texture2D capturedImage;

    void Start()
    {
        InitializeFirebase();
        uploadImageButton.onClick.AddListener(RequestCameraPermission);
    }

    private void RequestCameraPermission()
    {
        NativeCamera.Permission permission = NativeCamera.CheckPermission(true);

        if (permission == NativeCamera.Permission.Denied)
        {
            Debug.Log("Camera permission denied, requesting permission...");
            NativeCamera.RequestPermission(true);
        }
        else if (permission == NativeCamera.Permission.Granted)
        {
            Debug.Log("Camera permission granted, opening camera...");
            OpenCamera();
        }
        else
        {
            Debug.Log("Camera permission undecided, waiting for permission...");
            StartCoroutine(WaitForPermission());
        }
    }

    void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Failed to initialize Firebase with {task.Exception}");
                return;
            }
            FirebaseApp app = FirebaseApp.DefaultInstance;
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            storage = FirebaseStorage.DefaultInstance;
            if (FirebaseAuth.DefaultInstance.CurrentUser == null)
            {
                Debug.LogError("User not logged in!");
                return;
            }
            LoadUserData(FirebaseAuth.DefaultInstance.CurrentUser.UserId);
        });
    }

    private IEnumerator WaitForPermission()
    {
        float timeout = 10.0f; // 10 seconds timeout
        float startTime = Time.time;

        while (Time.time - startTime < timeout)
        {
            NativeCamera.Permission permission = NativeCamera.CheckPermission(true);
            if (permission == NativeCamera.Permission.Granted)
            {
                Debug.Log("Permission granted, opening camera...");
                OpenCamera();
                yield break;
            }
            else if (permission == NativeCamera.Permission.Denied)
            {
                Debug.LogError("Camera permission denied.");
                yield break;
            }

            yield return null;
        }

        Debug.LogError("Camera permission request timed out.");
    }

    private void OpenCamera()
    {
        NativeCamera.TakePicture((path) =>
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("Failed to capture image or image capture was cancelled.");
                return;
            }

            Debug.Log($"Image captured at path: {path}");

            byte[] fileData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(fileData))
            {
                capturedImage = texture;
                profileImage.texture = capturedImage; // Assign the texture to the RawImage component
                Debug.Log("Image ready for upload.");

                // Call method to upload image to Firebase (if needed right after capture)
                string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
                UpdateUserData(); // Assuming you want to upload immediately after capture
            }
            else
            {
                Debug.LogError("Failed to load texture from " + path);
            }
        }, maxSize: -1);
    }

    void LoadUserData(string userId)
    {
        DatabaseReference userRef = databaseReference.Child("users").Child(userId);
        userRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                DataSnapshot snapshot = task.Result;
                UserData userData = JsonUtility.FromJson<UserData>(snapshot.GetRawJsonValue());
                usernameInput.text = userData.Username;
                ageInput.text = userData.Age.ToString();
                genderInput.text = userData.Gender;
                interestsInput.text = userData.Interests;
                LoadProfileImage(userData.ImageUrl);
            }
            else
            {
                Debug.LogError("Failed to load user data: " + task.Exception.Message);
            }
        });
    }

    IEnumerator CaptureImage()
    {
        yield return new WaitForEndOfFrame();

        int width = Screen.width;
        int height = Screen.height;
        capturedImage = new Texture2D(width, height, TextureFormat.RGB24, false);

        capturedImage.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        capturedImage.Apply();
        profileImage.texture = capturedImage;
    }

    public void UpdateUserData()
    {
        loader.SetActive(true);

        string username = usernameInput.text;
        int age = int.TryParse(ageInput.text, out int result) ? result : 0;
        string gender = genderInput.text;
        string type = typeInput.text;
        string interests = interestsInput.text;

        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        UploadImageToStorageAndUserData(userId, username, age, gender, type, interests);
    }

    void UploadImageToStorageAndUserData(string userId, string username, int age, string gender, string type, string interests)
    {
        if (capturedImage == null)
        {
            Debug.LogError("No image captured to upload.");
            loader.SetActive(false);
            return;
        }

        string imageName = "profile_image_" + userId + ".png";
        byte[] imageBytes = capturedImage.EncodeToPNG();
        StorageReference storageRef = storage.GetReference("profile_images").Child(imageName);

        storageRef.PutBytesAsync(imageBytes).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                storageRef.GetDownloadUrlAsync().ContinueWithOnMainThread(downloadUrlTask =>
                {
                    if (downloadUrlTask.IsCompleted && !downloadUrlTask.IsFaulted)
                    {
                        string imageUrl = downloadUrlTask.Result.ToString();
                        DatabaseReference userRef = databaseReference.Child("users").Child(userId);
                        userRef.Child("username").SetValueAsync(username);
                        userRef.Child("Age").SetValueAsync(age);
                        userRef.Child("Gender").SetValueAsync(gender);
                        userRef.Child("Type").SetValueAsync(type);
                        userRef.Child("Interests").SetValueAsync(interests);
                        userRef.Child("imageUrl").SetValueAsync(imageUrl).ContinueWithOnMainThread(updateTask =>
                        {
                            loader.SetActive(false);
                            if (updateTask.IsCompleted && !updateTask.IsFaulted)
                            {
                                statusText.text = "Data updated successfully";
                            }
                            else
                            {
                                Debug.LogError("Failed to update data: " + updateTask.Exception.Message);
                                statusText.text = "Failed to update data";
                            }
                        });
                    }
                    else
                    {
                        Debug.LogError("Failed to get download URL: " + downloadUrlTask.Exception.Message);
                        loader.SetActive(false);
                        statusText.text = "Failed to update data";
                    }
                });
            }
            else
            {
                Debug.LogError("Failed to upload image: " + task.Exception.Message);
                loader.SetActive(false);
                statusText.text = "Failed to update data";
            }
        });
    }

    void LoadProfileImage(string imageUrl)
    {
        StartCoroutine(DownloadImage(imageUrl));
    }

    private IEnumerator DownloadImage(string imageUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download image: " + request.error);
        }
        else
        {
            profileImage.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }
    }

    int GetDropdownIndex(TMP_Dropdown dropdown, string value)
    {
        return dropdown.options.FindIndex(option => option.text == value);
    }

    [System.Serializable]
    public class UserData
    {
        public string Username;
        public int Age;
        public string Gender;
        public string Type;
        public string Interests;
        public string ImageUrl;
    }
}
