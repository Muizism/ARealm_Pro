using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelAnimation : MonoBehaviour
{
    public Animator animator; // Reference to the Animator component
    public AudioSource audioSource; // Reference to the AudioSource component

    // Start is called before the first frame update
    void Start()
    {
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

    // Update is called once per frame
    void Update()
    {
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
