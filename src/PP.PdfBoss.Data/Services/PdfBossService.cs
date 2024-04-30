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
using System.Text;

using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.Logging;

using PP.PdfBoss.Core.Dtos;
using PP.PdfBoss.Core.Interfaces;
using PP.PdfBoss.Core.MessageTypes;
using PP.PdfBoss.Core.Models;

using QPdfNet.Enums;

using static PP.PdfBoss.Core.Constants;

namespace PP.PdfBoss.Data.Services;

public class PdfBossService(
    ILogger<IPdfBossService> logger,
    IConfigurationService configurationService) : IPdfBossService
{
    public async Task OptimiseAsync(IEnumerable<FileDto> fileList, CancellationToken cancellationToken = default)
    {
        try
        {
            ConfigurationDto settings = await configurationService.LoadConfigurationAsync(cancellationToken);

            string command = string.Empty;
            if (settings.IsGhostScriptEnabled)
            {
                command = settings.CompressionMode switch
                {
                    CompressionType.High => "/screen",
                    CompressionType.Medium => "/ebook",
                    CompressionType.Low => "/printer",
                    _ => "/ebook",
                };
            }

            ConcurrentBag<Operation<ProcessResultDto>> filesProcessed = [];

            uint totalFiles = (uint)fileList.Count();
            uint fileCount = 1;

            if (settings.IsOutputFolderInUse && !string.IsNullOrEmpty(settings.OutputFolderPath))
            {
                if (!Path.Exists(settings.OutputFolderPath))
                {
                    Directory.CreateDirectory(settings.OutputFolderPath);
                }

                if (settings.ProcessMode == ProcessingType.IndividualFiles)
                {
                    foreach (FileDto file in fileList)
                    {
                        WeakReferenceMessenger.Default.Send(new ProgressMessage(fileCount * 100 / totalFiles + 1));
                        WeakReferenceMessenger.Default.Send(new StatusOperationMessage($"Processing file {fileCount++} of {totalFiles}..."));

                        Operation<ProcessResultDto> resultOp = new();

                        string fileName = Path.Combine(settings.OutputFolderPath, file.FileName.TrimEnd(".pdf".ToCharArray()));
                        string targetFile = $"{fileName}{settings.Suffix}.pdf";

                        if (settings.IsGhostScriptEnabled && File.Exists(settings.GhostScriptPath))
                        {
                            string firstPass = $"{targetFile}.tmp";

                            await RunGsOptimiserAsync(settings.GhostScriptPath,
                                $" -sDEVICE=pdfwrite -dCompatibilityLevel=\"1.7\" -dPDFSETTINGS={command} -dNOPAUSE -dQUIET -dBATCH -sOutputFile=\"{firstPass}\" \"{file.FilePath}\" ");

                            Operation<FileInfo> op = await RunOptimiserAsync(filePathIn: firstPass, filePathOut: targetFile, settings.CompressionMode);
                            if (op.HasSucceeded)
                            {
                                FileInfo info = new(file.FilePath);
                                resultOp.SetResult(new ProcessResultDto(
                                        TotalBytesProcessed: info.Length,
                                        TotalBytesOptimized: op.Result!.Length));
                            }

                            if (File.Exists(firstPass))
                                File.Delete(firstPass);
                        }
                        else
                        {
                            Operation<FileInfo> op = await RunOptimiserAsync(filePathIn: file.FilePath, filePathOut: targetFile, settings.CompressionMode);
                            if (op.HasSucceeded)
                            {
                                FileInfo info = new(file.FilePath);
                                resultOp.SetResult(new ProcessResultDto(
                                        TotalBytesProcessed: info.Length,
                                        TotalBytesOptimized: op.Result!.Length));
                            }
                        }

                        filesProcessed.Add(resultOp);
                    };
                }
                else
                if (settings.ProcessMode == ProcessingType.MergeFiles)
                {
                    string targetFile = Path.Combine(settings.OutputFolderPath, $"{settings.MergedFileName}.pdf");
                    Operation<ProcessResultDto> resultOp = new();

                    WeakReferenceMessenger.Default.Send(new ProgressMessage(totalFiles));
                    WeakReferenceMessenger.Default.Send(new StatusOperationMessage($"Processing {totalFiles} files..."));

                    if (settings.IsGhostScriptEnabled && File.Exists(settings.GhostScriptPath))
                    {
                        string firstPass = $"{targetFile}.tmp";

                        StringBuilder fileCompilation = new();

                        foreach (FileDto file in fileList)
                        {
                            fileCompilation.Append($"\"{file.FilePath}\" ");
                        }

                        await RunGsOptimiserAsync(settings.GhostScriptPath,
                            $" -sDEVICE=pdfwrite -dCompatibilityLevel=\"1.7\" -dPDFSETTINGS={command} -dNOPAUSE -dQUIET -dBATCH -sOutputFile=\"{firstPass}\" {fileCompilation}");

                        Operation<FileInfo> op = await RunOptimiserAsync(filePathIn: firstPass, filePathOut: targetFile, settings.CompressionMode);
                        if (op.HasSucceeded)
                        {
                            resultOp.SetResult(new ProcessResultDto(
                                TotalBytesProcessed: fileList.Sum(f => new FileInfo(f.FilePath).Length),
                                TotalBytesOptimized: op.Result!.Length));
                        }

                        if (File.Exists(firstPass))
                            File.Delete(firstPass);
                    }
                    else
                    {
                        Operation<FileInfo> op = await RunOptimiserAsync(fileList, filePathOut: targetFile, settings.CompressionMode);
                        if (op.HasSucceeded)
                        {
                            resultOp.SetResult(new ProcessResultDto(
                                TotalBytesProcessed: fileList.Sum(m => new FileInfo(m.FilePath).Length),
                                TotalBytesOptimized: op.Result!.Length));
                        }
                    }

                    filesProcessed.Add(resultOp);
                }
            }
            else
            {
                if (settings.ProcessMode == ProcessingType.IndividualFiles)
                {
                    foreach (FileDto file in fileList)
                    {
                        WeakReferenceMessenger.Default.Send(new ProgressMessage(fileCount * 100 / totalFiles + 1));
                        WeakReferenceMessenger.Default.Send(new StatusOperationMessage($"Processing file {fileCount++} of {totalFiles}..."));

                        Operation<ProcessResultDto> resultOp = new();

                        string fileName = file.FilePath.TrimEnd(".pdf".ToCharArray());
                        string targetFile = $"{fileName}{settings.Suffix}.pdf";

                        if (settings.IsGhostScriptEnabled && File.Exists(settings.GhostScriptPath))
                        {
                            string firstPass = $"{targetFile}.tmp";

                            await RunGsOptimiserAsync(settings.GhostScriptPath,
                                $" -sDEVICE=pdfwrite -dCompatibilityLevel=\"1.7\" -dPDFSETTINGS={command} -dNOPAUSE -dQUIET -dBATCH -sOutputFile=\"{firstPass}\" \"{file.FilePath}\" ");

                            Operation<FileInfo> op = await RunOptimiserAsync(filePathIn: firstPass, filePathOut: targetFile, settings.CompressionMode);
                            if (op.HasSucceeded)
                            {
                                FileInfo info = new(file.FilePath);
                                resultOp.SetResult(new ProcessResultDto(
                                        TotalBytesProcessed: info.Length,
                                        TotalBytesOptimized: op.Result!.Length));
                            }

                            if (File.Exists(firstPass))
                                File.Delete(firstPass);
                        }
                        else
                        {
                            Operation<FileInfo> op = await RunOptimiserAsync(filePathIn: file.FilePath, filePathOut: targetFile, settings.CompressionMode);
                            if (op.HasSucceeded)
                            {
                                FileInfo info = new(file.FilePath);
                                resultOp.SetResult(new ProcessResultDto(
                                        TotalBytesProcessed: info.Length,
                                        TotalBytesOptimized: op.Result!.Length));
                            }
                        }

                        filesProcessed.Add(resultOp);
                    };
                }
                else
                {
                    if (!Path.Exists(Defaults.MergedPath))
                    {
                        Directory.CreateDirectory(Defaults.MergedPath);
                    }

                    string targetFile = Path.Combine(Defaults.MergedPath, $"{settings.MergedFileName}.pdf");
                    Operation<ProcessResultDto> resultOp = new();

                    WeakReferenceMessenger.Default.Send(new ProgressMessage(totalFiles));
                    WeakReferenceMessenger.Default.Send(new StatusOperationMessage($"Processing {totalFiles} files..."));

                    if (settings.IsGhostScriptEnabled && File.Exists(settings.GhostScriptPath))
                    {
                        string firstPass = $"{targetFile}.tmp";

                        StringBuilder fileCompilation = new();

                        foreach (FileDto file in fileList)
                        {
                            fileCompilation.Append($"\"{file.FilePath}\" ");
                        }

                        await RunGsOptimiserAsync(settings.GhostScriptPath,
                            $" -sDEVICE=pdfwrite -dCompatibilityLevel=\"1.7\" -dPDFSETTINGS={command} -dNOPAUSE -dQUIET -dBATCH -sOutputFile=\"{firstPass}\" {fileCompilation}");

                        Operation<FileInfo> op = await RunOptimiserAsync(filePathIn: firstPass, filePathOut: targetFile, settings.CompressionMode);
                        if (op.HasSucceeded)
                        {
                            resultOp.SetResult(new ProcessResultDto(
                                TotalBytesProcessed: fileList.Sum(f => new FileInfo(f.FilePath).Length),
                                TotalBytesOptimized: op.Result!.Length));
                        }

                        if (File.Exists(firstPass))
                            File.Delete(firstPass);
                    }
                    else
                    {
                        Operation<FileInfo> op = await RunOptimiserAsync(fileList, filePathOut: targetFile, settings.CompressionMode);
                        if (op.HasSucceeded)
                        {
                            resultOp.SetResult(new ProcessResultDto(
                                TotalBytesProcessed: fileList.Sum(m => new FileInfo(m.FilePath).Length),
                                TotalBytesOptimized: op.Result!.Length));
                        }
                    }

                    filesProcessed.Add(resultOp);
                }
            }

            StatisticsDto savedStats = await configurationService.LoadStatisticsAsync(cancellationToken);

            uint totalFilesProcessed = (uint)filesProcessed.Count;
            decimal totalBytesProcessed = filesProcessed.Where(op => op.HasSucceeded).Sum(op => op.Result!.TotalBytesProcessed);
            decimal totalBytesOptimized = filesProcessed.Where(op => op.HasSucceeded).Sum(op => op.Result!.TotalBytesOptimized);
            decimal totalBytesSaved = totalBytesProcessed - totalBytesOptimized;

            StatisticsDto updatedStats = new(
                TotalFilesProcessed: savedStats.TotalFilesProcessed + totalFilesProcessed,
                TotalMegabytesProcessed: savedStats.TotalMegabytesProcessed + (totalBytesProcessed / OneMiBInBytes),
                TotalMegabytesOptimised: savedStats.TotalMegabytesOptimised + (totalBytesOptimized / OneMiBInBytes),
                TotalMegabytesSaved: savedStats.TotalMegabytesSaved + (totalBytesSaved / OneMiBInBytes)
                );

            await configurationService.SaveStatisticsAsync(updatedStats, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError("{m}", e.Message);
        }
    }

    private static Task<Operation<FileInfo>> RunOptimiserAsync(IEnumerable<FileDto> fileList, string filePathOut, int compression)
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

            int compressionLevel = compression switch
            {
                CompressionType.High => 9,
                CompressionType.Medium => 7,
                CompressionType.Low => 3,
                _ => 7,
            };

            job.OutputFile(filePathOut)
                .CompressionLevel(compressionLevel);

            if (compression == CompressionType.High)
            {
                job.CompressStreams(true)
                .StreamData(StreamData.Compress)
                .RecompressFlate()
                .ObjectStreams(ObjectStreams.Generate)
                .OptimizeImages();
            }
            else
            if (compression == CompressionType.Medium)
            {
                job.CompressStreams(true)
                .StreamData(StreamData.Compress)
                .RecompressFlate()
                .ObjectStreams(ObjectStreams.Generate)
                .OptimizeImages()
                .Linearize();
            }
            else
            if (compression == CompressionType.Low)
            {
                job.OptimizeImages()
                .Linearize();
            }

            ExitCode result = job.Run(out _);

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

    private static Task<Operation<FileInfo>> RunOptimiserAsync(string filePathIn, string filePathOut, int compression)
    {
        TaskCompletionSource<Operation<FileInfo>> taskCompletion = new();
        Operation<FileInfo> operation = new();

        try
        {
            int compressionLevel = compression switch
            {
                CompressionType.High => 9,
                CompressionType.Medium => 7,
                CompressionType.Low => 3,
                _ => 1,
            };

            using QPdfNet.Job job = new();
            job.InputFile(filePathIn)
                .OutputFile(filePathOut)
                .CompressionLevel(compressionLevel);

            if (compression == CompressionType.High)
            {
                job.CompressStreams(true)
                .StreamData(StreamData.Compress)
                .RecompressFlate()
                .ObjectStreams(ObjectStreams.Generate)
                .OptimizeImages();
            }
            else
            if (compression == CompressionType.Medium)
            {
                job.CompressStreams(true)
                .StreamData(StreamData.Compress)
                .RecompressFlate()
                .ObjectStreams(ObjectStreams.Generate)
                .OptimizeImages()
                .Linearize();
            }
            else
            if (compression == CompressionType.Low)
            {
                job.OptimizeImages()
                .Linearize();
            }

            ExitCode result = job.Run(out _);

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

    private static Task<int> RunGsOptimiserAsync(string executablePath, string command)
    {
        TaskCompletionSource<int> taskCompletion = new();

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
            taskCompletion.SetResult(process.ExitCode);
            process.Dispose();
        };

        process.Start();

        return taskCompletion.Task;
    }

    public async Task SplitAsync(FileDto file, CancellationToken cancellationToken)
    {
        try
        {
            ConfigurationDto settings = await configurationService.LoadConfigurationAsync(cancellationToken);
            string fileName;
            string targetFile;

            if (settings.IsOutputFolderInUse && !string.IsNullOrEmpty(settings.OutputFolderPath))
            {
                if (!Path.Exists(settings.OutputFolderPath))
                {
                    Directory.CreateDirectory(settings.OutputFolderPath);
                }

                fileName = Path.Combine(settings.OutputFolderPath, file.FileName.TrimEnd(".pdf".ToCharArray()));
                targetFile = $"{fileName}{settings.Suffix}_%d.pdf";
            }
            else
            {
                fileName = file.FilePath.TrimEnd(".pdf".ToCharArray());
                targetFile = $"{fileName}{settings.Suffix}.pdf";
            }

            WeakReferenceMessenger.Default.Send(new StatusOperationMessage($"Splitting pages..."));

            using QPdfNet.Job job = new();
            ExitCode result = job.InputFile(file.FilePath)
                .SplitPages()
                .OutputFile(targetFile)
                .CompressStreams(true)
                .StreamData(StreamData.Compress)
                .CompressionLevel(9)
                .RecompressFlate()
                .ObjectStreams(ObjectStreams.Generate)
                .OptimizeImages()
                .Run(out _);
        }
        catch (Exception e)
        {
            logger.LogError("{m}", e.Message);
        }
    }
}
