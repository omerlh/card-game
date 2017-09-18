using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using System;
using System.IO;
using System.Reactive.Linq;
using MFRC522;

namespace server
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IObservable<string> _cardsObservable;
        private readonly ICardReader _cardReader;

        public WebSocketMiddleware(RequestDelegate next, ICardReader cardReader)
        {
            _next = next;
            _cardReader = cardReader;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                // Not a web socket request
                await _next.Invoke(context);
                return;
            }

            var ct = context.RequestAborted;
            using (var socket = await context.WebSockets.AcceptWebSocketAsync())
            {
                while (socket.State == WebSocketState.Open)
                {
                    if (_cardReader.IsCardAvailable())
                    {
                        await SendStringAsync(socket, BitConverter.ToString(_cardReader.GetCardId()), ct);
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                }
            }
        }

        private static Task SendStringAsync(WebSocket socket, string data, CancellationToken ct = default(CancellationToken))
        {
            var buffer = Encoding.UTF8.GetBytes(data);
            var segment = new ArraySegment<byte>(buffer);
            return socket.SendAsync(segment, WebSocketMessageType.Text, true, ct);
        }

        private static async Task<string> ReceiveStringAsync(WebSocket socket, CancellationToken ct = default(CancellationToken))
        {
            // Message can be sent by chunk.
            // We must read all chunks before decoding the content
            var buffer = new ArraySegment<byte>(new byte[8192]);
            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    ct.ThrowIfCancellationRequested();

                    result = await socket.ReceiveAsync(buffer, ct);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);
                if (result.MessageType != WebSocketMessageType.Text)
                    throw new Exception("Unexpected message");

                // Encoding UTF8: https://tools.ietf.org/html/rfc6455#section-5.6
                using (var reader = new StreamReader(ms, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}