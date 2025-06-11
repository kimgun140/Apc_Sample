using NAudio.Dmo;
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
        // 여기서 리스트를 만들어서 그걸 통해서 재생하게 해야겠는데 
        //private readonly List<ISampleProvider> sources;

        public readonly IWavePlayer outputDevice;
        public readonly MixingSampleProvider mixer;

        public AudioPlaybackEngine(int sampleRate = 44100, int channelCount = 2)
        {
            //sources = new List<ISampleProvider>();


            outputDevice = new WaveOutEvent();
            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
            mixer.ReadFully = true;
            outputDevice.Init(mixer);
            outputDevice.Play();
        }
        public void RemoveMixerInput(ISampleProvider mixerInput)
        {// source에 스레드가 동시에 접근을 하나? 
         // 제거를 넣으려면 
            mixer.RemoveMixerInput(mixerInput);
        }
        public void PlaySound(string fileName)
        {
            var input = new AudioFileReader(fileName);
            AddMixerInput(new AutoDisposeFileReader(input));
        }

        public void PlaySound(CachedSound sound)
        {
            AddMixerInput(new CachedSoundSampleProvider(sound));
        }

        private void AddMixerInput(ISampleProvider input)
        {
            mixer.AddMixerInput(ConvertToRightChannelCount(input));
            // ConvertToRightChannelCount에서 input의 channel이 mixer의 채널이랑 같은지 비교 
            //MixingSampleProvider의 AddMixerInput가 source리스트에 add()
            //mixer.remove() 하려면 그대로
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

        public void Dispose()
        {
            outputDevice.Dispose();
        }

        public static readonly AudioPlaybackEngine Instance = new AudioPlaybackEngine(44100, 2);
    }
    class AutoDisposeFileReader : ISampleProvider
    {
        private readonly AudioFileReader reader;
        private bool isDisposed;
        public WaveFormat WaveFormat { get; private set; }
        public AutoDisposeFileReader(AudioFileReader reader)
        {
            this.reader = reader;
            this.WaveFormat = reader.WaveFormat;


        }
        public int Read(float[] buffers, int offset, int count)
        {
            if (isDisposed)
                return 0;

            int read = reader.Read(buffers, offset, count);
            if (read == 0)
            {
                reader.Dispose();
                isDisposed = true;
            }
            return read;

        }

    }
    public class CachedSound
    //리샘플링 과정의 성능저하를 줄이게 만든 별도 
    {
        public float[] AudioData { get; private set; }
        public WaveFormat WaveFormat { get; private set; }
        public CachedSound(string audioFileName)
        {
            using (var audioFileReader = new AudioFileReader(audioFileName))
            {

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
    public class CachedSoundSampleProvider : ISampleProvider
    {
        private readonly CachedSound cachedSound;

        private long Position;

        public CachedSoundSampleProvider(CachedSound cachedSound)
        {
            this.cachedSound = cachedSound;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var AvailableSamples = cachedSound.AudioData.Length - Position;
            var samplesTocopy = Math.Min(AvailableSamples, count);
            Array.Copy(cachedSound.AudioData, Position, buffer, offset, samplesTocopy);
            Position += samplesTocopy;

            return (int)samplesTocopy;
        }
        public WaveFormat WaveFormat { get { return cachedSound.WaveFormat; } }
    }
}
