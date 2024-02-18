using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Extensions;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using SimpleFileBrowser;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using TMPro;

public class UserSignUp : MonoBehaviour
{
    public TMP_InputField NameInput;
    public TMP_InputField EmailInput;
    public TMP_InputField PasswordInput;
    public TMP_InputField ConfirmPasswordInput;
    public Button UploadImageButton;

    public Text signUpMessageText;
    public Text uploadMsg;


    private FirebaseAuth auth;
    private DatabaseReference databaseReference;
    private byte[] imageBytes;

    private async void Awake()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.GetAuth(app);
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        }
        else
        {
            Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
        }

        // Set up the file browser
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".jpg", ".png"));
        FileBrowser.SetDefaultFilter(".jpg");
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");

        // Add listener to Upload Image Button
        UploadImageButton.onClick.AddListener(() => StartCoroutine(ShowLoadDialogCoroutine()));
       
    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Load Image", "Load");
        if (FileBrowser.Success)
        {
            string imagePath = FileBrowser.Result[0];
            imageBytes = FileBrowserHelpers.ReadBytesFromFile(imagePath);
            uploadMsg.text = "Uploaded successfully!";
        }
    }

    public async void OnSignUpButtonClicked()
    {
        string name = NameInput.text;
        string email = EmailInput.text;
        string password = PasswordInput.text;
        string confirmPassword = ConfirmPasswordInput.text;

        if (password != confirmPassword)
        {
            Debug.LogError("Passwords do not match.");
            return;
        }

        try
        {
            AuthResult authResult = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            FirebaseUser newUser = authResult.User;
            string userId = newUser.UserId;

            string imageUrl = await UploadProfilePicture(imageBytes);

            WriteNewUser(userId, name, email, imageUrl);

            Debug.Log("Sign-up successful! User ID: " + userId);
            signUpMessageText.text = "Profile created successfully!";
            signUpMessageText.color = Color.green;
        }
        catch (Exception e)
        {
            Debug.LogError("Sign-up failed: " + e.Message);
            signUpMessageText.text = "Sign-up failed: " + e.Message;
            signUpMessageText.color = Color.red;
        }
    }
 

    private async Task<string> UploadProfilePicture(byte[] imageBytes)
    {
        if (imageBytes == null)
        {
            Debug.LogError("No image selected");
            return null;
        }

        FirebaseStorage storage = FirebaseStorage.DefaultInstance;
        StorageReference storageRef = storage.GetReferenceFromUrl("gs://arealm-2dc27.appspot.com");
        StorageReference imageRef = storageRef.Child("userImages").Child(Guid.NewGuid().ToString());

        var uploadTask = imageRef.PutBytesAsync(imageBytes);
        await uploadTask;


        if (uploadTask.Exception != null)
        {
            Debug.LogError("Image upload failed: " + uploadTask.Exception);
            return null;

        }

        return (await imageRef.GetDownloadUrlAsync()).ToString();
    }

    private void WriteNewUser(string userId, string name, string email, string imageUrl)
    {
        User user = new User(name, email, imageUrl);
        string json = JsonUtility.ToJson(user);

        databaseReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWith(task => {
            if (task.IsFaulted)
            {
                // Handle the error...
                Debug.LogError("Error writing to the database: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                Debug.Log("Data written successfully to the database.");
            }
        });

    }

    // Adjusted User class to include imageUrl
    public class User
    {
        public string username;
        public string email;
        public string imageUrl;

        public User() { }

        public User(string username, string email, string imageUrl)
        {
            this.username = username;
            this.email = email;
            this.imageUrl = imageUrl;
        }
    }
}
