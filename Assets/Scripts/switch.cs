using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnClick : MonoBehaviour
{
    public string sceneToLoad;  // Name of the scene to load

    // Add this function to the button's click event in the Unity Editor
    public void OnButtonClicked()
    {
        // Load the specified scene
        SceneManager.LoadScene(sceneToLoad);
    }
}
