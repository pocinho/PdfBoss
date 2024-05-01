/*  PP.PdfBoss.Data\Services\PdfBossService.cs
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

using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Text;

using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.Logging;

using PP.PdfBoss.Core.Dtos;
using PP.PdfBoss.Core.Interfaces;
using PP.PdfBoss.Core.MessageTypes;
using PP.PdfBoss.Core.Models;

using QPdfNet.Enums;

namespace PP.PdfBoss.Data.Services;

public class PdfBossService(
    ILogger<PdfBossService> logger,
    IConfigurationService configurationService) : IPdfBossService
{
    private ConfigurationDto? _settings;
    private ConcurrentBag<Operation<ProcessResultDto>>? _filesProcessed;

    public async Task ProcessAsync(IEnumerable<FileDto> fileList, CancellationToken cancellationToken = default)
    {
        try
        {
            _settings = await configurationService.LoadConfigurationAsync(cancellationToken);
            _filesProcessed = [];

            if (_settings.ProcessMode == Core.Constants.ProcessMode.MergeFiles)
            {
                await MergeAsync(fileList, cancellationToken);
            }
            else
            {
                await OptimiseAsync(fileList, cancellationToken);
            }

            StatisticsDto savedStats = await configurationService.LoadStatisticsAsync(cancellationToken);

            uint totalFilesProcessed = (uint)_filesProcessed.Count;
            decimal? totalBytesProcessed = _filesProcessed.Where(op => op.HasSucceeded)?.Sum(op => op.Result!.TotalBytesProcessed);
            decimal? totalBytesOptimized = _filesProcessed.Where(op => op.HasSucceeded)?.Sum(op => op.Result!.TotalBytesOptimized);
            decimal? totalBytesSaved = (totalBytesProcessed ?? 0) - (totalBytesOptimized ?? 0);

            StatisticsDto updatedStats = new(
                TotalFilesProcessed: savedStats.TotalFilesProcessed + totalFilesProcessed,
                TotalMegabytesProcessed: savedStats.TotalMegabytesProcessed + ((totalBytesProcessed ?? 0) / Core.Constants.OneMiBInBytes),
                TotalMegabytesOptimised: savedStats.TotalMegabytesOptimised + ((totalBytesOptimized ?? 0) / Core.Constants.OneMiBInBytes),
                TotalMegabytesSaved: savedStats.TotalMegabytesSaved + ((totalBytesSaved ?? 0) / Core.Constants.OneMiBInBytes)
                );

            await configurationService.SaveStatisticsAsync(updatedStats, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError("{m}", e.Message);
        }
    }

    private async Task OptimiseAsync(IEnumerable<FileDto> fileList, CancellationToken cancellationToken)
    {
        uint fileCount = 1;
        uint totalFiles = (uint)fileList.Count();

        string command = _settings!.CompressionMode switch
        {
            Core.Constants.CompressionMode.High => "/screen",
            Core.Constants.CompressionMode.Medium => "/ebook",
            Core.Constants.CompressionMode.Low => "/printer",
            _ => "/ebook",
        };

        foreach (FileDto file in fileList)
        {
            WeakReferenceMessenger.Default.Send(new ProgressMessage(fileCount * 100 / totalFiles + 1));
            WeakReferenceMessenger.Default.Send(new StatusOperationMessage($"Processing file {fileCount++} of {totalFiles}..."));

            Operation<ProcessResultDto> resultOp = new();

            string fileName = file.FilePath.TrimEnd(".pdf".ToCharArray());
            if (_settings!.IsOutputFolderInUse && !string.IsNullOrEmpty(_settings.OutputFolderPath))
            {
                Directory.CreateDirectory(_settings.OutputFolderPath);
                fileName = Path.Combine(_settings.OutputFolderPath, file.FileName.TrimEnd(".pdf".ToCharArray()));
            }

            string targetFile = $"{fileName}{_settings.Suffix}.pdf";
            FileDto tmpFile = new(0, $"{Path.GetFileName(targetFile)}.tmp", $"{targetFile}.tmp");

            if (_settings.IsGhostScriptEnabled && File.Exists(_settings.GhostScriptPath))
            {

                await RunGsOptimiserAsync(_settings.GhostScriptPath,
                    $" -sDEVICE=pdfwrite -dCompatibilityLevel=\"1.7\" -dPDFSETTINGS={command} -dNOPAUSE -dQUIET -dBATCH -sOutputFile=\"{tmpFile.FilePath}\" \"{file.FilePath}\" ",
                    cancellationToken);

                Operation<FileInfo> op = await OptimiseAsync([tmpFile], filePathOut: targetFile, cancellationToken);
                if (op.HasSucceeded)
                {
                    FileInfo info = new(file.FilePath);
                    resultOp.SetResult(new ProcessResultDto(
                            TotalBytesProcessed: info.Length,
                            TotalBytesOptimized: op.Result!.Length));
                }

                if (File.Exists(tmpFile.FilePath))
                    File.Delete(tmpFile.FilePath);
            }
            else
            {
                Operation<FileInfo> op = await OptimiseAsync([file], filePathOut: targetFile, cancellationToken);
                if (op.HasSucceeded)
                {
                    FileInfo info = new(file.FilePath);
                    resultOp.SetResult(new ProcessResultDto(
                            TotalBytesProcessed: info.Length,
                            TotalBytesOptimized: op.Result!.Length));
                }
            }

            _filesProcessed!.Add(resultOp);
        };
    }

    private async Task MergeAsync(IEnumerable<FileDto> fileList, CancellationToken cancellationToken)
    {
        Operation<ProcessResultDto> resultOp = new();
        string targetFile;

        if (_settings!.IsOutputFolderInUse && !string.IsNullOrEmpty(_settings.OutputFolderPath))
        {
            Directory.CreateDirectory(_settings.OutputFolderPath);
            targetFile = Path.Combine(_settings.OutputFolderPath, $"{_settings.MergedFileName}.pdf");
        }
        else
        {
            Directory.CreateDirectory(Core.Constants.Defaults.MergedPath);
            targetFile = Path.Combine(Core.Constants.Defaults.MergedPath, $"{_settings.MergedFileName}.pdf");
        }

        WeakReferenceMessenger.Default.Send(new ProgressMessage(40));
        WeakReferenceMessenger.Default.Send(new StatusOperationMessage($"Merging {fileList.Count()} files..."));

        if (_settings.IsGhostScriptEnabled && File.Exists(_settings.GhostScriptPath))
        {
            FileDto tmpFile = new(0, $"{Path.GetFileName(targetFile)}.tmp", $"{targetFile}.tmp");

            StringBuilder fileCompilation = new();
            foreach (FileDto file in fileList)
            {
                fileCompilation.Append($"\"{file.FilePath}\" ");
            }

            string command = _settings.CompressionMode switch
            {
                Core.Constants.CompressionMode.High => "/screen",
                Core.Constants.CompressionMode.Medium => "/ebook",
                Core.Constants.CompressionMode.Low => "/printer",
                _ => "/ebook",
            };

            WeakReferenceMessenger.Default.Send(new ProgressMessage(60));
            WeakReferenceMessenger.Default.Send(new StatusOperationMessage($"Applying first pass optimisations..."));

            await RunGsOptimiserAsync(_settings.GhostScriptPath,
                $" -sDEVICE=pdfwrite -dCompatibilityLevel=\"1.7\" -dPDFSETTINGS={command} -dNOPAUSE -dQUIET -dBATCH -sOutputFile=\"{tmpFile.FilePath}\" {fileCompilation}",
                cancellationToken);

            WeakReferenceMessenger.Default.Send(new ProgressMessage(80));
            WeakReferenceMessenger.Default.Send(new StatusOperationMessage($"Applying final optimisations..."));

            Operation<FileInfo> op = await OptimiseAsync([tmpFile], targetFile, cancellationToken);
            if (op.HasSucceeded)
            {
                resultOp.SetResult(new ProcessResultDto(
                    TotalBytesProcessed: fileList.Sum(f => new FileInfo(f.FilePath).Length),
                    TotalBytesOptimized: op.Result!.Length));
            }

            WeakReferenceMessenger.Default.Send(new ProgressMessage(90));
            WeakReferenceMessenger.Default.Send(new StatusOperationMessage($"Applying cleanup..."));
            if (File.Exists(tmpFile.FilePath))
                File.Delete(tmpFile.FilePath);
        }
        else
        {
            WeakReferenceMessenger.Default.Send(new ProgressMessage(50));
            WeakReferenceMessenger.Default.Send(new StatusOperationMessage($"Applying optimisations..."));
            Operation<FileInfo> op = await OptimiseAsync(fileList, targetFile, cancellationToken);
            if (op.HasSucceeded)
            {
                resultOp.SetResult(new ProcessResultDto(
                    TotalBytesProcessed: fileList.Sum(f => new FileInfo(f.FilePath).Length),
                    TotalBytesOptimized: op.Result!.Length));
            }
        }

        _filesProcessed!.Add(resultOp);
    }

    private Task<Operation<FileInfo>> OptimiseAsync(IEnumerable<FileDto> fileList, string filePathOut, CancellationToken cancellationToken)
    {
        TaskCompletionSource<Operation<FileInfo>> taskCompletion = new();
        Operation<FileInfo> operation = new();

        try
        {
            using QPdfNet.Job job = new();
            var empty = job.Empty();

            foreach (FileDto file in fileList)
            {
                empty.Pages(file.FilePath, string.Empty);
            }

            switch (_settings!.CompressionMode)
            {
                case (Core.Constants.CompressionMode.High):
                    job.CompressionLevel(9)
                        .CompressStreams(true)
                        .StreamData(StreamData.Compress)
                        .RecompressFlate()
                        .ObjectStreams(ObjectStreams.Generate)
                        .OptimizeImages();
                    break;
                case (Core.Constants.CompressionMode.Medium):
                    job.CompressionLevel(7)
                        .CompressStreams(true)
                        .StreamData(StreamData.Compress)
                        .RecompressFlate()
                        .ObjectStreams(ObjectStreams.Generate)
                        .OptimizeImages()
                        .Linearize();
                    break;
                case (Core.Constants.CompressionMode.Low):
                    job.CompressionLevel(3)
                        .OptimizeImages()
                        .Linearize();
                    break;
            };

            ExitCode result = job.OutputFile(filePathOut).Run(out _);

            if (result == ExitCode.Success)
            {
                operation.SetResult(new FileInfo(filePathOut));
            }
        }
        catch (Exception e)
        {
            operation.SetFailed(e.Message);
        }

        taskCompletion.SetResult(operation);
        return taskCompletion.Task;
    }

    private static Task<Operation> RunGsOptimiserAsync(string executablePath, string command, CancellationToken cancellationToken)
    {
        TaskCompletionSource<Operation> taskCompletion = new();

        try
        {
            System.Diagnostics.ProcessStartInfo startInfo = new()
            {
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                FileName = executablePath,
                Arguments = command
            };

            System.Diagnostics.Process process = new()
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                Operation op = new();
                if (process.ExitCode == Core.Constants.ReturnCode.Success)
                {
                    op.SetSucceeded();
                }
                taskCompletion.SetResult(op);
                process.Dispose();
            };

            process.Start();
        }
        catch (Exception e)
        {
            taskCompletion.SetResult(new Operation(message: e.Message));
        }

        return taskCompletion.Task;
    }

    public async Task<Operation> SplitAsync(FileDto file, CancellationToken cancellationToken)
    {
        Operation operation = new();

        try
        {
            ConfigurationDto settings = await configurationService.LoadConfigurationAsync(cancellationToken);

            string fileName = file.FilePath.TrimEnd(".pdf".ToCharArray());

            if (settings.IsOutputFolderInUse && !string.IsNullOrEmpty(settings.OutputFolderPath))
            {
                Directory.CreateDirectory(settings.OutputFolderPath);
                fileName = Path.Combine(settings.OutputFolderPath, file.FileName.TrimEnd(".pdf".ToCharArray()));
            }

            string targetFile = $"{fileName}{settings.Suffix}.pdf";

            WeakReferenceMessenger.Default.Send(new StatusOperationMessage($"Splitting pages..."));

            using QPdfNet.Job job = new();
            job.InputFile(file.FilePath)
                .SplitPages()
                .RemoveUnreferencedResources(AutoYesNo.Auto);

            ExitCode result = job.OutputFile(targetFile).Run(out _);
            if (result == ExitCode.Success)
            {
                operation.SetSucceeded();
            }
        }
        catch (Exception e)
        {
            operation.SetFailed(e.Message);
        }

        return operation;
    }
}
