using Model.Generated;
using Shared.Database;
using Environment = Shared.Environment;

namespace Tests;

[Collection(DatabaseCollection.DatabaseCollectionName)]
public class JsonDumpImportTests
{
    [Fact]
    public void Create_EmptyDb_And_Import_From_Dump()
    {
        using var env = Environment.CreateDatabase(dbName: DatabaseCollection.GetTempDbDirectory());

        using (var session = new DbSession(env))
        {
            var json = File.ReadAllText("testdata/TestModelDump.json");
            JsonDump.FromJson(json, session);

            session.Commit();
        }

        using var readSession = new DbSession(env, readOnly: true);

        var obj = readSession.GetObjFromGuid<EntityDefinition>(Guid.Parse("a3ccbd8b-2256-414b-a402-1a091cb407a5"));

        Assert.NotNull(obj);
        Assert.Equal("TestingFolder", obj.Value.Name);
    }

    [Fact]
    public void Create_Db_From_Dump()
    {
        using var env = Environment.CreateDatabase(dbName: DatabaseCollection.GetTempDbDirectory(), dumpFile: DatabaseCollection.GetTestModelDumpFile());

        using var readSession = new DbSession(env, readOnly: true);

        var obj = readSession.GetObjFromGuid<EntityDefinition>(Guid.Parse("a3ccbd8b-2256-414b-a402-1a091cb407a5"));

        Assert.NotNull(obj);
        Assert.Equal("TestingFolder", obj.Value.Name);
    }
}
