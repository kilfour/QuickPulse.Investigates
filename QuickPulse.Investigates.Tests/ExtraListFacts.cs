namespace QuickPulse.Investigates.Tests;

public class ExtraListFacts
{
    [Fact]
    public void List_with_null_elements_equal()
    {
        var findings = Investigate.These(new List<int?> { null, 1 }, new List<int?> { null, 1 });
        Assert.True(findings.AllEqual);
    }

    [Fact]
    public void List_with_null_elements_not_equal()
    {
        var findings = Investigate.These(new List<int?> { null, 1 }, new List<int?> { null, 2 });
        Assert.False(findings.AllEqual);
        Assert.Equal("[1]: 1 /= 2", findings.Report);
    }

    [Fact]
    public void Array_vs_List_equal_via_IEnumerable()
    {
        var findings = Investigate.These<IEnumerable<int>>(new[] { 1, 2 }, new List<int> { 1, 2 });
        Assert.True(findings.AllEqual);
    }

    [Fact]
    public void Array_vs_List_not_equal_via_IEnumerable()
    {
        var findings = Investigate.These<IEnumerable<int>>(new[] { 1, 99 }, new List<int> { 1, 2 });
        Assert.False(findings.AllEqual);
        Assert.Equal("[1]: 99 /= 2", findings.Report);
    }

    public class NullableListHolder
    {
        public List<int>? Items { get; set; }
    }

    [Fact]
    public void Null_list_vs_empty_list_on_property()
    {
        var a = new NullableListHolder { Items = null };
        var b = new NullableListHolder { Items = new List<int>() };

        var findings = Investigate.These(a, b);
        Assert.False(findings.AllEqual);
        Assert.Equal("Items: null /= [ ]", findings.Report);
    }
}
