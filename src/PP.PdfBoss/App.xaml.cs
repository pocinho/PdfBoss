/*  PP.PdfBoss\App.xaml.cs
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
using System.Windows;

using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;

using MaterialDesignThemes.Wpf;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

using NLog;
using NLog.Extensions.Logging;
using NLog.Targets;

using PP.PdfBoss.Core.Interfaces;
using PP.PdfBoss.Core.MessageTypes;
using PP.PdfBoss.Data.Services;
using PP.PdfBoss.Handlers;
using PP.PdfBoss.ViewModels.AppSettings;
using PP.PdfBoss.ViewModels.AppSettings.Dialogs;
using PP.PdfBoss.ViewModels.Home;
using PP.PdfBoss.ViewModels.Home.Dialogs;
using PP.PdfBoss.ViewModels.Shell;
using PP.PdfBoss.ViewModels.Shell.Dialogs;
using PP.PdfBoss.Views.AppSettings;
using PP.PdfBoss.Views.AppSettings.Dialogs;
using PP.PdfBoss.Views.Home;
using PP.PdfBoss.Views.Home.Dialogs;
using PP.PdfBoss.Views.Shell;
using PP.PdfBoss.Views.Shell.Dialogs;

namespace PP.PdfBoss;

public partial class App : Application
{
    private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
    private const string MutexName = "F4EF96F9-72BB-4F1A-8E46-7CEC5ABB6AE1";
    private readonly Mutex _mutex;
#pragma warning disable IDE0044 // Add readonly modifier
    private bool _isUnique;
#pragma warning restore IDE0044 // Add readonly modifier

    public App()
    {
        _mutex = new Mutex(true, MutexName, out _isUnique);

        if (!_isUnique)
        {
            Util.Windows.ShowExistingWindow();
            Application.Current.Shutdown(0);
        }

        ConfigureLogging();
        _logger.Log(NLog.LogLevel.Debug, "Initializing...");
    }

    private static void ConfigureLogging()
    {
        NLog.LogManager.Setup().LoadConfiguration(builder =>
        {
            NLog.Layouts.JsonLayout layout = new()
            {
                EscapeForwardSlash = false,
                Attributes =
                {
                    new NLog.Layouts.JsonAttribute()
                    {
                        Name = "Version",
                        Layout = Core.Constants.VersionInfo
                    },
                    new NLog.Layouts.JsonAttribute()
                    {
                        Name = "DateTimeUtc",
                        Layout = "${date:universalTime=true:format=O}"
                    },
                    new NLog.Layouts.JsonAttribute()
                    {
                        Name = "Level",
                        Layout = "${level}"
                    },
                     new NLog.Layouts.JsonAttribute()
                    {
                        Name = "Type",
                        Layout = "${logger}"
                    },
                    new NLog.Layouts.JsonAttribute()
                    {
                        Name = "Message",
                        Layout = "${message}"
                    },
                    new NLog.Layouts.JsonAttribute()
                    {
                        Name = "Exception",
                        Layout = "${exception:format=ToString}"
                    }
                }
            };

            builder.ForLogger()
#if (DEBUG)
                .FilterMinLevel(NLog.LogLevel.Debug)
#else
                .FilterMinLevel(NLog.LogLevel.Info)
#endif
                .WriteToFile(
                    fileName: Core.Constants.LogFile,
                    archiveAboveSize: Core.Constants.HalfMiBInBytes,
                    maxArchiveFiles: 1,
                    encoding: System.Text.Encoding.UTF8,
                    layout: layout);

            builder.ForLogger()
                .FilterMinLevel(NLog.LogLevel.Warn)
                .WriteToMethodCall((logEvent, args)
                    => LogHandler.ShowDialogLog(logEvent));
        });
    }

    private static void ConfigureLog()
    {
        // Targets

        MethodCallTarget showLogTarget = new()
        {
            ClassName = typeof(LogHandler).AssemblyQualifiedName,
            MethodName = nameof(LogHandler.ShowDialogLog)
        };

        showLogTarget.Parameters.Add(new MethodCallParameter(""));

    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (!Directory.Exists(Core.Constants.PdfBossDir))
        {
            Directory.CreateDirectory(Core.Constants.PdfBossDir);
        }

        Ioc.Default.ConfigureServices(new ServiceCollection()

            .AddLogging(options =>
            {
                options.ClearProviders();
                options.AddNLog();
            })

            // Settings
            .AddSingleton<AppSettingsView>()
            .AddSingleton<IAppSettingsViewModel, AppSettingsViewModel>()
            .AddTransient<CreateNameDialogView>()
            .AddTransient<CreateNameDialogViewModel>()

            // Home
            .AddSingleton<AboutView>()
            .AddSingleton<AboutViewModel>()
            .AddSingleton<DashboardView>()
            .AddSingleton<DashboardViewModel>()
            .AddSingleton<ToolsView>()
            .AddSingleton<IToolsViewModel, ToolsViewModel>()
            .AddTransient<ProcessingDialogView>()
            .AddTransient<ProcessingDialogViewModel>()

            // Application
            .AddSingleton<ShellView>()
            .AddSingleton<ShellViewModel>()
            .AddSingleton<LogDialogView>()
            .AddSingleton<LogDialogViewModel>()

            // Services
            .AddTransient<IConfigurationService, ConfigurationService>()
            .AddTransient<IPdfBossService, PdfBossService>()

            .BuildServiceProvider()
        );

        SystemEvents.UserPreferenceChanging += SystemEvents_UserPreferenceChanging;

        ShellView startupForm = Ioc.Default.GetRequiredService<ShellView>();
        startupForm.Show();

        WeakReferenceMessenger.Default.Send(new StatusOperationMessage("Ready."));
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        SystemEvents.UserPreferenceChanging -= SystemEvents_UserPreferenceChanging;
    }

    private void SystemEvents_UserPreferenceChanging(object sender, UserPreferenceChangingEventArgs e)
    {
        PaletteHelper paletteHelper = new();
        Theme theme = paletteHelper.GetTheme();

        if (Util.Theme.IsDarkModeThemeUsed())
        {
            theme.SetBaseTheme(BaseTheme.Dark);
        }
        else
        {
            theme.SetBaseTheme(BaseTheme.Light);
        }

        paletteHelper.SetTheme(theme);
    }
}
