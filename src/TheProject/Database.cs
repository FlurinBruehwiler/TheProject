using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LightningDB;

namespace TheProject;

public class Transaction : IDisposable
{
    public static DatabaseConfiguration CreateConfiguration = new()
    {
        Flags = DatabaseOpenFlags.Create
    };

    public LightningTransaction LightningTransaction;
    public LightningDatabase Database;

    public Transaction(LightningEnvironment env)
    {
        LightningTransaction = env.BeginTransaction();
        Database = LightningTransaction.OpenDatabase(configuration: CreateConfiguration);
    }

    public void Commit()
    {
        LightningTransaction.Commit();
    }

    public void DebugPrintAllValues()
    {
        Console.WriteLine("DB Content:");

        using var cursor = LightningTransaction.CreateCursor(Database);
        var result = cursor.SetRange([0]);
        if (result == MDBResultCode.Success)
        {
            do
            {
                var current = cursor.GetCurrent();
                var currentKey = current.key.AsSpan();
                var currentValue = current.value.AsSpan();

                for (int i = 0; i < currentKey.Length / 16; i++)
                {
                    Console.Write(MemoryMarshal.Read<Guid>(currentKey.Slice(i * 16, 16)) + ", ");
                }

                Console.Write(":");

                Console.WriteLine((ValueTyp)currentValue[0]);
            }
            // Move to the next duplicate value
            while (cursor.Next().resultCode == MDBResultCode.Success);
        }
    }

    public void DeleteObj(Guid id)
    {
        var prefix = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref id, 1));

        using var cursor = LightningTransaction.CreateCursor(Database);

        Span<byte> tempBuf = stackalloc byte[4 * 16];

        //delete everything that starts with the ObjId
        if (cursor.SetRange(prefix) == MDBResultCode.Success)
        {
            do
            {
                var (_, k, v) = cursor.GetCurrent();

                if(!k.AsSpan().Slice(0, 16).SequenceEqual(prefix))
                    break;

                if (v.AsSpan()[0] == (byte)ValueTyp.Aso)
                {
                    //flip to get other assoc
                    k.AsSpan().Slice(0, 2 * 16).CopyTo(tempBuf.Slice(2 * 16, 2 * 16));
                    k.AsSpan().Slice(16 * 2, 2 * 16).CopyTo(tempBuf.Slice(0, 2 * 16));
                    LightningTransaction.Delete(Database, tempBuf);
                }

                cursor.Delete();
            } while (cursor.Next().resultCode == MDBResultCode.Success);
        }
    }

    public Guid CreateObj(Guid typId)
    {
        var id = Guid.NewGuid();

        Span<byte> keyBuf = stackalloc byte[2 * 16];
        Write(id, keyBuf.Slice(0, 16));
        Write(typId, keyBuf.Slice(16, 16));

        LightningTransaction.Put(Database, keyBuf, [(byte)ValueTyp.Obj]);

        return id;
    }

    public void CreateAso(Guid objIdA, Guid fldIdA, Guid objIdB, Guid fldIdB)
    {
        Span<byte> keyBuf = stackalloc byte[4 * 16];
        Write(objIdA, keyBuf.Slice(0*16, 16));
        Write(fldIdA, keyBuf.Slice(1*16, 16));
        Write(objIdB, keyBuf.Slice(2*16, 16));
        Write(fldIdB, keyBuf.Slice(3*16, 16));
        LightningTransaction.Put(Database, keyBuf, [(byte)ValueTyp.Aso]);

        Span<byte> otherKeyBuf = stackalloc byte[4 * 16];
        Write(objIdB, otherKeyBuf.Slice(0*16, 16));
        Write(fldIdB, otherKeyBuf.Slice(1*16, 16));
        Write(objIdA, otherKeyBuf.Slice(2*16, 16));
        Write(fldIdA, otherKeyBuf.Slice(3*16, 16));
        LightningTransaction.Put(Database, otherKeyBuf, [(byte)ValueTyp.Aso]);
    }

    public void RemoveAso(Guid objIdA, Guid fldIdA, Guid objIdB, Guid fldIdB)
    {
        Span<byte> keyBuf = stackalloc byte[4 * 16];
        Write(objIdA, keyBuf.Slice(0*16, 16));
        Write(fldIdA, keyBuf.Slice(1*16, 16));
        Write(objIdB, keyBuf.Slice(2*16, 16));
        Write(fldIdB, keyBuf.Slice(3*16, 16));
        LightningTransaction.Delete(Database, keyBuf);

        Span<byte> otherKeyBuf = stackalloc byte[4 * 16];
        Write(objIdB, otherKeyBuf.Slice(0*16, 16));
        Write(fldIdB, otherKeyBuf.Slice(1*16, 16));
        Write(objIdA, otherKeyBuf.Slice(2*16, 16));
        Write(fldIdA, otherKeyBuf.Slice(3*16, 16));
        LightningTransaction.Delete(Database, otherKeyBuf);
    }

    public void SetFldValue(Guid objId, Guid fldId, FldValue fldValue)
    {
        Span<byte> keyBuf = stackalloc byte[2 * 16];
        Write(objId, keyBuf.Slice(0*16, 16));
        Write(fldId, keyBuf.Slice(1*16, 16));

        if (Helper.MemoryEquals(fldValue, default))
        {
            LightningTransaction.Delete(Database, keyBuf);
        }
        else
        {
            Span<byte> valueBuf = stackalloc byte[1 + 16];
            valueBuf[0] = (byte)ValueTyp.Val;
            Write(fldValue, valueBuf.Slice(1, 16));

            LightningTransaction.Put(Database, keyBuf, valueBuf);
        }
    }

    public FldValue GetFldValue(Guid objId, Guid fldId)
    {
        Span<byte> keyBuf = stackalloc byte[2 * 16];
        Write(objId, keyBuf.Slice(0*16, 16));
        Write(fldId, keyBuf.Slice(1*16, 16));

        var (s, k, v) = LightningTransaction.Get(Database, keyBuf);

        if (s != MDBResultCode.Success) //todo logging
            return default;

        return MemoryMarshal.Read<FldValue>(v.AsSpan().Slice(1, 16));
    }

    public AsoFldEnumerable EnumerateAso(Guid objId, Guid fldId)
    {
        var cursor = LightningTransaction.CreateCursor(Database);

        return new AsoFldEnumerable(cursor, objId, fldId);
    }

    public static void Write<T>(T data, Span<byte> targetBuf) where T : unmanaged
    {
        MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref data, 1)).CopyTo(targetBuf);
    }

    public void Dispose()
    {
        LightningTransaction.Dispose();
        Database.Dispose();
    }
}

