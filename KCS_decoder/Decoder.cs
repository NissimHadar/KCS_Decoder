using System;
namespace KCS_decoder
{
    /// <summary>
    /// A bit is 1/300 seconds long
    /// 1 is (384 cycles of) 4800Hz
    /// 0 is (192 cycles of) 2400Hz
    /// 
    /// Therefore a bit is 80 ms long.  This is equivalent to 3840 samples
    /// 
    /// Message starts with a 1" header of '1's and ends with a 1" header of '0's
    /// 
    /// Each character is encoded in 11 bits:
    ///     Start bit   '0'
    ///     Data        7 bits ASCII code
    ///     Parity      1 bit
    ///     Stop bits   '1' and '1'
    ///     
    ///     Last character is Carriage Return
    ///     
    ///     An empty string will be returned if no message found
    /// 
    /// </summary>
    class Decoder
    {
        private int sampleRate;
        private int numChannels;
        private float[][] samples;

        private Goertzel goertzel;

        private char INVALID_CHAR = '\0';

        private int currentOffset = 0; // A pointer into the samples
        private string message;
        private double startTime;

        public Decoder(int sampleRate, int numChannels, float[][] samples)
        {
            this.sampleRate = sampleRate;
            this.numChannels = numChannels;
            this.samples = samples;

            goertzel = new Goertzel();
        }

        internal bool analyzeFile()
        {
            if (!findEndOfHeader())
            {
                return false;
            }

            message = "";
            char nextChar = getNextChar();
            while (nextChar != INVALID_CHAR)
            {
                message += nextChar;
                nextChar = getNextChar();
            }

            return true;
        }

        private char getNextChar()
        {
            // Bits are 80 ms long
            // The data will be sampled in 10 ms chunks
            // A bit will be detected when 6 out of 8 values are either 1 or 0
            // This will help with small amounts of offset furthur into the signal.
            int startBit = detectBit();
            if (startBit != 0)
            {
                return INVALID_CHAR;
            }

            byte val = 0;
            for (int j = 0; j < 7; ++j)
            {
                int bit = detectBit();
                val += (byte)(bit << j);
            }

            bool parity = (detectBit() == 1) ? true : false;
            if (parityOf(val) != parity)
            {
                return INVALID_CHAR;
            }

            int stopBit_0 = detectBit();

            int stopBit_1 = detectBit();
            if (stopBit_0 != 1 || stopBit_1 != 1)
            {
                return INVALID_CHAR;
            }

            return (char)val;
        }

        private bool parityOf(byte val)
        {
            bool parityIsOdd = false;
            byte mask = 0x01;
            for (int i = 0; i < 7; ++i)
            {
                bool bit = ((val & mask) == mask) ? true : false;
                mask <<= 1;
                parityIsOdd = (bit) ? !parityIsOdd : parityIsOdd;
            }

            return parityIsOdd;
        }

        // Returns the sample that the header starts at.
        // Will return -1 if the header is too short
        private bool findEndOfHeader()
        {
            // Find first bit
            int result = detectBit();
            while (result == -1)
            {
                result = detectBit();
            }

            if (result == -2)
            {
                // No header found
                return false;
            }
            // Find first start bit
            while (detectBit() == 1)
            {
            }

            // Go back 1 bit
            currentOffset -= (int)(0.080 * sampleRate);
            startTime = (double)currentOffset / sampleRate;

            return true;
        }

        // Returns the value of the bit at the current offset
        // A bit is detected when at least 60 ms of all '1's or all '0's have been detected
        // -1 is returned if no bit is found
        // -2 is returned if no more data
        int detectBit()
        {
            int num_0 = 0;
            int num_1 = 0;

            int increment   = (int)(0.010 * sampleRate);
            int windowWidth = (int)(0.060 * sampleRate);

            if (currentOffset >= samples[0].Length - windowWidth)
            {
                return -2;
            }

            for (int i = 0; i < 8 && currentOffset < samples[0].Length - windowWidth; ++i, currentOffset += increment)
            {
                double t = (double)currentOffset / sampleRate;
                if (goertzel.Analyze(sampleRate, samples[0], currentOffset, windowWidth, 2300, 2500))
                {
                    num_0++;
                }
                if (goertzel.Analyze(sampleRate, samples[0], currentOffset, windowWidth, 4700, 4900))
                {
                    num_1++;
                }
            }

            if (num_0 >= 6)
            {
                return 0;
            }
            else if (num_1 >= 6)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }

        internal string getSession()
        {
            string[] tokens = message.Split('@');
            if (tokens.Length == 2)
            {
                return tokens[0];
            }
            else
            {
                return "Session not found in message: " + message;
            }
        }

        internal string getDateTime()
        {
            string[] tokens = message.Split('@');
            if (tokens.Length == 2)
            {
                // Remove new line from end
                return tokens[1].Split('\n')[0];
            }
            else
            {
                return "Date and time not found in message: " + message;
            }
        }

        internal double getStartTime()
        {
            return startTime;
        }
    }
}
