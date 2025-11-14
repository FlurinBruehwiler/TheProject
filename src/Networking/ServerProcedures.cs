using System.Net.WebSockets;

namespace Networking;

public class ServerProcedures(WebSocket webSocket, Dictionary<Guid, PendingRequest> callbacks) : IServerProcedures
{
    public Task<ServerStatus> GetStatus(int a, int b)
    {
        var guid = NetworkingClient.SendRequest(webSocket, nameof(GetStatus), [ a, b ], false);
        return NetworkingClient.WaitForResponse<ServerStatus>(callbacks, guid);
    }
}
