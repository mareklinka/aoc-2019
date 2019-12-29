using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace _14
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var lines = File.ReadAllLines("input.txt");

            var reactions = ParseReactions(lines);

            var reactor = new Reactor(reactions);

            reactor.GetChemical("FUEL", 1);

            Assert.Equal(873899, reactor.OreCount);
        }

        [Fact]
        public void Test2()
        {
            var lines = File.ReadAllLines("input.txt");

            var reactions = ParseReactions(lines);

            var reactor = new Reactor(reactions);

            var q = 1000000000000 / 873899;
            var fuelCreated = BinaryFuelSearch(reactor, q, 2 * q);

            Assert.Equal(1893569, fuelCreated);
        }

        private static long BinaryFuelSearch(Reactor r, long min, long max)
        {
            while (true)
            {
                if (max == min || max == min + 1)
                {
                    return min;
                }

                var half = min + (max - min) / 2;

                r.Reset();
                r.GetChemical("FUEL", half);

                if (r.OreCount > 1000000000000)
                {
                    max = half;
                }
                else
                {
                    min = half;
                }
            }
        }

        [Theory]
        [InlineData("10 ORE => 10 A\n1 ORE => 1 B\n7 A, 1 B => 1 C\n7 A, 1 C => 1 D\n7 A, 1 D => 1 E\n7 A, 1 E => 1 FUEL", 31)]
        [InlineData("9 ORE => 2 A\n8 ORE => 3 B\n7 ORE => 5 C\n3 A, 4 B => 1 AB\n5 B, 7 C => 1 BC\n4 C, 1 A => 1 CA\n2 AB, 3 BC, 4 CA => 1 FUEL", 165)]
        public void TestOreCost(string reactionString, int expected)
        {
            var lines = reactionString.Split("\n");

            var reactions = ParseReactions(lines);
            var reactor = new Reactor(reactions);

            reactor.GetChemical("FUEL", 1);

            Assert.Equal(expected, reactor.OreCount);
        }

        private class Reactor
        {
            private readonly Dictionary<string, Reaction> _reactionsDict;
            private Dictionary<string, long> _tank = new Dictionary<string, long>();
            public long OreCount { get; private set; }

            public Reactor(List<Reaction> reactions)
            {
                _reactionsDict = reactions.ToDictionary(_ => _.Target.Chemical);
            }

            public void Reset()
            {
                _tank.Clear();
                OreCount = 0;
            }

            public void GetChemical(string chemical, long quantity)
            {
                while (true)
                {
                    if (chemical == "ORE")
                    {
                        checked
                        {
                            OreCount += quantity;
                        }
                        
                        return;
                    }

                    if (_tank.TryGetValue(chemical, out var remainder) && remainder >= quantity)
                    {
                        _tank[chemical] -= quantity;
                        return;
                    }

                    var reaction = _reactionsDict[chemical];

                    var q = (long) Math.Ceiling((quantity - remainder) / (decimal) reaction.Target.Quantity);

                    foreach (var reactionSource in reaction.Sources)
                    {
                        GetChemical(reactionSource.Chemical, reactionSource.Quantity * q);
                    }

                    _tank[chemical] = remainder + reaction.Target.Quantity * q;
                }
            }
        }

        private List<Reaction> ParseReactions(string[] lines)
        {
            var reactions = lines.Select(ParseReaction).ToList();

            return reactions;
        }

        private Reaction ParseReaction(string s)
        {
            var split = s.Split(" => ");
            var source = split[0];
            var target = split[1];

            var targetChemical = ParseComponent(target);
            var components = source.Split(", ").Select(ParseComponent).ToList();

            return new Reaction { Target = targetChemical, Sources = components };
        }

        private static Component ParseComponent(string description)
        {
            var split = description.Split(" ");
            return new Component {Chemical = split[1], Quantity = int.Parse(split[0])};
        }

        [DebuggerDisplay("{Quantity} {Chemical}")]
        private class Component : IEquatable<Component>
        {
            public int Quantity { get; set; }

            public string Chemical { get; set; }

            public bool Equals(Component other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Quantity == other.Quantity && Chemical == other.Chemical;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Component) obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Quantity, Chemical);
            }
        }

        private class Reaction
        {
            public Component Target { get; set; }

            public List<Component> Sources { get; set; }
        }
    }
}
