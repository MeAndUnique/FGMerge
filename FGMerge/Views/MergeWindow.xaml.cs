using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows;
using System.Xml;
using FGMerge.ViewModels;
using Microsoft.Extensions.Options;
using ReactiveUI;

namespace FGMerge.Views
{
    /// <summary>
    /// Interaction logic for MergeWindow.xaml
    /// </summary>
    public partial class MergeWindow : IMergeView
    {
        private readonly AppSettings _settings;
        private readonly IShutdownService _shutdownService;
        private readonly IMergeResolver _mergeResolver;
        private readonly IFileWriter _fileWriter;
        private readonly IErrorView _errorView;

        public MergeWindow(IOptions<AppSettings> settings, IShutdownService shutdownService, IMergeResolver mergeResolver, IFileWriter fileWriter, IErrorView errorView)
        {
            _settings = settings.Value;
            _shutdownService = shutdownService;
            _mergeResolver = mergeResolver;
            _fileWriter = fileWriter;
            _errorView = errorView;
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                this.Bind(ViewModel, vm => vm.SelectedNode, view => view.SelectedView.ViewModel)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.MergeList.SelectedItem).BindTo(this, x => x.ViewModel!.SelectedNode);
            });
        }

        public void Show(IReadOnlyCollection<MergeGroup> mergeGroups)
        {
            ViewModel = new MergeViewModel();
            ViewModel.Initialize(mergeGroups);
            MergeList.ItemsSource = ViewModel.Groups;
            Show();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (ViewModel != null)
            {
                bool hasConflicts = ViewModel.Groups.Any(group => group.HasConflicts);
                string message = "Would you like to save the merged results?";
                MessageBoxButton buttons = MessageBoxButton.YesNoCancel;
                MessageBoxImage icon = MessageBoxImage.Question;
                if (hasConflicts)
                {
                    message = "There are unresolved conflicts. Would you like to abort the merge?";
                    buttons = MessageBoxButton.OKCancel;
                    icon = MessageBoxImage.Exclamation;
                }

                MessageBoxResult result = MessageBox.Show(message, "Are you sure?", buttons, icon);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        XmlDocument mergedDocument = ViewModel.GenerateResult(_mergeResolver);
                        if (_fileWriter.WriteFile(_settings.MergedFile, mergedDocument))
                        {
                            _shutdownService.Shutdown(0);
                        }
                        else
                        {
                            _errorView.ShowErrorMessage("Unable to save merge results!");
                        }

                        break;
                    case MessageBoxResult.OK: // OK is used to quit without saving when there are still conflicts.
                    case MessageBoxResult.No:
                        _shutdownService.Shutdown(1);
                        break;
                    default: // Accounts for Cancel and None
                        e.Cancel = true;
                        break;
                }
            }

            base.OnClosing(e);
        }
    }
}
