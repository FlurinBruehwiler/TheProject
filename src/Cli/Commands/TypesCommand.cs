using System.CommandLine;
using Cli.Utils;
using Shared;
using Shared.Database;
using Environment = Shared.Environment;

namespace Cli.Commands;

public static class TypesCommand
{
    public static Command Build()
    {
        var cmd = new Command("types", "List entity types");

        cmd.SetHandler((DirectoryInfo? dbDir) =>
        {
            var resolvedDb = DbPath.Resolve(dbDir, allowCwd: true);

            using var env = Environment.Open(resolvedDb);
            using var session = new DbSession(env);

            var mdl = session.GetObjFromGuid<Model.Generated.Model>(env.ModelGuid);

            foreach (var e in mdl!.Value.GetAllEntityDefinitions().OrderBy(x => x.Key))
            {
                Console.WriteLine($"{e.Key}\t{e.Id}");
            }
        }, CliOptions.Db);

        return cmd;
    }
}
