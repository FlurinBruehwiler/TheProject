
namespace TheProject.Generated;

public struct Folder : ITransactionObject
{
    public Transaction _transaction { get; set; }
    public Guid _objId { get; set; }

    public int ProtectionLevel => _transaction.GetFldValue(_objId, new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11)).ToInt32();

    public Folder? ParentFolder => GeneratedCodeHelper.GetNullableAssoc<Folder>(_transaction, _objId, new Guid());

    public AssocCollection<Folder> Subfolders => new(_transaction, _objId, new Guid());
}
