/*  PP.PdfBoss.ViewModels\AppSettings\Dialogs\CreateNameDialogViewModel.cs
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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MaterialDesignThemes.Wpf;

namespace PP.PdfBoss.ViewModels.AppSettings.Dialogs;

public partial class CreateNameDialogViewModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string? _name;

    [RelayCommand]
    private static void Cancel()
    {
        DialogHost.CloseDialogCommand.Execute(null, null);
    }

    [RelayCommand(CanExecute = nameof(IsValidName))]
    private void Save()
    {
        DialogHost.CloseDialogCommand.Execute(Name, null);
    }

    private bool IsValidName()
    {
        return !string.IsNullOrWhiteSpace(Name) &&
            !Name.Contains('<') &&
            !Name.Contains('>') &&
            !Name.Contains(':') &&
            !Name.Contains(' ') &&
            !Name.Contains('\"') &&
            !Name.Contains('\'') &&
            !Name.Contains('/') &&
            !Name.Contains('\\') &&
            !Name.Contains('|') &&
            !Name.Contains('?') &&
            !Name.Contains('!') &&
            !Name.Contains('*') &&
            !Name.Contains(';') &&
            !Name.Contains('&') &&
            !Name.Contains('%') &&
            !Name.Contains('$') &&
            !Name.Contains(';') &&
            !Name.Contains('[') &&
            !Name.Contains(']') &&
            !Name.Contains('«') &&
            !Name.Contains('}') &&
            !Name.Contains('{') &&
            !Name.Contains('»') &&
            !Name.Contains('~') &&
            !Name.Contains('`') &&
            !Name.Contains('´') &&
            !Name.EndsWith('.') &&
            !Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
    }
}
