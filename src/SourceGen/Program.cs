using System.Runtime.InteropServices;
using System.Text;
using SourceGen;
using TheProject;

var model = ProjectModel.CreateFromDirectory("Model");

foreach (var entity in model.EntityDefinitions)
{
    var sourceBuilder = new SourceBuilder();

    sourceBuilder.AppendLine("namespace TheProject;");
    sourceBuilder.AppendLine("{");

    sourceBuilder.AddIndent();
    sourceBuilder.AppendLine($"public struct {entity.Key}");
    sourceBuilder.AppendLine("{");
    sourceBuilder.AddIndent();

    sourceBuilder.AppendLine("public Transaction _transaction { get; set; }");
    sourceBuilder.AppendLine("public Guid _objId { get; set; }");

    foreach (var field in entity.FieldDefinitions)
    {
        var dataType = field.DataType switch
        {
            FieldDataType.Integer => "int",
            FieldDataType.Decimal => "decimal",
            FieldDataType.String => "string",
            FieldDataType.DateTime => "DateTime",
            _ => throw new ArgumentOutOfRangeException()
        };

        var toFunction = field.DataType switch
        {
            FieldDataType.Integer => "ToInt32()",
            FieldDataType.Decimal => "ToDecimal()",
            FieldDataType.String => "ToString()",
            FieldDataType.DateTime => "ToDateTime()",
            _ => throw new ArgumentOutOfRangeException()
        };

        sourceBuilder.AppendLine($"public {dataType} {field.Key} => _transaction.GetFldValue(_objId, {GetGuidLiteral(field.Id)}).{toFunction};\n");
    }

    foreach (var refField in entity.ReferenceFieldDefinitions)
    {
        if (refField.RefType == RefType.SingleOptional)
        {
            sourceBuilder.AppendLine($"public {refField.OtherReferenceFieldDefinition.OwningEntityDefinition}? {refField.Key} => GeneratedCodeHelper.GetNullableAssoc<Folder>(_transaction, _objId, {GetGuidLiteral(refField.Id)});\n");
        }
        if (refField.RefType == RefType.SingleMandatory)
        {
            sourceBuilder.AppendLine($"public {refField.OtherReferenceFieldDefinition.OwningEntityDefinition} {refField.Key} => GeneratedCodeHelper.GetAssoc<Folder>(_transaction, _objId, {GetGuidLiteral(refField.Id)});\n");
        }
        else if (refField.RefType == RefType.Multiple)
        {
            sourceBuilder.AppendLine($"public AssocCollection<{refField.OtherReferenceFieldDefinition.OwningEntityDefinition}> {refField.Key} => new(_transaction, _objId, {GetGuidLiteral(refField.Id)});");
        }
    }
}

string GetGuidLiteral(Guid guid)
{
    Span<byte> guidData = stackalloc byte[16];
    MemoryMarshal.Write(guidData, guid);

    var sb = new StringBuilder();

    sb.Append("new Guid([");

    var isFirst = true;
    foreach (var b in guidData)
    {
        if (!isFirst)
            sb.Append(", ");

        sb.Append(b);
        isFirst = false;
    }

    sb.AppendLine("])");

    return sb.ToString();
}
