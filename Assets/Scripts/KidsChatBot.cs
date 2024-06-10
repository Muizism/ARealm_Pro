/*using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class KidsChatBot : MonoBehaviour
{
    public Button startSpeakingButton;

    // The maximum recording duration in seconds
    private float maxRecordingDuration = 10f;

    // Flag to indicate whether recording is in progress
    private bool isRecording = false;

    // Flag to indicate whether the recording has been stopped
    private bool isRecordingStopped = false;

    // Variable to store the reference to the microphone recording
    private AudioClip recordedAudioClip;
    private AudioSource audioSource;

    // Original color of the button
    private Color originalButtonColor;

    void Start()
    {
        // Save the original color of the button
        originalButtonColor = startSpeakingButton.image.color;
        // Initialize audioSource
        audioSource = GetComponent<AudioSource>();

        Debug.Log("Start method called.");
        // audioSource.PlayOneShot(congrats);
    }

    public void OnStartSpeakingClick()
    {
        // Start recording
        recordedAudioClip = Microphone.Start(null, false, Mathf.CeilToInt(maxRecordingDuration), 44100);
        isRecording = true;
        isRecordingStopped = false;

        Debug.Log("Recording started.");

        // Update button color during recording
        StartCoroutine(UpdateButtonColorDuringRecording());

        // Use a coroutine to stop recording after a specified duration
        StartCoroutine(StopRecordingAfterDuration(maxRecordingDuration));
    }

    public void OnStopSpeakingClick()
    {
        if (isRecording && !isRecordingStopped)
        {
            // Stop recording
            Microphone.End(null);
            isRecording = false;
            isRecordingStopped = true;

            Debug.Log("Recording stopped.");

            // Update button color after recording stops
            startSpeakingButton.image.color = originalButtonColor;

            // Use a coroutine to wait for the recording to complete
            StartCoroutine(WaitForRecordingComplete());
        }
        else
        {
            Debug.Log("Recording is not in progress or has already been stopped.");
        }
    }

    IEnumerator StopRecordingAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);

        // Stop recording after the specified duration
        OnStopSpeakingClick();
    }

    IEnumerator WaitForRecordingComplete()
    {
        // Wait until the recording is complete
        while (Microphone.IsRecording(null))
        {
            yield return null;
        }

        //send request to chatbot
        Debug.Log("voiceRecorded");
        StartCoroutine(UploadAud(@"C:\Users\DEEBYTE COMPUTERS\Documents\Sound Recordings\Recording.m4a"));

    }

    IEnumerator UploadAud(string audioFilePath)
    {
        WWWForm form = new WWWForm();
        byte[] audioBytes = System.IO.File.ReadAllBytes(audioFilePath);
        form.AddBinaryData("audio", audioBytes, System.IO.Path.GetFileName(audioFilePath), "audio/mpeg");

        UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:5000/process_audio", form);
        www.downloadHandler = new DownloadHandlerAudioClip(www.url, AudioType.MPEG);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
        }
        else

        {
            Debug.Log("processed");
            AudioClip receivedClip = DownloadHandlerAudioClip.GetContent(www);
            AudioSource audioSource = GetComponent<AudioSource>();
            audioSource.clip = receivedClip;
            audioSource.Play();
        }
    }

    IEnumerator UpdateButtonColorDuringRecording()
    {
        while (isRecording && !isRecordingStopped)
        {
            // Update the button color during recording (e.g., fade in)
            float lerpValue = Mathf.PingPong(Time.time, 1f); // PingPong between 0 and 1
            startSpeakingButton.image.color = Color.Lerp(originalButtonColor, Color.gray, lerpValue);

            yield return null;
        }
    }

}--------------------------------YAhan tak sai hay
*/



using System.Collections;
using TMPro;

