using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
public class ImageUploader : MonoBehaviour
{
    public void StartImageUpload(byte[] imageBytes, string userId)
    {
        StartCoroutine(UploadProfilePicture(imageBytes, userId));
    }

    private IEnumerator UploadProfilePicture(byte[] imageBytes, string userId)
    {
        Debug.Log("Starting image upload...");

        if (imageBytes == null || imageBytes.Length == 0)
        {
            Debug.LogError("Image bytes are empty.");
            yield break;
        }

        string filename = "uploaded_image.png"; // You can modify this to a more suitable filename
        string apiEndpoint = $"https://qkwqrr0387.execute-api.us-east-1.amazonaws.com/arealm/arealm/{userId},{filename}";
        Debug.Log("API endpoint: " + apiEndpoint);

        using (UnityWebRequest request = new UnityWebRequest(apiEndpoint, "PUT"))
        {
            request.uploadHandler = new UploadHandlerRaw(imageBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "image/png");

            Debug.Log("Sending UnityWebRequest...");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Image upload failed: " + request.error);
            }
            else
            {
                string responseUrl = request.downloadHandler.text;
                Debug.Log("Image uploaded successfully. Image URL: " + responseUrl);
            }
        }
    }
}
