﻿/*  PP.PdfBoss.Core\Dtos\ConfigurationDto.cs
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

namespace PP.PdfBoss.Core.Dtos;

public record ConfigurationDto(
    int ProcessMode,
    int CompressionMode,
    string Suffix,
    string MergedFileName,
    bool IsOutputFolderInUse,
    string? OutputFolderPath,
    bool IsGhostScriptEnabled,
    string? GhostScriptPath
    );