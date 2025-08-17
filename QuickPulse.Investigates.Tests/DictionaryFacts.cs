using QuickPulse.Explains.Text;

namespace QuickPulse.Investigates.Tests;

public class DictionaryFacts
{
    [Fact]
    public void Dict_int_int_equal()
    {
        var left = new Dictionary<int, int> { [1] = 42, [2] = 666 };
        var right = new Dictionary<int, int> { [1] = 42, [2] = 666 };

        var findings = Investigate.These(left, right);
        Assert.True(findings.AllEqual);
    }

    [Fact]
    public void Dict_int_int_equal_unordered()
    {
        var left = new Dictionary<int, int> { [1] = 42, [2] = 666 };
        var right = new Dictionary<int, int> { [2] = 666, [1] = 42 };

        var findings = Investigate.These(left, right);
        Assert.True(findings.AllEqual);
    }

    [Fact]
    public void Dict_int_int_value_not_equal()
    {
        var left = new Dictionary<int, int> { [1] = 42 };
        var right = new Dictionary<int, int> { [1] = 666 };

        var findings = Investigate.These(left, right);
        Assert.False(findings.AllEqual);
        Assert.Equal("[key:1]: 42 /= 666", findings.Report);
    }

    [Fact]
    public void Dict_int_int_missing_key_this_smaller()
    {
        var left = new Dictionary<int, int> { [1] = 42 };
        var right = new Dictionary<int, int> { [1] = 42, [2] = 666 };

        var findings = Investigate.These(left, right);
        Assert.False(findings.AllEqual);
        Assert.Equal("[key:2]: null /= 666", findings.Report);
    }

    [Fact]
    public void Dict_int_int_missing_key_that_smaller()
    {
        var left = new Dictionary<int, int> { [1] = 42, [2] = 666 };
        var right = new Dictionary<int, int> { [1] = 42 };

        var findings = Investigate.These(left, right);
        Assert.False(findings.AllEqual);
        Assert.Equal("[key:2]: 666 /= null", findings.Report);
    }

    [Fact]
    public void Dict_string_int_value_not_equal_quotes_key()
    {
        var left = new Dictionary<string, int> { ["id"] = 1 };
        var right = new Dictionary<string, int> { ["id"] = 2 };

        var findings = Investigate.These(left, right);
        Assert.False(findings.AllEqual);
        Assert.Equal("[key:\"id\"]: 1 /= 2", findings.Report);
    }

    public class MapHolder
    {
        public Dictionary<int, int> Map { get; } = new();
    }

    [Fact]
    public void Dict_in_object_equal()
    {
        var a = new MapHolder(); a.Map[1] = 42; a.Map[2] = 666;
        var b = new MapHolder(); b.Map[2] = 666; b.Map[1] = 42;

        var findings = Investigate.These(a, b);
        Assert.True(findings.AllEqual);
    }

    [Fact]
    public void Dict_in_object_not_equal_multiple_errors()
    {
        var a = new MapHolder(); a.Map[1] = 1; a.Map[3] = 3;
        var b = new MapHolder(); b.Map[1] = 2; b.Map[2] = 2;

        var findings = Investigate.These(a, b);
        Assert.False(findings.AllEqual);

        // Expect keys sorted: 1 (value diff), 2 (missing on left), 3 (missing on right)
        var reader = LinesReader.FromText(findings.Report);
        Assert.Equal("Map[key:1]: 1 /= 2", reader.NextLine());
        Assert.Equal("Map[key:2]: null /= 2", reader.NextLine());
        Assert.Equal("Map[key:3]: 3 /= null", reader.NextLine());
    }
}
