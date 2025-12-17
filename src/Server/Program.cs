using Shared;
using Shared.Database;
using Shared.Generated;

//we can store all fields objId+fieldIds that where changed in a dictionary within the transaction,
//when saving, we have a separate table where we store the "history" of all objects
//we could directly add the entries to hist db in a new transaction.
//what we want is to group often used objects together for better cache efficiency, and so that these pages can be unloaded from memory

try
{
    var env = Shared.Environment.Create(new Dictionary<Guid, IndexType>
    {
        { Folder.Fields.Name, IndexType.String },
        { Folder.Fields.TestDateField, IndexType.DateTime },
    });

    using (var tsx = new DbSession(env))
    {
        new Folder(tsx)
        {
            Name = "Hallo Flurin",
            TestDateField = new DateTime(2004, 09, 13),
        };

        new Folder(tsx)
        {
            Name = "Foo der Erste flurin",
            TestDateField = new DateTime(2004, 09, 13),
        };

        new Folder(tsx)
        {
            Name = "Flurin Flu"
        };

        new Folder(tsx)
        {
            Name = "Anna max Wynn"
        };

        new Folder(tsx)
        {
            Name = "Flurin"
        };

        tsx.Commit();
    }

    {
        using var t = new DbSession(env);

        var result = Searcher.Search<Folder>(t, new SearchCriterion
        {
            Type = SearchCriterion.CriterionType.DateTime,
            DateTime = new SearchCriterion.DateTimeCriterion
            {
                FieldId = Folder.Fields.TestDateField,
                From = new DateTime(2003, 1, 1),
                To = new DateTime(2005, 1, 1)
            }
        },new SearchCriterion
        {
            Type = SearchCriterion.CriterionType.String,
            String = new SearchCriterion.StringCriterion
            {
                FieldId = Folder.Fields.Name,
                Value = "flurin",
                Type = SearchCriterion.StringCriterion.MatchType.Substring
            }
        });

        foreach (Folder folder in result)
        {
            Console.WriteLine(folder.Name);
        }

        // foreach (var matchType in Enum.GetValues<SearchCriterion.StringCriterion.MatchType>())
        // {
        //     Console.WriteLine(matchType + ":");
        //
        //     foreach (var folder in Searcher.Search<Folder>(t, new SearchCriterion
        //              {
        //                  Type = SearchCriterion.CriterionType.String,
        //                  String = new SearchCriterion.StringCriterion
        //                  {
        //                      FieldId = Folder.Fields.Name,
        //                      Value = "FLurIN",
        //                      Type = matchType
        //                  }
        //              }))
        //     {
        //         Console.WriteLine(folder.Name);
        //     }
        //
        //     Console.WriteLine();
        // }
    }
}
catch (Exception e)
{
    Console.WriteLine(e);
    Console.WriteLine(e.StackTrace);
}