using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

public class WebSocketServer
{
    private WebSocket? Websocket;
    private readonly HttpListener Listener;
    private bool ClientNotifs = true;

    // Event handlers
    public event EventHandler<string>? OnMessageReceived;

    public WebSocketServer()
    {
        Listener = new HttpListener();
    }

    // Start the server
    public async Task Start(string IP, int Port)
    {
        // HTTPS? - Yes, chrome was making problems
        string Prefix = $"http://{IP}:{Port}/Websocket/";

        Listener.Prefixes.Add(Prefix);
        Listener.Start();
        Write($"[yt-plus] Server fired up and ready ({IP}:{Port})!");

        while (true)
        {
            try {
                HttpListenerContext context = await Listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest) {
                    _ = ProcessWebSocketRequest(context);
                } else {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            } catch (Exception ex) {
                Write($"Websocket error occured: {ex.Message}");
            }
        }
    }

    public void Stop()
    {
        Listener.Stop();
        Listener.Close();
        Write("Server has stopped");
    }

    public async Task Send(object message)
    {
        if (Websocket == null || Websocket.State != WebSocketState.Open) {
            return;
        }

        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            await Websocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        } catch (Exception ex) {
            Write($"Error sending `{message.ToString()}` to client: {ex.Message}");
        }
    }

    private async Task ProcessWebSocketRequest(HttpListenerContext context)
    {
        // Client connects
        HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
        Websocket = wsContext.WebSocket;

        if (ClientNotifs) Write("Extension has connected");

        byte[] buffer = new byte[1024];

        while (Websocket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result = await Websocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            // Client message received
            if (result.MessageType == WebSocketMessageType.Text)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                OnMessageReceived?.Invoke(this, message);
            }
            // Client disconnects
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                await Websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                if (ClientNotifs) Write("Extension has disconnected");
                Websocket = null;
                break;
            }
        }
    }
}