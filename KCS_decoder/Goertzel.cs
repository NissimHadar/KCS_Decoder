using System;
using System.Linq;

namespace KCS_decoder
{
    class Goertzel
    {
        public Goertzel() {
        }

        public bool Analyze(int sampleRate, float[] samples, int offset, int window_size, float f_start, float f_end)
        {
            double f_step            = (float)sampleRate / window_size;
            double f_step_normalized = 1.0 / window_size;

            int k_start = (int)Math.Floor(f_start / f_step);
            int k_end   = (int)Math.Ceiling(f_end / f_step);

            double[] results = new double[k_end - k_start + 1];
            for (int k = k_start; k < k_end; ++k)
            {
                double f = k * f_step_normalized;

                double w_real = 2 * Math.Cos(2.0 * Math.PI * f);

                double d1 = 0;
                double d2 = 0;

                for (int n = 0; n < window_size; n++)
                {
                    double y = samples[offset + n] + w_real * d1 - d2;
                    d2 = d1;
                    d1 = y;
                }

                results[k - k_start] = d2 * d2 + d1 * d1 - w_real * d1 * d2;
            }

            double maxValue = results.Max();
            return (maxValue > 1000);
        }
    }
}
