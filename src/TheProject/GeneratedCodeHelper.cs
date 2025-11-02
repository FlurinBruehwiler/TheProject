using System.Collections;

namespace TheProject;

public interface ITransactionObject
{
    Transaction _transaction { get; set; }
    Guid _objId { get; set; }
}

public static class GeneratedCodeHelper
{
    public static T? GetNullableAssoc<T>(Transaction transaction, Guid objId, Guid fldId) where T : struct, ITransactionObject
    {
        var asoValue = transaction.GetSingleAsoValue(objId, fldId);
        if (!asoValue.HasValue)
            return null;

        return new T { _transaction = transaction, _objId = asoValue.Value };
    }

    public static T GetAssoc<T>(Transaction transaction, Guid objId, Guid fldId) where T : struct, ITransactionObject
    {
        var asoValue = transaction.GetSingleAsoValue(objId, fldId);
        if (!asoValue.HasValue)
            throw new Exception("what should happen here?");

        return new T { _transaction = transaction, _objId = asoValue.Value };
    }
}

public struct AssocCollection<T> : ICollection<T> where T : ITransactionObject
{
    public Guid _objId;
    public Guid _fldId;
    public Transaction _transaction;

    public AssocCollection(Transaction transaction, Guid objId, Guid fldId)
    {
        _transaction = transaction;
        _objId = objId;
        _fldId = fldId;
    }

    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(T item)
    {
        _transaction.CreateAso(_objId, _fldId, item._objId, new Guid()); //todo find other assoc field
    }

    public void Clear()
    {
        //todo
    }

    public bool Contains(T item)
    {
        foreach (var otherObj in _transaction.EnumerateAso(_objId, _fldId))
        {
            if (otherObj.ObjId == item._objId)
                return true;
        }

        return false;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(T item)
    {
        throw new NotImplementedException();
    }

    public int Count { get; }
    public bool IsReadOnly => false;

}

