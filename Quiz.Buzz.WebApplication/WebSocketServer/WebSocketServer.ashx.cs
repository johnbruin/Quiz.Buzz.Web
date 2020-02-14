using Microsoft.Web.WebSockets;
using System.Web;

namespace Quiz.Buzz.WebApplication
{
    /// <summary>
    /// WebSocketServer
    /// </summary>
    public class WebSocketServer : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            // Is this a web socket request?
            if (context.IsWebSocketRequest)
            {
                // Yes, so accept this request and pass in our custom handler.
                context.AcceptWebSocketRequest(new BuzzWebSocketHandler());
            }
        }

        #region IHttpHandler Members
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        #endregion
    }
}