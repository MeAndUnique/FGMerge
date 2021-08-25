using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using FGMerge.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FGMerge
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private IHost _host;

        protected override void OnStartup(StartupEventArgs e)
        {
            _host = CreateHostBuilder(e.Args).Start();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
            _host.Dispose();
            base.OnExit(e);
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(config =>
                {
                    ConfigureAppConfiguration(config, args);
                })
                .ConfigureServices(ConfigureServices);
        }

        private static void ConfigureAppConfiguration(IConfigurationBuilder config, string[] args)
        {
            Dictionary<string, string> switchMappings = new()
            {
                { "--e", nameof(AppSettings.ExecutionMode) },
                { "--mode", nameof(AppSettings.ExecutionMode) },
                { "--b", nameof(AppSettings.BaseFile) },
                { "--base", nameof(AppSettings.BaseFile) },
                { "--l", nameof(AppSettings.LocalFile) },
                { "--local", nameof(AppSettings.LocalFile) },
                { "--m", nameof(AppSettings.MergedFile) },
                { "--merged", nameof(AppSettings.MergedFile) },
                { "--r", nameof(AppSettings.RemoteFile) },
                { "--remote", nameof(AppSettings.RemoteFile) },
                { "--dc", nameof(AppSettings.DefaultDiffCommand) },
                { "--mc", nameof(AppSettings.DefaultMergeCommand) },
                { "--a", nameof(AppSettings.AutoResolveMerge) },
                { "--auto", nameof(AppSettings.AutoResolveMerge) },
            };

            config.Sources.Clear();
            config.AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"), false, false);
            config.AddCommandLine(args, switchMappings);
        }

        private static void ConfigureServices(HostBuilderContext hostingContext, IServiceCollection services)
        {
            services.Configure<AppSettings>(hostingContext.Configuration);
            services.AddHostedService<LauncherService>();

            services.AddScoped<IShutdownService, ShutdownService>();
            services.AddScoped<IFileLoader, FileLoader>();
            services.AddScoped<IFileWriter, FileWriter>();
            services.AddScoped<IMergeCalculator, MergeCalculator>();
            services.AddScoped<IMergeResolver, MergeResolver>();

            services.AddScoped<IErrorView, ErrorWindow>();
            services.AddScoped<IMergeView, MergeWindow>();
        }
    }
}
