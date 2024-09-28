using Mirivoice.Mirivoice.Core.Format;
using NAudio.Wave;
using System.IO;

namespace Mirivoice.Engines.TTS
{
    public abstract class BaseEngine
    {
        public virtual void Init(Voicer voicer)
        {

        }

        protected void SaveAudio(float[] audioData, int sampleRate, string outputPath)
        {
            using (var writer = new WaveFileWriter(outputPath, new WaveFormat(sampleRate, 16, 1)))
            {
                foreach (var sample in audioData)
                {
                    writer.WriteSample(sample);
                }
            }
        }

        protected string ReadTxtFile(string txtPath)
        {
            using (StreamReader sr = new StreamReader(txtPath))
            {
                return (sr.ReadToEnd());
            }
        }

        public virtual void Inference(string ipaText, string cacheFilePath, int spkid)
        {

        }
    }
}
