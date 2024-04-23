using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class dummmmy : MonoBehaviour
{
    public TextMeshProUGUI loginMessageText;
    public GameObject loader;

    public void OnLoginButtonClicked()
    {
        // Activate the loader
        loader.SetActive(true);

        // Delay for 3 seconds before loading the HomeScreen scene
        StartCoroutine(LoadHomeScreenAfterDelay(4));
    }

    // Coroutine to load HomeScreen after delay
    private IEnumerator LoadHomeScreenAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        loginMessageText.text = "Logged in succesfully";
        // Load the HomeScreen scene
        SceneManager.LoadScene("HomeScreen");
    }

    // Other methods...
}
