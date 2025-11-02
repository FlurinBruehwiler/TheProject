using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheProject;

public class ProjectModel
{
    public EntityDefinition[] EntityDefinitions = [];

    public static ProjectModel CreateFromDirectory(string dir)
    {
        List<EntityDefinition> entities = [];
        foreach (var entityJson in Directory.EnumerateFiles(dir))
        {
            var entity = JsonSerializer.Deserialize<EntityDefinition>(entityJson);
            entities.Add(entity!);
        }

        var dict = entities.SelectMany(x => x.ReferenceFieldDefinitions).ToDictionary(x => x.Id, x => x);
        foreach (var entityDefinition in entities)
        {
            foreach (var refField in entityDefinition.ReferenceFieldDefinitions)
            {
                refField.OwningEntityDefinition = entityDefinition;
                refField.OtherReferenceFieldDefinition = dict[refField.OtherReferenceFieldDefinitionGuid];
            }
        }

        return new ProjectModel
        {
            EntityDefinitions = entities.ToArray()
        };
    }
}

public class EntityDefinition
{
    public Guid Id;
    public string Key = "";
    public TranslationText Name;
    public FieldDefinition[] FieldDefinitions;
    public ReferenceFieldDefinition[] ReferenceFieldDefinitions;
}

public class FieldDefinition
{
    public Guid Id;
    public string Key = "";
    public TranslationText Name;
    public FieldDataType DataType;
    public bool Mandatory;
}

public class ReferenceFieldDefinition
{
    public Guid Id;
    public string Key = "";
    public TranslationText Name;
    public RefType RefType;
    public EntityDefinition OwningEntityDefinition;

    public Guid OtherReferenceFieldDefinitionGuid;

    [JsonIgnore] public ReferenceFieldDefinition OtherReferenceFieldDefinition;
}

public enum RefType
{
    SingleOptional,
    SingleMandatory,
    Multiple
}

public enum FieldDataType
{
    Integer,
    Decimal,
    String,
    DateTime
}

public struct TranslationText
{
    public string Default;
}