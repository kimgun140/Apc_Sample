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
        private readonly IWavePlayer outputDevice;
        private readonly MixingSampleProvider mixer;


        public AudioPlaybackEngine(int SampleRate = 44100, int ChannedlCount = 2)
        {
            outputDevice = new WaveOutEvent();
            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, ChannedlCount));
            mixer.ReadFully = true;

            outputDevice.Init(mixer);
            outputDevice.Play();
        }
        public void PlaySound(CachedSound sound)
        {
            AddMixerInput(new CachedSoundSampleProvider(sound));
        }
        public void PlaySound(string FileName)
        {
            var input = new AudioFileReader(FileName);
            AddMixerInput(new AutoDisposeFileReader(input));
        }
        private void AddMixerInput(ISampleProvider input)
        {
            mixer.AddMixerInput(ConvertToRightChannelCount(input));
        }
        private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
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
    // 오디오 파일 미리 로드 
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
