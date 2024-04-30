/*  PP.PdfBoss.ViewModels\Shell\Dialogs\LogDialogViewModel.cs
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

using System.Text.Json;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MaterialDesignThemes.Wpf;

using NLog;

using PP.PdfBoss.Core.Dtos;

namespace PP.PdfBoss.ViewModels.Shell.Dialogs;

public partial class LogDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private string? _logText;

    public void LoadData(LogEventInfo log)
    {
        string na = "N/A";

        string logLevel = log.Level == null ? na : log.Level.ToString();
        string logType = string.IsNullOrEmpty(log.LoggerName) ? na : log.LoggerName;
        string logMessage = string.IsNullOrEmpty(log.FormattedMessage) ? na : log.FormattedMessage;
        string logException = log.Exception == null ? na : log.Exception.ToString();

        LogDto dto = new(
            Version: Core.Constants.VersionInfo,
            DateTimeUtc: log.TimeStamp.ToUniversalTime(),
            Level: logLevel,
            Type: logType,
            Message: logMessage,
            Exception: logException
            );

        LogText = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });

        Title = $"PdfBoss: {logLevel}";
    }

    [RelayCommand]
    private static void Close()
    {
        DialogHost.CloseDialogCommand.Execute(null, null);
    }
}
