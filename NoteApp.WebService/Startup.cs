using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin;
using Owin;

namespace NoteApp.WebService {
   using WebSocketAccept = System.Action<
           System.Collections.Generic.IDictionary<string, object>, // WebSocket Accept parameters
           System.Func< // WebSocketFunc callback
               System.Collections.Generic.IDictionary<string, object>, // WebSocket environment
               System.Threading.Tasks.Task>>;

   using WebSocketSendAsync = System.Func<
                  System.ArraySegment<byte>, // data
                  int, // message type
                  bool, // end of message
                  System.Threading.CancellationToken, // cancel
                  System.Threading.Tasks.Task>;
   // closeStatusDescription

   using WebSocketReceiveAsync = System.Func<
               System.ArraySegment<byte>, // data
               System.Threading.CancellationToken, // cancel
               System.Threading.Tasks.Task<
                   System.Tuple< // WebSocketReceiveTuple
                       int, // messageType
                       bool, // endOfMessage
                       int?, // count
                       int?, // closeStatus
                       string>>>; // closeStatusDescription

   using WebSocketCloseAsync = System.Func<
               int, // closeStatus
               string, // closeDescription
               System.Threading.CancellationToken, // cancel
               System.Threading.Tasks.Task>;
   using System.Net.WebSockets;
   using System.Threading;
   using System.Diagnostics;

   public class Startup {
      // This code configures Web API. The Startup class is specified as a type
      // parameter in the WebApp.Start method.
      public void Configuration(IAppBuilder appBuilder) {
         // Configure Web API for self-host. 
         HttpConfiguration config = new HttpConfiguration();
         config.MapHttpAttributeRoutes();
         config.Routes.MapHttpRoute("DefaultApi", "api/{controller}");
         appBuilder.UseWebApi(config);
         appBuilder.Use(UpgradeToWebSockets);
      }

      private Task UpgradeToWebSockets(IOwinContext context, Func<Task> next) {
         var accept = context.Get<WebSocketAccept>("websocket.Accept");
         if (accept == null) {
            // Not a websocket request
            return next();
         }

         accept(null, WebSocketEcho);

         return Task.FromResult<object>(null);
      }

      private async Task WebSocketEcho(IDictionary<string, object> wsEnv) {
         var wsSendAsync = (WebSocketSendAsync)wsEnv["websocket.SendAsync"];
         var wsRecieveAsync = (WebSocketReceiveAsync)wsEnv["websocket.ReceiveAsync"];
         var wsCloseAsync = (WebSocketCloseAsync)wsEnv["websocket.CloseAsync"];
         var wsVersion = (string)wsEnv["websocket.Version"];
         var wsCallCancelled = (CancellationToken)wsEnv["websocket.CallCancelled"];

         // note: make sure to catch errors when calling sendAsync, receiveAsync and closeAsync
         // for simiplicity this code does not handle errors
         var buffer = new ArraySegment<byte>(new byte[6]);
         while (true) {
            var webSocketResultTuple = await wsRecieveAsync(buffer, wsCallCancelled);
            int wsMessageType = webSocketResultTuple.Item1;
            bool wsEndOfMessge = webSocketResultTuple.Item2;
            int? count = webSocketResultTuple.Item3;
            int? closeStatus = webSocketResultTuple.Item4;
            string closeStatusDescription = webSocketResultTuple.Item5;

            Debug.Write(Encoding.UTF8.GetString(buffer.Array, 0, count.Value));

            await wsSendAsync(new ArraySegment<byte>(buffer.Array, 0, count.Value), 1, wsEndOfMessge, wsCallCancelled);

            if (wsEndOfMessge)
               break;
         }

         await wsCloseAsync((int)WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
      }
   }
}
