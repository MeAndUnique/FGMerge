using System;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FGMerge.Views;

namespace FGMerge
{
    public class LauncherService : IHostedService
    {
        private readonly IMergeCalculator _mergeCalculator;
        private readonly IErrorView _errorView;
        private readonly IMergeView _mergeView;
        private readonly AppSettings _settings;

        public LauncherService(IOptions<AppSettings> options, IMergeCalculator mergeCalculator, IErrorView errorView, IMergeView mergeView)
        {
            _mergeCalculator = mergeCalculator;
            _errorView = errorView;
            _mergeView = mergeView;
            _settings = options.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            switch (_settings.ExecutionMode.ToLowerInvariant())
            {
                case "diff":
                    _errorView.ShowErrorMessage("Sorry, diff viewing is not yet supported.");
                    //LaunchDiff();
                    break;
                case "merge":
                    LaunchMerge();
                    break;
                default:
                    _errorView.ShowErrorMessage("An ExecutionMode parameter with a value of \"Diff\" or \"Merge\" is required.");
                    break;
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void LaunchDiff()
        {
            FileInfo? localFile = VerifyFile(nameof(_settings.LocalFile), _settings.LocalFile);
            FileInfo? remoteFile = VerifyFile(nameof(_settings.RemoteFile), _settings.RemoteFile);

            if (CheckFile(localFile) && CheckFile(remoteFile))
            {
                // TODO launch diff view
            }
            else
            {
                LaunchDefault(_settings.DefaultDiffCommand);
            }
        }

        private void LaunchMerge()
        {
            FileInfo? baseFile = VerifyFile(nameof(_settings.BaseFile), _settings.BaseFile);
            FileInfo? localFile = VerifyFile(nameof(_settings.LocalFile), _settings.LocalFile);
            FileInfo? remoteFile = VerifyFile(nameof(_settings.RemoteFile), _settings.RemoteFile);
            VerifyFile(nameof(_settings.MergedFile), _settings.MergedFile);

            if(CheckFile(baseFile) && CheckFile(localFile) && CheckFile(remoteFile))
            {
                var mergeGroups = _mergeCalculator.Calculate(baseFile!, localFile!, remoteFile!);
                if (_settings.AutoResolveMerge && mergeGroups.SelectMany(group=> group.Nodes).All(node => node.MergedNode != null))
                {
                    //TODO wtf man
                }
                else
                {
                    _mergeView.Show(mergeGroups);
                }
            }
            else
            {
                LaunchDefault(_settings.DefaultMergeCommand);
            }
        }

        private FileInfo? VerifyFile(string name, string file)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                _errorView.ShowErrorMessage($"A {name} parameter must be provided.");
                return null;
            }

            try
            {
                return new FileInfo(file);
            }
            catch
            {
                _errorView.ShowErrorMessage($"\"{file}\" is not usable for {name}.");
                return null;
            }
        }

        private bool CheckFile(FileInfo? file)
        {
            // TODO consider more robust logic to check the contents.
            return file != null && file.Exists && file.Extension == ".xml";
        }

        private void LaunchDefault(string command)
        {
            char split = ' ';
            if (command[0] == '"')
            {
                split = '"';
            }

            string[] parts = command.Split(split, 2, StringSplitOptions.RemoveEmptyEntries);
            string exe = parts[0];
            string args = parts[1]
                .Replace("$BASE", _settings.BaseFile)
                .Replace("$LOCAL", _settings.LocalFile)
                .Replace("$MERGED", _settings.MergedFile)
                .Replace("$REMOTE", _settings.RemoteFile);
            ProcessStartInfo info = new(exe, args) {UseShellExecute = true};
            Process? process = Process.Start(info);
            if (process != null)
            {
                process.EnableRaisingEvents = true;
                process.Exited += ProcessOnExited;
                if (process.HasExited)
                {
                    ProcessOnExited(process, EventArgs.Empty);
                }
                else
                {
                    process.WaitForExit();
                }
            }
            else
            {
                _errorView.ShowErrorMessage($"Unable to execute default command:{Environment.NewLine}{command}");
            }
        }

        private void ProcessOnExited(object? sender, EventArgs e)
        {
            Application.Current.Shutdown((sender as Process)?.ExitCode ?? 1);
        }
    }
}