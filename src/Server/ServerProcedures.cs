using Networking;

namespace Server;

public class ServerProceduresImpl(ConnectedClient connectedClient) : IServerProcedures
{
    public Task<ServerStatus> GetStatus(int a, int b)
    {
        Console.WriteLine("Getting Status");

        connectedClient.ClientProcedures.Ping();

        return Task.FromResult(default(ServerStatus));
    }
}