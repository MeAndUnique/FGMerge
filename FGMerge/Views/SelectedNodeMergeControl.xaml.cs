using System.Reactive.Disposables;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace FGMerge.Views
{
    /// <summary>
    /// Interaction logic for SelectedNodeMergeControl.xaml
    /// </summary>
    public partial class SelectedNodeMergeControl
    {
        public SelectedNodeMergeControl()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                this.Bind(ViewModel, vm => vm.MergedText, view => view.MergeText.Text)
                    .DisposeWith(disposables);

                // Bind any validations that reference the Name property 
                // to the text of the NameError UI control.
                this.BindValidation(ViewModel, vm => vm.MergedText, view => view.MergeError.Text)
                    .DisposeWith(disposables);

                // Bind any validations attached to this particular view model
                // to the text of the FormErrors UI control.
                this.BindValidation(ViewModel, view => view.MergeError.Text)
                    .DisposeWith(disposables);
            });
        }
    }
}
