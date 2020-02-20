using log4net;
using Quizz.Buzz;
using System;
using System.Configuration;
using System.Threading;
using WebSocketSharp;

namespace Quiz.Buzz.WindowsService
{
    /// <summary>
    /// Contains actual background operations that our Windows service performs
    /// </summary>
    class BackgroundService
    {
        //Get a logger for this class from log4net LogManager
        private static ILog logger = LogManager.GetLogger(typeof(BackgroundService));

        //Web Socket
        private static WebSocket ws;

        //PlayStation Buzzer Manager
        private static BuzzerManager buzzerManager = new BuzzerManager();
        readonly bool stopRequested;
        Thread thread;

        //Starts the thread
        public void Start()
        {
            logger.Info("Start called.");

            //Try to connect to the websocket server
            var url = ConfigurationManager.AppSettings.Get("WebSocketServerURL");
            ws = new WebSocket(url);
            ws.Connect();

            if (ws.IsAlive)
            {
                thread = new Thread(ServiceThread);
                thread.Start();
            }
        }

        private void Ws_OnError(object sender, ErrorEventArgs e)
        {
            logger.Info(e.Message);
        }

        //Stops the thread
        public void Stop()
        {
            logger.Debug("Stop called.");
            thread.Abort();
        }

        //Service thread that performs background tasks and writes logs
        private void ServiceThread()
        {
            logger.Info("Service thread started.");

            //Wait for Initialization of the BuzzerManager
            buzzerManager.WaitForInitialization();
            logger.Info("Buzzers initialized.");

            //Connected
            ws.Send("SERVER");
            logger.Info("Websocket connected.");

            //Receive a message
            ws.OnMessage += Ws_OnMessage;

            //Send a message
            buzzerManager.OnAnswerClick += BuzzerManager_OnAnswerClick;

            while (!stopRequested)
            {
                if (stopRequested)
                    break;
            }

            logger.Info("Service thread exiting.");
        }

        private void BuzzerManager_OnAnswerClick(object sender, AnswerClickEventArgs ace)
        {
            var message = ace.Answer.ToString().ToUpper() + ace.PlayerId.ToString();
            logger.Info(message);
            ws.Send(message);
        }

        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            logger.Info(e.Data);

            if (e.Data.StartsWith("CLIENT") || e.Data.StartsWith("RESET"))
            {
                buzzerManager.AllLightOff();
            }

            if (e.Data.StartsWith("LIGHTSWITCH"))
            {
                short.TryParse(e.Data.Replace("LIGHTSWITCH", string.Empty), out short playerId);
                if (buzzerManager.IsLightOn(playerId))
                {
                    buzzerManager.SetLightOff(playerId);
                }
                else
                {
                    buzzerManager.SetLightOn(playerId);
                }
            }

            if (e.Data.StartsWith("LIGHTFLASH"))
            {
                short.TryParse(e.Data.Replace("LIGHTFLASH", string.Empty), out short playerId);
                buzzerManager.FlashLight(playerId);
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