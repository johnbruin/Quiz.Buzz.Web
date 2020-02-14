using log4net;
using Quizz.Buzz;
using System.Threading;
using WebSocketSharp;

namespace DebuggableWindowsService
{
    /// <summary>
    /// Contains actual background operations that our Windows service performs
    /// </summary>
    class SampleBackgroundService
    {
        //Get a logger for this class from log4net LogManager
        private static ILog logger = LogManager.GetLogger(typeof(SampleBackgroundService));

        //Web Socket
        private static WebSocket ws = new WebSocket("ws://localhost:54086/WebSocketServer/WebSocketServer.ashx");

        //PlayStation Buzzer Manager
        private static BuzzerManager buzzerManager = new BuzzerManager();

        bool stopRequested;

        //Starts the thread
        public void Start()
        {
            logger.Debug("Start called.");

            Thread thr = new Thread(ServiceThread);
            thr.Start();
        }

        //Stops the thread
        public void Stop()
        {
            logger.Debug("Stop called.");
            stopRequested = true;
        }

        //Service thread that performs background tasks and writes logs
        private void ServiceThread()
        {
            logger.Debug("Service thread started.");

            //Wait for Initialization of the BuzzerManager
            buzzerManager.WaitForInitialization();
            logger.Debug("Buzzers initialized.");

            //Try to connect to the websocket server
            while (!ws.IsAlive)
            {
                try
                {
                    ws.Connect();
                }
                catch { }
            }

            //Connected
            ws.Send("SERVER");
            logger.Debug("Websocket connected.");

            //Receive a message
            ws.OnMessage += Ws_OnMessage;            

            //Send a message
            buzzerManager.OnAnswerClick += BuzzerManager_OnAnswerClick;
     
            //Wait in a loop
            while (!stopRequested)
            {
                if (stopRequested)
                    break;
            }
            ws.Send("QUIT");
            ws.Close();

            logger.Debug("Service thread exiting.");
        }

        private void BuzzerManager_OnAnswerClick(object sender, AnswerClickEventArgs ace)
        {
            var message = ace.Answer.ToString().ToUpper() + ace.PlayerId.ToString();
            logger.Info(message);
            ws.Send(message);
        }

        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Data.StartsWith("LIGHTSWITCH"))
            {
                short.TryParse(e.Data.Replace("LIGHTSWITCH", string.Empty), out short playerId);
                buzzerManager.FlashLight(playerId, 10);
                if (buzzerManager.IsLightOn(playerId))
                {
                    buzzerManager.SetLightOff(playerId);
                }
                else
                {
                    buzzerManager.SetLightOn(playerId);
                }
            }

            if (e.Data.StartsWith("LIGHTON"))
            {
                short.TryParse(e.Data.Replace("LIGHTON", string.Empty), out short playerId);
                buzzerManager.SetLightOn(playerId);
            }

            if (e.Data.StartsWith("LIGHTOFF"))
            {
                short.TryParse(e.Data.Replace("LIGHTOFF", string.Empty), out short playerId);
                buzzerManager.SetLightOff(playerId);
            }
        }
    }
}