/*  PP.PdfBoss.ViewModels\Shell\ShellViewModel.cs
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
using CommunityToolkit.Mvvm.Messaging;

using PP.PdfBoss.Core.MessageTypes;
using PP.PdfBoss.Util;
using PP.PdfBoss.Views.AppSettings;
using PP.PdfBoss.Views.Home;

namespace PP.PdfBoss.ViewModels.Shell;

public partial class ShellViewModel(
    DashboardView dashboardContent,
    AboutView aboutContent,
    AppSettingsView appSettingsContent,
    ToolsView toolsContent) : ObservableValidator
{
    [ObservableProperty]
    private object _currentView = toolsContent;

    [ObservableProperty]
    private object _statusBarOperationContent = string.Empty;

    [ObservableProperty]
    private object _statusBarInfoContent = string.Empty;

    private readonly object _dashboardContent = dashboardContent;
    private readonly object _aboutContent = aboutContent;
    private readonly object _appSettingsContent = appSettingsContent;
    private readonly object _toolsContent = toolsContent;

    private bool _isInitialized = false;

    [RelayCommand]
    private void Loaded()
    {
        if (!_isInitialized)
        {
            _isInitialized = true;

            WeakReferenceMessenger.Default.Register<StatusOperationMessage>(this, (r, m) =>
            {
                StatusBarOperationContent = m.Value;
                AddConsoleMessage(m.Value);
            });

            WeakReferenceMessenger.Default.Register<StatusInfoMessage>(this, (r, m) =>
            {
                StatusBarInfoContent = m.Value;
                AddConsoleMessage(m.Value);
            });

            WeakReferenceMessenger.Default.Register<ConsoleMessage>(this, (r, m) =>
            {
                AddConsoleMessage(m.Value);
            });
        }
    }

    [RelayCommand]
    private void NavigateToAbout() =>
        CurrentView = _aboutContent;

    [RelayCommand]
    private void NavigateToDashboard() =>
        CurrentView = _dashboardContent;

    [RelayCommand]
    private void NavigateToSettings() =>
        CurrentView = _appSettingsContent;

    [RelayCommand]
    private void NavigateToTools() =>
        CurrentView = _toolsContent;

    [RelayCommand]
    private static void MenuItemToolsMouseEnter() =>
        WeakReferenceMessenger.Default.Send(
            new StatusInfoMessage("The tools available to use in this app."));

    [RelayCommand]
    private static void MenuItemDashboardMouseEnter() =>
        WeakReferenceMessenger.Default.Send(
            new StatusInfoMessage("The dashboard shows some usage stats from this app."));

    [RelayCommand]
    private static void MenuItemSettingsMouseEnter() =>
        WeakReferenceMessenger.Default.Send(
            new StatusInfoMessage("The settings panel helps you configure this application."));

    [RelayCommand]
    private static void MenuItemInfoMouseEnter() =>
        WeakReferenceMessenger.Default.Send(
            new StatusInfoMessage("The info panel shows details about this application."));

    [ObservableProperty]
    private bool _isBottomExpanderExpanded = false;

    [ObservableProperty]
    private string? _consoleText;

    private readonly ConsoleLogger _consoleLogger = new(maxLines: 10);

    private void AddConsoleMessage(string? message)
    {
        ConsoleText = _consoleLogger.Push(message);
    }

    internal void ToggleExpander()
    {
        IsBottomExpanderExpanded = !IsBottomExpanderExpanded;
    }
}
