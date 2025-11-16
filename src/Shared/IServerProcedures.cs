using MemoryPack;

namespace Model;

[MemoryPackable]
public partial struct ServerStatus
{

}

public interface IServerProcedures
{
    public Task<ServerStatus> GetStatus(int a, int b);
}