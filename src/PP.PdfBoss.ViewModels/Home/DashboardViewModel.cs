/*  PP.PdfBoss.ViewModels\Home\DashboardViewModel.cs
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

using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;

using Microsoft.Extensions.Logging;

using PP.PdfBoss.Core.Dtos;
using PP.PdfBoss.Core.Interfaces;

using SkiaSharp;

namespace PP.PdfBoss.ViewModels.Home;

public partial class DashboardViewModel(
    ILogger<DashboardViewModel> logger,
    IConfigurationService configurationService) : ObservableObject
{
    private StatisticsDto? _statistics;

    [ObservableProperty]
    private string _title = "Dashboard";

    [ObservableProperty]
    private uint _totalFilesProcessed = 0;

    [ObservableProperty]
    private string? _totalSizeProcessed;

    [ObservableProperty]
    private string? _totalSizeOptimised;

    [ObservableProperty]
    private string? _totalSizeSaved;

    [ObservableProperty]
    private IEnumerable<ISeries>? _pieChartSeries;

    [RelayCommand]
    private async Task Loaded()
    {
        await CalculateRatio();
    }

    private async Task CalculateRatio()
    {
        try
        {
            _statistics = await configurationService.LoadStatisticsAsync();

            TotalFilesProcessed = _statistics.TotalFilesProcessed;
            TotalSizeProcessed = $"{decimal.Round(_statistics.TotalMegabytesProcessed, 2, MidpointRounding.AwayFromZero)} Megabytes";
            TotalSizeOptimised = $"{decimal.Round(_statistics.TotalMegabytesOptimised, 2, MidpointRounding.AwayFromZero)} Megabytes";
            TotalSizeSaved = $"{decimal.Round(_statistics.TotalMegabytesSaved, 2, MidpointRounding.AwayFromZero)} Megabytes";

            ulong gcd = Util.Math.GreatestCommonDivisor([
                    Convert.ToUInt64(_statistics.TotalMegabytesProcessed),
                    Convert.ToUInt64(_statistics.TotalMegabytesOptimised),
                    Convert.ToUInt64(_statistics.TotalMegabytesSaved)
                    ]);

            decimal sizeProcessedRatio = 1;
            decimal sizeOptimizedRatio = 1;
            decimal sizeSavedRatio = 0;

            if (gcd > 0)
            {
                sizeProcessedRatio = _statistics.TotalMegabytesProcessed / gcd;
                sizeOptimizedRatio = _statistics.TotalMegabytesOptimised / gcd;
                sizeSavedRatio = _statistics.TotalMegabytesSaved / gcd;
            }

            PieChartSeries =
            [
                new PieSeries<decimal>
                {
                    Values = [decimal.Round(sizeProcessedRatio, 2, MidpointRounding.AwayFromZero)],
                    Name = "Processed",
                    Stroke = null,
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    Fill = new SolidColorPaint(SKColors.IndianRed)
                },
                new PieSeries<decimal>
                {
                    Values = [decimal.Round(sizeOptimizedRatio, 2, MidpointRounding.AwayFromZero)],
                    Name = "Optimised",
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    Fill = new SolidColorPaint(SKColors.DeepSkyBlue)
                },
                new PieSeries<decimal>
                {
                    Values = [decimal.Round(sizeSavedRatio, 2, MidpointRounding.AwayFromZero)],
                    Name = "Saved",
                    Stroke = null,
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    Fill = new SolidColorPaint(SKColors.MediumSeaGreen)
                }
            ];
        }
        catch (Exception e)
        {
            logger.LogError("{m}", e.Message);
        }
    }
}
