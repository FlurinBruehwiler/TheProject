using LightningDB;

namespace Shared;

public class Environment
{
    public required LightningEnvironment LightningEnvironment;
    public required LightningDatabase ObjectDb;
    public required LightningDatabase HistoryDb;

    public static Environment Create()
    {
        var env = new LightningEnvironment("database", new EnvironmentConfiguration
        {
            MaxDatabases = 128
        });
        env.Open();

        using var lightningTransaction = env.BeginTransaction();

        var objDb = lightningTransaction.OpenDatabase(null, new DatabaseConfiguration
        {
            Flags = DatabaseOpenFlags.Create
        });

        var histDb = lightningTransaction.OpenDatabase(name: "HistoryDb", new DatabaseConfiguration
        {
            Flags = DatabaseOpenFlags.Create
        });

        return new Environment
        {
            LightningEnvironment = env,
            ObjectDb = objDb,
            HistoryDb = histDb
        };
    }
}