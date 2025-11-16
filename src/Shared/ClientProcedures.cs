using System.Threading.Channels;

namespace Model;

public class ClientProcedures(Channel<Stream> sendMessage, Dictionary<Guid, PendingRequest> callbacks) : IClientProcedures
{
    public void Ping()
    {
        var guid = NetworkingClient.SendRequest(sendMessage, nameof(Ping), [  ], true);
    }
}
