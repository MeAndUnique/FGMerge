using System.Reactive.Disposables;
using System.Windows.Media;
using FGMerge.ViewModels;
using ReactiveUI;

namespace FGMerge.Views
{
    /// <summary>
    /// Interaction logic for MergeNodeListItem.xaml
    /// </summary>
    public partial class MergeNodeListItem
    {
        public MergeNodeListItem()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                ViewModel = DataContext as NodeViewModel;
                this.Bind(ViewModel, vm => vm.Id, view => view.NameText.Text)
                    .DisposeWith(disposables);

                ViewModel.WhenAnyValue(x => x.HasChange, x => x.HasConflict, GetStatusText)
                    .BindTo(this, x => x.StatusText.Text);

                ViewModel.WhenAnyValue(x => x.HasConflict, GetStatusColor)
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
