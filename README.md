# Introduction
There are plenty of tools that modify PDF files. Some are free and online. The objective of this project is to create a practical interface to merge, split and optimise PDF files, offline, with reasonable defaults.

For general usage guidelines, please read the manual in doc/PdfBossManual.md.

# Features
- .Net 8.0
- CommunityToolkit MVVM
- Material Design in XAML (with system aware dark mode)
- Centralized Package Management
- .editorconfig formatting

# Getting Started

Requires the [.NET Desktop Runtime 8.0.4 (x64)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

Please check the [instructions on how to install](https://learn.microsoft.com/en-us/dotnet/core/install/windows?tabs=net80).

## Installation

Versions available in the build folder:
- PdfBoss-0.4.0.0-ClickOnce.zip

Unzip and run setup. This ClickOnce installer checks your machine for the requirements, installs and runs PdfBoss.

To uninstall, use "Installed Apps" on Windows. (The folder "C:\Users\username\AppData\Local\PdfBoss" contains saved settings and statistics, you may have to delete this folder as well.)

## Build and Test
Use the latest .NET 8.0 SDK.

Run 'dotnet build' and/or 'dotnet run' in src/PP.PdfBoss/.

## Contribute
Checkout a local branch from develop.
Make the changes in the local branch you created.
Create a pull request to target branch develop.

# License

Copyright 2024 Paulo Pocinho.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.