using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine.Networking;

public class AudioRecorder : MonoBehaviour
{
    public Button recordButton;
    private AudioClip recordedClip;
    private const int SampleRate = 44100;
    private const int ClipLength = 15;
    public TextMeshProUGUI statusText;
    private void Start()
    {
        // Add a listener to your button
        recordButton.onClick.AddListener(StartRecording);
    }

    private void StartRecording()
    {
        recordedClip = Microphone.Start(null, false, ClipLength, SampleRate);
        Invoke("StopRecording", ClipLength);
        statusText.text = "Recording audio...";
       

    }

    private void StopRecording()
    {
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
            SaveAudioClip(recordedClip);
            statusText.text = "Recording stopped...";
            StartCoroutine(WaitForRecordingToCompleteAndUpload());
        }
    }

    private void SaveAudioClip(AudioClip clip)
    {
        var filePath = Path.Combine(Application.persistentDataPath, "recordedAudio.wav");
        
        SaveWavFile(filePath, clip);
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
        audioBytes = WavUtility.FromAudioClip(recordedClip, out filepath, out length, out samples);
        StartCoroutine(UploadAudio(audioBytes));
    }
    IEnumerator UploadAudio(byte[] audioBytes)
    {

        // Create form and add the WAV file byte array to it
        WWWForm form = new WWWForm();
        form.AddBinaryData("audio", audioBytes, "recording.wav", "audio/wav");

        // Create a UnityWebRequest to post the form data to the server
        UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:5000/process_audio", form);
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
    private void SaveWavFile(string filePath, AudioClip clip)
    {
        // Create header information for WAV format
        var header = new byte[44];
        var memStream = new MemoryStream();
        var dataStream = new MemoryStream();

        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        foreach (var sample in samples)
        {
            var bytes = BitConverter.GetBytes((short)(sample * 32767));
            dataStream.Write(bytes, 0, bytes.Length);
        }

        // Fill in the WAV header
        var totalDataLen = dataStream.Length + 36;
        var audioDataLen = dataStream.Length;
        var sampleRate = clip.frequency;
        var numChannels = clip.channels;
        var byteRate = sampleRate * numChannels * 2; // 2 bytes per sample

        Buffer.BlockCopy(BitConverter.GetBytes(0x46464952), 0, header, 0, 4); // "RIFF"
        Buffer.BlockCopy(BitConverter.GetBytes(totalDataLen), 0, header, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(0x45564157), 0, header, 8, 4); // "WAVE"
        Buffer.BlockCopy(BitConverter.GetBytes(0x20746D66), 0, header, 12, 4); // "fmt "
        Buffer.BlockCopy(BitConverter.GetBytes(16), 0, header, 16, 4); // Subchunk1Size
        Buffer.BlockCopy(BitConverter.GetBytes((short)1), 0, header, 20, 2); // Audio format (PCM)
        Buffer.BlockCopy(BitConverter.GetBytes((short)numChannels), 0, header, 22, 2);
        Buffer.BlockCopy(BitConverter.GetBytes(sampleRate), 0, header, 24, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(byteRate), 0, header, 28, 4);
        Buffer.BlockCopy(BitConverter.GetBytes((short)(numChannels * 2)), 0, header, 32, 2); // Block align
        Buffer.BlockCopy(BitConverter.GetBytes((short)16), 0, header, 34, 2); // Bits per sample
        Buffer.BlockCopy(BitConverter.GetBytes(0x61746164), 0, header, 36, 4); // "data"
        Buffer.BlockCopy(BitConverter.GetBytes(audioDataLen), 0, header, 40, 4);

        // Combine header and data
        memStream.Write(header, 0, header.Length);
        dataStream.WriteTo(memStream);

        // Save to file
        File.WriteAllBytes(filePath, memStream.ToArray());
        statusText.text = "Wait for a chatbot...";
        Debug.Log("Saved audio to: " + filePath);
    }
}