//using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class KidsChatBot : MonoBehaviour
{
    public Button startSpeakingButton;
    private float maxRecordingDuration = 10f;
    private bool isRecording = false;
    private bool isRecordingStopped = false;
    private AudioClip recordedAudioClip;
    private AudioSource audioSource;
    private Color originalButtonColor;
    public TMP_Text text;
    public Animator animator;
   
   


    void Start()
    {
        originalButtonColor = startSpeakingButton.image.color;
        audioSource = GetComponent<AudioSource>();
      

        // Ensure audioSource is not null
        if (audioSource == null)
        {
            Debug.LogError("AudioSource reference not set!");
            return;
        }

        // Disable animator initially
    

    }
    private void Update()
    {
        if (audioSource.isPlaying && !animator.GetBool("IsPlaying"))
        {
            animator.SetBool("IsPlaying", true);
            bool hasState = animator.HasState(0, Animator.StringToHash("lipSync"));
            if (hasState)
            {
                animator.Play("Base Layer");
                Debug.Log("Playing Base Layern.");
            }
            else
            {
                Debug.LogError("The animation state 'base layer is not  is layer.");
            }
        }
    }
    public void OnStartSpeakingClick()
    {
        if (!isRecording)
        {
            recordedAudioClip = Microphone.Start(null, false, Mathf.CeilToInt(maxRecordingDuration), 44100);
            isRecording = true;
            isRecordingStopped = false;
            StartCoroutine(UpdateButtonColorDuringRecording());
            StartCoroutine(StopRecordingAfterDuration(maxRecordingDuration));
            text.text = "Recording started..";

        }
    }

    public void OnStopSpeakingClick()
    {
        if (isRecording && !isRecordingStopped)
        {
            Microphone.End(null);
            isRecording = false;
            isRecordingStopped = true;
            startSpeakingButton.image.color = originalButtonColor;
            text.text = "Recording stopped..";
            text.text = "Waiting for response..";
            StartCoroutine(WaitForRecordingToCompleteAndUpload());
        }
    }

    IEnumerator StopRecordingAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        OnStopSpeakingClick();
    }

    IEnumerator WaitForRecordingToCompleteAndUpload()
    {
        while (Microphone.IsRecording(null))
        {
            yield return null;
        }

        byte[] audioBytes;
        string filepath;
        int length, samples;

        // Convert AudioClip to WAV bytes
        audioBytes = WavUtility.FromAudioClip(recordedAudioClip, out filepath, out length, out samples);
        StartCoroutine(UploadAudio(audioBytes));
    }

    /*IEnumerator UploadAudio(byte[] audioBytes)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("audio", audioBytes, "recording.wav", "audio/wav");
        UnityWebRequest.Post("http://127.0.0.1:5000/process_audio", form);
        www.downloadHandler = new DownloadHandlerAudioClip(www.url, AudioType.WAV);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
        }
        else
        {
            Debug.Log("Audio processed successfully.");
            AudioClip receivedClip = DownloadHandlerAudioClip.GetContent(www);
            if (audioSource != null && receivedClip != null)
            {
                audioSource.clip = receivedClip;
                audioSource.Play();
            }
            else
            {
                Debug.LogError("Error playing back the received audio clip.");
            }
        }
    }*/


    IEnumerator UploadAudio(byte[] audioBytes)
    {
        // Create form and add the WAV file byte array to it
        WWWForm form = new WWWForm();
        form.AddBinaryData("audio", audioBytes, "recording.wav", "audio/wav");

        // Create a UnityWebRequest to post the form data to the server
        UnityWebRequest www = UnityWebRequest.Post("http://34.239.119.231:5000/process_audio", form);
        www.downloadHandler = new DownloadHandlerAudioClip(www.url, AudioType.MPEG);

        // Send the request and wait for the response
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
            Debug.Log("sending");
        }
        else
        {

            AudioClip receivedClip = DownloadHandlerAudioClip.GetContent(www);
            AudioSource audioSource = GetComponent<AudioSource>();
            audioSource.clip = receivedClip;
            text.text = "Hafiz Umar speaking..";
            

            Debug.Log("Animator is enabled");
            audioSource.Play();


            /*    Debug.Log("Audio processed successfully.");
                AudioClip receivedClip = DownloadHandlerAudioClip.GetContent(www);
                if (audioSource != null && receivedClip != null)
                {
                    audioSource.clip = receivedClip;
                    audioSource.Play();
                }
                else
                {
                    Debug.LogError("Error playing back the received audio clip.");
                }*/
        }
    }



    IEnumerator UpdateButtonColorDuringRecording()
    {
        while (isRecording && !isRecordingStopped)
        {
            float lerpValue = Mathf.PingPong(Time.time, 1f);
            startSpeakingButton.image.color = Color.Lerp(originalButtonColor, Color.red, lerpValue);
            yield return null;
        }

        // Reset the button color once recording is stopped
        if (!isRecording)
        {
            startSpeakingButton.image.color = originalButtonColor;
            Debug.Log("Recording");
        }
    }
}
