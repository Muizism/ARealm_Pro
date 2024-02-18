using System;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using Firebase.Extensions;

using UnityEngine.SceneManagement;

public class UserDataFetcher : MonoBehaviour
{
    public TMP_Text userNameText; // For displaying user's name
    public Image userProfileImage; // For displaying user's profile picture
    public TMP_Text gender;
    public TMP_Text type;
    public Button logoutButton;

    private FirebaseAuth auth;
    private DatabaseReference databaseReference;

    private async void Awake()
    {
        Debug.Log("Firebase Dependency Check Starting...");
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            Debug.Log("Firebase Dependencies Available");
            FirebaseApp app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            FetchUserData();
        }
        else
        {
            Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
        }

        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(OnLogout);
        }
        else
        {
            Debug.LogError("Logout button reference not set in the inspector.");
        }
    }

    private void FetchUserData()
    {
        Debug.Log("Fetching User Data...");
        FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            Debug.Log("User is not null, UID: " + user.UserId);
            string userId = user.UserId;
            databaseReference.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Error accessing database: " + task.Exception);
                }
                else if (task.IsCompleted)
                {
                    Debug.Log("Database task completed successfully");
                    DataSnapshot snapshot = task.Result;
                    if (snapshot.Exists)
                    {
                        string imageUrl = snapshot.Child("imageUrl").Value?.ToString() ?? "Image URL not found";

                        Debug.Log($"Name: {name}, Image URL: {imageUrl}");

                        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(imageUrl))
                        {
                            userNameText.text = name;
                            StartCoroutine(LoadProfilePicture(imageUrl));
                        }
                        else
                        {
                            Debug.LogError("Name or Image URL is missing in the snapshot.");
                        }

                        Debug.Log("Snapshot exists. Raw snapshot data: " + snapshot.GetRawJsonValue());
                        if (snapshot.HasChild("Gender") && snapshot.Child("Gender").Value != null)
                        {
                            string userGender = snapshot.Child("Gender").Value.ToString();
                            Debug.Log($"Gender found: {userGender}");
                            gender.text = userGender;
                        }
                        else
                        {
                            Debug.LogError("Gender key not found in snapshot.");
                        }

                        if (snapshot.HasChild("Type") && snapshot.Child("Type").Value != null)
                        {
                            string userType = snapshot.Child("Type").Value.ToString();
                            Debug.Log($"Type found: {userType}");
                            // Assign the userType to the TMP_Text field
                            type.text = userType;
                        }
                        else
                        {
                            Debug.LogError("type key not found in snapshot.");
                        }

                        if (snapshot.HasChild("username") && snapshot.Child("username").Value != null)
                        {
                            string name = snapshot.Child("username").Value.ToString();
                            Debug.Log($"Username found: {name}");
                            userNameText.text = name;

                        }
                        else
                        {
                            Debug.LogError("Username key not found in snapshot.");
                        }


                    }
                    else
                    {
                        Debug.LogError("Snapshot does not exist for user: " + userId);
                    }
                }
            });
        }
        else
        {
            Debug.LogError("No authenticated user found.");
        }
    }
    public void OnLogout()
    {
        Debug.Log("Logging out user...");

        if (auth != null)
        {
            auth.SignOut(); // Sign out from Firebase Auth
        }

        PlayerPrefs.SetInt("IsLoggedIn", 0); // Reset the flag
        PlayerPrefs.Save();

        // Load the login screen (replace "LoginSceneName" with your actual login scene name)
        SceneManager.LoadScene("login");
    }

    private IEnumerator LoadProfilePicture(string imageUrl)
    {
        Debug.Log("Loading profile picture from URL: " + imageUrl);
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.LogError("Error loading image: " + webRequest.error);
            }
            else
            {
                Debug.Log("Image loaded successfully");
                Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
                userProfileImage.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
    }
}
