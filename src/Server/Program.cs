using Server;
using Shared;
using Shared.Generated;

//we can store all fields objId+fieldIds that where changed in a dictionary within the transaction,
//when saving, we have a separate table where we store the "history" of all objects
//we could directly add the entries to hist db in a new transaction.
//what we want is to group often used objects together for better cache efficiency, and so that these pages can be unloaded from memory

var env = Shared.Environment.Create();

{
    var tsx = new Transaction(env);
    var folder = new Folder(tsx);
    folder.Name = "Anita";
    Console.WriteLine(folder.Name);

    var folder2 = new Folder(tsx);
    folder2.Name = "Max";
    Console.WriteLine(folder2.Name);

    var folder3 = new Folder(tsx);
    folder3.Name = "Wynn";
    Console.WriteLine(folder3.Name);

    var result = Searcher.Search<Folder>(tsx, new FieldCriterion { FieldId = Folder.Fields.Name, Value = "Max" }).ToArray();

    Console.WriteLine(result.Length == 1);
    Console.WriteLine(result.First() == folder2);
}


// Logging.LogFlags = LogFlags.Error | LogFlags.Performance;
//
// var sm = new ServerManager();
//
// Helper.FireAndForget(sm.LogMetrics());
//
// await sm.ListenForConnections();

