using LightningDB;



using (var tx = env.BeginTransaction())
{
    using (var db = tx.OpenDatabase(configuration: new DatabaseConfiguration
           {
               Flags = DatabaseOpenFlags.Create
           }))
    {
        ReadOnlySpan<byte> key = [123];
        ReadOnlySpan<byte> value = [21];
        tx.Put(db, key, value);
        tx.Commit();
    }
}

Console.WriteLine("Hello, World!");


