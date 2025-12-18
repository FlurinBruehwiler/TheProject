using Shared;
using Shared.Database;
using TestModel.Generated;
using Xunit.Abstractions;
using Environment = Shared.Environment;

namespace Tests;

public class IndexingTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public IndexingTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Exact_String_Search()
    {
        var testModel = ProjectModel.CreateFromDirectory("TestModel");
        var env = Environment.Create(testModel, dbName: nameof(Exact_String_Search));

        using var tsx = new DbSession(env);

        new TestingFolder(tsx)
        {
            Name = "Barbapapa Ba"
        };

        new TestingFolder(tsx)
        {
            Name = "Foooooo"
        };

        var barbapapaFolder = new TestingFolder(tsx)
        {
            Name = "Barbapapa"
        };

        tsx.Commit();

        var result = Searcher.Search<TestingFolder>(tsx, new SearchCriterion
        {
            Type = SearchCriterion.CriterionType.String,
            String = new SearchCriterion.StringCriterion
            {
                FieldId = TestingFolder.Fields.Name,
                Value = "Barbapapa",
                Type = SearchCriterion.StringCriterion.MatchType.Exact
            }
        });

        Assert.Equal([barbapapaFolder], result);
    }

    [Fact]
    public void Substring_Search()
    {
        var testModel = ProjectModel.CreateFromDirectory("TestModel");
        var env = Environment.Create(testModel, dbName: nameof(Substring_Search));

        using var tsx = new DbSession(env);

        var folderA = new TestingFolder(tsx)
        {
            Name = "oooHooo"
        };

        var folderB = new TestingFolder(tsx)
        {
            Name = "oooooo"
        };

        tsx.Commit();

        var result = Searcher.Search<TestingFolder>(tsx, new SearchCriterion
        {
            Type = SearchCriterion.CriterionType.String,
            String = new SearchCriterion.StringCriterion
            {
                FieldId = TestingFolder.Fields.Name,
                Value = "oooo",
                Type = SearchCriterion.StringCriterion.MatchType.Substring,
            }
        });

        Assert.Equal([folderB], result);
    }

    [Fact]
    public void Fuzzy_Search()
    {
        var testModel = ProjectModel.CreateFromDirectory("TestModel");
        var env = Environment.Create(testModel, dbName: nameof(Fuzzy_Search));

        using var tsx = new DbSession(env);

        new TestingFolder(tsx)
        {
            Name = "Foxtrott"
        };

        var folderB = new TestingFolder(tsx)
        {
            Name = "Firefox"
        };

        tsx.Commit();

        var result = Searcher.Search<TestingFolder>(tsx, new SearchCriterion
        {
            Type = SearchCriterion.CriterionType.String,
            String = new SearchCriterion.StringCriterion
            {
                FieldId = TestingFolder.Fields.Name,
                Value = "firfox",
                Type = SearchCriterion.StringCriterion.MatchType.Fuzzy,
                FuzzyCutoff = 0.5f
            }
        });

        Assert.Equal([folderB], result);
    }
}