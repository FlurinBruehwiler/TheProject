using BaseModel.Generated;
using FDMF.Core.Database;
using FDMF.SourceGen;
using Environment = FDMF.Core.Environment;
using Helper = FDMF.SourceGen.Helper;

var root = Helper.GetRootDir();

//Main
using var env2 = Environment.CreateDatabase("temp2");

using (var session = new DbSession(env2))
{
    var model = session.GetObjFromGuid<Model>(env2.ModelGuid);
    ModelGenerator.Generate(model!.Value, Path.Combine(root, "FDMF.Core/Generated"));
}

//Test Data
using var env = Environment.CreateDatabase("temp", Path.Combine(root, "FDMF.Tests/testdata/TestModelDump.json"));

using (var session = new DbSession(env))
{
    var model = session.GetObjFromGuid<Model>(env.ModelGuid);
    ModelGenerator.Generate(model!.Value, Path.Combine(root, "FDMF.Tests/Generated"));
}

//Networking
NetworkingGenerator.Generate(Path.Combine(root, "FDMF.Core/IServerProcedures.cs"));
NetworkingGenerator.Generate(Path.Combine(root, "FDMF.Core/IClientProcedures.cs"));






