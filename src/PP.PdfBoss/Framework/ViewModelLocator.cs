/*  PP.PdfBoss\Framework\ViewModelLocator.cs
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

using CommunityToolkit.Mvvm.DependencyInjection;

using PP.PdfBoss.Core.Interfaces;
using PP.PdfBoss.ViewModels.AppSettings.Dialogs;
using PP.PdfBoss.ViewModels.Home;
using PP.PdfBoss.ViewModels.Home.Dialogs;
using PP.PdfBoss.ViewModels.Shell;
using PP.PdfBoss.ViewModels.Shell.Dialogs;

namespace PP.PdfBoss.Framework;

public record ViewModelLocator
{
    public static ShellViewModel ShellViewModel
        => Ioc.Default.GetRequiredService<ShellViewModel>();

    public static LogDialogViewModel LogDialogViewModel
    => Ioc.Default.GetRequiredService<LogDialogViewModel>();

    public static AboutViewModel AboutViewModel
        => Ioc.Default.GetRequiredService<AboutViewModel>();

    public static DashboardViewModel DashboardViewModel
        => Ioc.Default.GetRequiredService<DashboardViewModel>();

    public static IAppSettingsViewModel AppSettingsViewModel
        => Ioc.Default.GetRequiredService<IAppSettingsViewModel>();

    public static IToolsViewModel ToolsViewModel
        => Ioc.Default.GetRequiredService<IToolsViewModel>();

    public static CreateNameDialogViewModel CreateNameDialogViewModel
        => Ioc.Default.GetRequiredService<CreateNameDialogViewModel>();

    public static ProcessingDialogViewModel ProcessingDialogViewModel
        => Ioc.Default.GetRequiredService<ProcessingDialogViewModel>();
}
