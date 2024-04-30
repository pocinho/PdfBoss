/*  PP.PdfBoss.Tests\Services\ConfigurationServiceTests.cs
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

using FakeItEasy;

using FluentAssertions;

using PP.PdfBoss.Core.Dtos;
using PP.PdfBoss.Core.Interfaces;

namespace PP.PdfBoss.Tests.Services;

public class ConfigurationServiceTests
{
    [Fact]
    public async Task OnConfigFileDoesNotExist_LoadConfigurationAsync_ShouldReturnValidConfig()
    {
        // Arrange
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

        var service = A.Fake<IConfigurationService>();
        A.CallTo(() => service.LoadConfigurationAsync(new CancellationToken())).Returns(Task.FromResult(dto));

        // Act
        var testConfig = await service.LoadConfigurationAsync(new CancellationToken());

        // Assert
        A.CallTo(() => service.LoadConfigurationAsync(new CancellationToken())).MustHaveHappened();
        testConfig.Should().NotBeNull();
        testConfig.ProcessMode.Should().Be(dto.ProcessMode);
        testConfig.CompressionMode.Should().Be(dto.CompressionMode);
        testConfig.Suffix.Should().Be(dto.Suffix);
        testConfig.MergedFileName.Should().Be(dto.MergedFileName);
        testConfig.IsOutputFolderInUse.Should().Be(dto.IsOutputFolderInUse);
        testConfig.OutputFolderPath.Should().Be(dto.OutputFolderPath);
        testConfig.IsGhostScriptEnabled.Should().Be(dto.IsGhostScriptEnabled);
        testConfig.GhostScriptPath.Should().Be(dto.GhostScriptPath);
    }
}
