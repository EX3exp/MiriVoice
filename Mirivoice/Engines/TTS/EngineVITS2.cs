using DynamicData;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Mirivoice.Engines;
using Mirivoice.Engines.TTS;
using Mirivoice.Mirivoice.Core;
using Mirivoice.Mirivoice.Core.Format;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VYaml.Serialization;

public class EngineVITS2 : BaseEngine
{

    private string ModelPath;
    private ConfigVITS2 configVITS2;
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

    public override void WarmupSession(InferenceSession session)
    {
        if (!init)
        {
            Log.Warning("EngineVITS2 is initialized but session is null. Warmup skipped.");
            return;
        }
        if (session == null)
        {
            Log.Warning("EngineVITS2 is not initialized. Warmup skipped.");
            return;
        }


        try
        {
            // Warmup the ONNX session by running a dummy inference to reduce initial latency
            Tensor<long> textTensor = new DenseTensor<long>([1, 1]);
            textTensor[0, 0] = 0;

            Tensor<long> lengthTensor = new DenseTensor<long>([1]);
            lengthTensor[0] = 1;

            Tensor<long> sidTensor = new DenseTensor<long>([1]);
            sidTensor[0] = 0;

            Tensor<float> scalesTensor = new DenseTensor<float>([3]);
            scalesTensor[0] = 0.5f; scalesTensor[1] = 1.0f; scalesTensor[2] = 0.5f;

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input", textTensor),
                NamedOnnxValue.CreateFromTensor("input_lengths", lengthTensor),
                NamedOnnxValue.CreateFromTensor("scales", scalesTensor),
                NamedOnnxValue.CreateFromTensor("sid", sidTensor)
            };

            
            using (var results = session.Run(inputs))
            {
                var dummy = results.First().AsTensor<float>();
            }
            Log.Information("Warm up VITS2 Engine");
        }
        catch (Exception ex)
        {
            Log.Warning($"Failed to warm up VITS2 Engine, but the engine is still usable: {ex.Message}");
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

        InferenceSession session = MainManager.Instance.SessionM.GetSession(ModelPath, configVITS2, this);

        Tensor<long> phonemeIds = GetText($"{configVITS2._pad}{ipaText}");
        int textLength = (int)phonemeIds.Length;

        Tensor<long> textTensor = new DenseTensor<long>([1, textLength]);
        // expand dimensions
        long[,] text = new long[1, phonemeIds.Length];
        for (int i = 0; i < textLength; ++i)
        {
            textTensor[0, i] = phonemeIds[i];
        }

        Tensor<long> lengthTensor = new DenseTensor<long>([1]);
        Tensor<long> sidTensor = new DenseTensor<long>([1]);
        sidTensor[0] = spkid;
        lengthTensor[0] = textLength;

        // scales array
        Tensor<float> scalesTensor = new DenseTensor<float>([3]);
        float reducer = 0.56F; // Reduce the noise scale to make the audio less noisy. The default value is 0.56 (kinda magic number)
        scalesTensor[0] = expression.VITS2Noise1 * reducer; // Noise1 / noise_scale
        scalesTensor[1] = expression.VITS2Speed; // Speed / length_scale. If increased, the speed of the speech will be faster
        scalesTensor[2] = expression.VITS2Noise2 * reducer; // Noise2 / noise_scale_w


        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input", textTensor),
            NamedOnnxValue.CreateFromTensor("input_lengths", lengthTensor),
            NamedOnnxValue.CreateFromTensor("scales", scalesTensor),
            NamedOnnxValue.CreateFromTensor("sid", sidTensor)
        };

        using (var results = session.Run(inputs))
        {
                
            var audioTensor = results.First().AsTensor<float>();
            var audio = audioTensor.ToArray();

            // makes the audio smoother and less harsh by applying a simple low-pass filter (exponential moving average)
            float[] smoothedAudio = new float[audio.Length];
            smoothedAudio[0] = audio[0];

            // if the alpha is closer to 1, the audio will be smoother but slower to respond to changes.
            // If the alpha is closer to 0, the audio will be less smooth but faster to respond to changes.
            float alpha = 0.88f;

            for (int i = 1; i < audio.Length; i++)
            {
                smoothedAudio[i] = (alpha * audio[i]) + ((1.0f - alpha) * smoothedAudio[i - 1]);
            }

            SaveAudio(smoothedAudio, configVITS2.data.sampling_rate, cacheFilePath);
        }
        
    }


}
