namespace Playground;

class Program
{
    static void Main()
    {
        // Utility: generate BusinessModelDump.json content.
        // GUIDs were generated via Guid.NewGuid() and then fixed here.

        var ids = new[]
        {
            // 1..71
            "e166943a-b841-4523-a513-8cb9510dea7e", // modelGuid
            "cd030f9b-24ed-4792-a48d-c187939cae85", // BusinessCase TypId
            "40d51f0a-31f0-4a04-9d16-77f454fd914e", // Document TypId
            "81daacd7-4b3f-44bf-b73c-52c7012ad758", // OrganizationalUnit TypId
            "3777d451-b036-4772-9358-5a67ab44763b", // User TypId
            "3c498c35-ec98-4705-9c3e-442588838f4e", // Session TypId
            "30561d20-43d6-435d-a5db-9f151b3ec970", // AgendaItem TypId
            "8a047a4b-d907-46c7-8d27-1c8b04ceae77", // DocumentCategory TypId
            "84070119-dce8-46a2-b181-bc98cc33df58", // Folder TypId

            // 10..43 scalar field ids (34)
            "2a743537-6e95-4ec3-8422-be79838b7bd5", // BusinessCase.Title
            "24c95c72-2331-4b22-ad50-d6924265f009", // BusinessCase.Number
            "d1576aac-1e5e-4832-93ed-d912b4911f02", // BusinessCase.State
            "b51f1c43-9fb0-4e79-99bd-041a69e95346", // BusinessCase.CreatedAt
            "0c52f663-a159-4e40-91e4-f7327b13e793", // BusinessCase.Locked
            "f2284a04-897d-44da-a3f2-ed06d75f67b0", // BusinessCase.IsArchived

            "3d50a185-4c99-43ab-9e18-0398aedbfa49", // Document.Title
            "542d76bd-5a9d-450c-8eda-d4a76eba4b58", // Document.CreatedAt
            "e9783281-dcd3-4031-a5ed-a1ed13bbad24", // Document.State
            "a9b59ad3-d030-49f6-b939-159d78d27461", // Document.Locked
            "a978a147-ab06-40e0-86b6-268f7f7fb3c1", // Document.FileName
            "885cd140-64f5-478f-a42c-2bea6aceaf3b", // Document.FileSize

            "88bc11c2-903d-4517-b079-4f3fc5ce20b9", // OrganizationalUnit.Name
            "26d197fc-716e-4377-83a0-93e134f33b8c", // OrganizationalUnit.Code

            "105daf0f-6978-4dcb-bd64-a78a1a36b2c4", // User.UserName
            "46be95ae-0738-4c18-ae24-51a97b649a51", // User.DisplayName
            "46135953-da64-4c2c-8fd6-b8618c11cec0", // User.Email
            "76440653-66af-4c24-b3f8-80f39afe52a6", // User.IsActive
            "9b76cfbf-e9b5-41db-b655-b496fca80732", // User.CurrentUser

            "7c8e8231-f6ed-4738-9a02-c3855d262cde", // Session.Title
            "7adfd10f-1b04-464f-81e3-875fc5148f1d", // Session.StartAt
            "7b73f645-1d83-4632-9f2a-c1dc6f982e43", // Session.EndAt
            "0364620f-0b33-441e-85fe-0272149b1a87", // Session.IsPublic

            "9fc27b47-1962-4fa0-aca2-a7eb970aa22d", // AgendaItem.Title
            "1199121d-bc49-4963-9327-f3f6855fe4b3", // AgendaItem.Position
            "9afad796-18ca-44ab-a0ad-2b2a84c03de7", // AgendaItem.State
            "2cf80089-0848-47d3-b940-680440beb671", // AgendaItem.IsConfidential

            "b71be27a-717c-4866-9452-b72d06a325e9", // DocumentCategory.Key
            "8e2ba669-3ebd-4758-8d71-c0de543da922", // DocumentCategory.Name
            "63e373d8-95be-4c3a-a481-038a057ae3bf", // DocumentCategory.SortOrder
            "ff7545ed-394a-4c9f-b73b-c597120e021a", // DocumentCategory.IsConfidentialDefault

            "015b557a-cd86-41bb-8eb1-8cd4ce163dd9", // Folder.Name
            "aab2556c-adf6-47db-a6c3-53b6d80afb3d", // Folder.Path
            "f9a1e438-70f0-48b9-bdb3-e4eb8c023e73", // Folder.CreatedAt

            // 44..71 reference field ids (28)
            "77e01c0c-f7a4-47e8-b996-3790d2f601f6", // OU.Parent
            "c1e8b920-cd17-40b1-8785-ea2d9ca84b59", // OU.Children

            "f86ce70e-57df-4056-8ff2-9cc0f6af444e", // OU.Members
            "ca5cc6fa-8006-42a0-a85d-8e1c081f9be6", // User.MemberOfUnits

            "7ced3833-3857-4846-9ffb-b2acea9fb9f3", // BusinessCase.OwnerUnit
            "6b06a7d3-2c1a-4482-a7f0-271154f15229", // OU.BusinessCases

            "6af1eef1-4b48-478e-b3f4-d2634edb0f7a", // BusinessCase.Owners
            "3ec3c42b-98ea-48dd-8835-01863be885a5", // User.OwnedBusinessCases

            "5163b291-34a1-4849-85ff-3c71d356b43f", // BusinessCase.Sessions
            "7aff7bdb-743e-4ff9-abdd-936839cc72d9", // Session.BusinessCase

            "6bc1dd86-abde-4108-9e94-48c4ec85b6bf", // BusinessCase.Documents
            "2df0f6aa-582d-4daf-89a1-def70bf75aed", // Document.BusinessCase

            "9d376b51-f16b-4269-b08b-8c9e948237c2", // Session.AgendaItems
            "838b1b29-52b3-4395-869a-13a00a04fe74", // AgendaItem.Session

            "a8a91cbb-255b-4e27-835b-b0f5896d6b9d", // AgendaItem.Documents
            "637a1b99-0ccd-4eb4-9915-630170bf03c5", // Document.AgendaItems

            "25627768-6f43-4230-86bb-ffc1c57446be", // Document.Category
            "3115abc8-17d2-4542-a085-78edbb3b2ab7", // DocumentCategory.Documents

            "5d7c1847-2984-4c4d-9a08-b2aa0e772bb1", // Document.Folder
            "6a2ae56b-92f2-4201-ab74-bdb9a8515c2b", // Folder.Documents

            "5f89cd1f-20ba-4e8f-945d-19df08f305a9", // Folder.Parent
            "41ac6643-7a18-4878-b07a-8fe8d9851933", // Folder.Subfolders

            "cc315293-3eac-476a-8480-de499673f608", // Document.CreatedBy
            "d53ecf20-65c1-4f44-8e3a-fcec2bf26f52", // User.CreatedDocuments

            "35cfa966-9d9f-434f-95a7-507bb313f26f", // Document.OwnerUnit
            "2157a52c-8270-45ed-a05d-5b387829b376", // OU.Documents

            "3f0771ed-7629-495c-bff2-5b8d3939f169", // Document.ExplicitViewers
            "7c857219-3363-4183-9746-cca341d0c5f4", // User.ExplicitlyViewableDocuments
        };

        string Id(int i) => ids[i - 1];

        // Base model type ids.
        const string TypModel = "5739AE0D-E179-4D46-BAE2-081F99D699DA";
        const string TypEntityDefinition = "c15f876f-4f74-4034-9acb-03bc3b521e81";
        const string TypFieldDefinition = "42a6f33d-a938-4ad8-9682-aabdc92a53d2";
        const string TypReferenceFieldDefinition = "2147ed0f-b37d-4429-a3f8-8312c1620383";

        var modelGuid = Id(1);
        var businessCase = Id(2);
        var document = Id(3);
        var orgUnit = Id(4);
        var user = Id(5);
        var session = Id(6);
        var agendaItem = Id(7);
        var documentCategory = Id(8);
        var folder = Id(9);

        // Scalar fields.
        var bc_Title = Id(10);
        var bc_Number = Id(11);
        var bc_State = Id(12);
        var bc_CreatedAt = Id(13);
        var bc_Locked = Id(14);
        var bc_IsArchived = Id(15);

        var doc_Title = Id(16);
        var doc_CreatedAt = Id(17);
        var doc_State = Id(18);
        var doc_Locked = Id(19);
        var doc_FileName = Id(20);
        var doc_FileSize = Id(21);

        var ou_Name = Id(22);
        var ou_Code = Id(23);

        var user_UserName = Id(24);
        var user_DisplayName = Id(25);
        var user_Email = Id(26);
        var user_IsActive = Id(27);
        var user_CurrentUser = Id(28);

        var session_Title = Id(29);
        var session_StartAt = Id(30);
        var session_EndAt = Id(31);
        var session_IsPublic = Id(32);

        var ai_Title = Id(33);
        var ai_Position = Id(34);
        var ai_State = Id(35);
        var ai_IsConfidential = Id(36);

        var cat_Key = Id(37);
        var cat_Name = Id(38);
        var cat_SortOrder = Id(39);
        var cat_IsConfidentialDefault = Id(40);

        var folder_Name = Id(41);
        var folder_Path = Id(42);
        var folder_CreatedAt = Id(43);

        // Reference fields.
        var ou_Parent = Id(44);
        var ou_Children = Id(45);

        var ou_Members = Id(46);
        var user_MemberOfUnits = Id(47);

        var bc_OwnerUnit = Id(48);
        var ou_BusinessCases = Id(49);

        var bc_Owners = Id(50);
        var user_OwnedBusinessCases = Id(51);

        var bc_Sessions = Id(52);
        var session_BusinessCase = Id(53);

        var bc_Documents = Id(54);
        var doc_BusinessCase = Id(55);

        var session_AgendaItems = Id(56);
        var ai_Session = Id(57);

        var ai_Documents = Id(58);
        var doc_AgendaItems = Id(59);

        var doc_Category = Id(60);
        var cat_Documents = Id(61);

        var doc_Folder = Id(62);
        var folder_Documents = Id(63);

        var folder_Parent = Id(64);
        var folder_Subfolders = Id(65);

        var doc_CreatedBy = Id(66);
        var user_CreatedDocuments = Id(67);

        var doc_OwnerUnit = Id(68);
        var ou_Documents = Id(69);

        var doc_ExplicitViewers = Id(70);
        var user_ExplicitlyViewableDocuments = Id(71);

        // Build JSON model.
        var entities = new Dictionary<string, object>();

        object EntityDef(string key, string id, IEnumerable<string> fields, IEnumerable<string> refs)
        {
            return new Dictionary<string, object>
            {
                ["$type"] = TypEntityDefinition,
                ["Key"] = key,
                ["Name"] = key,
                ["Id"] = id,
                ["Model"] = modelGuid,
                ["FieldDefinitions"] = fields.ToArray(),
                ["ReferenceFieldDefinitions"] = refs.ToArray(),
            };
        }

        object FieldDef(string key, string id, string dataType, bool indexed, string owningEntity)
        {
            return new Dictionary<string, object>
            {
                ["$type"] = TypFieldDefinition,
                ["Key"] = key,
                ["Name"] = key,
                ["Id"] = id,
                ["DataType"] = dataType,
                ["IsIndexed"] = indexed,
                ["OwningEntity"] = owningEntity,
            };
        }

        object RefDef(string key, string id, string refType, string owningEntity, string other)
        {
            return new Dictionary<string, object>
            {
                ["$type"] = TypReferenceFieldDefinition,
                ["Key"] = key,
                ["Name"] = key,
                ["Id"] = id,
                ["RefType"] = refType,
                ["OwningEntity"] = owningEntity,
                ["OtherReferenceFields"] = other,
            };
        }

        entities[modelGuid] = new Dictionary<string, object>
        {
            ["$type"] = TypModel,
            ["Name"] = "BusinessModel",
            ["EntityDefinitions"] = new[] { businessCase, document, orgUnit, user, session, agendaItem, documentCategory, folder },
        };

        entities[businessCase] = EntityDef(
            "BusinessCase",
            businessCase,
            new[] { bc_Title, bc_Number, bc_State, bc_CreatedAt, bc_Locked, bc_IsArchived },
            new[] { bc_OwnerUnit, bc_Owners, bc_Sessions, bc_Documents }
        );

        entities[document] = EntityDef(
            "Document",
            document,
            new[] { doc_Title, doc_CreatedAt, doc_State, doc_Locked, doc_FileName, doc_FileSize },
            new[] { doc_BusinessCase, doc_AgendaItems, doc_Category, doc_Folder, doc_CreatedBy, doc_OwnerUnit, doc_ExplicitViewers }
        );

        entities[orgUnit] = EntityDef(
            "OrganizationalUnit",
            orgUnit,
            new[] { ou_Name, ou_Code },
            new[] { ou_Parent, ou_Children, ou_Members, ou_BusinessCases, ou_Documents }
        );

        entities[user] = EntityDef(
            "User",
            user,
            new[] { user_UserName, user_DisplayName, user_Email, user_IsActive, user_CurrentUser },
            new[] { user_MemberOfUnits, user_OwnedBusinessCases, user_CreatedDocuments, user_ExplicitlyViewableDocuments }
        );

        entities[session] = EntityDef(
            "Session",
            session,
            new[] { session_Title, session_StartAt, session_EndAt, session_IsPublic },
            new[] { session_BusinessCase, session_AgendaItems }
        );

        entities[agendaItem] = EntityDef(
            "AgendaItem",
            agendaItem,
            new[] { ai_Title, ai_Position, ai_State, ai_IsConfidential },
            new[] { ai_Session, ai_Documents }
        );

        entities[documentCategory] = EntityDef(
            "DocumentCategory",
            documentCategory,
            new[] { cat_Key, cat_Name, cat_SortOrder, cat_IsConfidentialDefault },
            new[] { cat_Documents }
        );

        entities[folder] = EntityDef(
            "Folder",
            folder,
            new[] { folder_Name, folder_Path, folder_CreatedAt },
            new[] { folder_Documents, folder_Parent, folder_Subfolders }
        );

        // Scalar field objects.
        entities[bc_Title] = FieldDef("Title", bc_Title, "String", indexed: true, owningEntity: businessCase);
        entities[bc_Number] = FieldDef("Number", bc_Number, "String", indexed: true, owningEntity: businessCase);
        entities[bc_State] = FieldDef("State", bc_State, "String", indexed: true, owningEntity: businessCase);
        entities[bc_CreatedAt] = FieldDef("CreatedAt", bc_CreatedAt, "DateTime", indexed: true, owningEntity: businessCase);
        entities[bc_Locked] = FieldDef("Locked", bc_Locked, "Boolean", indexed: true, owningEntity: businessCase);
        entities[bc_IsArchived] = FieldDef("IsArchived", bc_IsArchived, "Boolean", indexed: true, owningEntity: businessCase);

        entities[doc_Title] = FieldDef("Title", doc_Title, "String", indexed: true, owningEntity: document);
        entities[doc_CreatedAt] = FieldDef("CreatedAt", doc_CreatedAt, "DateTime", indexed: true, owningEntity: document);
        entities[doc_State] = FieldDef("State", doc_State, "String", indexed: true, owningEntity: document);
        entities[doc_Locked] = FieldDef("Locked", doc_Locked, "Boolean", indexed: true, owningEntity: document);
        entities[doc_FileName] = FieldDef("FileName", doc_FileName, "String", indexed: false, owningEntity: document);
        entities[doc_FileSize] = FieldDef("FileSize", doc_FileSize, "Integer", indexed: false, owningEntity: document);

        entities[ou_Name] = FieldDef("Name", ou_Name, "String", indexed: true, owningEntity: orgUnit);
        entities[ou_Code] = FieldDef("Code", ou_Code, "String", indexed: true, owningEntity: orgUnit);

        entities[user_UserName] = FieldDef("UserName", user_UserName, "String", indexed: true, owningEntity: user);
        entities[user_DisplayName] = FieldDef("DisplayName", user_DisplayName, "String", indexed: true, owningEntity: user);
        entities[user_Email] = FieldDef("Email", user_Email, "String", indexed: true, owningEntity: user);
        entities[user_IsActive] = FieldDef("IsActive", user_IsActive, "Boolean", indexed: true, owningEntity: user);
        entities[user_CurrentUser] = FieldDef("CurrentUser", user_CurrentUser, "Boolean", indexed: true, owningEntity: user);

        entities[session_Title] = FieldDef("Title", session_Title, "String", indexed: true, owningEntity: session);
        entities[session_StartAt] = FieldDef("StartAt", session_StartAt, "DateTime", indexed: true, owningEntity: session);
        entities[session_EndAt] = FieldDef("EndAt", session_EndAt, "DateTime", indexed: false, owningEntity: session);
        entities[session_IsPublic] = FieldDef("IsPublic", session_IsPublic, "Boolean", indexed: true, owningEntity: session);

        entities[ai_Title] = FieldDef("Title", ai_Title, "String", indexed: true, owningEntity: agendaItem);
        entities[ai_Position] = FieldDef("Position", ai_Position, "Integer", indexed: true, owningEntity: agendaItem);
        entities[ai_State] = FieldDef("State", ai_State, "String", indexed: true, owningEntity: agendaItem);
        entities[ai_IsConfidential] = FieldDef("IsConfidential", ai_IsConfidential, "Boolean", indexed: true, owningEntity: agendaItem);

        entities[cat_Key] = FieldDef("Key", cat_Key, "String", indexed: true, owningEntity: documentCategory);
        entities[cat_Name] = FieldDef("Name", cat_Name, "String", indexed: true, owningEntity: documentCategory);
        entities[cat_SortOrder] = FieldDef("SortOrder", cat_SortOrder, "Integer", indexed: true, owningEntity: documentCategory);
        entities[cat_IsConfidentialDefault] = FieldDef("IsConfidentialDefault", cat_IsConfidentialDefault, "Boolean", indexed: true, owningEntity: documentCategory);

        entities[folder_Name] = FieldDef("Name", folder_Name, "String", indexed: true, owningEntity: folder);
        entities[folder_Path] = FieldDef("Path", folder_Path, "String", indexed: true, owningEntity: folder);
        entities[folder_CreatedAt] = FieldDef("CreatedAt", folder_CreatedAt, "DateTime", indexed: true, owningEntity: folder);

        // Reference field objects.
        entities[ou_Parent] = RefDef("Parent", ou_Parent, "SingleOptional", orgUnit, ou_Children);
        entities[ou_Children] = RefDef("Children", ou_Children, "Multiple", orgUnit, ou_Parent);

        entities[ou_Members] = RefDef("Members", ou_Members, "Multiple", orgUnit, user_MemberOfUnits);
        entities[user_MemberOfUnits] = RefDef("MemberOfUnits", user_MemberOfUnits, "Multiple", user, ou_Members);

        entities[bc_OwnerUnit] = RefDef("OwnerUnit", bc_OwnerUnit, "SingleOptional", businessCase, ou_BusinessCases);
        entities[ou_BusinessCases] = RefDef("BusinessCases", ou_BusinessCases, "Multiple", orgUnit, bc_OwnerUnit);

        entities[bc_Owners] = RefDef("Owners", bc_Owners, "Multiple", businessCase, user_OwnedBusinessCases);
        entities[user_OwnedBusinessCases] = RefDef("OwnedBusinessCases", user_OwnedBusinessCases, "Multiple", user, bc_Owners);

        entities[bc_Sessions] = RefDef("Sessions", bc_Sessions, "Multiple", businessCase, session_BusinessCase);
        entities[session_BusinessCase] = RefDef("BusinessCase", session_BusinessCase, "SingleOptional", session, bc_Sessions);

        entities[bc_Documents] = RefDef("Documents", bc_Documents, "Multiple", businessCase, doc_BusinessCase);
        entities[doc_BusinessCase] = RefDef("BusinessCase", doc_BusinessCase, "SingleOptional", document, bc_Documents);

        entities[session_AgendaItems] = RefDef("AgendaItems", session_AgendaItems, "Multiple", session, ai_Session);
        entities[ai_Session] = RefDef("Session", ai_Session, "SingleOptional", agendaItem, session_AgendaItems);

        entities[ai_Documents] = RefDef("Documents", ai_Documents, "Multiple", agendaItem, doc_AgendaItems);
        entities[doc_AgendaItems] = RefDef("AgendaItems", doc_AgendaItems, "Multiple", document, ai_Documents);

        entities[doc_Category] = RefDef("Category", doc_Category, "SingleOptional", document, cat_Documents);
        entities[cat_Documents] = RefDef("Documents", cat_Documents, "Multiple", documentCategory, doc_Category);

        entities[doc_Folder] = RefDef("Folder", doc_Folder, "SingleOptional", document, folder_Documents);
        entities[folder_Documents] = RefDef("Documents", folder_Documents, "Multiple", folder, doc_Folder);

        entities[folder_Parent] = RefDef("Parent", folder_Parent, "SingleOptional", folder, folder_Subfolders);
        entities[folder_Subfolders] = RefDef("Subfolders", folder_Subfolders, "Multiple", folder, folder_Parent);

        entities[doc_CreatedBy] = RefDef("CreatedBy", doc_CreatedBy, "SingleOptional", document, user_CreatedDocuments);
        entities[user_CreatedDocuments] = RefDef("CreatedDocuments", user_CreatedDocuments, "Multiple", user, doc_CreatedBy);

        entities[doc_OwnerUnit] = RefDef("OwnerUnit", doc_OwnerUnit, "SingleOptional", document, ou_Documents);
        entities[ou_Documents] = RefDef("Documents", ou_Documents, "Multiple", orgUnit, doc_OwnerUnit);

        entities[doc_ExplicitViewers] = RefDef("ExplicitViewers", doc_ExplicitViewers, "Multiple", document, user_ExplicitlyViewableDocuments);
        entities[user_ExplicitlyViewableDocuments] = RefDef("ExplicitlyViewableDocuments", user_ExplicitlyViewableDocuments, "Multiple", user, doc_ExplicitViewers);

        // Sanity: ensure no duplicate keys and all ids are present.
        var keySet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var k in entities.Keys)
        {
            if (!keySet.Add(k))
                throw new Exception($"Duplicate entity key: {k}");
        }

        var root = new Dictionary<string, object>
        {
            ["modelGuid"] = modelGuid,
            ["entities"] = entities,
        };

        var json = System.Text.Json.JsonSerializer.Serialize(root, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
        });

        Console.WriteLine(json);
    }
}
