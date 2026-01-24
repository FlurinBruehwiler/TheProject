using Shared.Database;
using Environment = Shared.Environment;

class Program
{
    static void Main()
    {
        var env = Environment.CreateDatabase("temp");

        using var session = new DbSession(env);
        Console.WriteLine(JsonDump.GetJsonDump(session));
    }
}