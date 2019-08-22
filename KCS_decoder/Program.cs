namespace KCS_decoder
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                System.Console.WriteLine("Usage: KCS_Decoder <full filename>");
                System.Environment.Exit(-1);
            }
            WaveReader waveReader = new WaveReader();

            if (!waveReader.readWaveFileFirst20Minutes(args[0]))
            {
                System.Console.WriteLine("Could not read 'wav' file: " + args[0]);
                System.Environment.Exit(-1);
            }

            Decoder decoder = new Decoder(waveReader.sampleRate, waveReader.numChannels, waveReader.channelData);

            if (decoder.analyzeFile())
            {
                System.Console.WriteLine("session: " + decoder.getSession());
                System.Console.WriteLine("date/time: " + decoder.getDateTime());
                System.Console.WriteLine("for start time, subtract " + decoder.getStartTime() + " seconds");
            }
            else
            {
                System.Console.WriteLine("No aural sync detected in first 20 minutes of file");
            }
        }
    }
}
