using System.Net.WebSockets;
using Client;
using Networking;

//todo, the goal is that the server and client can be in the same process!!!
//we still want to serialize / deserialize everything, so we get the exact same behaviour
//but we don't want a direct dependency on the WebSocket in the ServerProcedures

var ws = new ClientWebSocket();

Console.WriteLine("Trying to connect...");
await ws.ConnectAsync(new Uri("ws://localhost:8080/connect/"), CancellationToken.None);
Console.WriteLine("Connected!");

Dictionary<Guid, PendingRequest> pendingRequests = [];
_ = NetworkingClient.ProcessMessagesForWebSocket(ws, new ClientProceduresImpl(), pendingRequests).ContinueWith(x =>
{
    Console.WriteLine(x.Exception?.ToString());
}, TaskContinuationOptions.OnlyOnFaulted);

IServerProcedures serverProcedures = new ServerProcedures(x =>
{
    try
    {
        x.Seek(0, SeekOrigin.Begin);
        using var stream = WebSocketStream.CreateWritableMessageStream(ws, WebSocketMessageType.Binary);
        x.CopyTo(stream);
    }
    catch (Exception e)
    {
        Console.WriteLine($"Connection closed {e.Message}");
    }
}, pendingRequests);

while (true)
{
    var res = await serverProcedures.GetStatus(1, 2);
    Console.WriteLine(res);
    await Task.Delay(1000);
}

namespace Client
{
    class ClientProceduresImpl : IClientProcedures
    {
        public void Ping()
        {
            Console.WriteLine("Got ping");
        }
    }
}