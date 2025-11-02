using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LightningDB;

namespace TheProject;

public class Database
{
    public LightningEnvironment Environment;
    public DatabaseConfiguration Configuration;

    public static Database Create(string path)
    {
        var db = new Database();
        db.Environment = new LightningEnvironment("mydb.db");
        db.Environment.Open();

        db.Configuration = new DatabaseConfiguration
        {
            Flags = DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesSort
        };

        return db;
    }

    public void ApplyChanges(Dataset dataset)
    {
        using (var tx = Environment.BeginTransaction())
        {
            using (var db = tx.OpenDatabase(configuration: Configuration))
            {
                ApplyObjChange(dataset, tx, db);
                ApplyAsoChange(dataset, tx, db);
                ApplyFldChange(dataset);

                tx.Commit();
            }
        }
    }

    private void ApplyObjChange(Dataset dataset, LightningTransaction tx, LightningDatabase db)
    {
        foreach (var obj in dataset.Obj)
        {
            if (obj.State == State.Deleted)
            {
                Obj o = obj;
                var key = Obj.GetKey(ref o);

                tx.Delete(db, key);
            }
            else if (obj.State == State.Added)
            {
                Obj o = obj;
                var key = Obj.GetKey(ref o);

                tx.Put(db, key, ReadOnlySpan<byte>.Empty);
            }
        }
    }

    private const int SizeOfGuid = 16;

    private void ApplyAsoChange(Dataset dataset, LightningTransaction tx, LightningDatabase db)
    {
        Span<byte> keyBuffer = stackalloc byte[SizeOfGuid + SizeOfGuid];
        Span<byte> valueBuffer = stackalloc byte[SizeOfGuid + SizeOfGuid];

        foreach (var aso in dataset.Aso)
        {
            if (aso.State == State.Added)
            {
                var a = aso;
                //key = ObjIdA + FldIdA
                MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref a.ObjIdA, 1)).CopyTo(keyBuffer);
                MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref a.FldIdA, 1)).CopyTo(keyBuffer.Slice(SizeOfGuid));

                //value = ObjIdB + FldIdB
                MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref a.ObjIdB, 1)).CopyTo(valueBuffer);
                MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref a.FldIdB, 1)).CopyTo(valueBuffer.Slice(SizeOfGuid));

                tx.Put(db, keyBuffer, valueBuffer);
            }
            else if (aso.State == State.Deleted)
            {

            }
        }
    }

    private void ApplyFldChange(Dataset dataset)
    {

    }
}

public class Dataset
{
    public List<Obj> Obj;
    public List<Aso> Aso;
    public List<Fld> Fld;
}

public struct Obj
{
    public State State;

    public Guid Id;
    public Guid TypId;

    public static ReadOnlySpan<byte> GetKey(ref Obj obj)
    {
        return MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref obj.Id, 1));
    }
}

public struct Aso
{
    public State State;

    public Guid FldIdA;
    public Guid FldIdB;
    public Guid ObjIdA;
    public Guid ObjIdB;

}

public struct Fld
{
    public State State;
    public Guid FldId;
    public FldData Data;
}

[InlineArray(16)]
public struct FldData
{
    public byte Data;
}

public enum State
{
    Added,
    Deleted
}
