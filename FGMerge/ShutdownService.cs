using System.Windows;

namespace FGMerge
{
    public class ShutdownService : IShutdownService
    {
        public void Shutdown(int returnCode)
        {
            Application.Current.Shutdown(returnCode);
        }
    }
}