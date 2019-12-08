using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace _06
{
    public class UnitTest1
    {
        [Fact]
        public void Part1()
        {
            var file = File.ReadAllLines("input.txt");
            var count = CountOrbits(file);

            Assert.Equal(139597, count);
        }

        [Fact]
        public void Part2()
        {
            var file = File.ReadAllLines("input.txt");
            var count = CountOrbitalTransfers(file);

            Assert.Equal(286, count);
        }

        [Fact]
        public void TestOrbits()
        {
            var file = File.ReadAllLines("input_small.txt");
            Assert.Equal(42, CountOrbits(file));
        }

        [Fact]
        public void TestOrbitalTransfers()
        {
            var file = File.ReadAllLines("input_small_2.txt");
            Assert.Equal(4, CountOrbitalTransfers(file));
        }

        private int CountOrbitalTransfers(string[] orbitLines)
        {
            var lines = orbitLines.Select(_ => _.Split(")")).ToArray();

            var satellites = new Dictionary<string, List<string>>();
            var centers = new Dictionary<string, string>();
            var objects = new HashSet<string>();

            foreach (var l in lines)
            {
                objects.Add(l[0]);
                objects.Add(l[1]);

                if (satellites.TryGetValue(l[0], out var list))
                {
                    list.Add(l[1]);
                }
                else
                {
                    satellites.Add(l[0], new List<string> { l[1] });
                }

                centers.Add(l[1], l[0]);
            }

            var currentCenter = centers["YOU"];
            var targetCenter = centers["SAN"];

            var min = GetPaths(currentCenter, targetCenter, centers, satellites, new List<string> { "YOU" });
            return min.Count - 1;
        }

        private List<string> GetPaths(string currentCenter, string targetCenter,
            Dictionary<string, string> centers, Dictionary<string, List<string>> satellites, List<string> acc)
        {
            if (currentCenter == targetCenter)
            {
                return acc;
            }

            centers.TryGetValue(currentCenter, out var nextCenter);

            if (!satellites.TryGetValue(currentCenter, out var nextSatellites))
            {
                nextSatellites = new List<string>();
            }

            List<string> available;

            if (nextCenter != null)
            {
                available = nextSatellites.Concat(new List<string> { nextCenter }).Except(acc).ToList();
            }
            else
            {
                available = nextSatellites.Except(acc).ToList();
            }

            if (!available.Any())
            {
                return null;
            }

            var min = available.Select(_ =>
            {
                var list = acc.ToList();
                list.Add(currentCenter);
                return GetPaths(_, targetCenter, centers, satellites, list);
            }).Where(_ => _ != null).OrderBy(_ => _.Count).FirstOrDefault();

            return min;
        }

        private int CountOrbits(string[] orbitLines)
        {
            var lines = orbitLines.Select(_ => _.Split(")")).ToArray();

            var satellites = new Dictionary<string, List<string>>();
            var objects = new HashSet<string>();

            foreach (var l in lines)
            {
                objects.Add(l[0]);
                objects.Add(l[1]);

                if (satellites.TryGetValue(l[0], out var list))
                {
                    list.Add(l[1]);
                }
                else
                {
                    satellites.Add(l[0], new List<string> { l[1] });
                }
            }

            var count = 0;

            var objectQ = new Queue<string>();
            var distanceQ = new Queue<int>();

            objectQ.Enqueue("COM");
            distanceQ.Enqueue(0);

            while (objectQ.Any())
            {
                var obj = objectQ.Dequeue();
                var distance = distanceQ.Dequeue();

                count += distance;

                if (satellites.TryGetValue(obj, out var sats))
                {
                    foreach (var sat in sats)
                    {
                        objectQ.Enqueue(sat);
                        distanceQ.Enqueue(distance + 1);
                    }
                }
            }

            return count;
        }
    }
}
