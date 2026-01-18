using System.Text.Json;
using Shared;
using Shared.Database;
using TestModel.Generated;
using Environment = Shared.Environment;

namespace Tests;

[Collection(DatabaseCollection.DatabaseCollectionName)]
public class JsonDumpImportTests
{
    [Fact]
    public void FromJson_Creates_Objects_And_Fields_And_Assocs()
    {
        var testModel = ProjectModel.CreateFromDirectory("TestModel");
        using var env = Environment.Create(testModel, dbName: DatabaseCollection.GetTempDbDirectory());

        using (var session = new DbSession(env))
        {
            var parentId = Guid.NewGuid();
            var childId = Guid.NewGuid();

            var payload = new
            {
                entities = new Dictionary<string, object>
                {
                    [parentId.ToString()] = new Dictionary<string, object>
                    {
                        ["$type"] = TestingFolder.TypId.ToString(),
                        ["Name"] = "Parent",
                    },
                    [childId.ToString()] = new Dictionary<string, object>
                    {
                        ["$type"] = TestingFolder.TypId.ToString(),
                        ["Name"] = "Child",
                        ["Parent"] = parentId.ToString(),
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);


            JsonDump.FromJson(json, env, session);
            session.Commit();
        }

        using var readSession = new DbSession(env, readOnly: true);

        var folders = Searcher.Search<TestingFolder>(readSession).ToList();
        Assert.Equal(2, folders.Count);

        var dump = JsonDump.GetJsonDump(env, readSession);
        Assert.Contains("\"Name\": \"Parent\"", dump);
        Assert.Contains("\"Name\": \"Child\"", dump);
    }

    [Fact]
    public void FromJson_Updates_Existing_Object_Instead_Of_Creating_Duplicate()
    {
        var testModel = ProjectModel.CreateFromDirectory("TestModel");
        using var env = Environment.Create(testModel, dbName: DatabaseCollection.GetTempDbDirectory());

        Guid fixedId;

        using (var session = new DbSession(env))
        {
            var folder = new TestingFolder(session) { Name = "Before" };
            fixedId = folder.ObjId;
            session.Commit();
        }

        using (var session = new DbSession(env))
        {
            var payload = new
            {
                entities = new Dictionary<string, object>
                {
                    [fixedId.ToString()] = new Dictionary<string, object>
                    {
                        ["$type"] = TestingFolder.TypId.ToString(),
                        ["Name"] = "After",
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);


            JsonDump.FromJson(json, env, session);
            session.Commit();
        }

        using var readSession = new DbSession(env, readOnly: true);
        var loaded = readSession.GetObjFromGuid<TestingFolder>(fixedId);
        Assert.Equal("After", loaded.Name);

        var count = Searcher.Search<TestingFolder>(readSession).Count();
        Assert.Equal(1, count);
    }

    [Fact]
    public void FromJson_Removes_Missing_Fields_And_Assocs_To_Match_Json()
    {
        var testModel = ProjectModel.CreateFromDirectory("TestModel");
        using var env = Environment.Create(testModel, dbName: DatabaseCollection.GetTempDbDirectory());

        Guid aId;
        Guid bId;

        using (var session = new DbSession(env))
        {
            var a = new TestingFolder(session) { Name = "A" };
            var b = new TestingFolder(session) { Name = "B" };
            a.Parent = b;
            a.TestIntegerField = 123;

            aId = a.ObjId;
            bId = b.ObjId;
            session.Commit();
        }

        using (var session = new DbSession(env))
        {
            var payload = new
            {
                entities = new Dictionary<string, object>
                {
                    [aId.ToString()] = new Dictionary<string, object>
                    {
                        ["$type"] = TestingFolder.TypId.ToString(),
                        ["Name"] = "A",
                    },
                    [bId.ToString()] = new Dictionary<string, object>
                    {
                        ["$type"] = TestingFolder.TypId.ToString(),
                        ["Name"] = "B",
                    },
                }
            };

            var json = JsonSerializer.Serialize(payload);


            JsonDump.FromJson(json, env, session);
            session.Commit();
        }

        using var readSession = new DbSession(env, readOnly: true);
        var aReloaded = readSession.GetObjFromGuid<TestingFolder>(aId);

        Assert.Equal("A", aReloaded.Name);

        // Unset numeric fields are stored as "missing" (no VAL entry), which the generated getter can't read.
        Assert.Empty(readSession.GetFldValue(aId, TestingFolder.Fields.TestIntegerField));

        Assert.Null(aReloaded.Parent);
    }
}
