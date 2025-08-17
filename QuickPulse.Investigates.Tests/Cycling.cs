namespace QuickPulse.Investigates.Tests;

public class Cycling
{
    public sealed class Address
    {
        public string Street { get; set; } = string.Empty;
    }

    public sealed class Owner
    {
        public Address? Home { get; set; }
        public Address? Office { get; set; }
    }

    [Fact]
    public void Shared_left_instance_compared_against_two_right_instances_detects_diff()
    {
        var shared = new Address { Street = "Main" };
        var left = new Owner { Home = shared, Office = shared };

        var right = new Owner
        {
            Home = new Address { Street = "Main" },
            Office = new Address { Street = "Side" }
        };

        var findings = Investigate.These(left, right);
        Assert.False(findings.AllEqual);
        Assert.Equal("Office.Street: \"Main\" /= \"Side\"", findings.Report);
    }

    [Fact]
    public void Shared_right_instance_compared_against_two_left_instances_detects_diff()
    {
        var shared = new Address { Street = "Main" };
        var right = new Owner { Home = shared, Office = shared };

        var left = new Owner
        {
            Home = new Address { Street = "Main" },
            Office = new Address { Street = "Side" }
        };

        var findings = Investigate.These(left, right);
        Assert.False(findings.AllEqual);
        Assert.Equal("Office.Street: \"Side\" /= \"Main\"", findings.Report);
    }

    public sealed class Node
    {
        public int Id { get; set; }
        public Node? Next { get; set; }
    }

    [Fact]
    public void Cyclic_graph_equal_does_not_overflow_and_is_equal()
    {
        var a1 = new Node { Id = 1 };
        var a2 = new Node { Id = 2 };
        a1.Next = a2; a2.Next = a1; // 2-cycle

        var b1 = new Node { Id = 1 };
        var b2 = new Node { Id = 2 };
        b1.Next = b2; b2.Next = b1;

        var findings = Investigate.These(a1, b1);
        Assert.True(findings.AllEqual);
    }

    [Fact]
    public void Cyclic_graph_reports_nested_difference_once()
    {
        var a1 = new Node { Id = 1 };
        var a2 = new Node { Id = 2 };
        a1.Next = a2; a2.Next = a1;

        var b1 = new Node { Id = 1 };
        var b2 = new Node { Id = 99 };
        b1.Next = b2; b2.Next = b1;

        var findings = Investigate.These(a1, b1);
        Assert.False(findings.AllEqual);
        Assert.Equal("Next.Id: 2 /= 99", findings.Report);
    }
}
