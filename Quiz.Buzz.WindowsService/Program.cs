using log4net;
using System;
using System.IO;
using System.Reflection;
using System.ServiceProcess;

namespace Quiz.Buzz.WindowsService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            //Set the working directory to the folder that this executable resides
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            ILog logger = LogManager.GetLogger(typeof(Program));

#if DEBUG   //Run as a regular console application in Debug mode
            //Manually create an instance of SampleBackgroundService class and call its start method          

            logger.Info("Starting services...");

            var _backgroundService = new BackgroundService();
            _backgroundService.Start();

            logger.Info("Services started. Press enter to stop...");
            Console.ReadLine();

            logger.Info("Stopping service...");
            _backgroundService.Stop();
            logger.Info("Stopped.");

#else       //Create and run the real Windows service instance in Release mode
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new WindowsService()
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
