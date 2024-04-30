/*  PP.PdfBoss\Handlers\LogHandler.cs
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

using MaterialDesignThemes.Wpf;

using NLog;

using PP.PdfBoss.ViewModels.Shell.Dialogs;
using PP.PdfBoss.Views.Shell.Dialogs;

namespace PP.PdfBoss.Handlers;

public class LogHandler
{
    public static async void ShowDialogLog(LogEventInfo? logEvent)
    {
        try
        {
            if (logEvent == null)
                return;

            LogDialogView logView = Ioc.Default.GetRequiredService<LogDialogView>();
            (logView.DataContext as LogDialogViewModel)?.LoadData(logEvent);
            _ = await DialogHost.Show(logView, "ShellDialog");
        }
        catch (Exception)
        {
        }
    }
}
