using System.Net.WebSockets;
using Client;
using Networking;

Console.WriteLine("Hello, World!");

var ws = new ClientWebSocket();
await ws.ConnectAsync(new Uri("ws://localhost:8080"), CancellationToken.None);
Console.WriteLine("Connected!");

Dictionary<Guid, PendingRequest> pendingRequests = [];

_ = Task.Run(async () => await NetworkingClient.ProcessMessagesForWebSocket(ws, new MessageHandler(), pendingRequests));

namespace Client
{
    class MessageHandler
    {

    }
}