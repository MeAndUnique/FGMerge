using System.Reactive.Disposables;
using System.Windows.Media;
using FGMerge.ViewModels;
using ReactiveUI;

namespace FGMerge.Views
{
    /// <summary>
    /// Interaction logic for MergeGroupListItem.xaml
    /// </summary>
    public partial class MergeGroupListItem
    {
        public MergeGroupListItem()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                ViewModel = DataContext as GroupViewModel;
                this.Bind(ViewModel, vm => vm.Name, view => view.NameText.Text)
                    .DisposeWith(disposables);

                ViewModel.WhenAnyValue(x => x.HasChanges, x => x.HasConflicts, GetStatusText)
                    .BindTo(this, x => x.StatusText.Text);

                ViewModel.WhenAnyValue(x => x.HasConflicts, GetStatusColor)
                    .BindTo(this, x => x.StatusText.Foreground);
            });
        }

        private string GetStatusText(bool hasChanges, bool hasConflicts)
        {
            return hasConflicts ? "⚠" : hasChanges ? "✓" : string.Empty;
        }

        private Brush GetStatusColor(bool hasConflicts)
        {
            return hasConflicts ? Brushes.Red : Brushes.Green;
        }
    }
}
