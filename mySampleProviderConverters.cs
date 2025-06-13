using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LooPPlaySample
{
    public static class SampleProviderConverters
    {
        //
        // 요약:
        //     Helper function to go from IWaveProvider to a SampleProvider Must already be
        //     PCM or IEEE float
        //
        // 매개 변수:
        //   waveProvider:
        //     The WaveProvider to convert
        //
        // 반환 값:
        //     A sample provider
        public static ISampleProvider ConvertWaveProviderIntoSampleProvider(IWaveProvider waveProvider)
        {
            if (waveProvider.WaveFormat.Encoding == WaveFormatEncoding.Pcm)
            {
                if (waveProvider.WaveFormat.BitsPerSample == 8)
                {
                    return new Pcm8BitToSampleProvider(waveProvider);
                }

                if (waveProvider.WaveFormat.BitsPerSample == 16)
                {
                    return new Pcm16BitToSampleProvider(waveProvider);
                }

                if (waveProvider.WaveFormat.BitsPerSample == 24)
                {
                    return new Pcm24BitToSampleProvider(waveProvider);
                }

                if (waveProvider.WaveFormat.BitsPerSample == 32)
                {
                    return new Pcm32BitToSampleProvider(waveProvider);
                }

                throw new InvalidOperationException("Unsupported bit depth");
            }

            if (waveProvider.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
            {
                if (waveProvider.WaveFormat.BitsPerSample == 64)
                {
                    return new WaveToSampleProvider64(waveProvider);
                }

                return new WaveToSampleProvider(waveProvider);
            }

            throw new ArgumentException("Unsupported source encoding");
        }
    }
}
