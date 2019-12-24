using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace _24
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var eris = File.ReadAllLines("input.txt");

            var rows = eris.Length;
            var cols = eris[0].Length;

            var erisArray = new bool[eris.Length, eris[0].Length];
            var erisCounts = new int[eris.Length, eris[0].Length];

            for (var row = 0; row < eris.Length; row++)
            {
                var line = eris[row];
                for (var col = 0; col < line.Length; col++)
                {
                    erisArray[row, col] = line[col] == '#';
                    erisCounts[row, col] = CountPosition(eris, row, col);
                }
            }

            var states = new HashSet<long>();
            var list = new List<(int, int, int)>();
            long stateKey = 0;

            while (true)
            {
                foreach (var (r, c, change) in list)
                {
                    erisCounts[r, c] += change;
                }

                list.Clear();

                stateKey = Enumerable.Range(0, rows * cols)
                    .Sum(_ => (erisArray[_ / rows, _ % cols] ? 1 : 0) * (long)Math.Pow(2, _));

                if (states.Contains(stateKey))
                {
                    break;
                }

                states.Add(stateKey);

                for (var row = 0; row < rows; row++)
                {
                    for (var col = 0; col < cols; col++)
                    {
                        var isInfested = erisArray[row, col];
                        var bugsAround = erisCounts[row, col];

                        if (isInfested && bugsAround != 1)
                        {
                            // dies
                            // decrease around
                            erisArray[row, col] = false;
                            foreach (var (r, c) in Around(row, col, rows, cols))
                            {
                                list.Add((r, c, -1));
                            }
                        }
                        else if (!isInfested && (bugsAround == 1 || bugsAround == 2))
                        {
                            // spawns
                            // increase around
                            erisArray[row, col] = true;
                            foreach (var (r, c) in Around(row, col, rows, cols))
                            {
                                list.Add((r, c, +1));
                            }
                        }
                    }
                }
            }

            Assert.Equal(32506911, stateKey);
        }

        [Fact]
        public void Test2()
        {
            var eris = File.ReadAllLines("input.txt");

            var rows = eris.Length;
            var cols = eris[0].Length;

            var erisDictionary = new Dictionary<(int, int, int), bool>();
            var erisCounts = new Dictionary<(int, int, int), int>();

            for (var row = 0; row < eris.Length; row++)
            {
                var line = eris[row];
                for (var col = 0; col < line.Length; col++)
                {
                    if (row == 2 && col == 2)
                    {
                        continue;
                    }

                    erisDictionary[(row, col, 0)] = line[col] == '#';
                }
            }

            for (var level = -1; level <= 1; ++level)
            {
                for (var row = 0; row < rows; row++)
                {
                    for (var col = 0; col < cols; col++)
                    {
                        if (row == 2 && col == 2)
                        {
                            continue;
                        }

                        erisCounts[(row, col, level)] = CountPosition2(erisDictionary, row, col, level, rows, cols);
                    }
                }
            }

            var list = new List<(int, int, int, int)>();

            for (var i = 0; i < 200; ++i)
            {
                foreach (var (r, c, l, change) in list)
                {
                    if (erisCounts.TryGetValue((r, c, l), out var current))
                    {
                        erisCounts[(r, c, l)] = current + change;
                    }
                    else
                    {
                        erisCounts[(r, c, l)] = change;
                    }
                }

                list.Clear();

                var availableLevels = erisCounts.Keys.Select(_ => _.Item3).Distinct().ToList();

                var min = availableLevels.Min();
                var max = availableLevels.Max();

                for (var level = min; level <= max; ++level)
                {
                    for (var row = 0; row < rows; row++)
                    {
                        for (var col = 0; col < cols; col++)
                        {
                            if (col == 2 && row == 2)
                            {
                                // another level
                                continue;
                            }

                            erisDictionary.TryGetValue((row, col, level), out var isInfested);
                            erisCounts.TryGetValue((row, col, level), out var bugsAround);

                            if (isInfested && bugsAround != 1)
                            {
                                // dies
                                // decrease around
                                erisDictionary[(row, col, level)] = false;
                                foreach (var (r, c, l) in Around2(row, col, level, rows, cols))
                                {
                                    list.Add((r, c, l, -1));
                                }
                            }
                            else if (!isInfested && (bugsAround == 1 || bugsAround == 2))
                            {
                                // spawns
                                // increase around
                                erisDictionary[(row, col, level)] = true;
                                foreach (var (r, c, l) in Around2(row, col, level, rows, cols))
                                {
                                    list.Add((r, c, l, + 1));
                                }
                            }
                        }
                    }
                }
            }

            Assert.Equal(2025, erisDictionary.Values.Count(_ => _));
        }

        private IEnumerable<(int, int)> Around(int row, int col, int rows, int cols)
        {
            if (row - 1 >= 0)
            {
                yield return (row - 1, col);
            }

            if (col - 1 >= 0)
            {
                yield return (row, col - 1);
            }

            if (row + 1 < rows)
            {
                yield return (row + 1, col);
            }

            if (col + 1 < cols)
            {
                yield return (row, col + 1);
            }
        }

        private IEnumerable<(int, int, int)> Around2(int row, int col, int level, int rows, int cols)
        {
            if (row - 1 < 0)
            {
                yield return (1, 2, level - 1);
            }

            if (row - 1 >= 0)
            {
                if (row == 3 && col == 2)
                {
                    // moving into the center tile
                    for (var x = 0; x < 5; ++x)
                    {
                        yield return (rows - 1, x, level + 1);
                    }
                }
                else
                {
                    yield return (row - 1, col, level);
                }
            }

            if (row + 1 >= rows)
            {
                yield return (3, 2, level - 1);
            }

            if (row + 1 < rows)
            {
                if (row == 1 && col == 2)
                {
                    // moving into the center tile
                    for (var x = 0; x < 5; ++x)
                    {
                        yield return (0, x, level + 1);
                    }
                }
                else
                {
                    yield return (row + 1, col, level);
                }
            }

            if (col - 1 < 0)
            {
                yield return (2, 1, level - 1);
            }

            if (col - 1 >= 0)
            {
                if (col == 3 && row == 2)
                {
                    // moving into the center tile
                    for (var x = 0; x < 5; ++x)
                    {
                        yield return (x, cols - 1, level + 1);
                    }
                }
                else
                {
                    yield return (row, col - 1, level);
                }
            }

            if (col + 1 >= cols)
            {
                yield return (2, 3, level - 1);
            }

            if (col + 1 < cols)
            {
                if (row == 2 && col == 1)
                {
                    // moving into the center tile
                    for (var x = 0; x < 5; ++x)
                    {
                        yield return (x, 0, level + 1);
                    }
                }
                else
                {
                    yield return (row, col + 1, level);
                }
            }
        }

        private int CountPosition(string[] eris, int row, int col)
        {
            var count = 0;

            count += row - 1 >= 0 && eris[row - 1][col] == '#' ? 1 : 0;
            count += row + 1 < eris.Length && eris[row + 1][col] == '#' ? 1 : 0;

            count += col - 1 >= 0 && eris[row][col - 1] == '#' ? 1 : 0;
            count += col + 1 < eris[0].Length && eris[row][col + 1] == '#' ? 1 : 0;

            return count;
        }

        private int CountPosition2(Dictionary<(int, int, int), bool> eris, int row, int col, int level, int rows, int cols)
        {
            var count = 0;

            foreach (var (r, c, l) in Around2(row, col, level, rows, cols))
            {
                if (eris.TryGetValue((r, c, l), out var isInfested) && isInfested)
                {
                    ++count;
                }
            }

            return count;
        }
    }
}
