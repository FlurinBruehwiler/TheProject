using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;


namespace Shared.Database;

public static class JsonDump
{
    public static string GetJsonDump(Environment env, DbSession dbSession)
    {
        var entityById = env.Model.EntityDefinitions.ToDictionary(x => x.Id, x => x);

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();

            writer.WritePropertyName("entities");
            writer.WriteStartObject();

            foreach (var (objId, typId) in dbSession.EnumerateObjs())
            {
                writer.WritePropertyName(objId.ToString());
                writer.WriteStartObject();

                writer.WriteString("$type", typId.ToString());

                if (entityById.TryGetValue(typId, out var entity))
                {
                    foreach (var field in entity.Fields)
                    {
                        var raw = dbSession.GetFldValue(objId, field.Id);
                        if (raw.Length == 0)
                            continue;

                        writer.WritePropertyName(field.Key);
                        WriteScalarFieldValue(writer, field.DataType, raw);
                    }

                    foreach (var refField in entity.ReferenceFields)
                    {
                        ArrayBufferWriter<Guid>? collected = null;
                        foreach (var aso in dbSession.EnumerateAso(objId, refField.Id))
                        {
                            collected ??= new ArrayBufferWriter<Guid>();
                            collected.GetSpan(1)[0] = aso.ObjId;
                            collected.Advance(1);

                            if (refField.RefType != RefType.Multiple)
                                break;
                        }

                        if (collected == null || collected.WrittenCount == 0)
                            continue;

                        writer.WritePropertyName(refField.Key);

                        if (refField.RefType == RefType.Multiple)
                        {
                            writer.WriteStartArray();
                            foreach (var id in collected.WrittenSpan)
                            {
                                writer.WriteStringValue(id.ToString());
                            }
                            writer.WriteEndArray();
                        }
                        else
                        {
                            writer.WriteStringValue(collected.WrittenSpan[0].ToString());
                        }
                    }
                }

                writer.WriteEndObject();
            }

            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteScalarFieldValue(Utf8JsonWriter writer, FieldDataType type, ReadOnlySpan<byte> raw)
    {
        // Values are stored as the payload of VAL entries; DbSession.GetFldValue already strips the ValueTyp tag.
        switch (type)
        {
            case FieldDataType.String:
                writer.WriteStringValue(Encoding.Unicode.GetString(raw));
                return;
            case FieldDataType.Integer:
                writer.WriteNumberValue(raw.Length >= sizeof(long) ? MemoryMarshal.Read<long>(raw) : 0L);
                return;
            case FieldDataType.Decimal:
                writer.WriteNumberValue(raw.Length >= sizeof(decimal) ? MemoryMarshal.Read<decimal>(raw) : 0m);
                return;
            case FieldDataType.DateTime:
                writer.WriteStringValue(raw.Length >= sizeof(long)
                    ? MemoryMarshal.Read<DateTime>(raw).ToString("O")
                    : default(DateTime).ToString("O"));
                return;
            case FieldDataType.Boolean:
                writer.WriteBooleanValue(raw.Length >= sizeof(bool) && MemoryMarshal.Read<bool>(raw));
                return;
            default:
                writer.WriteStringValue(Convert.ToBase64String(raw));
                return;
        }
    }
}