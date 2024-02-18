using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Extensions;
using Firebase.Auth;
using System;
using System.Collections;
using System.IO;
using TMPro;

public class UserProfileManager : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField ageInput;
    public TMP_Dropdown genderDropdown;
    public TMP_Dropdown typeDropdown;
    public TMP_InputField interestsInput;
    public RawImage profileImage;
    public GameObject loader;
    public TMP_Text statusText;
    public Button uploadbtn;

    private DatabaseReference databaseReference;
    private FirebaseStorage storage;
    private Texture2D capturedImage;

    void Start()
    {
        InitializeFirebase();
        uploadbtn.onClick.AddListener(OnCaptureImageButtonClicked);
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
                genderDropdown.value = GetDropdownIndex(genderDropdown, userData.Gender);
                typeDropdown.value = GetDropdownIndex(typeDropdown, userData.Type);
                interestsInput.text = userData.Interests;
                LoadProfileImage(userData.ImageUrl);
            }
            else
            {
                Debug.LogError("Failed to load user data: " + task.Exception.Message);
            }
        });
    }
    public void OnCaptureImageButtonClicked()
    {
        StartCoroutine(CaptureImage());
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
        string gender = genderDropdown.options.Count > genderDropdown.value ? genderDropdown.options[genderDropdown.value].text : "";
        string type = typeDropdown.options.Count > typeDropdown.value ? typeDropdown.options[typeDropdown.value].text : "";
        string interests = interestsInput.text;

        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        UploadImageToStorageAndUserData(userId, username, age, gender, type, interests);
    }

    void UploadImageToStorageAndUserData(string userId, string username, int age, string gender, string type, string interests)
    {
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


    void UpdateImageUrlInDatabase(string imageUrl)
    {
        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        DatabaseReference userRef = databaseReference.Child("users").Child(userId).Child("ImageUrl");
        userRef.SetValueAsync(imageUrl).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log("Image URL updated successfully");
            }
            else
            {
                Debug.LogError("Failed to update image URL: " + task.Exception.Message);
            }
        });
    }


    void LoadProfileImage(string imageUrl)
    {
        profileImage.texture = Resources.Load<Texture>("placeholder_image");
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