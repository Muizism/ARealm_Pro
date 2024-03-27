using System.IO;
using UnityEngine;

public static class WavUtility
{
    public static byte[] FromAudioClip(AudioClip clip, out string filepath, out int length, out int samples)
    {
        filepath = string.Empty; // If you don't use the filepath, you might not need this
        length = 0;
        samples = 0;

        if (clip == null)
        {
            return null;
        }

        using (var memoryStream = new MemoryStream())
        {
            using (var writer = new BinaryWriter(memoryStream))
            {
                var hz = clip.frequency;
                var channels = clip.channels;
                var samplesCount = clip.samples;

                writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + samplesCount * 2); // Placeholder for file size
                writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
                writer.Write(new char[4] { 'f', 'm', 't', ' ' });
                writer.Write(16); // PCM chunk size
                writer.Write((ushort)1); // Format tag
                writer.Write((ushort)channels); // Channels
                writer.Write(hz); // Samples per sec
                writer.Write(hz * channels * 2); // Avg bytes per sec
                writer.Write((ushort)(channels * 2)); // Block align
                writer.Write((ushort)16); // Bits per sample
                writer.Write(new char[4] { 'd', 'a', 't', 'a' });
                writer.Write(samplesCount * channels * 2); // Data chunk size

                float[] audioSamples = new float[samplesCount];
                clip.GetData(audioSamples, 0);

                foreach (float sample in audioSamples)
                {
                    writer.Write((short)(sample * 32767));
                }

                writer.Seek(4, SeekOrigin.Begin);
                writer.Write((int)(memoryStream.Length - 8)); // Correct file size minus RIFF header

                length = (int)memoryStream.Length;
                samples = samplesCount;

                // IMPORTANT: Do not close the writer here, as it will close the underlying stream.
            }

            // Return the bytes from the MemoryStream after completing all writes
            return memoryStream.ToArray();
        }
    }
}