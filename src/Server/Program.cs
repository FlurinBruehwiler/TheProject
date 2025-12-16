using Shared;
using Shared.Database;
using Shared.Generated;

//we can store all fields objId+fieldIds that where changed in a dictionary within the transaction,
//when saving, we have a separate table where we store the "history" of all objects
//we could directly add the entries to hist db in a new transaction.
//what we want is to group often used objects together for better cache efficiency, and so that these pages can be unloaded from memory

try
{
    var env = Shared.Environment.Create([Folder.Fields.Name]);

    using (var tsx = new DbSession(env))
    {
        new Folder(tsx)
        {
            Name = "Hallo Flurin"
        };

        new Folder(tsx)
        {
            Name = "Brühwiler Flurin der Erste"
        };

        new Folder(tsx)
        {
            Name = "Flu Flu"
        };

        new Folder(tsx)
        {
            Name = "Anna max Wynn"
        };

        tsx.Commit();
    }

    {
        using var t = new DbSession(env);

        foreach (var folder in Searcher.Search<Folder>(t, new FieldCriterion
                 {
                     FieldId = Folder.Fields.Name,
                     Value = "Flurin"
                 }))
        {
            Console.WriteLine(folder.Name);
        }
    }
}
catch (Exception e)
{
    Console.WriteLine(e);
    Console.WriteLine(e.StackTrace);
}