using System.Windows.Forms;

namespace Quiz.Buzz.SystemTrayApp
{
    public class STAApplicationContext : ApplicationContext
    {
        private ViewManager _viewManager;
        private BackgroundService _backgroundService;

        public STAApplicationContext()
        {
            _backgroundService = new BackgroundService();
            _viewManager = new ViewManager(_backgroundService);
            _backgroundService.Start();
        }

        protected override void Dispose(bool disposing)
        {
            _viewManager = null;
            _backgroundService.Stop();
            _backgroundService = null;
        }
    }
}
