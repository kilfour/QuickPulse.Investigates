using QuickPulse.Explains.Text;

namespace QuickPulse.Investigates.Tests;

public class CollectionsTests
{
    [Fact]
    public void Ordered_equal()
    {
        var one = new List<int> { 1, 2 };
        var two = new List<int> { 1, 2 };
        var findings = Investigate.These(one, two);
        Assert.True(findings.AllEqual, findings.Report);
    }

    [Fact]
    public void Ordered_not_equal()
    {
        var one = new List<int> { 1, 2 };
        var two = new List<int> { 2, 1 };
        var findings = Investigate.These(one, two);
        Assert.False(findings.AllEqual);
        var reader = LinesReader.FromText(findings.Report);
        Assert.Equal("[0]: 1 /= 2", reader.NextLine());
        Assert.Equal("[1]: 2 /= 1", reader.NextLine());
        Assert.True(reader.EndOfContent());
    }

    [Fact(Skip = "not implemented")]
    public void NotOrdered_equal()
    {
        var one = new List<int> { 1, 2 };
        var two = new List<int> { 2, 1 };
        //var options = Options.Sorting<List<int>>(a => a);
        var findings = Investigate.These(one, two);
        Assert.True(findings.AllEqual, findings.Report);
    }

    [Fact]
    public void HashSet_equal()
    {
        var one = new HashSet<int> { 1, 2 };
        var two = new HashSet<int> { 1, 2 };
        var findings = Investigate.These(one, two);
        Assert.True(findings.AllEqual, findings.Report);
    }

    [Fact]
    public void HashSet_null_equal()
    {
        var one = new HashSet<int?> { 1, null };
        var two = new HashSet<int?> { null, 1 };
        var findings = Investigate.These(one, two);
        Assert.True(findings.AllEqual, findings.Report);
    }

    [Fact]
    public void HashSet_not_equal()
    {
        var one = new HashSet<int> { 1, 2 };
        var two = new HashSet<int> { 1, 3 };
        var findings = Investigate.These(one, two);
        Assert.False(findings.AllEqual);
        var reader = LinesReader.FromText(findings.Report);
        Assert.Equal("[0]: 2 /= null", reader.NextLine());
        Assert.Equal("[1]: null /= 3", reader.NextLine());
        Assert.True(reader.EndOfContent());
    }

    [Fact]
    public void HashSet_equal_again()
    {
        var one = new HashSet<int> { 1, 2 };
        var two = new HashSet<int> { 2, 1 };
        var findings = Investigate.These(one, two);
        Assert.True(findings.AllEqual, findings.Report);
    }

    [Fact]
    public void HashSet_equal_ordinal_case()
    {
        var one = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "A" };
        var two = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "a" };
        var findings = Investigate.These(one, two);
        Assert.True(findings.AllEqual, findings.Report);
    }

    [Fact]
    public void HashSet_equal_dedupes()
    {
        var one = new HashSet<int>() { 1, 1 };
        var two = new HashSet<int> { 1 };
        var findings = Investigate.These(one, two);
        Assert.True(findings.AllEqual, findings.Report);
    }
}