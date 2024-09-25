using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Mirivoice.Engines;
using Mirivoice.ViewModels;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.Mirivoice.Plugins.Builtin.Phonemizers;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
namespace Mirivoice;


public partial class VoicersStyleBox : UserControl
{
    public VoicersStyleBoxViewModel viewModel;
    private readonly MainViewModel v;
    private Voicer voicer;
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
        cachePath = Guid.NewGuid().ToString();
        this.voicer = voicer;
        voicer.CurrentVoicerMeta = voicer.VoicerMetaCollection[metaIndex];
        this.v = v;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = viewModel = new VoicersStyleBoxViewModel();
    }

    public async void OnSamplePlayButtonClick(object sender, RoutedEventArgs e)
    {
        if (isPlaying)
        {
            v.StopAudio();
            isPlaying = false;
            return;
        }
        if (voicer != null)
        {
            isPlaying = true;
            string ipa = await phonemizer.ConvertToIPA(phrase, DispatcherPriority.ApplicationIdle);
            voicer.Inference(ipa, cachePath, null);
            v.PlayAudio(cachePath);
        }
    }
}