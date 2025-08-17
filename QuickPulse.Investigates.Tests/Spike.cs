using QuickPulse.Bolts;
using QuickPulse.Explains.Text;

namespace QuickPulse.Investigates.Tests;

public class Spike
{
    [Fact]
    public void Strings_equal()
    {
        var findings = Investigate.These("same", "same");
        Assert.True(findings.AllEqual, findings.Report);
    }

    [Fact]
    public void Strings_not_equal()
    {
        var findings = Investigate.These("same", "not");
        Assert.False(findings.AllEqual);
        Assert.Equal("\"same\" /= \"not\"", findings.Report);
    }

    public class Person { public string Name { get; set; } = string.Empty; }

    [Fact]
    public void Object_Equal()
    {
        var findings = Investigate.These(
                new Person() { Name = "Alice" },
                new Person() { Name = "Alice" });
        Assert.True(findings.AllEqual);
    }

    [Fact]
    public void Object_Not_Equal()
    {
        var findings = Investigate.These(
                new Person() { Name = "Alice" },
                new Person() { Name = "Bob" });
        Assert.False(findings.AllEqual);
        Assert.Equal("Name: \"Alice\" /= \"Bob\"", findings.Report);
    }

    public class AgedPerson : Person { public int Age { get; set; } }
    [Fact]
    public void Object_Not_Equal_multiple_errors()
    {
        var findings = Investigate.These(
                new AgedPerson() { Name = "Alice", Age = 25 },
                new AgedPerson() { Name = "Bob", Age = 22 });
        Assert.False(findings.AllEqual);
        var reader = LinesReader.FromText(findings.Report);
        Assert.Equal("Age: 25 /= 22", reader.NextLine());
        Assert.Equal("Name: \"Alice\" /= \"Bob\"", reader.NextLine());
    }

    [Fact]
    public void Boxed_int_equal()
    {
        var findings = Investigate.These(new Box<int>(42), new Box<int>(42));
        Assert.True(findings.AllEqual);
    }

    [Fact]
    public void Boxed_int_not_equal()
    {
        var findings = Investigate.These(new Box<int>(42), new Box<int>(666));
        Assert.False(findings.AllEqual);
        Assert.Equal("Value: 42 /= 666", findings.Report);
    }

    [Fact]
    public void List_int_equal()
    {
        var findings = Investigate.These(new List<int>() { 42 }, new List<int>() { 42 });
        Assert.True(findings.AllEqual);
    }

    [Fact]
    public void List_int_not_equal()
    {
        var findings = Investigate.These(new List<int>() { 42 }, new List<int>() { 666 });
        Assert.False(findings.AllEqual);
        Assert.Equal("[0]: 42 /= 666", findings.Report);
    }

    [Fact]
    public void List_int_count_not_equal_this_smaller()
    {
        var findings = Investigate.These(new List<int>() { 42 }, new List<int>() { 42, 666 });
        Assert.False(findings.AllEqual);
        Assert.Equal("[1]: null /= 666", findings.Report);
    }

    [Fact]
    public void List_int_count_not_equal_that_smaller()
    {
        var findings = Investigate.These(new List<int>() { 42, 666 }, new List<int>() { 42 });
        Assert.False(findings.AllEqual);
        Assert.Equal("[1]: 666 /= null", findings.Report);
    }

    public class ListHolder() { public List<int> TheList { get; } = []; }

    [Fact]
    public void List_int_in_an_object_equals()
    {
        var one = new ListHolder();
        one.TheList.Add(42);

        var two = new ListHolder();
        two.TheList.Add(42);

        var findings = Investigate.These(one, two);
        Assert.True(findings.AllEqual);
    }

    [Fact]
    public void List_int_in_an_object_not_equals()
    {
        var one = new ListHolder();
        one.TheList.Add(42);
        one.TheList.Add(666);

        var two = new ListHolder();
        two.TheList.Add(43);
        two.TheList.Add(777);

        var findings = Investigate.These(one, two);
        Assert.False(findings.AllEqual);
        var reader = LinesReader.FromText(findings.Report);
        Assert.Equal("TheList[0]: 42 /= 43", reader.NextLine());
        Assert.Equal("TheList[1]: 666 /= 777", reader.NextLine());
    }
}
