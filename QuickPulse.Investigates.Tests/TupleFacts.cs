using System.Runtime.CompilerServices; // ITuple
using QuickPulse.Explains.Text;

namespace QuickPulse.Investigates.Tests
{
    public class TupleFacts
    {
        // ── ValueTuple<T1,T2> ──────────────────────────────────────────────

        [Fact]
        public void ValueTuple_2_equal()
        {
            var findings = Investigate.These((42, 666), (42, 666));
            Assert.True(findings.AllEqual);
        }

        [Fact]
        public void ValueTuple_2_not_equal_first()
        {
            var findings = Investigate.These((41, 666), (42, 666));
            Assert.False(findings.AllEqual);
            Assert.Equal("[0]: 41 /= 42", findings.Report);
        }

        [Fact]
        public void ValueTuple_2_not_equal_second()
        {
            var findings = Investigate.These((42, 667), (42, 666));
            Assert.False(findings.AllEqual);
            Assert.Equal("[1]: 667 /= 666", findings.Report);
        }

        // Different arity → use ITuple to compare apples-to-apples at compile time
        [Fact]
        public void ValueTuple_length_mismatch_this_smaller()
        {
            ITuple left = (42, 1);
            ITuple right = (42, 1, 7);

            var findings = Investigate.These(left, right);
            Assert.False(findings.AllEqual);
            Assert.Equal("[2]: null /= 7", findings.Report);
        }

        [Fact]
        public void ValueTuple_length_mismatch_that_smaller()
        {
            ITuple left = (42, 1, 7);
            ITuple right = (42, 1);

            var findings = Investigate.These(left, right);
            Assert.False(findings.AllEqual);
            Assert.Equal("[2]: 7 /= null", findings.Report);
        }

        // Nested in an object
        public class WithCoords { public (int X, int Y) Coords { get; set; } }

        [Fact]
        public void ValueTuple_in_object_equal()
        {
            var a = new WithCoords { Coords = (1, 2) };
            var b = new WithCoords { Coords = (1, 2) };

            var findings = Investigate.These(a, b);
            Assert.True(findings.AllEqual);
        }

        [Fact]
        public void ValueTuple_in_object_not_equal_reports_indexed_elements()
        {
            var a = new WithCoords { Coords = (1, 99) };
            var b = new WithCoords { Coords = (1, 2) };

            var findings = Investigate.These(a, b);
            Assert.False(findings.AllEqual);
            Assert.Equal("Coords[1]: 99 /= 2", findings.Report);
        }

        // ── System.Tuple<T1,T2> ────────────────────────────────────────────

        [Fact]
        public void RefTuple_2_equal()
        {
            var findings = Investigate.These(Tuple.Create(1, 2), Tuple.Create(1, 2));
            Assert.True(findings.AllEqual);
        }

        [Fact]
        public void RefTuple_2_not_equal_multiple_lines_ordered()
        {
            var findings = Investigate.These(Tuple.Create(1, 99), Tuple.Create(2, 100));
            Assert.False(findings.AllEqual);

            var reader = LinesReader.FromText(findings.Report);
            Assert.Equal("[0]: 1 /= 2", reader.NextLine());
            Assert.Equal("[1]: 99 /= 100", reader.NextLine());
        }

        // Different arity with classic Tuple → also compare via ITuple
        [Fact]
        public void RefTuple_length_mismatch()
        {
            ITuple left = Tuple.Create(1, 2);
            ITuple right = Tuple.Create(1, 2, 3);

            var findings = Investigate.These(left, right);
            Assert.False(findings.AllEqual);
            Assert.Equal("[2]: null /= 3", findings.Report);
        }
    }
}
