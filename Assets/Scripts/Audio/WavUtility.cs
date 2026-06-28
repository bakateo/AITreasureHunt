using System;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    public static byte[] FromAudioClip(AudioClip clip)
    {
        if (clip == null)
        {
            return Array.Empty<byte>();
        }

        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        byte[] pcmData = ConvertToPcm16(samples);
        return AddWavHeader(pcmData, clip.frequency, clip.channels);
    }

    private static byte[] ConvertToPcm16(float[] samples)
    {
        byte[] pcmData = new byte[samples.Length * 2];

        for (int i = 0; i < samples.Length; i++)
        {
            float clampedSample = Mathf.Clamp(samples[i], -1f, 1f);
            short intSample = (short)(clampedSample * short.MaxValue);

            byte[] bytes = BitConverter.GetBytes(intSample);
            pcmData[i * 2] = bytes[0];
            pcmData[i * 2 + 1] = bytes[1];
        }

        return pcmData;
    }

    private static byte[] AddWavHeader(byte[] pcmData, int sampleRate, int channels)
    {
        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            int byteRate = sampleRate * channels * 2;
            int blockAlign = channels * 2;
            int dataSize = pcmData.Length;

            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataSize);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write((short)blockAlign);
            writer.Write((short)16);

            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(dataSize);
            writer.Write(pcmData);

            return stream.ToArray();
        }
    }
}