using System.Text;
using FDMF.Core.Database;
using Environment = FDMF.Core.Environment;

namespace FDMF.Tests;

[Collection(DatabaseCollection.DatabaseCollectionName)]
public class DbSessionTests
{
    [Fact]
    public void DeleteObj_Removes_Obj_And_All_Values()
    {
        using var env = Environment.CreateDatabase(dbName: DatabaseCollection.GetTempDbDirectory(), dumpFile: DatabaseCollection.GetTestModelDumpFile());
        using var session = new DbSession(env);

        var objId = session.CreateObj(TestModel.Generated.TestingFolder.TypId);
        session.SetFldValue(objId, TestModel.Generated.TestingFolder.Fields.Name, Encoding.Unicode.GetBytes("abc"));
        long i = 123;
        session.SetFldValue(objId, TestModel.Generated.TestingFolder.Fields.TestIntegerField, i.AsSpan());

        session.DeleteObj(objId);

        Assert.Equal(Guid.Empty, session.GetTypId(objId));
        Assert.Empty(session.GetFldValue(objId, TestModel.Generated.TestingFolder.Fields.Name));
        Assert.Empty(session.GetFldValue(objId, TestModel.Generated.TestingFolder.Fields.TestIntegerField));
    }

    [Fact]
    public void DeleteObj_Removes_Associations_On_Both_Sides()
    {
        using var env = Environment.CreateDatabase(dbName: DatabaseCollection.GetTempDbDirectory(), dumpFile: DatabaseCollection.GetTestModelDumpFile());
        using var session = new DbSession(env);

        var a = session.CreateObj(TestModel.Generated.TestingFolder.TypId);
        var b = session.CreateObj(TestModel.Generated.TestingFolder.TypId);

        // a.Parent -> b (and therefore b.Subfolders contains a)
        session.CreateAso(a, TestModel.Generated.TestingFolder.Fields.Parent, b, TestModel.Generated.TestingFolder.Fields.Subfolders);

        Assert.Equal(1, session.GetAsoCount(a, TestModel.Generated.TestingFolder.Fields.Parent));
        Assert.Equal(1, session.GetAsoCount(b, TestModel.Generated.TestingFolder.Fields.Subfolders));

        session.DeleteObj(a);

        Assert.Equal(0, session.GetAsoCount(b, TestModel.Generated.TestingFolder.Fields.Subfolders));
        Assert.Equal(Guid.Empty, session.GetTypId(a));
    }

    [Fact]
    public void RemoveAllAso_Removes_All_Associations_And_All_Opposites()
    {
        using var env = Environment.CreateDatabase(dbName: DatabaseCollection.GetTempDbDirectory(), dumpFile: DatabaseCollection.GetTestModelDumpFile());
        using var session = new DbSession(env);

        var parent = session.CreateObj(TestModel.Generated.TestingFolder.TypId);
        var childA = session.CreateObj(TestModel.Generated.TestingFolder.TypId);
        var childB = session.CreateObj(TestModel.Generated.TestingFolder.TypId);

        session.CreateAso(childA, TestModel.Generated.TestingFolder.Fields.Parent, parent, TestModel.Generated.TestingFolder.Fields.Subfolders);
        session.CreateAso(childB, TestModel.Generated.TestingFolder.Fields.Parent, parent, TestModel.Generated.TestingFolder.Fields.Subfolders);

        Assert.Equal(2, session.GetAsoCount(parent, TestModel.Generated.TestingFolder.Fields.Subfolders));

        session.RemoveAllAso(parent, TestModel.Generated.TestingFolder.Fields.Subfolders);

        Assert.Equal(0, session.GetAsoCount(parent, TestModel.Generated.TestingFolder.Fields.Subfolders));
        Assert.Equal(0, session.GetAsoCount(childA, TestModel.Generated.TestingFolder.Fields.Parent));
        Assert.Equal(0, session.GetAsoCount(childB, TestModel.Generated.TestingFolder.Fields.Parent));
    }
}
