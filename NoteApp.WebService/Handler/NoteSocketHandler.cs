using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Web.WebSockets;

namespace NoteApp.WebService.Handler {
   using WebSocketSendAsync = Func<ArraySegment<byte>, int, bool, CancellationToken, Task>;
   public class NoteSocketHandler : WebSocketHandler {
      private static WebSocketCollection connections = new WebSocketCollection();
      private WebSocketSendAsync _sendFunc;
      private CancellationToken _token;

      public NoteSocketHandler(WebSocketSendAsync sendFunc, CancellationToken token) {
         _sendFunc = sendFunc;
         _token = token;
      }

      public override void OnOpen() {
         connections.Add(this);
      }

      public override void OnClose() {
         connections.Remove(this);
      }

      public override void OnMessage(string message) {
         foreach (NoteSocketHandler connection in connections) {
            connection.SendMessage(message);
         }
      }

      public async Task SendMessage(string message) {
         var buffer = new ArraySegment<byte>(new byte[100]);
         var bytes = Encoding.UTF8.GetBytes(message);
         
         Array.Copy(bytes, 0, buffer.Array, 0, bytes.Length);
         await _sendFunc(new ArraySegment<byte>(buffer.Array, 0, bytes.Length), 1, true, _token);         
      }
   }
}