public struct AsoEnumeratorObj
{
    public Guid ObjId;
    public Guid FldId;
}

public struct AsoFldEnumerable : IEnumerable<AsoEnumeratorObj>
{
    private readonly LightningCursor _cursor;
    private readonly Guid _objId;
    private readonly Guid _fldId;

    public AsoFldEnumerable(LightningCursor cursor, Guid objId, Guid fldId)
    {
        _cursor = cursor;
        _objId = objId;
        _fldId = fldId;
    }

    public AsoFldEnumerator GetEnumerator()
    {
        return new AsoFldEnumerator(_cursor, _objId, _fldId);
    }

    IEnumerator<AsoEnumeratorObj> IEnumerable<AsoEnumeratorObj>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public struct AsoFldEnumerator : IEnumerator<AsoEnumeratorObj>
{
    private AsoEnumeratorObj _current1;
    private LightningCursor cursor;
    private bool isFirst;
    private Guid objId;
    private Guid fldId;

    public AsoFldEnumerator(LightningCursor cursor, Guid objId, Guid fldId)
    {
        isFirst = true;
        this.cursor = cursor;
        this.objId = objId;
        this.fldId = fldId;
    }

    public void Dispose()
    {
        cursor.Dispose();
    }

    public bool MoveNext()
    {
        MDBResultCode code;
        MDBValue key;

        Span<byte> prefixKeyBuf = stackalloc byte[2 * 16];
        Transaction.Write(objId, prefixKeyBuf.Slice(0*16, 16));
        Transaction.Write(fldId, prefixKeyBuf.Slice(1*16, 16));

        if (isFirst)
        {
            code = cursor.SetRange(prefixKeyBuf);
            if (code != MDBResultCode.Success)
                return false;

            (_, key, _) = cursor.GetCurrent();
            isFirst = false;
        }
        else
        {
            (code, key, _) = cursor.Next();
        }

        if (code != MDBResultCode.Success)
            return false;

        if (!key.AsSpan().Slice(0, 2 * 16).SequenceEqual(prefixKeyBuf))
            return false;

        _current1.ObjId = MemoryMarshal.Read<Guid>(key.AsSpan().Slice(2 * 16, 16));
        _current1.FldId = MemoryMarshal.Read<Guid>(key.AsSpan().Slice(3 * 16, 16));

        return true;
    }

    public void Reset()
    {
        throw new NotImplementedException();
    }

    public AsoEnumeratorObj Current => _current1;

    AsoEnumeratorObj IEnumerator<AsoEnumeratorObj>.Current => _current1;

    object IEnumerator.Current => throw new NotImplementedException();
}

public static class Helper
{
    public static bool MemoryEquals<T>(T val, T other) where T : unmanaged
    {
        ReadOnlySpan<byte> value = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref val, 1));

        ReadOnlySpan<byte> otherValue = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref other, 1));

        return value.SequenceEqual(otherValue);
    }
}

//Keys:
//OBJ: ObjId, TypId
//ASO: ObjIdA, FldIdA, ObjIdB, FldIdB
//ASO: ObjIdB, FldIdB, ObjIdA, FldIdA
//VAL: ObjId, FldId

[InlineArray(16)]
public struct FldValue
{
    public byte Data;

    public static FldValue FromInt32(int intValue)
    {
        ReadOnlySpan<byte> intAsBytes = MemoryMarshal.AsBytes(
            MemoryMarshal.CreateReadOnlySpan(ref intValue, 1)
        );

        FldValue fldValue = default;
        Span<byte> fldValueAsBytes = MemoryMarshal.AsBytes(
            MemoryMarshal.CreateSpan(ref fldValue, 1)
        );

        intAsBytes.CopyTo(fldValueAsBytes);

        return fldValue;
    }

    public unsafe int ToInt32()
    {
        var c = this;
        return *(int*)&c;
    }
}

public enum ValueTyp : byte
{
    Obj = 0,
    Aso = 1,
    Val = 2,
}