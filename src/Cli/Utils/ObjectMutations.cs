using Shared;
using Shared.Database;

namespace Cli.Utils;

public static class ObjectMutations
{
    public enum MultiRefMode
    {
        Add,
        Replace
    }

    public static void ApplySets(DbSession session, ProjectModel model, EntityDefinition entity, Guid objId, IEnumerable<string> setPairs, MultiRefMode multiRefMode)
    {
        var scalarByKey = entity.Fields.ToDictionary(f => f.Key, StringComparer.OrdinalIgnoreCase);
        var refByKey = entity.ReferenceFields.ToDictionary(f => f.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var pair in setPairs ?? Array.Empty<string>())
        {
            var (k, v) = Pairs.Split(pair);

            if (scalarByKey.TryGetValue(k, out var fld))
            {
                if (v.Length == 0)
                {
                    session.SetFldValue(objId, fld.Id, ReadOnlySpan<byte>.Empty);
                }
                else
                {
                    var bytes = EncodingUtils.EncodeScalar(fld.DataType, v);
                    session.SetFldValue(objId, fld.Id, bytes);
                }

                continue;
            }

            if (refByKey.TryGetValue(k, out var rf))
            {
                if (v.Length == 0)
                {
                    session.RemoveAllAso(objId, rf.Id);
                    continue;
                }

                var parts = v.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length == 0)
                    continue;

                if (rf.RefType is RefType.SingleMandatory or RefType.SingleOptional)
                {
                    if (parts.Length != 1)
                        throw new Exception($"Reference field '{k}' is single-valued. Provide exactly one ObjId.");

                    if (!Guid.TryParse(parts[0], out var otherObjId))
                        throw new Exception($"Invalid guid '{parts[0]}' for ref field '{k}'");

                    session.RemoveAllAso(objId, rf.Id);
                    if (otherObjId != Guid.Empty)
                        session.CreateAso(objId, rf.Id, otherObjId, rf.OtherReferenceField.Id);
                }
                else
                {
                    if (multiRefMode == MultiRefMode.Replace)
                        session.RemoveAllAso(objId, rf.Id);

                    foreach (var part in parts)
                    {
                        if (!Guid.TryParse(part, out var otherObjId))
                            throw new Exception($"Invalid guid '{part}' for ref field '{k}'");

                        if (otherObjId != Guid.Empty)
                            session.CreateAso(objId, rf.Id, otherObjId, rf.OtherReferenceField.Id);
                    }
                }

                continue;
            }

            throw new Exception($"Unknown field '{k}' for type '{entity.Key}'");
        }

        ValidateMandatorySingleRefs(session, entity, objId);
    }

    public static void ValidateMandatorySingleRefs(DbSession session, EntityDefinition entity, Guid objId)
    {
        foreach (var rf in entity.ReferenceFields)
        {
            if (rf.RefType != RefType.SingleMandatory)
                continue;

            var val = session.GetSingleAsoValue(objId, rf.Id);
            if (!val.HasValue)
                throw new Exception($"Missing mandatory reference field '{rf.Key}' for type '{entity.Key}'.");
        }
    }
}
