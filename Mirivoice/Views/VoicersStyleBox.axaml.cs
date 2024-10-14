using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Mirivoice.Mirivoice.Core;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.Mirivoice.Core.Managers;
using Mirivoice.Mirivoice.Plugins.Builtin.Phonemizers;
using Mirivoice.ViewModels;
using System;
using System.IO;
using Serilog;
namespace Mirivoice;


public partial class VoicersStyleBox : UserControl
{
    public VoicersStyleBoxViewModel viewModel;
    private readonly MainViewModel v;
    private Voicer voicer; // shared
    private string phrase;
    private BasePhonemizer phonemizer;
    private string cachePath;
    private int sid;

    private bool isPlaying = false;
    public VoicersStyleBox(Voicer voicer, int metaIndex, MainViewModel v)
    {
        InitializeComponent();
        viewModel.DescText = voicer.Info.VoicerMetas[metaIndex].Description;
        viewModel.StyleName = voicer.Info.VoicerMetas[metaIndex].Style;
        sid = voicer.Info.VoicerMetas[metaIndex].SpeakerId;

        phrase = voicer.Info.VoicerMetas[metaIndex].Phrase;
        phonemizer = LineBoxViewModel.GetPhonemizer(voicer.Info.LangCode);
        cachePath = AudioManager.GetUniqueCachePath();
        this.voicer = voicer;
        this.v = v;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = viewModel = new VoicersStyleBoxViewModel();
    }

    public async void OnSamplePlayButtonClick(object sender, RoutedEventArgs e)
    {
        if (phrase is null || phrase.Trim().Equals(string.Empty))
        {
            Log.Warning("Current phrase is null or empty. Skip sample play.");
            return;
        }
        if (MainManager.Instance.AudioM.IsPlaying)
        {
            v.StopAudio();
            return;
        }
        if (File.Exists(cachePath))
        {
            isPlaying = true;
            v.PlayAudio(cachePath);
            return;
        }
        if (voicer != null)
        {
            string ipa = await phonemizer.ConvertToIPA(phrase, DispatcherPriority.ApplicationIdle);
            voicer.Inference(ipa, cachePath, null, sid);
            v.PlayAudio(cachePath);
            return;
        }
        
    }
}