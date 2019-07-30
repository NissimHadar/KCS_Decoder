using System;
using System.IO;
using System.Text;

namespace KCS_decoder
{
    internal class WaveReader
    {
        public int sampleRate { get; set; }

        public short numChannels { get; set; }

        // Data for each channel
        public float[][] channelData { get; set; }

        // Container for wave file data
        private byte[] wav;

        public bool readWaveFile(string filename)
        {
            wav = File.ReadAllBytes(filename);

            // Check if file is RIFF (Resource container
            string chunkID = Encoding.UTF8.GetString(wav, 0, 4);
            if (chunkID != "RIFF")
            {
                return false;
            }

            // Check if file is WAVE
            string format = Encoding.UTF8.GetString(wav, 8, 4);
            if (format != "WAVE")
            {
                return false;
            }

            // Get parameters from format chunk
            int offset = findChunk("fmt ", 12);

            short audioFormat   = BitConverter.ToInt16(wav, offset +  8);
            numChannels         = BitConverter.ToInt16(wav, offset + 10);
            sampleRate          = BitConverter.ToInt32(wav, offset + 12);
            int   byteRate      = BitConverter.ToInt32(wav, offset + 16);
            short blockAlign    = BitConverter.ToInt16(wav, offset + 20);
            short bitsPerSample = BitConverter.ToInt16(wav, offset + 22);

            offset = findChunk("data", offset + 24);

            int dataSize       = BitConverter.ToInt32(wav, offset + 4);
            int bytesPerSample = bitsPerSample / 8;
            int numSamples     = dataSize / bytesPerSample;

            // Copy data to the channels
            channelData = new float[numChannels][];
            int numSamplesPerChannel = numSamples / numChannels;
            for (int i = 0; i < numChannels; ++i)
            {
                channelData[i] = new float[numSamplesPerChannel];

                // Copy actual data, converting from int to float(-1.0 .. 1.0)
                for (int j = 0; j < numSamplesPerChannel; ++j)
                {
                    // Note that we have to be careful with the interleaving
                    int valueOffset = offset + 8 + j * bytesPerSample * numChannels + i * bytesPerSample;
                    int value = BitConverter.ToInt16(wav, valueOffset);
                    channelData[i][j] = (float)value / short.MaxValue;
                }
            }

            return true;
        }

        private int findChunk(string chunkName, int offset)
        {
            string nextChunkName = Encoding.UTF8.GetString(wav, offset, 4);

            while (nextChunkName != chunkName)
            {
                int chunkLength = BitConverter.ToInt32(wav, offset + 4);
                offset += (8 + chunkLength);
                nextChunkName = Encoding.UTF8.GetString(wav, offset, 4);
            }

            return offset;
        }
    }
}