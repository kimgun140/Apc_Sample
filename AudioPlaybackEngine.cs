using LooPPlaySample;
using NAudio.Dmo;
using NAudio.Utils;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apc_Sample
{
    class AudioPlaybackEngine : IDisposable
    {
        private readonly IWavePlayer outputDevice;
        public readonly MixingSampleProvider mixer;
        public readonly myMixingSampleProvider myMixer;



        public WaveFormat WaveFormat { get; private set; }

        public AudioPlaybackEngine(int sampleRate = 44100, int channelCount = 2)
        {
            //sources = new List<ISampleProvider>();
            outputDevice = new WaveOutEvent();

            //mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
            //mixer.ReadFully = true;
            //outputDevice.Init(mixer);
            //outputDevice.Play();


            myMixer = new myMixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
            myMixer.ReadFully = true;
            outputDevice.Init(myMixer);
            outputDevice.Play();



        }
        public void RemoveMixerInput(ISampleProvider mixerInput)
        {
            mixer.RemoveMixerInput(mixerInput);
        }

        private void AddMixerInput(ISampleProvider input)
        {
            myMixer.AddMixerInput(ConvertToRightChannelCount(input));
        }
        //public void myAddMixerInput(ISampleProvider mixerInput)
        //{
        //    //mixer.AddMixerInput(ConvertToRightChannelCount(mixerInput));
        //    lock (sources)
        //    {
        //        if (sources.Count >= 1024)
        //        {
        //            throw new InvalidOperationException("Too many mixer inputs");
        //        }

        //        sources.Add(mixerInput);
        //        // 암튼 ISampleProvider를 넣어 
        //    }

        //    if (WaveFormat == null)
        //    {
        //        WaveFormat = mixerInput.WaveFormat;
        //    }
        //    else if (WaveFormat.SampleRate != mixerInput.WaveFormat.SampleRate || WaveFormat.Channels != mixerInput.WaveFormat.Channels)
        //    {
        //        throw new ArgumentException("All mixer inputs must have the same WaveFormat");
        //    }

        //}
        public void PlaySound(string fileName)
        {
            var input = new AudioFileReader(fileName);
            AddMixerInput(new AutoDisposeFileReader(input));
        }

        public ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
        {
            if (input.WaveFormat.Channels == mixer.WaveFormat.Channels)
            {
                return input;
            }
            if (input.WaveFormat.Channels == 1 && mixer.WaveFormat.Channels == 2)
            {
                return new MonoToStereoSampleProvider(input);
            }
            throw new NotImplementedException("Not yet implemented this channel count conversion");
        }

        public void PlaySound(CachedSound sound)
        {
            AddMixerInput(new CachedSoundSampleProvider(sound));
        }



        public void Dispose()
        {
            outputDevice.Dispose();
        }
        //public event EventHandler<SampleProviderEventArgs> MixerInputEnded;
        //public bool ReadFully { get; set; }
        //public int Read(float[] buffer, int offset, int count)
        //{
        //    int outputSamples = 0;
        //    sourceBuffer = BufferHelpers.Ensure(sourceBuffer, count);
        //    lock (sources)
        //    {
        //        int index = sources.Count - 1;
        //        while (index >= 0)
        //        {
        //            var source = sources[index];
        //            int samplesRead = source.Read(sourceBuffer, 0, count);
        //            int outIndex = offset;
        //            for (int n = 0; n < samplesRead; n++)
        //            {
        //                if (n >= outputSamples)
        //                {
        //                    buffer[outIndex++] = sourceBuffer[n];
        //                }
        //                else
        //                {
        //                    buffer[outIndex++] += sourceBuffer[n];
        //                }
        //            }
        //            outputSamples = Math.Max(samplesRead, outputSamples);
        //            if (samplesRead < count)
        //            {
        //                MixerInputEnded?.Invoke(this, new SampleProviderEventArgs(source));
        //                sources.RemoveAt(index);
        //            }
        //            index--;
        //        }
        //    }
        //    // optionally ensure we return a full buffer
        //    if (ReadFully && outputSamples < count)
        //    {
        //        int outputIndex = offset + outputSamples;
        //        while (outputIndex < offset + count)
        //        {
        //            buffer[outputIndex++] = 0;
        //        }
        //        outputSamples = count;
        //    }
        //    return outputSamples;
        //}


        public static readonly AudioPlaybackEngine Instance = new AudioPlaybackEngine(44100, 2);
    }
    class CachedSound
    {
        public float[] AudioData { get; private set; }
        public WaveFormat WaveFormat { get; private set; }
        public CachedSound(string audioFileName)
        {
            using (var audioFileReader = new AudioFileReader(audioFileName))
            {
                // TODO: could add resampling in here if required
                WaveFormat = audioFileReader.WaveFormat;
                var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
                var readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
                int samplesRead;
                while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    wholeFile.AddRange(readBuffer.Take(samplesRead));
                }
                AudioData = wholeFile.ToArray();
            }
        }
    }
    //class CachedSound
    //{
    //    public byte[] AudioData { get; private set; }
    //    public WaveFormat WaveFormat { get; private set; }
    //    public CachedSound(string audioFileName)
    //    {
    //        using (var audioFileReader = new AudioFileReader(audioFileName))
    //        {
    //            // TODO: could add resampling in here if required
    //            WaveFormat = audioFileReader.WaveFormat;
    //            var wholeFile = new List<byte>((int)(audioFileReader.Length ));
    //            var readBuffer = new byte[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
    //            int samplesRead;
    //            while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
    //            {
    //                wholeFile.AddRange(readBuffer.Take(samplesRead));
    //            }
    //            AudioData = wholeFile.ToArray();
    //        }
    //    }
    //}

    class CachedSoundSampleProvider : ISampleProvider
    {
        private readonly CachedSound cachedSound;
        private long position;

        public CachedSoundSampleProvider(CachedSound cachedSound)
        {
            this.cachedSound = cachedSound;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var availableSamples = cachedSound.AudioData.Length - position;
            var samplesToCopy = Math.Min(availableSamples, count);
            Array.Copy(cachedSound.AudioData, position, buffer, offset, samplesToCopy);
            position += samplesToCopy;
            return (int)samplesToCopy;
        }

        public WaveFormat WaveFormat { get { return cachedSound.WaveFormat; } }
    }

    class myMixingSampleProvider : ISampleProvider
    {
        //private readonly CachedSound cachedSound;
        //private long position;

        private readonly List<ISampleProvider> sources;
        private float[] sourceBuffer;
        private const int MaxInputs = 1024; // protect ourselves against doing something silly

        public myMixingSampleProvider(int sampleRate = 44100, int channelCount = 2)
        {
            ReadFully = true;
        }


        public myMixingSampleProvider(WaveFormat waveFormat)
        {
            if (waveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
            {
                throw new ArgumentException("Mixer wave format must be IEEE float");
            }
            sources = new List<ISampleProvider>();
            WaveFormat = waveFormat;
        }
        //public int Read(float[] buffer, int offset, int count)
        //{
        //    var availableSamples = cachedSound.AudioData.Length - position;
        //    var samplesToCopy = Math.Min(availableSamples, count);
        //    Array.Copy(cachedSound.AudioData, position, buffer, offset, samplesToCopy);
        //    position += samplesToCopy;
        //    return (int)samplesToCopy;
        //}
        public myMixingSampleProvider(IEnumerable<ISampleProvider> sources)
        {
            this.sources = new List<ISampleProvider>();
            foreach (var source in sources)
            {
                AddMixerInput(source);
            }
            if (this.sources.Count == 0)
            {
                throw new ArgumentException("Must provide at least one input in this constructor");
            }
        }
        public void AddMixerInput(IWaveProvider mixerInput)
        {
            AddMixerInput(SampleProviderConverters.ConvertWaveProviderIntoSampleProvider(mixerInput));
        }
        public void AddMixerInput(ISampleProvider mixerInput)
        {
            // we'll just call the lock around add since we are protecting against an AddMixerInput at
            // the same time as a Read, rather than two AddMixerInput calls at the same time
            lock (sources)
            {
                if (sources.Count >= MaxInputs)
                {
                    throw new InvalidOperationException("Too many mixer inputs");
                }
                sources.Add(mixerInput);
            }
            if (WaveFormat == null)
            {
                WaveFormat = mixerInput.WaveFormat;
            }
            else
            {
                if (WaveFormat.SampleRate != mixerInput.WaveFormat.SampleRate ||
                    WaveFormat.Channels != mixerInput.WaveFormat.Channels)
                {
                    throw new ArgumentException("All mixer inputs must have the same WaveFormat");
                }
            }
        }
        public void RemoveMixerInput(ISampleProvider mixerInput)
        {
            lock (sources)
            {
                sources.Remove(mixerInput);
            }
        }
        public void RemoveAllMixerInputs()
        {
            lock (sources)
            {
                sources.Clear();
            }
        }
        public WaveFormat WaveFormat { get; private set; }

        public event EventHandler<SampleProviderEventArgs> MixerInputEnded;
        public bool ReadFully { get; set; }
        public int Read(float[] buffer, int offset, int count)
        {
            int outputSamples = 0;
            sourceBuffer = BufferHelpers.Ensure(sourceBuffer, count);
            lock (sources)
            {
                int index = sources.Count - 1;
                while (index >= 0)
                {
                    var source = sources[index];
                    int samplesRead = source.Read(sourceBuffer, 0, count);
                    int outIndex = offset;
                    for (int n = 0; n < samplesRead; n++)
                    {
                        if (n >= outputSamples)
                        {
                            buffer[outIndex++] = sourceBuffer[n];
                        }
                        else
                        {
                            buffer[outIndex++] += sourceBuffer[n];
                        }
                    }
                    outputSamples = Math.Max(samplesRead, outputSamples);
                    if (samplesRead < count)
                    {
                        MixerInputEnded?.Invoke(this, new SampleProviderEventArgs(source));
                        sources.RemoveAt(index);
                    }
                    index--;
                }
            }
            // optionally ensure we return a full buffer
            if (ReadFully && outputSamples < count)
            {
                int outputIndex = offset + outputSamples;
                while (outputIndex < offset + count)
                {
                    buffer[outputIndex++] = 0;
                }
                outputSamples = count;
            }
            return outputSamples;
        }
        public class SampleProviderEventArgs : EventArgs
        {
            /// <summary>
            /// Constructs a new SampleProviderEventArgs
            /// </summary>
            public SampleProviderEventArgs(ISampleProvider sampleProvider)
            {
                SampleProvider = sampleProvider;
            }

            /// <summary>
            /// The Sample Provider
            /// </summary>
            public ISampleProvider SampleProvider { get; private set; }
        }
    }


    class AutoDisposeFileReader : ISampleProvider
    {
        private readonly AudioFileReader reader;
        private bool isDisposed;
        public AutoDisposeFileReader(AudioFileReader reader)
        {
            this.reader = reader;
            this.WaveFormat = reader.WaveFormat;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (isDisposed)
                return 0;
            int read = reader.Read(buffer, offset, count);
            if (read == 0)
            {
                reader.Dispose();
                isDisposed = true;
            }
            return read;
        }

        public WaveFormat WaveFormat { get; private set; }
    }

    public class SampleToWaveProvider : IWaveProvider
    {
        private readonly ISampleProvider source;

        //
        // 요약:
        //     The waveformat of this WaveProvider (same as the source)
        public WaveFormat WaveFormat => source.WaveFormat;

        public SampleToWaveProvider(ISampleProvider source)
        {
            if (source.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
            {
                throw new ArgumentException("Must be already floating point");
            }

            this.source = source;
        }
        public int Read(byte[] buffer, int offset, int count)
        {
            int count2 = count / 4;
            WaveBuffer waveBuffer = new WaveBuffer(buffer);
            return source.Read(waveBuffer.FloatBuffer, offset / 4, count2) * 4;
        }
    }
}
