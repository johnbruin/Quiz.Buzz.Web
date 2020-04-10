using log4net;
using Quiz.Buzz.Usb;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using WebSocketSharp;

namespace Quiz.Buzz.SystemTrayApp
{
    public class LogEventArgs : EventArgs
    {
        public string Message { get; set; }
    }

    /// <summary>
    /// Contains actual background operations that our system tray application performs
    /// </summary>
    public class BackgroundService
    {
        public event EventHandler<LogEventArgs> LogChanged;

        //PlayStation Buzzer Manager
        public BuzzerManager BuzzerManager = new BuzzerManager();

        public string WebSocketUrl;

        private List<string> _log = new List<string>();

        //Get a logger for this class from log4net LogManager
        private static ILog logger = LogManager.GetLogger(typeof(BackgroundService));

        //Web Socket
        private static WebSocket ws;
        
        private bool stopRequested;
        Thread thread;

        //Starts the thread
        public void Start()
        {
            LogMessage("Start called");

            //Try to connect to the websocket server
            WebSocketUrl = ConfigurationManager.AppSettings.Get("WebSocketServerURL");
            ws = new WebSocket(WebSocketUrl);
            ws.Connect();

            if (ws.IsAlive)
            {
                thread = new Thread(ServiceThread);
                thread.Start();
            }
        }

        private void Ws_OnError(object sender, ErrorEventArgs e)
        {
            LogMessage(e.Message);
        }

        //Stops the thread
        public void Stop()
        {
            stopRequested = true;
            LogMessage("Stop called");
            thread.Abort();
        }

        //Service thread that performs background tasks and writes logs
        private void ServiceThread()
        {
            LogMessage("Service thread started");

            //Wait for Initialization of the BuzzerManager
            BuzzerManager.WaitForInitialization();
            LogMessage("Buzzers initialized");

            //Connected
            ws.Send("SERVER");
            LogMessage("Connected with " + ws.Url.OriginalString);

            //Receive a message
            ws.OnMessage += Ws_OnMessage;

            //Send a message
            BuzzerManager.OnAnswerClick += BuzzerManager_OnAnswerClick;

            while (!stopRequested)
            {
                if (stopRequested)
                    break;
            }

            LogMessage("Service thread exiting");
        }

        private void BuzzerManager_OnAnswerClick(object sender, AnswerClickEventArgs ace)
        {
            var message = ace.Answer.ToString().ToUpper() + ace.PlayerId.ToString();
            LogMessage(message);
            ws.Send(message);
        }

        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Data.StartsWith("CLIENT") || e.Data.StartsWith("RESET"))
            {
                LogMessage("Received: " + e.Data);
                BuzzerManager.AllLightOff();
            }

            if (e.Data.StartsWith("LIGHTSWITCH"))
            {
                LogMessage("Received: " + e.Data);
                short.TryParse(e.Data.Replace("LIGHTSWITCH", string.Empty), out short playerId);
                if (BuzzerManager.IsLightOn(playerId))
                {
                    BuzzerManager.SetLightOff(playerId);
                }
                else
                {
                    BuzzerManager.SetLightOn(playerId);
                }
            }

            if (e.Data.StartsWith("LIGHTFLASH"))
            {
                LogMessage("Received: " + e.Data);
                short.TryParse(e.Data.Replace("LIGHTFLASH", string.Empty), out short playerId);
                BuzzerManager.FlashLight(playerId);
            }

            if (e.Data.StartsWith("LIGHTON"))
            {
                LogMessage("Received: " + e.Data);
                short.TryParse(e.Data.Replace("LIGHTON", string.Empty), out short playerId);
                BuzzerManager.SetLightOn(playerId);
            }

            if (e.Data.StartsWith("LIGHTOFF"))
            {
                LogMessage("Received: " + e.Data);
                short.TryParse(e.Data.Replace("LIGHTOFF", string.Empty), out short playerId);
                BuzzerManager.SetLightOff(playerId);
            }
        }

        private void LogMessage(string message)
        {
            _log.Add(message);
            logger.Info(message);
            var args = new LogEventArgs
            {
                Message = message
            };
            OnLogChanged(args);
        }

        protected virtual void OnLogChanged(LogEventArgs e)
        {
            LogChanged?.Invoke(this, e);
        }
    }
}