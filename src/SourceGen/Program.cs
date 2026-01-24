using Shared.Database;
using SourceGen;
using Environment = Shared.Environment;
using Helper = SourceGen.Helper;

var root = Helper.GetRootDir();

var env = Environment.CreateDatabase("temp", Path.Combine(root, "Tests/testdata/TestModelDump.json"));

using (var session = new DbSession(env))
{
    var model = session.GetObjFromGuid<Model.Generated.Model>(env.ModelGuid);
    ModelGenerator.Generate(model!.Value, Path.Combine(root, "Tests/Generated"));
}

NetworkingGenerator.Generate(Path.Combine(root, "Shared/IServerProcedures.cs"));
NetworkingGenerator.Generate(Path.Combine(root, "Shared/IClientProcedures.cs"));





