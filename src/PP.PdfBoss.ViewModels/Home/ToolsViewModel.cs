/*  PP.PdfBoss.ViewModels\Home\ToolsViewModel.cs
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
using System.IO;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using MaterialDesignThemes.Wpf;

using Microsoft.Extensions.Logging;

using PP.PdfBoss.Core.Interfaces;
using PP.PdfBoss.Core.MessageTypes;
using PP.PdfBoss.Core.Models;
using PP.PdfBoss.ViewModels.Home.Dialogs;
using PP.PdfBoss.Views.Home.Dialogs;

namespace PP.PdfBoss.ViewModels.Home;

public partial class ToolsViewModel(
    ILogger<ToolsViewModel> logger,
    IPdfBossService pdfBossService) : ObservableValidator, IToolsViewModel
{
    [ObservableProperty]
    private string _title = "Tools";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GridOrderUpCommand))]
    [NotifyCanExecuteChangedFor(nameof(GridOrderDownCommand))]
    [NotifyCanExecuteChangedFor(nameof(GridRemoveCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClearCommand))]
    [NotifyCanExecuteChangedFor(nameof(OptimiseCommand))]
    [NotifyCanExecuteChangedFor(nameof(GridSplitPagesCommand))]
    private ObservableCollection<FileDto>? _fileList;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GridOrderUpCommand))]
    [NotifyCanExecuteChangedFor(nameof(GridOrderDownCommand))]
    [NotifyCanExecuteChangedFor(nameof(GridRemoveCommand))]
    [NotifyCanExecuteChangedFor(nameof(GridSplitPagesCommand))]
    private FileDto? _fileItem;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClearCommand))]
    [NotifyCanExecuteChangedFor(nameof(OptimiseCommand))]
    [NotifyCanExecuteChangedFor(nameof(LoadFilesCommand))]
    [NotifyCanExecuteChangedFor(nameof(GridOrderUpCommand))]
    [NotifyCanExecuteChangedFor(nameof(GridOrderDownCommand))]
    [NotifyCanExecuteChangedFor(nameof(GridRemoveCommand))]
    [NotifyCanExecuteChangedFor(nameof(GridSplitPagesCommand))]
    private bool _isProcessing;

    private bool _isInitialized;

    [RelayCommand]
    private void Loaded()
    {
        if (!_isInitialized)
        {
            _isInitialized = true;

            WeakReferenceMessenger.Default.Register<ProgressMessage>(this, (r, m)
                => ProgressValue = m.Value);
        }
    }


    [RelayCommand(CanExecute = nameof(CanLoad))]
    private void LoadFiles()
    {
        try
        {
            Microsoft.Win32.OpenFileDialog? dlg = new()
            {
                Filter = "PDF file (*.pdf)|*.pdf",
                DefaultExt = "pdf",
                CheckFileExists = true,
                Multiselect = true,
            };

            bool? result = dlg?.ShowDialog();

            if (result.HasValue && result.Value)
            {
                int record = 0;

                if (IsFileListPopulated())
                {
                    record = FileList!.Last().Order;

                    IEnumerable<FileDto> newFiles = dlg!.FileNames.Select(f => new FileDto(-1, Path.GetFileName(f), f));

                    List<FileDto> newList = new(FileList!.Count + newFiles.Count());
                    newList.AddRange(FileList);

                    foreach (FileDto file in newFiles)
                    {
                        if (newList.Exists(f => f.FileName.Equals(file.FileName)))
                            continue;

                        newList.Add(new FileDto(++record, file.FileName, file.FilePath));
                    }

                    FileDto? fileItem = FileItem;
                    FileList = new ObservableCollection<FileDto>(newList.DistinctBy(m => m.FileName));
                    FileItem = fileItem;
                }
                else
                {
                    IEnumerable<FileDto> files = dlg!.FileNames.Select(f => new FileDto(record++, Path.GetFileName(f), f));

                    FileList = new ObservableCollection<FileDto>(files);
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError("{m}", e.Message);
        }
    }

    private bool CanLoad()
    {
        return !IsProcessing;
    }

    [RelayCommand(CanExecute = nameof(IsFileListPopulated))]
    private void Clear()
    {
        FileList = null;
    }

    [ObservableProperty]
    private float _progressValue = 0;

    [RelayCommand(CanExecute = nameof(IsFileListPopulated))]
    private async Task Optimise()
    {
        ProcessingDialogView dialog = new();

        try
        {
            IsProcessing = true;
            (dialog.DataContext as ProcessingDialogViewModel)!.IsActive = true;

            _ = DialogHost.Show(dialog, "ShellDialog");

            await Task.Run(async () => await pdfBossService.OptimiseAsync(FileList!));

            (dialog.DataContext as ProcessingDialogViewModel)!.IsActive = false;
        }
        catch (Exception e)
        {
            logger.LogError("{m}", e.Message);
        }
        finally
        {
            IsProcessing = false;
            DialogHost.Close("ShellDialog");
            WeakReferenceMessenger.Default.Send(new StatusOperationMessage("Ready."));
        }
    }

    private bool IsFileListPopulated()
    {
        return FileList != null && FileList.Any() && !IsProcessing;
    }

    [RelayCommand(CanExecute = nameof(IsFileItemSelected))]
    private async Task GridSplitPages()
    {
        ProcessingDialogView dialog = new();

        try
        {
            IsProcessing = true;
            (dialog.DataContext as ProcessingDialogViewModel)!.IsActive = true;

            _ = DialogHost.Show(dialog, "ShellDialog");

            await Task.Run(async () => await pdfBossService.SplitAsync(FileItem!));

            (dialog.DataContext as ProcessingDialogViewModel)!.IsActive = false;
        }
        catch (Exception e)
        {
            logger.LogError("{m}", e.Message);
        }
        finally
        {
            IsProcessing = false;
            DialogHost.Close("ShellDialog");
            WeakReferenceMessenger.Default.Send(new StatusOperationMessage("Ready."));
        }
    }

    [RelayCommand(CanExecute = nameof(IsFileItemSelected))]
    private void GridOrderUp()
    {
        try
        {
            if (FileList != null &&
                FileList.Count > 1 &&
                FileItem != null &&
                FileItem.Order > 0)
            {
                int position = FileItem.Order;
                FileDto pivot = FileList
                    .Where(f => f.Order == position - 1)
                    .Select(f => new FileDto(f.Order + 1, f.FileName, f.FilePath))
                    .First();
                FileDto file = new(FileItem.Order - 1, FileItem.FileName, FileItem.FilePath);
                List<FileDto> newList = new(FileList.Count);

                newList.AddRange(FileList.Where(f => f.Order < position - 1));
                newList.Add(file);
                newList.Add(pivot);
                newList.AddRange(FileList.Where(f => f.Order > position));

                FileList = new ObservableCollection<FileDto>(newList);
                FileItem = file;
            }
        }
        catch (Exception e)
        {
            logger.LogError("{m}", e.Message);
        }
    }

    private bool IsFileItemSelected()
    {
        return FileList != null && FileItem != null && !IsProcessing;
    }

    [RelayCommand(CanExecute = nameof(IsFileItemSelected))]
    private void GridOrderDown()
    {
        try
        {
            if (FileList != null &&
                FileList.Count > 1 &&
                FileItem != null &&
                FileItem.Order < (FileList.Count - 1))
            {
                int position = FileItem.Order;
                FileDto pivot = FileList
                    .Where(f => f.Order == position + 1)
                    .Select(f => new FileDto(f.Order - 1, f.FileName, f.FilePath))
                    .First();
                FileDto file = new(FileItem.Order + 1, FileItem.FileName, FileItem.FilePath);
                List<FileDto> newList = new(FileList.Count);

                newList.AddRange(FileList.Where(f => f.Order < position));
                newList.Add(pivot);
                newList.Add(file);
                newList.AddRange(FileList.Where(f => f.Order > (position + 1)));

                FileList = new ObservableCollection<FileDto>(newList);
                FileItem = file;
            }
        }
        catch (Exception e)
        {
            logger.LogError("{m}", e.Message);
        }
    }

    [RelayCommand(CanExecute = nameof(IsFileItemSelected))]
    private void GridRemove()
    {
        try
        {
            if (FileList != null &&
                FileItem != null)
            {
                int position = FileItem.Order;
                List<FileDto> newList = new(FileList.Count);
                newList.AddRange(FileList.Where(f => f.Order < position));
                newList.AddRange(FileList.Where(f => f.Order > FileItem.Order)
                    .Select(f => new FileDto(position++, f.FileName, f.FilePath)));

                FileList = new ObservableCollection<FileDto>(newList);
                FileItem = FileList.FirstOrDefault(f => f.Order == position);
            }
        }
        catch (Exception e)
        {
            logger.LogError("{m}", e.Message);
        }
    }
}
