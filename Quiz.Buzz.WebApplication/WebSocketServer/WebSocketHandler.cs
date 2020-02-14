using Microsoft.Web.WebSockets;

namespace Quiz.Buzz.WebApplication
{
    public class BuzzWebSocketHandler : WebSocketHandler
    {
        /// <summary>
        /// Holds the connected clients.
        /// </summary>
        private static WebSocketCollection _clients = new WebSocketCollection();

        private static string _server;
        private static string _client;

        /// <summary>
        /// When a client connection opens.
        /// </summary>
        public override void OnOpen()
        {
            // There is no server yet...
            if (_clients.Count > 1 && string.IsNullOrEmpty(_server))
            {
                _clients.Clear();
                this.Close();
            }

            if (_clients.Count < 2)
            {
                // Add client.
                _clients.Add(this);
            }
            else
            {
                this.Close();
            }
        }

        /// <summary>
        /// When receiving a message from a client.
        /// </summary>
        /// <param name="message"></param>
        public override void OnMessage(string message)
        {
            if (message == "CLIENT")
            {
                _client = this.WebSocketContext.SecWebSocketKey;
            }
            else if (message == "SERVER")
            {
                _server = this.WebSocketContext.SecWebSocketKey;
            }
            else if (message == "QUIT")
            {
                foreach (var client in _clients)
                {
                    if (client.WebSocketContext.SecWebSocketKey != _server)
                    {
                        client.Close();
                    }
                }
            }
            // Broadcast message to all connected clients.
            _clients.Broadcast(message);
        }

        /// <summary>
        /// When receiving a message from a client.
        /// </summary>
        /// <param name="message"></param>
        public override void OnMessage(byte[] message)
        {
            // Broadcast message to all connected clients.
            _clients.Broadcast(message);
        }

        /// <summary>
        /// When a client connection closes.
        /// </summary>
        public override void OnClose()
        {
            // Remove client.
            _clients.Remove(this);
        }
    }
}