using System.ServiceProcess;

namespace Quiz.Buzz.WindowsService
{
    /// <summary>
    /// Windows service class whose OnStart and OnStop methods are called 
    /// when the Windows service is started or stopped.
    /// </summary>
    public partial class WindowsService : ServiceBase
    {
        BackgroundService backgroundService;

        public WindowsService()
        {
            InitializeComponent();
        }

        //Called when the Windows service is started
        protected override void OnStart(string[] args)
        {
            backgroundService = new BackgroundService();
            backgroundService.Start();
        }

        //Called when the Windows service is stopped
        protected override void OnStop()
        {
            backgroundService.Stop();
        }
    }
}
