using System.CommandLine;
using Cli.Utils;
using Shared.Database;
using Environment = Shared.Environment;

namespace Cli.Commands;

public static class DbDumpJsonCommand
{
    public static Command Build()
    {
        var outOption = new Option<FileInfo?>("--out", description: "Optional output file path. Defaults to stdout.");

        var cmd = new Command("dump-json", "Dump the entire ObjectDb as JSON")
        {
            TreatUnmatchedTokensAsErrors = true
        };

        cmd.AddOption(CliOptions.Db);
        cmd.AddOption(outOption);

        cmd.SetHandler((DirectoryInfo? dbDir, FileInfo? outFile) =>
        {
            var resolvedDb = DbPath.Resolve(dbDir, allowCwd: true);

            using var env = Environment.Open(resolvedDb);
            using var session = new DbSession(env, readOnly: true);

            var json = JsonDump.GetJsonDump(env, session);

            if (outFile is null)
            {
                Console.WriteLine(json);
                return;
            }

            File.WriteAllText(outFile.FullName, json);
            Console.WriteLine($"Wrote JSON dump to '{outFile.FullName}'.");
        }, CliOptions.Db, outOption);

        return cmd;
    }
}
