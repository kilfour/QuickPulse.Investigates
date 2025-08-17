using System.Runtime.CompilerServices; // ITuple
using QuickPulse.Explains.Text;

namespace QuickPulse.Investigates.Tests
{
    public class TrickyNestedFacts
    {
        private sealed class Key
        {
            public int Id { get; }
            public Key(int id) => Id = id;
            public override bool Equals(object? obj) => obj is Key k && k.Id == Id;
            public override int GetHashCode() => Id.GetHashCode();
            public override string ToString() => $"Key({Id})";
        }

        private sealed class Holder
        {
            public Dictionary<object, object> Buckets { get; } = new();
        }

        [Fact]
        public void Nested_dict_list_tuple_mismatch_and_missing_key()
        {
            var left = new Holder();
            left.Buckets[new Key(10)] = new List<ITuple> { (1, 1), (2, 2) };

            var right = new Holder();
            right.Buckets[new Key(10)] = new List<ITuple> { (1, 1), (2, 2, 9) };
            right.Buckets["extra"] = 7;

            var findings = Investigate.These(left, right);
            Assert.False(findings.AllEqual);

            var reader = LinesReader.FromText(findings.Report);

            Assert.Equal("Buckets[key:{ Id: 10 }][1][2]: null /= 9", reader.NextLine());
            Assert.Equal("Buckets[key:\"extra\"]: null /= 7", reader.NextLine());
        }
    }
}
