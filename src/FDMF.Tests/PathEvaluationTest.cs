using BaseModel.Generated;
using FDMF.Core.Database;
using FDMF.Core.PathLayer;
using TestModel.Generated;
using Environment = FDMF.Core.Environment;

namespace FDMF.Tests;

[Collection(DatabaseCollection.DatabaseCollectionName)]
public sealed class PathEvaluationTest
{
    [Fact]
    public void Basic_Test()
    {
        using var env = Environment.CreateDatabase(dbName: DatabaseCollection.GetTempDbDirectory(), dumpFile: DatabaseCollection.GetTestModelDumpFile());
        using var session = new DbSession(env);

        var model = session.GetObjFromGuid<Model>(env.ModelGuid)!.Value;

        var childFolder = new TestingFolder(session);
        var parentFolder = new TestingFolder(session);
        parentFolder.Name = "Parent";
        childFolder.Parent = parentFolder;

        var src = "P(TestingFolder): this->Parent[$.Name=\"Parent\"]";
        var parse = PathLangParser.Parse(src);
        var predicate = parse.Predicates.First();
        var bind = PathLangBinder.Bind(model, session, parse.Predicates);

        Assert.True(PathEvaluation.Evaluate(session, childFolder.ObjId, predicate, bind.SemanticModel));
        Assert.False(PathEvaluation.Evaluate(session, parentFolder.ObjId, predicate, bind.SemanticModel));
    }

    [Fact]
    public void Basic_Test_2()
    {
        using var env = Environment.CreateDatabase(dbName: DatabaseCollection.GetTempDbDirectory(), dumpFile: DatabaseCollection.GetTestModelDumpFile());
        using var session = new DbSession(env);

        var model = session.GetObjFromGuid<Model>(env.ModelGuid)!.Value;

        var parentFolder = new TestingFolder(session);
        parentFolder.Name = "Parent";

        var childFolder = new TestingFolder(session);
        childFolder.Parent = parentFolder;

        var childFolder2 = new TestingFolder(session);
        childFolder2.Parent = parentFolder;

        var src = "P(TestingFolder): this->Parent->Subfolders->Parent->Subfolders->Parent[$.Name=\"Parent\"]";
        var parse = PathLangParser.Parse(src);
        var predicate = parse.Predicates.First();
        var bind = PathLangBinder.Bind(model, session, parse.Predicates);

        Assert.True(PathEvaluation.Evaluate(session, childFolder.ObjId, predicate, bind.SemanticModel));
        Assert.True(PathEvaluation.Evaluate(session, childFolder2.ObjId, predicate, bind.SemanticModel));
        Assert.False(PathEvaluation.Evaluate(session, parentFolder.ObjId, predicate, bind.SemanticModel));
    }

    [Fact]
    public void Basic_Test_3()
    {
        using var env = Environment.CreateDatabase(dbName: DatabaseCollection.GetTempDbDirectory(), dumpFile: DatabaseCollection.GetTestModelDumpFile());
        using var session = new DbSession(env);

        var model = session.GetObjFromGuid<Model>(env.ModelGuid)!.Value;

        var parentFolder2 = new TestingFolder(session);
        parentFolder2.Name = "Parent2";

        var parentFolder = new TestingFolder(session);
        parentFolder.Name = "Parent";

        var childFolder = new TestingFolder(session);
        childFolder.Parent = parentFolder;

        var childFolder2 = new TestingFolder(session);
        childFolder2.Parent = parentFolder2;

        var src = "P(TestingFolder): this->Parent->Subfolders->Parent->Subfolders->Parent[$.Name=\"Parent\"]";
        var parse = PathLangParser.Parse(src);
        var predicate = parse.Predicates.First();
        var bind = PathLangBinder.Bind(model, session, parse.Predicates);

        Assert.True(PathEvaluation.Evaluate(session, childFolder.ObjId, predicate, bind.SemanticModel));
        Assert.False(PathEvaluation.Evaluate(session, childFolder2.ObjId, predicate, bind.SemanticModel));
        Assert.False(PathEvaluation.Evaluate(session, parentFolder.ObjId, predicate, bind.SemanticModel));
        Assert.False(PathEvaluation.Evaluate(session, parentFolder2.ObjId, predicate, bind.SemanticModel));
    }

    [Fact]
    public void Complex_Test()
    {
        using var env = Environment.CreateDatabase(dbName: DatabaseCollection.GetTempDbDirectory(), dumpFile: DatabaseCollection.GetBusinessModelDumpFile());
    }
}