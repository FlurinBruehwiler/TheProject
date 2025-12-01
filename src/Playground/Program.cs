using System;
using System.Text;
using LightningDB;

class Program
{
    static void Main()
    {
        // Specify the path to the database environment
        using var env = new LightningEnvironment("path_to_your_database", new EnvironmentConfiguration
        {
            MaxDatabases = 128
        });
        env.Open();

        LightningDatabase mainDb;

        // Begin a transaction and open (or create) a database
        using (var tx = env.BeginTransaction())
        {
            //maindb
            mainDb = tx.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });
            {
                // Put a key-value pair into the database
                tx.Put(mainDb, Encoding.UTF8.GetBytes("hello"), Encoding.UTF8.GetBytes("world"));
            }

            using (var db = tx.OpenDatabase("testDb", configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
            {
                // Put a key-value pair into the database
                tx.Put(db, Encoding.UTF8.GetBytes("hello"), Encoding.UTF8.GetBytes("world"));
            }

            tx.Commit();
        }

        // Begin a read-only transaction to retrieve the value
        using (var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly))
        // using (var db = tx.OpenDatabase())
        {
            var (resultCode, key, value) = tx.Get(mainDb, Encoding.UTF8.GetBytes("hello"));
            if (resultCode == MDBResultCode.Success)
            {
                Console.WriteLine($"{Encoding.UTF8.GetString(key.AsSpan())}: {Encoding.UTF8.GetString(value.AsSpan())}");
            }
            else
            {
                Console.WriteLine("Key not found.");
            }
        }
    }
}