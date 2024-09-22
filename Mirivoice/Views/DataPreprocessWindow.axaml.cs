using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Mirivoice.Mirivoice.Plugins.Builtin.Phonemizers;
using Mirivoice.ViewModels;
using Mirivoice.Views;
using System.IO;
using System.Collections.Generic;
using Avalonia.Threading;
using System.Threading.Tasks;
namespace Mirivoice;
using Serilog;

public partial class DataPreprocessWindow : Window
{
    string transcriptContent = string.Empty;
    string transcriptContentConverted = string.Empty;
    int langCodeIndex = 0;
    private readonly MainViewModel v;

    public DataPreprocessWindow(MainViewModel v, Window owner)
    {
        owner.Closed += (_, __) => Close();
        InitializeComponent();
        this.v = v;
    }

    BasePhonemizer GetPhonemizerWithIndex()
    {
        switch (langCodeIndex)
        {
            case 0:
                return LineBoxViewModel.GetPhonemizer("ko");
            default:
                return new DefaultPhonemizer();
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void OpenTranscriptFile(object sender, RoutedEventArgs args)
    {
        this.FindControl<ProgressBar>("progressBar").Value = 0;
        var topLevel = TopLevel.GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Transcription",
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>() { FilePickerFileTypes.TextPlain }
        });

        if (files.Count >= 1)
        {
            await using var stream = await files[0].OpenReadAsync();
            using var streamReader = new StreamReader(stream);
            transcriptContent = await streamReader.ReadToEndAsync();
            this.FindControl<Button>("convertButton").IsEnabled = true;
            this.FindControl<TextBlock>("filePathText").Text = files[0].Name;
        }
        else
        {
            this.FindControl<Button>("convertButton").IsEnabled = false;
        }
    }

    private void LangChanged(object sender, SelectionChangedEventArgs args)
    {
        var comboBox = sender as ComboBox;
        var selectedItem = comboBox.SelectedItem as ComboBoxItem;
        var content = selectedItem.Content as string;
        langCodeIndex = comboBox.SelectedIndex;
    }

    private async void SaveTranscriptFile(object sender, RoutedEventArgs args)
    {
        this.FindControl<ProgressBar>("progressBar").Value = 0;
        var topLevel = TopLevel.GetTopLevel(this);
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Transcription",
            DefaultExtension = "txt.cleaned",
            SuggestedFileName = "filelist",
        });

        if (file is not null)
        {
            await ConvertTranscript();
            await using var stream = await file.OpenWriteAsync();
            using var streamWriter = new StreamWriter(stream);

            
            await streamWriter.WriteAsync(transcriptContentConverted);
            this.FindControl<Button>("fileSelectButton").IsEnabled = true;
            this.FindControl<ComboBox>("langSelectCombobox").IsEnabled = true;
        }
    }

    async Task ConvertTranscript()
    {
        var phonemizer = GetPhonemizerWithIndex();
        string[] lines = transcriptContent.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        List<string> results = new List<string>();
        this.FindControl<Button>("convertButton").IsEnabled = false;
        this.FindControl<Button>("fileSelectButton").IsEnabled = false;
        this.FindControl<ComboBox>("langSelectCombobox").IsEnabled = false;
        this.FindControl<ProgressBar>("progressBar").Maximum = lines.Length;

        int index = 0;
        foreach (var line in lines)
        {
            this.FindControl<ProgressBar>("progressBar").Value = index;
            if (line.Length == 0)
            {
                Log.Error($"Empty line found. Skipping line index of {index}");
                continue;
            }
            if (line.Split("|").Length < 3)
            {
                Log.Error($"Invalid line format. Transcript's each line should be line:  \"filepath | spkid | text\": {line}");
                continue;
            }
            string[] datas = line.Split("|");
            int textIndex = 2;
            Task<string> res = phonemizer.ConvertToIPA(datas[textIndex], DispatcherPriority.ApplicationIdle);
            string convertedLine = await res;
            results.Add(string.Join("|", datas[0], datas[1], convertedLine));
            ++index;
        }

        transcriptContentConverted = string.Join("\n", results);
        
    }
}
