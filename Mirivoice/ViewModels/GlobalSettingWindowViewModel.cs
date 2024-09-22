﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirivoice.Mirivoice.Core;
using ReactiveUI;

namespace Mirivoice.ViewModels
{
    public class GlobalSettingWindowViewModel: ViewModelBase
    {
        private string _rebootMessage;
        public string RebootMessage
        {
            get => _rebootMessage;
            set
            {
                this.RaiseAndSetIfChanged(ref _rebootMessage, value);
                OnPropertyChanged(nameof(RebootMessage));
            }
        }
        private int _selectedLanguageIndex;
        public int SelectedLanguageIndex
        {
            get => _selectedLanguageIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedLanguageIndex, value);
                OnPropertyChanged(nameof(SelectedLanguageIndex));

                switch (value)
                {
                    case 0:// english ui
                        MainManager.Instance.Setting.Langcode = "en-US";
                        MainManager.Instance.Setting.Save();
                        RebootMessage = "Please restart MiriVoice to apply changes.";
                        break;
                    case 1:// korean ui
                        MainManager.Instance.Setting.Langcode = "ko-KR";
                        MainManager.Instance.Setting.Save();
                        RebootMessage = "설정 내용을 반영하려면, 미리보이스를 껐다 켜 주세요.";
                        break;
                    default: // default: english ui
                        MainManager.Instance.Setting.Langcode = "en-US";
                        MainManager.Instance.Setting.Save();
                        RebootMessage = "Please restart MiriVoice to apply changes.";
                        break;
                }
            }
        }

        private bool _clearCacheOnQuit;
        public bool ClearCacheOnQuit
        {
            get => _clearCacheOnQuit;
            set
            {
                this.RaiseAndSetIfChanged(ref _clearCacheOnQuit, value);
                OnPropertyChanged(nameof(ClearCacheOnQuit));
                MainManager.Instance.Setting.ClearCacheOnQuit = value;
                MainManager.Instance.Setting.Save();
            }
        }
        public GlobalSettingWindowViewModel()
        {
        }
    }
}