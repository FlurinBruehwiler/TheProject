using System.Diagnostics.CodeAnalysis;
using FDMF.Core.Utils;
using LightningDB;

namespace FDMF.Core.Database;

public sealed class TransactionalKvStore : IDisposable
{
    public readonly LightningTransaction ReadTransaction;
    public readonly LightningDatabase Database;
    private readonly Arena _arena;
    public readonly LightningEnvironment Environment;

    public bool IsReadOnly { get; }

    public readonly BPlusTree? ChangeSet;

    public TransactionalKvStore(LightningEnvironment env, LightningDatabase database, Arena arena, bool readOnly = false)
    {
        ReadTransaction = env.BeginTransaction(TransactionBeginFlags.ReadOnly);
        Database = database;
        _arena = arena;
        Environment = env;
        IsReadOnly = readOnly;

        if (!readOnly)
        {
            ChangeSet = new BPlusTree();
        }
    }

    public void Commit(LightningTransaction writeTransaction)
    {
        if (IsReadOnly)
            throw new InvalidOperationException("TransactionalKvStore is read-only");

        var cursor = ChangeSet!.CreateCursor();

        Span<byte> startKey = stackalloc byte[1];
        startKey[0] = 0;

        if (cursor.SetRange(startKey) == ResultCode.Success)
        {
            do
            {
                var (_, key, valueWithFlag) = cursor.GetCurrent();

                if (key.Length == 0)
                {
                    Logging.Log(LogFlags.Error, "Invalid key!!");
                    continue;
                }

                var flag = GetFlag(valueWithFlag);
                if (flag == ValueFlag.AddModify)
                {
                    writeTransaction.Put(Database, key, GetPayload(valueWithFlag));
                }
                else if (flag == ValueFlag.Delete)
                {
                    writeTransaction.Delete(Database, key);
                }
                else
                {
                    Logging.Log(LogFlags.Error, "Invalid value flag!!");
                }
            } while (cursor.Next().ResultCode == ResultCode.Success);
        }

        ChangeSet!.Clear();
    }

