using DynamicData;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Mirivoice.Engines;
using Mirivoice.Engines.TTS;
using Mirivoice.Mirivoice.Core.Format;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VYaml.Serialization;

public class EngineVITS2 : BaseEngine
{

    private string ModelPath;
    private ConfigVITS2 configVITS2;
    private Voicer voicer;
    private bool init = false;
    public EngineVITS2()
    {

        
        
    }

    public override void Init(Voicer voicer)
    {
        if (voicer.Info.ConfigPath == null)
        {
            Log.Error("Config path is not set.");
            return;
        }
        if (voicer.RootPath == null)
        {
            Log.Error("Voicer Root path is not set.");
            return;
        }
        string configYamlPath = Path.Combine(voicer.RootPath, voicer.Info.ConfigPath);
        //Log.Debug($"Loading config from {configYamlPath}");
        if (Path.Exists(configYamlPath))
        {
            //Log.Debug($"Loading config from {configYamlPath}");
            var yamlUtf8Bytes2 = System.Text.Encoding.UTF8.GetBytes(ReadTxtFile(configYamlPath));
            configVITS2 = YamlSerializer.Deserialize<ConfigVITS2>(yamlUtf8Bytes2);
            string modelPath = Path.Combine(voicer.RootPath, configVITS2.ModelPath);
            if (Path.Exists(modelPath))
            {
                //Log.Debug($"Loading model from {modelPath}");

                ModelPath = modelPath;
                this.voicer = voicer;
                init = true;
            }
            else
            {
                Log.Error($"Model file not found: {modelPath}");
                init = false;
            }
        }
        else
        {
            Log.Error($"Config file not found: {voicer.RootPath}");
            init = false;
        }
    }

    Tensor<long> TextToSequence(string ipaText) { // use long because 64-bit integer
        /*Converts a string of text to a sequence of IDs corresponding to the symbols in the text.
        Args:
          text: ipa-converted string to convert to a sequence
        Returns:
          List of integers corresponding to the symbols in the text
        */
        string[] strArr = ipaText.Split("\t");

        Tensor<long> sequence = new DenseTensor<long>(strArr.Length);

        int index = 0;
        foreach (string s in strArr)
        {
            if (configVITS2.symbols.Contains(s) || s == "<pad><pad>")
            {
                string s_;
                if (s == "<pad><pad>")
                {
                    s_ = "<pad>";
                }
                else
                {
                    s_ = s;
                }
                long symbolId = configVITS2.symbols.IndexOf(s_);
                sequence[index] = symbolId;
                ++index;  
            }
            else
            {
                Log.Error($"Text contains a symbol that isn't in the data symbols : {s}");
                ++index;
                continue;
            }
        }
        
        return sequence;
    }

    private Tensor<long> GetText(string ipaText)
    {
        Tensor<long> textSequence = TextToSequence(ipaText);
        if (configVITS2.data.add_blank)
        {
            textSequence = Intersperse(textSequence, 0);
        }
        return textSequence;
    }

    private Tensor<long> Intersperse(Tensor<long> sequence, long interspersedValue)
    {
        Tensor<long> interspersed = new DenseTensor<long>((int)sequence.Length * 2);
        for (int i = 0; i < sequence.Length; ++i)
        {
            interspersed[i * 2] = sequence[i];
            interspersed[i * 2 + 1] = interspersedValue;
        }
        return interspersed;
    }

    public override void Inference(string ipaText, string cacheFilePath, int spkid, MExpressionsWrapper expression)
    {
        if (!init)
        {
            Log.Error("EngineVITS2 is not initialized.");
            return;
        }

        Tensor<long> phonemeIds = GetText($"{configVITS2._pad}{ipaText}");

            // expand dimensions
            long[,] text = new long[1, phonemeIds.Length];
            for (int i = 0; i < phonemeIds.Length; ++i)
            {
                text[0, i] = phonemeIds[i];
            }
            // to 1D array
            long[] textArray = text.Cast<long>().ToArray();

            Tensor<long> textTensor = new DenseTensor<long>(textArray, new[] { 1, (int)phonemeIds.Length });

            // second dimension length
            long[] textLengths = { text.GetLength(1) };


            Tensor<long> lengthTensor = new DenseTensor<long>(textLengths.Length);
            Tensor<long> sidTensor = new DenseTensor<long>(1);
            sidTensor[0] = spkid;
            for (int i = 0; i < textLengths.Length; ++i)
            {
                lengthTensor[i] = textLengths[i];
            }

            // scales array
            Tensor<float> scalesTensor = new DenseTensor<float>(3);
            scalesTensor[0] = expression.VITS2Noise1; // Noise1 / noise_scale
            scalesTensor[1] = expression.VITS2Speed; // Speed / length_scale. If increased, the speed of the speech will be faster
            scalesTensor[2] = expression.VITS2Noise2; // Noise2 / noise_scale_w


            var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input", textTensor),
            NamedOnnxValue.CreateFromTensor("input_lengths", lengthTensor),
            NamedOnnxValue.CreateFromTensor("scales", scalesTensor),
            NamedOnnxValue.CreateFromTensor("sid", sidTensor)
        };

            var options = new SessionOptions();
            using (var results = new InferenceSession(ModelPath, options).Run(inputs))
            {
                
                var audioTensor = results.First().AsTensor<float>();
                var audio = audioTensor.ToArray();

                SaveAudio(audio, configVITS2.data.sampling_rate, cacheFilePath);
            }
        
    }


}
