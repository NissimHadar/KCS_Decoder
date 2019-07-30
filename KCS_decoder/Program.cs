namespace KCS_decoder
{
    class Program
    {
        static void Main(string[] args)
        {
            WaveReader waveReader = new WaveReader();

            if (!waveReader.readWaveFile(@"C:\KCS\ZOOM0014.wav"))
            {
                System.Environment.Exit(-1);
            }

            Decoder decoder = new Decoder(waveReader.sampleRate, waveReader.numChannels, waveReader.channelData);

            string s = decoder.getMessage();
            int fds = 45;
        }
    }
}
