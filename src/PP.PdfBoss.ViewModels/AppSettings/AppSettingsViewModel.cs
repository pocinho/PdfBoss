/*  PP.PdfBoss.ViewModels\AppSettings\AppSettingsViewModel.cs
 *
 *  Copyright 2024 Paulo Pocinho.
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MaterialDesignThemes.Wpf;

using Microsoft.Extensions.Logging;

using PP.PdfBoss.Core.Dtos;
using PP.PdfBoss.Core.Interfaces;
using PP.PdfBoss.Core.Models;
using PP.PdfBoss.ViewModels.AppSettings.Dialogs;
using PP.PdfBoss.Views.AppSettings.Dialogs;

namespace PP.PdfBoss.ViewModels.AppSettings;

public partial class AppSettingsViewModel(
    ILogger<AppSettingsViewModel> logger,
    IConfigurationService configurationService) : ObservableValidator, IAppSettingsViewModel
{
    private bool _isInitialized = false;

    [ObservableProperty]
    private string _title = "Settings";

    [ObservableProperty]
    private ObservableCollection<ProcessModeDto> _processModeList = new(Core.Constants.ProcessMode);

    [Required(ErrorMessage = "Required.")]
    [NotifyDataErrorInfo]
    [ObservableProperty]
    private ProcessModeDto? _processModeItem;

    [ObservableProperty]
    private int? _processModeValue = 0;

    [ObservableProperty]
    private ObservableCollection<ProcessModeDto> _compressionModeList = new(Core.Constants.CompressionMode);

    [Required(ErrorMessage = "Required.")]
    [NotifyDataErrorInfo]
    [ObservableProperty]
    private ProcessModeDto? _compressionModeItem;

    [ObservableProperty]
    private int? _compressionModeValue = 0;

    [Required(ErrorMessage = "Required.")]
    [NotifyDataErrorInfo]
    [ObservableProperty]
    private string? _suffixName;

    [Required(ErrorMessage = "Required.")]
    [NotifyDataErrorInfo]
    [ObservableProperty]
    private string? _mergedName;

    [Required(ErrorMessage = "Required.")]
    [NotifyDataErrorInfo]
    [ObservableProperty]
    private string? _outputFolderPath;

    [ObservableProperty]
    private bool _isOutputFolderInUse;

    [ObservableProperty]
    private bool _isNoticeVisible;

    [ObservableProperty]
    private string? _noticeText = "Individual files are saved to original paths with suffix.";

    [ObservableProperty]
    private bool _isGsEnabled;

    [ObservableProperty]
    private string? _ghostScriptPath;

    [ObservableProperty]
    private bool _isDarkModeEnabled = Util.Theme.IsDarkModeThemeUsed();

    partial void OnIsDarkModeEnabledChanged(bool value)
    {
        PaletteHelper paletteHelper = new PaletteHelper();
        Theme theme = paletteHelper.GetTheme();

        if (value)
        {
            theme.SetBaseTheme(BaseTheme.Dark);
        }
        else
        {
            theme.SetBaseTheme(BaseTheme.Light);
        }

        paletteHelper.SetTheme(theme);

    }

    partial void OnProcessModeValueChanged(int? value)
    {
        if ((value is 0) && !IsOutputFolderInUse)
        {
            NoticeText = $"Individual files are saved to original paths with suffix.";
            IsNoticeVisible = true;
        }
        else
        if ((value is 1) && !IsOutputFolderInUse)
        {
            NoticeText = $"Saving merged file to default path: {Core.Constants.Defaults.MergedPath}";
            IsNoticeVisible = true;
        }
        else
        {
            IsNoticeVisible = false;
        }
    }

    partial void OnIsOutputFolderInUseChanged(bool value)
    {
        if (!value && (ProcessModeValue is 0))
        {
            NoticeText = $"Individual files are saved to original paths with suffix.";
            IsNoticeVisible = true;
        }
        else
        if (!value && (ProcessModeValue is 1))
        {
            NoticeText = $"Saving merged file to default path: {Core.Constants.Defaults.MergedPath}";
            IsNoticeVisible = true;
        }
        else
        {
            IsNoticeVisible = false;
        }
    }

    [RelayCommand]
    private async Task SelectSuffixName()
    {
        CreateNameDialogView dialog = new();
        (dialog.DataContext as CreateNameDialogViewModel)!.Name = SuffixName;

        object? result = await DialogHost.Show(dialog, "ShellDialog");

        if (result != null)
        {
            SuffixName = (result as string)!;
        }
    }

    [RelayCommand]
    private async Task SelectMergedName()
    {
        CreateNameDialogView dialog = new();
        (dialog.DataContext as CreateNameDialogViewModel)!.Name = MergedName;

        object? result = await DialogHost.Show(dialog, "ShellDialog");

        if (result != null)
        {
            MergedName = (result as string)!;
        }
    }

    [RelayCommand]
    private void SelectOutputFolder()
    {
        Microsoft.Win32.OpenFolderDialog? dlg = new()
        {
            Multiselect = false,
        };

        bool? result = dlg?.ShowDialog();

        if (result.HasValue && result.Value)
        {
            OutputFolderPath = dlg!.FolderName;
        }
    }

    private async void SaveConfigurationAsync()
    {
        try
        {
            if (_isInitialized &&
                ProcessModeItem != null &&
                CompressionModeItem != null &&
                !string.IsNullOrEmpty(SuffixName) &&
                !string.IsNullOrEmpty(MergedName))
            {
                ConfigurationDto config = new(
                    ProcessModeItem.Id,
                    CompressionModeItem.Id,
                    SuffixName,
                    MergedName,
                    IsOutputFolderInUse,
                    OutputFolderPath,
                    IsGsEnabled,
                    GhostScriptPath);

                await configurationService.SaveConfigurationAsync(config);
            }
        }
        catch (Exception e)
        {
            logger.LogError("{m}", e.Message);
        }
    }

    partial void OnProcessModeItemChanged(ProcessModeDto? value)
    {
        if (value != null)
        {
            SaveConfigurationAsync();
        }
    }

    partial void OnCompressionModeItemChanged(ProcessModeDto? value)
    {
        if (value != null)
        {
            SaveConfigurationAsync();
        }
    }

    partial void OnSuffixNameChanged(string? value)
    {
        SaveConfigurationAsync();
    }

    partial void OnMergedNameChanged(string? value)
    {
        SaveConfigurationAsync();
    }

    partial void OnIsOutputFolderInUseChanged(bool oldValue, bool newValue)
    {
        SaveConfigurationAsync();
    }

    partial void OnOutputFolderPathChanged(string? value)
    {
        SaveConfigurationAsync();
    }

    partial void OnIsGsEnabledChanged(bool oldValue, bool newValue)
    {
        SaveConfigurationAsync();
    }

    partial void OnGhostScriptPathChanged(string? value)
    {
        SaveConfigurationAsync();
    }

    [RelayCommand]
    private async Task Loaded()
    {
        if (!_isInitialized)
        {
            _isInitialized = true;

            ConfigurationDto configuration = await configurationService.LoadConfigurationAsync();

            ProcessModeValue = configuration.ProcessMode;
            CompressionModeValue = configuration.CompressionMode;
            SuffixName = configuration.Suffix;
            MergedName = configuration.MergedFileName;
            IsOutputFolderInUse = configuration.IsOutputFolderInUse;
            OutputFolderPath = configuration.OutputFolderPath;
            IsGsEnabled = configuration.IsGhostScriptEnabled;
            GhostScriptPath = configuration.GhostScriptPath;

            IsNoticeVisible = !IsOutputFolderInUse;
        }
    }

    [RelayCommand]
    private void SelectGhostScriptPath()
    {
        try
        {
            Microsoft.Win32.OpenFileDialog? dlg = new()
            {
                Filter = "gswin64c|gswin64c.exe",
                DefaultExt = "exe",
                CheckFileExists = true,
                Multiselect = false,
            };

            bool? result = dlg?.ShowDialog();

            if (result.HasValue && result.Value)
            {
                GhostScriptPath = dlg!.FileName;
            }
        }
        catch (Exception e)
        {
            logger.LogError("{message}", e.Message);
        }
    }
}
