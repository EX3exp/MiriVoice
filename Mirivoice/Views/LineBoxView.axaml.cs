﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Mirivoice.Commands;
using Mirivoice.Engines;
using Mirivoice.Mirivoice.Core;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.Mirivoice.Core.Managers;
using Mirivoice.ViewModels;
using Serilog;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;


namespace Mirivoice.Views
{
    public partial class LineBoxView: UserControl
    {
        private bool mouseEntered;
        private bool isDragging;

        public MExpressionsWrapper Exp = new MExpressionsWrapper();
        public UserControl ExpressionEditor { get; set; }
        public readonly MainViewModel v;
        private bool dragInit;

        public bool DeActivatePhonemizer; // phonemizer will phonemize only when DeActivatePhonemizer=true
        private string _currentCacheName;

        public LineBoxViewModel viewModel;
        public string CurrentCacheName
        {
            get { return _currentCacheName; }
            private set
            {
            }
        }

        private VoicerMeta lastInferencedVoicerMeta;

        public bool IsCacheIsVaild = false;

        public string IPAText;
        private bool _shouldPhonemize;

        private List<MResult> backup;
        private bool _showBackUp;
        public bool ShowBackUp // set true when restoring MResults 
        {
            get { return _showBackUp; }
            set
            {
                if (value != _showBackUp)
                {
                    _showBackUp = value;
                }
                if (value)
                {
                    //Log.Debug("ShowBackUp: true");
                    if (backup is null)
                    {
                        return;
                    }
                    MResultsCollection = new ObservableCollection<MResult>(backup);
                    v.MResultsCollection = MResultsCollection;
                    v.OnPropertyChanged(nameof(v.MResultsCollection));
                }
                else
                {
                    //Log.Debug("ShowBackUp: false");
                    backup = new List<MResult>(MResultsCollection);
                    DeActivatePhonemizer = false;
                    ShouldPhonemize = true;
                    
                }
            }
        }
        public bool ShouldPhonemize
        {
            get { return _shouldPhonemize; }
            set
            {

                if (value != _shouldPhonemize)
                {
                    _shouldPhonemize = value;
                }
                
                if (value)
                {
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await PhonemizeLine();
                        _shouldPhonemize = false;
                        DeActivatePhonemizer = true;
                    });
                }
            }
        }

        public string lastPhonemizedText = string.Empty;
        private async Task PhonemizeLine(bool ApplyToCurrentEdit = true)
        {
            if (viewModel is null)
            {
                return;
            }
            string line = singleLineEditorView.viewModel.mTextBoxEditor.CurrentScript;
            try
            {
                
                string textChanged = line;
                
                if (ApplyToCurrentEdit)
                {
                    v.IsArcActive = true;
                    v.OnPropertyChanged("IsArcActive");

                    v.SingleTextBoxEditorEnabled = false;
                    v.OnPropertyChanged(nameof(v.SingleTextBoxEditorEnabled));
                    
                }
                if (!lastPhonemizedText.Equals(textChanged))
                {
                    IsCacheIsVaild = false;
                }
                if (viewModel.phonemizer is null)
                {
                    Log.Error("Skip Phonemizing: phonemizer is null");
                }
                else if (textChanged == string.Empty)
                {
                    Log.Warning("Skip Phonemizing: phonemizer is not null, but LineText is empty");
                }
                
                else if (lastPhonemizedText.Equals(textChanged))
                {
                    Log.Information("Skip Phonemizing: text is not changed");
                }
                else if (IsCacheIsVaild)
                {
                    Log.Information("Skip Phonemizing: Cache is valid");
                }
                else
                {
                    Log.Debug("Phonemizing: {0}", textChanged);
                    await Task.Run(() => viewModel.phonemizer.PhonemizeAsync(textChanged, this));
                    lastPhonemizedText = textChanged;
                }


            }
            finally
            {
                if (ApplyToCurrentEdit)
                {
                    v.IsArcActive = false;
                    v.OnPropertyChanged("IsArcActive");
                    v.SingleTextBoxEditorEnabled = true;
                    v.OnPropertyChanged(nameof(v.SingleTextBoxEditorEnabled));
                }

                
            }

        }
        int targetIndex;
        double lastY;
        

        public bool ShouldPhonemizeWhenSelected = false;

        public SingleLineEditorView singleLineEditorView;
        public ObservableCollection<MResult> MResultsCollection { get; set; } = new ObservableCollection<MResult>();

        

        public LineBoxView(MainViewModel v, string line="")
        {
            InitializeComponent();
            
            //lockButton = this.FindControl<ToggleButton>("lockButton");
            _currentCacheName = AudioManager.GetUniqueCachePath();

            mouseEntered = false;
            isDragging = false;

            this.AddHandler(PointerReleasedEvent, OnDragEnd, RoutingStrategies.Tunnel);

            targetIndex = -1;

            this.v = v;

            DeActivatePhonemizer = true;

            if (line != string.Empty)
            {
                ShouldPhonemizeWhenSelected = true;
            }
            else
            {
                ShouldPhonemizeWhenSelected = false;
            }

            

            singleLineEditorView = new SingleLineEditorView(this);

            singleLineEditorView.SetLine(line);
            ResetExpEditor(v.voicerSelector.CurrentVoicer.Info.Type);


        }


        


        public LineBoxView(MLinePrototype mLinePrototype, MainViewModel v, int index, int voicerIndex, int metaIndex, bool isDuplicating)
        {
            this.v = v;
            InitializeComponent(voicerIndex, metaIndex);
            viewModel.LineText = mLinePrototype.LineText;
            IPAText = mLinePrototype.IPAText;
            viewModel.SetLineNo(index + 1);
            _currentCacheName = AudioManager.GetUniqueCachePath();
            this.CurrentCacheName = mLinePrototype.cacheName;
            Exp = mLinePrototype.Exp;

            //lockButton = this.FindControl<ToggleButton>("lockButton");

            if (mLinePrototype.PhonemeEdit is not null || mLinePrototype.PhonemeEdit.Length > 0)
            {
                this.MResultsCollection = new ObservableCollection<MResult>(
                    mLinePrototype.PhonemeEdit.Select(m => new MResult(m, this)).ToArray()
                    );
                if (!isDuplicating)
                {
                    UpdateMResultsCollection();
                }
                ShouldPhonemize = false;
            }
            else
            {
                ShouldPhonemize = true;
            }

            mouseEntered = false;
            isDragging = false;

            this.AddHandler(PointerReleasedEvent, OnDragEnd, RoutingStrategies.Tunnel);

            targetIndex = -1;

            string line = viewModel.LineText;

            this.v = v;

            
            DeActivatePhonemizer = true;

            if (line != string.Empty)
            {
                ShouldPhonemizeWhenSelected = true;
            }
            else
            {
                ShouldPhonemizeWhenSelected = false;
            }
            if (mLinePrototype.PhonemeEdit.Length > 0)
            {
                // if phonemeEdit is not empty, use phonemeEdit's results
                singleLineEditorView = new SingleLineEditorView(this, false);
                ShouldPhonemizeWhenSelected = false;    
            }
            else
            {
                // if phonemeEdit is empty, use newly phonemized one
                singleLineEditorView = new SingleLineEditorView(this);
            }
            ResetExpEditor(v.voicerSelector.CurrentVoicer.Info.Type);
            viewModel.OnStyleChanged();
        }
        // Commands

        

        //public MCommand LockLineBoxCommand { get; set; }
        //LockLineBoxReceiver lockLineBoxReceiver;



        public void ScrollToEnd()
        {
            ItemsControl itemControl = this.Parent as ItemsControl;
            
            if (itemControl != null)
            {
                ScrollViewer scrollViewer = itemControl.Parent as ScrollViewer;

                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToEnd();
                }

            }
        }

        public void AutoScroll()
        {
            ItemsControl itemControl = this.Parent as ItemsControl;

            if (itemControl != null)
            {
                ScrollViewer scrollViewer = itemControl.Parent as ScrollViewer;
                scrollViewer.SetCurrentValue(ScrollViewer.SmallChangeProperty, this.Height);
                if (scrollViewer != null)
                {
                    scrollViewer.LineDown();
                }

            }
        }

        MExpressionsWrapper lastExp = new MExpressionsWrapper();

        public void ResetExpEditor(string voicerMetaType )
        {
            switch (voicerMetaType)
            {
                case "VITS2": //TODO resolve hardcoding
                    ExpressionEditor = new ExpressionEditViewVITS2(this);
                    break;
                default:
                    ExpressionEditor = null;
                    break;
            }
        }
        public async Task StartInference()
        {
            if (viewModel.voicerSelector.CurrentVoicer is null)
            {
                Log.Warning("CurrentVoicer is null");
                return;
            }

            //Log.Debug("StartInference");
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (ShouldPhonemizeWhenSelected)
                {
                    DeActivatePhonemizer = false;
                    ShouldPhonemize = true;

                    await viewModel.phonemizer.GenerateIPAAsync(this);
                }

                if (viewModel.voicerSelector.CurrentVoicer is not null)
                {
                    //Log.Debug($"LineBoxView Inference: {IPAText}");

                    
                    string lastIPAText = IPAText;
                    
                    await viewModel.phonemizer.GenerateIPAAsync(this);

                    if (lastIPAText != IPAText)
                    {
                        IsCacheIsVaild = false;
                    }
                    if (lastInferencedVoicerMeta is not null &&  viewModel.voicerSelector.CurrentVoicer.CurrentVoicerMeta.SpeakerId != lastInferencedVoicerMeta.SpeakerId)
                    {
                        Log.Debug("meta changed");
                        IsCacheIsVaild = false;
                    }
                    if (lastExp != Exp)
                    {
                        Log.Debug("exp changed");
                        IsCacheIsVaild = false;
                    }
                    if (!System.IO.Path.Exists(CurrentCacheName))
                    {
                        IsCacheIsVaild = false;
                    }
                    if (!IsCacheIsVaild)
                    {
                        //Log.Debug("Cache is not valid -- Starting Inference");
                        CurrentCacheName = AudioManager.GetUniqueCachePath();
                        viewModel.voicerSelector.CurrentVoicer.Inference(IPAText, CurrentCacheName, this);

                    }

                    lastInferencedVoicerMeta = viewModel.voicerSelector.CurrentVoicer.CurrentVoicerMeta;
                    lastExp = Exp;
                }
                else
                {
                    Log.Warning("CurrentVoicerMeta is null");
                    return;
                }
            }
            , DispatcherPriority.ApplicationIdle);
            //Log.Debug("EndInference");
        }

        public void InferenceThisOnly()
        {

        }
        public void StartProgress(int minTaskNum, int maxTaskNum, string taskName)
        {
            v.StartProgress(minTaskNum, maxTaskNum, taskName);
        }

        public void SetProgressValue(int value)
        {
            v.SetProgressValue(value);
        }

        public void ProgressProgressbar(int progressAmount)
        {
            v.ProgressProgressbar(progressAmount);
        }

        public void EndProgress()
        {
            v.EndProgress();
        }

        public void UnLock(bool isLocked = false)
        {
            viewModel.CanHitTest = isLocked;
            // TODO
        }

        private void OnDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            DelLineBoxCommand delLineBoxCommand = new DelLineBoxCommand(v, this);
            MainManager.Instance.cmd.ExecuteCommand(delLineBoxCommand);
            
        }

        private void OnDuplicateButtonClick(object sender, RoutedEventArgs e)
        {
            DuplicateLineBoxCommand DuplicateLineBoxCommand = new DuplicateLineBoxCommand(this);
            MainManager.Instance.cmd.ExecuteCommand(DuplicateLineBoxCommand);
        }

        /*private void OnLockButtonClick(object sender, RoutedEventArgs e)
        {
            MainManager.Instance.cmd.ExecuteCommand(LockLineBoxCommand);
        }*/

        private void OnEntered(object sender, PointerEventArgs e)
        {
            //Log.Debug("Entered");
            mouseEntered = true;
            dragInit = false;
            isDragging = false;
        }


        private void OnDragStart(object sender, RoutedEventArgs e)
        {
            viewModel.IsSelected = true;
            if (v.CurrentEdit is null)
            {
                v.CurrentEdit = new PhonemeEditView();
                v.OnPropertyChanged(nameof(v.CurrentEdit));
            }
            switch (v.CurrentEditIndex)
            {
                case 1:
                    v.CurrentEdit = this.ExpressionEditor;
                    v.OnPropertyChanged(nameof(v.CurrentEdit));
                    break;
                default:
                    break;
            }

            if (!mouseEntered)
            {
                return;
            }
            //Log.Debug("DragStart");

            
            

            ItemsControl parent = this.Parent as ItemsControl;
            
            
      



            if (parent != null)
            {
                foreach (LineBoxView item in v.LineBoxCollection)
                {
                    if (item != this)
                    {
                        item.viewModel.IsSelected = false;
                    }
                }
                isDragging = true;
                dragInit = true;
                

                // Disable all other toggle buttons
                foreach (LineBoxView item in v.LineBoxCollection)
                {
                    if (item != this)
                    {
                        item.viewModel.CanHitTest = false;
                    }
                }

                if (ShouldPhonemizeWhenSelected)
                {
                    string text = viewModel.LineText;
                    if (text is not null && text != string.Empty)
                    {
                        ShouldPhonemize = true;
                        DeActivatePhonemizer = false;
                        ShouldPhonemizeWhenSelected = false;
                    }

                }
                if (singleLineEditorView is not null && lastPhonemizedText is not null && singleLineEditorView.viewModel.mTextBoxEditor.CurrentScript is not null)
                {
                    if (!lastPhonemizedText.Equals(singleLineEditorView.viewModel.mTextBoxEditor.CurrentScript))
                    {
                        DeActivatePhonemizer = false;
                        ShouldPhonemize = true;
                    }
                }
                

                if (v.CurrentLineBox != this)
                {
                    
                    v.CurrentSingleLineEditor = singleLineEditorView;
                    UpdateMResultsCollection();
                    Log.Debug("CurrentLineBox: {0}", v.CurrentLineBox);
                    Log.Debug("CurrentSingleLineTextBox: {0}", v.CurrentSingleLineEditor);

                    ShouldPhonemize = true;
                    

                    v.CurrentLineBox = this;
                    v.OnPropertyChanged(nameof(v.CurrentLineBox));




                    v.SingleTextBoxEditorEnabled = true;
                    v.OnPropertyChanged(nameof(v.CurrentSingleLineEditor));
                    v.OnPropertyChanged(nameof(v.SingleTextBoxEditorEnabled));

                }
            }
            else
            {
                viewModel.IsSelected = false;
                isDragging = false;
            }

            

        }

        private void HighlightBorders(LineBoxView target, bool highlightTop=true) // if highlightTop is true, highlight top border, else highlight bottom border
        {
            ItemsControl parent = this.Parent as ItemsControl;
            if (parent != null)
            {
                foreach (LineBoxView item in v.LineBoxCollection)
                {
                    if (item != target)
                    {
                        item.viewModel.BorderThickness = viewModel.OriginalThickness;
                        item.viewModel.BorderColor = viewModel.OriginalBorderColor;
                    }
                    else if (target != null)
                    {
                        item.viewModel.BorderThickness = new Thickness(viewModel.OriginalThickness.Left,
                            highlightTop ? 13 : viewModel.OriginalThickness.Top,
                            viewModel.OriginalThickness.Right,
                            highlightTop ? viewModel.OriginalThickness.Bottom : 13);

                        item.viewModel.BorderColor = Brushes.DarkOliveGreen;
                    }
                }
            }
        }

        public void UpdateMResultsCollection()
        {
            v.MResultsCollection = MResultsCollection;

            v.OnPropertyChanged(nameof(v.MResultsCollection));
            
        }

        private void OnDragging(object sender, PointerEventArgs e)
        {
            ItemsControl parent = this.Parent as ItemsControl;
            if (dragInit)
            {
                lastY = e.GetPosition(parent).Y;

                //Log.Debug("LastY: {0}", lastY);
                dragInit = false;

                return;
            }


            if (parent != null)
            {
                
                if (isDragging)
                {
                    bool highlightTop;
                    this.Margin = new Thickness(50, this.Margin.Top + e.GetPosition(this).Y, this.Margin.Right, this.Margin.Bottom - e.GetPosition(this).Y);
                    if (lastY < e.GetPosition(parent).Y)
                    {
                        // moving downward --target's Bottom Border should be highlighted
                        targetIndex = parent.Items.IndexOf(this) + ((int)(e.GetPosition(parent).Y - lastY) / 104);
                        highlightTop = false;

                    }
                    else
                    {
                        // moving upward --target's Top Border should be highlighted
                        targetIndex = parent.Items.IndexOf(this) + ((int)(e.GetPosition(parent).Y - lastY) / 104);
                        highlightTop = true;

                    }
                    if (targetIndex < 0)
                    {
                        targetIndex = 0;
                    }
                    else if (targetIndex >= v.LineBoxCollection.Count)
                    {
                        targetIndex = v.LineBoxCollection.Count - 1;
                    }

                    if (targetIndex != v.LineBoxCollection.IndexOf(this))
                    {
                        LineBoxView target = v.LineBoxCollection[targetIndex];
                        HighlightBorders(target, highlightTop);
                    }
                    else
                    {
                        HighlightBorders(null);
                    }
                    //Log.Debug($"Dragging {e.GetPosition(parent).Y}, {targetIndex}");
                    
                }

            }
        }

        private void OnDragEnd(object sender, PointerReleasedEventArgs e)
        {
            //Log.Debug("DragEnd");
            
            ItemsControl parent = this.Parent as ItemsControl;
            this.Margin = viewModel.OriginalMargin;
            HighlightBorders(null);
            mouseEntered = false;
            dragInit = false;

            if (targetIndex == -1)
            {
                isDragging = false;
                foreach (LineBoxView item in v.LineBoxCollection)
                {
                    item.viewModel.CanHitTest = true;
                }
                return;
            }
            if (parent != null)
            {
                foreach (LineBoxView item in v.LineBoxCollection)
                {
                    item.viewModel.CanHitTest = true;
                }
                if (isDragging)
                {
                    
                    Log.Information("Swapping {0} and {1}", v.LineBoxCollection.IndexOf(this), targetIndex);
                    
                    if (v.LineBoxCollection.IndexOf(this) != targetIndex )
                    {


                        SwapLineBoxCommand SwapLineBoxCommand = new SwapLineBoxCommand(v);
                        SwapLineBoxCommand.SetLineBoxesIdx(v.LineBoxCollection.IndexOf(this), targetIndex);
                        MainManager.Instance.cmd.ExecuteCommand(SwapLineBoxCommand);
                    }
                }
                

                
            }
            // reset targetIndex
            targetIndex = -1;
            isDragging = false;

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.DataContext = viewModel = new LineBoxViewModel(this);
            
        }

        private void InitializeComponent(int voicerIndex=0, int metaIndex=0)
        {
            AvaloniaXamlLoader.Load(this);
            this.DataContext = viewModel = new LineBoxViewModel(voicerIndex, metaIndex, this);


        }

    }


}