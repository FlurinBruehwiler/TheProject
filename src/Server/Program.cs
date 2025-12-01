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
    folder.Name = "Hi";

    // var tsx2 = new Transaction(env);
    // var folder2 = new Folder(tsx2);
    // folder2.Name = "Foo";
    //
    // tsx2.Commit();

    tsx.Commit();
}


// Logging.LogFlags = LogFlags.Error | LogFlags.Performance;
//
// var sm = new ServerManager();
//
// Helper.FireAndForget(sm.LogMetrics());
//
// await sm.ListenForConnections();

