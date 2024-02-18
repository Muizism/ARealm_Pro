using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Namespace for TextMeshPro

public class SceneLoader : MonoBehaviour
{
  
    public string loginScene = "login";

    public float delayInSeconds = 4f;
 

    void Start()
    {
     
        Invoke("LoadNextScene", delayInSeconds);
    }

    void LoadNextScene()
    {
        
            SceneManager.LoadScene(loginScene);
        
    }

   
}
