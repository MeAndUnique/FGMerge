using System.ComponentModel;
using System.Windows;

namespace FGMerge.Views
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : IErrorView
    {
        private readonly IShutdownService _shutdownService;

        public ErrorWindow(IShutdownService shutdownService)
        {
            _shutdownService = shutdownService;
            InitializeComponent();
        }

        public void ShowErrorMessage(string message)
        {
            ErrorMessage.Text = message;
            Show();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _shutdownService.Shutdown(1);
            e.Cancel = true;
            base.OnClosing(e);
        }

        private void ButtonOK_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
