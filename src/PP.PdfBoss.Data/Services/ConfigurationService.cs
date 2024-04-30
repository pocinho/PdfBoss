/*  PP.PdfBoss.Data\Services\ConfigurationService.cs
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

using Microsoft.Extensions.Logging;

using PP.PdfBoss.Core.Dtos;
using PP.PdfBoss.Core.Interfaces;

namespace PP.PdfBoss.Data.Services;

public class ConfigurationService(ILogger<ConfigurationService> logger) : IConfigurationService
{
    public async Task<ConfigurationDto> LoadConfigurationAsync(CancellationToken cancellationToken = default)
    {
        ConfigurationDto dto = new(
            Core.Constants.Defaults.ProcessingType,
            Core.Constants.Defaults.CompressionType,
            Core.Constants.Defaults.SuffixName,
            Core.Constants.Defaults.MergedName,
            Core.Constants.Defaults.IsOutputFolderInUse,
            Core.Constants.Defaults.OutputFolderPath,
            Core.Constants.Defaults.IsGhostScriptEnabled,
            Core.Constants.Defaults.GhostScriptPath
            );

        try
        {
            if (!File.Exists(Core.Constants.ConfigurationFile))
            {
                string configWrite = JsonSerializer.Serialize(dto);
                await File.WriteAllTextAsync(Core.Constants.ConfigurationFile, configWrite, cancellationToken);
            }
            else
            {
                string configRead = await File.ReadAllTextAsync(Core.Constants.ConfigurationFile, cancellationToken);
                dto = JsonSerializer.Deserialize<ConfigurationDto>(configRead)!;
            }
        }
        catch (Exception e)
        {
            logger.LogError("{m}", e.Message);
        }

        return dto;
    }

    public async Task<StatisticsDto> LoadStatisticsAsync(CancellationToken cancellationToken = default)
    {
        StatisticsDto dto = new(0, 0, 0, 0);

        try
        {
            if (!File.Exists(Core.Constants.StatisticsFile))
            {
                string statsWrite = JsonSerializer.Serialize(dto);
                await File.WriteAllTextAsync(Core.Constants.StatisticsFile, statsWrite, cancellationToken);
            }
            else
            {
                string statsRead = await File.ReadAllTextAsync(Core.Constants.StatisticsFile, cancellationToken);
                dto = JsonSerializer.Deserialize<StatisticsDto>(statsRead)!;
            }
        }
        catch (Exception e)
        {
            logger.LogError("{m}", e.Message);
        }

        return dto;
    }

    public async Task SaveConfigurationAsync(ConfigurationDto config, CancellationToken cancellationToken = default)
    {
        try
        {
            string configWrite = JsonSerializer.Serialize(config);
            await File.WriteAllTextAsync(Core.Constants.ConfigurationFile, configWrite, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError("{m}", e.Message);
        }
    }

    public async Task SaveStatisticsAsync(StatisticsDto updatedStats, CancellationToken cancellationToken = default)
    {
        try
        {
            string statsWrite = JsonSerializer.Serialize(updatedStats);
            await File.WriteAllTextAsync(Core.Constants.StatisticsFile, statsWrite, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError("{m}", e.Message);
        }
    }
}
