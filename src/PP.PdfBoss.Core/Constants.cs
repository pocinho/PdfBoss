/*  PP.PdfBoss.Core\Constants.cs
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

using System.IO;

using PP.PdfBoss.Core.Models;

namespace PP.PdfBoss.Core;

public record Constants
{
    public static readonly string License = @"Copyright 2024 Paulo Pocinho.

Licensed under the Apache License, Version 2.0 (the ""License""); you may not use this file except in compliance with the License. You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an ""AS IS"" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.";

    public static readonly string VersionInfo = "PdfBoss (version 0.4.0.0)";
    public static readonly string PdfBossDocs = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PdfBoss");
    public static readonly string LogFile = Path.Combine(AppContext.BaseDirectory, "log\\app.log");
    public static readonly string PdfBossDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PdfBoss");
    public static readonly string ConfigurationFile = Path.Combine(PdfBossDir, "config.json");
    public static readonly string StatisticsFile = Path.Combine(PdfBossDir, "stats.json");

    public const int OneMiBInBytes = 1048576;
    public const int HalfMiBInBytes = 524288;

    public const int MaxParallelTasks = 4;

    public record ReturnCode
    {
        public const int Success = 0;
    }

    public record ProcessMode
    {
        public const int IndividualFiles = 0;
        public const int MergeFiles = 1;
    }

    public static readonly IEnumerable<ProcessModeDto> ProcessModeList =
    [
        new ProcessModeDto(ProcessMode.IndividualFiles, "Individual Files"),
        new ProcessModeDto(ProcessMode.MergeFiles, "Merge Files")
    ];

    public record CompressionMode
    {
        public const int High = 0;
        public const int Medium = 1;
        public const int Low = 2;
    }

    public static readonly IEnumerable<ProcessModeDto> CompressionModeList =
    [
        new ProcessModeDto(CompressionMode.High, "High"),
        new ProcessModeDto(CompressionMode.Medium, "Medium"),
        new ProcessModeDto(CompressionMode.Low, "Low")
    ];

    public record Defaults
    {
        public const int ProcessingType = 0;
        public const int CompressionType = 1;
        public static readonly string SuffixName = "_PdfBoss";
        public static readonly string MergedName = "PdfBoss_merged";
        public const bool IsOutputFolderInUse = true;
        public static readonly string MergedPath = PdfBossDocs;
        public static readonly string OutputFolderPath = PdfBossDocs;
        public const bool IsGhostScriptEnabled = false;
        public static readonly string GhostScriptPath = string.Empty;
    }
}
