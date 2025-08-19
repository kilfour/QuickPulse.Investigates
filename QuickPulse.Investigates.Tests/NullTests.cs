namespace QuickPulse.Investigates.Tests;

public class NullTests
{
    [Fact]
    public void Simple()
    {
        Investigate.These("test", null);
    }

    public class Trying { public List<string> TheList { get; set; } = []; }

    [Fact]
    public void ListProp()
    {
        Investigate.These(new Trying(), new Trying { TheList = null! });
    }
}