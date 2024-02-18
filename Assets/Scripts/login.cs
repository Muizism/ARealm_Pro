using System;
using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Auth;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GuardianLogin : MonoBehaviour
{
    public TMP_InputField emailInputObject;
    public TMP_InputField passwordInputObject;
  
    public GameObject loader;  // Reference to your loader GameObject

    private FirebaseAuth auth;
    private TMP_InputField emailInput;
    private TMP_InputField passwordInput;
    public TextMeshProUGUI loginMessageText;

    private async void Awake()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (loginMessageText == null)
        {
            Debug.LogError("Login message text is not assigned!");
            return;
        }
        if (dependencyStatus == DependencyStatus.Available)
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.GetAuth(app);

            // Get components from GameObjects
            emailInput = emailInputObject.GetComponentInChildren<TMP_InputField>();
            passwordInput = passwordInputObject.GetComponentInChildren<TMP_InputField>();
      

            Debug.Log("Firebase dependencies resolved successfully.");

            // Automatically log in if credentials are saved
            if (PlayerPrefs.HasKey("email") && PlayerPrefs.HasKey("password"))
            {
                emailInput.text = PlayerPrefs.GetString("email");
                passwordInput.text = PlayerPrefs.GetString("password");
                Debug.Log("Auto-login with saved credentials.");
                OnLoginButtonClicked();
            }
        }
        else
        {
            Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
        }
    }

    public async void OnLoginButtonClicked()
    {
        // Activate the loader
        loader.SetActive(true);

        string email = emailInput.text;
        string password = passwordInput.text;
        Debug.Log("Email:" + email);
        Debug.Log("Password:" + password);

        try
        {
            await auth.SignInWithEmailAndPasswordAsync(email, password);
            Debug.Log("Logged in successfully!");
            loginMessageText.text = "Logged in successfully!";
            PlayerPrefs.SetInt("IsLoggedIn", 1);
            PlayerPrefs.Save();
            loginMessageText.color = Color.green;
            SceneManager.LoadScene("HomeScreen");
            // Note: Remember Me functionality removed

            // If you want to switch scenes after successful login, you can uncomment the following line
            // SceneManager.LoadScene("YourMainMenuSceneName");
        }
        catch (Exception e)
        {
            Debug.LogError("Login failed: " + e.Message);
            loginMessageText.text = "Failed to login!";
            loginMessageText.color = Color.red;
        }
        finally
        {
            // Deactivate the loader, whether login succeeds or fails
            loader.SetActive(false);
        }
    }

    public void OnSignUpButtonClicked()
    {
        // Navigate to the sign-up scene
        SceneManager.LoadScene("YourSignUpSceneName");
    }

    public void OnSignOutButtonClicked()
    {
        // Sign out and clear saved credentials
        auth.SignOut();
        PlayerPrefs.DeleteKey("email");
        PlayerPrefs.DeleteKey("password");

        Debug.Log("Signed out and cleared saved credentials.");
    }

    // Other functions...
}