    public ResultCode Put(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
    {
        if (IsReadOnly)
            throw new InvalidOperationException("TransactionalKvStore is read-only");

        var keySlice = _arena.AllocateSlice(key);
        var valueSlice = CopyValueWithFlag(value, ValueFlag.AddModify);

        return ChangeSet!.Put(keySlice, valueSlice);
    }

    public ResultCode Get(ReadOnlySpan<byte> key, [UnscopedRef] out ReadOnlySpan<byte> value)
    {
        if (!IsReadOnly)
        {
            var res = ChangeSet!.Get(key);
            if (res.ResultCode == ResultCode.Success)
            {
                var flag = GetFlag(res.Value);
                if (flag == ValueFlag.Delete)
                {
                    value = ReadOnlySpan<byte>.Empty;
                    return ResultCode.NotFound;
                }

                value = GetPayload(res.Value);
                return ResultCode.Success;
            }
        }


        var (lmdbResult, _, lmdbValue) = ReadTransaction.Get(Database, key);
        if (lmdbResult == MDBResultCode.Success)
        {
            value = lmdbValue.AsSpan();
            return ResultCode.Success;
        }

        value = ReadOnlySpan<byte>.Empty;
        return ResultCode.NotFound;
    }

    public ResultCode Delete(ReadOnlySpan<byte> key)
    {
        if (IsReadOnly)
            throw new InvalidOperationException("TransactionalKvStore is read-only");

        if (ReadTransaction.Get(Database, key).resultCode == MDBResultCode.Success)
        {
            var keySlice = _arena.AllocateSlice(key);
            var valueSlice = CopyValueWithFlag(ReadOnlySpan<byte>.Empty, ValueFlag.Delete);
            ChangeSet!.Put(keySlice, valueSlice);
        }
        else
        {
            ChangeSet!.Delete(key);
        }

        return ResultCode.Success;
    }

    public Cursor CreateCursor()
    {
        return new Cursor(this)
        {
            LightningCursor = ReadTransaction.CreateCursor(Database),
            ChangeSetCursor = IsReadOnly ? null : ChangeSet!.CreateCursor()
        };
    }

    private enum ValueFlag : byte
    {
        AddModify = 1,
        Delete = 2,
    }

    private Slice<byte> CopyValueWithFlag(ReadOnlySpan<byte> value, ValueFlag flag)
    {
        var mem = _arena.AllocateSlice<byte>(value.Length + 1);
        mem.Span[0] = (byte)flag;
        value.CopyTo(mem.Span.Slice(1));
        return mem;
    }

    private static ValueFlag GetFlag(ReadOnlySpan<byte> valueWithFlag)
    {
        return valueWithFlag.Length == 0 ? ValueFlag.Delete : (ValueFlag)valueWithFlag[0];
    }

    private static ReadOnlySpan<byte> GetPayload(ReadOnlySpan<byte> valueWithFlag)
    {
        return valueWithFlag.Length <= 1 ? ReadOnlySpan<byte>.Empty : valueWithFlag.Slice(1);
    }


    public sealed class Cursor : IDisposable
    {
        private readonly TransactionalKvStore _store;

        public required LightningCursor LightningCursor;
        public BPlusTree.Cursor? ChangeSetCursor;

        public bool BaseIsFinished;
        public bool ChangeIsFinished;

        private bool _isAfterDelete;
        private byte[]? _afterDeleteKey;

        private byte[]? _lastVisibleKey;


        public Cursor(TransactionalKvStore store)
        {
            _store = store;
        }

        public readonly ref struct CursorResult
        {
            public readonly ResultCode ResultCode;
            public readonly ReadOnlySpan<byte> Key;
            public readonly ReadOnlySpan<byte> Value;

            public CursorResult(ResultCode resultCode, ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
            {
                ResultCode = resultCode;
                Key = key;
                Value = value;
            }

            public void Deconstruct(out ResultCode resultCode, out ReadOnlySpan<byte> key, out ReadOnlySpan<byte> value)
            {
                resultCode = ResultCode;
                key = Key;
                value = Value;
            }
        }

        public ResultCode SetRange(ReadOnlySpan<byte> key)
        {
            _isAfterDelete = false;
            _afterDeleteKey = null;

            BaseIsFinished = LightningCursor.SetRange(key) == MDBResultCode.NotFound;


            if (_store.IsReadOnly)
            {
                ChangeIsFinished = true;
                return BaseIsFinished ? ResultCode.NotFound : ResultCode.Success;
            }

            ChangeIsFinished = ChangeSetCursor!.SetRange(key) == ResultCode.NotFound;


            return ResultCode.Success;
        }

        public ResultCode Delete()
        {
            if (_store.IsReadOnly)
                throw new InvalidOperationException("TransactionalKvStore is read-only");

            var current = GetCurrent();
            var currentKey = current.Key;

            if (current.ResultCode != ResultCode.Success || currentKey.Length == 0)
                return ResultCode.NotFound;

            // LMDB semantics: after cursor_del, the cursor is positioned on the deleted item and GET_CURRENT returns NotFound.
            // The next Next() call should move to the next item.
            _afterDeleteKey = currentKey.ToArray();
            _isAfterDelete = true;

            // Delegate overlay semantics (base vs changeset) to store-level Delete.
            _store.Delete(_afterDeleteKey);

            return ResultCode.Success;
        }


        private bool TryGetBaseCurrent(out ReadOnlySpan<byte> key, out ReadOnlySpan<byte> value)
        {
            if (BaseIsFinished)
            {
                key = default;
                value = default;
                return false;
            }

            var cur = LightningCursor.GetCurrent();
            key = cur.key.AsSpan();
            value = cur.value.AsSpan();
            return true;
        }

        private bool TryGetChangeCurrent(out ReadOnlySpan<byte> key, out ReadOnlySpan<byte> payload, out ValueFlag flag)
        {
            if (ChangeIsFinished)
            {
                key = default;
                payload = default;
                flag = default;
                return false;
            }

            var cur = ChangeSetCursor!.GetCurrent();
            if (cur.ResultCode != ResultCode.Success || cur.Key.Length == 0)
            {
                ChangeIsFinished = true;
                key = default;
                payload = default;
                flag = default;
                return false;
            }

            key = cur.Key;
            flag = GetFlag(cur.Value);
            payload = GetPayload(cur.Value);
            return true;
        }

        private void AdvanceBase()
        {
            BaseIsFinished = LightningCursor.Next().resultCode == MDBResultCode.NotFound;
        }

        private void AdvanceChange()
        {
            ChangeIsFinished = ChangeSetCursor!.Next().ResultCode == ResultCode.NotFound;
        }

        private CursorResult ReturnBase(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
        {
            _lastVisibleKey = key.ToArray();
            return new CursorResult(ResultCode.Success, key, value);
        }

        private CursorResult ReturnChange(ReadOnlySpan<byte> key, ReadOnlySpan<byte> payload)
        {
            _lastVisibleKey = key.ToArray();
            return new CursorResult(ResultCode.Success, key, payload);
        }

        private void AdvancePastLastVisibleKeyIfNeeded()
        {
            if (_lastVisibleKey == null)
                return;

            if (!BaseIsFinished)
            {
                var bc = LightningCursor.GetCurrent().key.AsSpan();
                if (bc.SequenceEqual(_lastVisibleKey))
                    AdvanceBase();
            }

            if (!ChangeIsFinished)
            {
                var cc = ChangeSetCursor!.GetCurrent();
                if (cc.ResultCode == ResultCode.Success && cc.Key.Length > 0 && cc.Key.SequenceEqual(_lastVisibleKey))
                    AdvanceChange();
            }
        }

        public CursorResult GetCurrent()
        {
            if (_isAfterDelete)
                return new CursorResult(ResultCode.NotFound, ReadOnlySpan<byte>.Empty, ReadOnlySpan<byte>.Empty);

            while (true)
            {
                var hasBase = TryGetBaseCurrent(out var baseKey, out var baseVal);
                var hasChange = TryGetChangeCurrent(out var changeKey, out var changeVal, out var changeFlag);

                if (!hasBase && !hasChange)
                    return new CursorResult(ResultCode.NotFound, ReadOnlySpan<byte>.Empty, ReadOnlySpan<byte>.Empty);

                if (!hasChange)
                    return ReturnBase(baseKey, baseVal);

                if (!hasBase)
                {
                    if (changeFlag == ValueFlag.Delete)
                    {
                        AdvanceChange();
                        continue;
                    }

                    return ReturnChange(changeKey, changeVal);
                }

                var cmp = BPlusTree.CompareLexicographic(baseKey, changeKey);

                if (cmp < 0)
                    return ReturnBase(baseKey, baseVal);

                if (cmp > 0)
                {
                    if (changeFlag == ValueFlag.Delete)
                    {
                        AdvanceChange();
                        continue;
                    }

                    return ReturnChange(changeKey, changeVal);
                }

                // Same key
                if (changeFlag == ValueFlag.Delete)
                {
                    AdvanceBase();
                    AdvanceChange();
                    continue;
                }

                return ReturnChange(changeKey, changeVal);
            }
        }

        public CursorResult Next()
        {
            if (_isAfterDelete)
            {
                _isAfterDelete = false;
                var key = _afterDeleteKey;
                _afterDeleteKey = null;

                if (key == null)
                    return new CursorResult(ResultCode.NotFound, ReadOnlySpan<byte>.Empty, ReadOnlySpan<byte>.Empty);

                if (SetRange(key) == ResultCode.NotFound)
                    return new CursorResult(ResultCode.NotFound, ReadOnlySpan<byte>.Empty, ReadOnlySpan<byte>.Empty);

                return GetCurrent();
            }

            if (ChangeIsFinished && BaseIsFinished)
            {
                if (_lastVisibleKey == null)
                    return new CursorResult(ResultCode.NotFound, ReadOnlySpan<byte>.Empty, ReadOnlySpan<byte>.Empty);

                if (SetRange(_lastVisibleKey) == ResultCode.NotFound)
                    return new CursorResult(ResultCode.NotFound, ReadOnlySpan<byte>.Empty, ReadOnlySpan<byte>.Empty);

                AdvancePastLastVisibleKeyIfNeeded();
                return GetCurrent();
            }

            // Normal advancement: move the cursor(s) that currently contribute the visible entry.
            if (ChangeIsFinished)
            {
                AdvanceBase();
                return GetCurrent();
            }

            if (BaseIsFinished)
            {
                AdvanceChange();
                return GetCurrent();
            }

            var baseKey = LightningCursor.GetCurrent().key.AsSpan();
            var changeCur = ChangeSetCursor!.GetCurrent();
            if (changeCur.ResultCode != ResultCode.Success || changeCur.Key.Length == 0)
            {
                ChangeIsFinished = true;
                AdvanceBase();
                return GetCurrent();
            }

            var changeKey = changeCur.Key;
            var cmp = BPlusTree.CompareLexicographic(baseKey, changeKey);

            if (cmp == 0)
            {
                AdvanceBase();
                AdvanceChange();
            }
            else if (cmp < 0)
            {
                AdvanceBase();
            }
            else
            {
                AdvanceChange();
            }

            return GetCurrent();
        }


        public void Dispose()
        {
            LightningCursor.Dispose();
        }
    }

    public void Dispose()
    {
        ReadTransaction.Dispose();
    }
}
