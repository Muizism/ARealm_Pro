using UnityEngine;
using UnityEngine.UI; // Import this namespace to work with UI elements

public class ModelTriggerBehaviour : MonoBehaviour
{
    public Animator animator; // Reference to the Animator component
    public Button myButton; // Public reference to the Button
    private bool animatorEnabled = false; // Flag to track the animator's state

    void Start()
    {
        // Ensure the animator starts off disabled
        if (animator != null)
        {
            animator.enabled = false;
        }
        else
        {
            Debug.LogError("Animator reference not set in the inspector.");
        }

        // Add a listener to the Button to call ToggleAnimator() when clicked
        if (myButton != null)
        {
            myButton.onClick.AddListener(ToggleAnimator);
        }
        else
        {
            Debug.LogError("Button reference not set in the inspector.");
        }
    }

    // This method will be called when the button is clicked
    public void ToggleAnimator()
    {
        if (animator != null)
        {
            // Toggle the flag
            animatorEnabled = !animatorEnabled;

            // Set the animator's enabled state based on the flag
            animator.enabled = animatorEnabled;

            // Optionally, log the current state
            Debug.Log($"Animator enabled: {animator.enabled}");
        }
    }
}
