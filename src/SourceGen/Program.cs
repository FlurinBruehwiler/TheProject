using SourceGen;

var root = Helper.GetRootDir();

NetworkingGenerator.Generate(Path.Combine(root, "Shared/IServerProcedures.cs"));
NetworkingGenerator.Generate(Path.Combine(root, "Shared/IClientProcedures.cs"));





