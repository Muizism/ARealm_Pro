using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI; // Required for permission checks on Android

public class ModelAnimation : MonoBehaviour
{
    public Animator animator; // Reference to the Animator component
    public AudioSource audioSource; // Reference to the AudioSource component


    private void AskForPermissions()
    {
        // Permissions required by the app
        string[] permissions = new string[]
        {
            Permission.Microphone,
            Permission.Camera,
            Permission.ExternalStorageWrite,
            Permission.ExternalStorageRead,
           
            // Add other permissions here
            // Example: Permission.ExternalStorageWrite
        };

        foreach (var permission in permissions)
        {
            if (!Permission.HasUserAuthorizedPermission(permission))
            {
                // This will prompt the user for permission
                Permission.RequestUserPermission(permission);
            }
        }
    }

    void Start()
    {
        AskForPermissions(); // Ask for all required permissions at start

        // Ensure animator is not null
        if (animator == null)
        {
            Debug.LogError("Animator reference not set!");
           
            return;
        }

        // Ensure audioSource is not null
        if (audioSource == null)
        {
            Debug.LogError("AudioSource reference not set!");
          
            return;
        }

        // Disable animator initially
        animator.enabled = false;
    }

    void Update()
    {
        // Debugging to ensure the AudioSource is working as expected
        Debug.Log($"AudioSource isPlaying: {audioSource.isPlaying}");
       

        // Check if the audio source is playing
        if (audioSource.isPlaying)
        {
            // If it is playing, enable the animator
            animator.enabled = true;
            // Trigger animations based on conditions (e.g., if audio is speaking)
            animator.SetTrigger("lipSync");
            animator.SetTrigger("standTalk");
        }
        else
        {
            // If the audio source is not playing, disable the animator
            animator.enabled = false;
        }
    }
}
