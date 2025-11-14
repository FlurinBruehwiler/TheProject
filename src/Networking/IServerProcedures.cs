namespace Networking;

public struct ServerStatus
{

}

public interface IServerProcedures
{
    public Task<ServerStatus> GetStatus(int a, int b);
}