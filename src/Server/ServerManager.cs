using System.Net;
using System.Net.WebSockets;
using Networking;

namespace Server;

public class ConnectedClient
{
    public required IClientProcedures ClientProcedures;
}

public class ServerManager
{
    public List<ConnectedClient> ConnectedClients = [];
    public Dictionary<Guid, PendingRequest> Callbacks = [];

    public async Task ListenForConnections()
    {
        var url = "http://localhost:8080/connect/";

        var listener = new HttpListener();
        listener.Prefixes.Add(url);
        listener.Start();

        Console.WriteLine($"Listening on {url}");

        while (true)
        {
            var context = await listener.GetContextAsync();
            if (context.Request.IsWebSocketRequest)
            {
                var wsContext = await context.AcceptWebSocketAsync(subProtocol: null);

                Console.WriteLine("Client connected!");

                var connectedClient = new ConnectedClient
                {
                    ClientProcedures = new ClientProcedures(x =>
                    {
                        x.Seek(0, SeekOrigin.Begin);
                        using var stream = WebSocketStream.CreateWritableMessageStream(wsContext.WebSocket, WebSocketMessageType.Binary);
                        x.CopyTo(stream);
                    }, Callbacks)
                };

                ConnectedClients.Add(connectedClient);

                _ = NetworkingClient.ProcessMessagesForWebSocket(wsContext.WebSocket, new ServerProceduresImpl(connectedClient), Callbacks).ContinueWith(x =>
                {
                    Console.WriteLine(x.Exception?.ToString());
                }, TaskContinuationOptions.OnlyOnFaulted);;
            }
            else
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }
    }
}