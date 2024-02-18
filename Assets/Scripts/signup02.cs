using System;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using TMPro;
using System.Collections;
using System.Threading.Tasks;
using Firebase.Extensions;

public class SignUpAndUpload02 : MonoBehaviour
{
    public TMP_InputField NameInput;
    public TMP_InputField EmailInput;
    public TMP_InputField PasswordInput;
    public TMP_InputField ConfirmPasswordInput;
    public Button UploadImageButton;
    public Button SignUpButton;
    public Text signUpMessageText;
    public Text uploadMsg;

    private FirebaseAuth auth;
    private DatabaseReference databaseReference;
    private byte[] imageBytes;

    private void Awake()
    {
        InitializeFirebase();
        UploadImageButton.onClick.AddListener(RequestCameraPermission);
        SignUpButton.onClick.AddListener(OnSignUpButtonClicked);
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                Debug.Log("Firebase initialized successfully.");
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
            }
        });
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

    private IEnumerator WaitForPermission()
    {
        while (true)
        {
            NativeCamera.Permission permission = NativeCamera.CheckPermission(true);
            if (permission == NativeCamera.Permission.Granted)
            {
                Debug.Log("Permission granted after waiting, opening camera...");
                OpenCamera();
                yield break;
            }
            else if (permission == NativeCamera.Permission.Denied)
            {
                Debug.LogError("Camera permission denied after waiting.");
                yield break;
            }

            yield return null;
        }
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

            byte[] fileData = System.IO.File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(fileData)) // Automatically resizes the texture
            {
                imageBytes = texture.EncodeToJPG();
                Destroy(texture);

                uploadMsg.text = "Image captured!";
                Debug.Log("Image ready for upload.");
            }
            else
            {
                Debug.LogError("Failed to load texture from " + path);
            }
        }, maxSize: -1);
    }


    private async Task<string> UploadProfilePictureAsync(byte[] imageBytes)
    {
        if (imageBytes == null || imageBytes.Length == 0)
        {
            Debug.LogError("No image selected or imageBytes is empty");
            return null;
        }

        FirebaseStorage storage = FirebaseStorage.DefaultInstance;
        StorageReference storageRef = storage.GetReferenceFromUrl("gs://arealm-2dc27.appspot.com");
        StorageReference imageRef = storageRef.Child("userImages").Child(Guid.NewGuid().ToString());

        Debug.Log("Uploading image to Firebase...");
        var uploadTask = imageRef.PutBytesAsync(imageBytes);
        await uploadTask;

        if (uploadTask.Exception != null)
        {
            Debug.LogError("Image upload failed: " + uploadTask.Exception);
            return null;
        }

        return (await imageRef.GetDownloadUrlAsync()).ToString();
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

            string imageUrl = await UploadProfilePictureAsync(imageBytes);
            if (string.IsNullOrEmpty(imageUrl))
            {
                Debug.LogError("Failed to upload image or get its URL.");
                return;
            }

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

    private void WriteNewUser(string userId, string name, string email, string imageUrl)
    {
        User user = new User(name, email, imageUrl);
        string json = JsonUtility.ToJson(user);

        databaseReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWith(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("Error writing to the database: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                Debug.Log("Data written successfully to the database.");
            }
        });
    }

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
