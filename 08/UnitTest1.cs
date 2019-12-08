using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace _08
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _output;

        public UnitTest1(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Part1()
        {
            const int width = 25;
            const int height = 6;

            var lines = File.ReadAllLines("input.txt");

            var all = string.Join("", lines);
            const int size = width * height;

            var layerResult = 0;
            var zeroCount = int.MaxValue;

            for (var i = 0; i < all.Length / size; ++i)
            {
                var layer = all[(size * i)..(size * (i + 1))];

                var zeroes = layer.Count(c => c == '0');

                if (zeroes < zeroCount)
                {
                    zeroCount = zeroes;
                    layerResult = layer.Count(c => c == '1') * layer.Count(c => c == '2');
                }
            }

            Assert.Equal(1320, layerResult);
        }

        [Fact]
        public void Part2()
        {
            const int width = 25;
            const int height = 6;

            const int size = width * height;
            var image = new int[size];

            for (var i = 0; i < image.Length; i++)
            {
                image[i] = 2;
            }

            var lines = File.ReadAllLines("input.txt");

            var all = string.Join("", lines);

            for (var l = (all.Length / size) - 1; l >=0 ; --l)
            {
                var layer = all[(size * l)..(size * (l + 1))];

                for (var i = 0; i < image.Length; i++)
                {
                    if (layer[i] != '2')
                    {
                        image[i] = layer[i] - 48;
                    }
                }
            }

            var sb = new StringBuilder();

            for (var i = 0; i < image.Length; i++)
            {
                if (i > 0 && i % width == 0)
                {
                    sb.Append('\n');
                }

                switch (image[i])
                {
                    case 0:
                        sb.Append('█');
                        break;
                    case 1:
                        sb.Append('░');
                        break;
                    case 2:
                        sb.Append(' ');
                        break;
                }
            }

            _output.WriteLine(sb.ToString());
        }
    }
}
