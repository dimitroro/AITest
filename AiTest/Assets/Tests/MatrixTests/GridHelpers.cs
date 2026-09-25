using System.Collections.Generic;
using NUnit.Framework;

namespace Matrix.Tests
{
    internal static class GridHelpers
    {
        /// <summary>Builds a matrix from a string grid: '.' passable, '#' wall. Rows separated by newlines.</summary>
        public static Matrix<char> FromAscii(string ascii)
        {
            string[] rows = ascii.Replace("\r\n", "\n").Split('\n');
            int height = rows.Length;
            int width = rows[0].Length;
            char[] cells = new char[width * height];
            for (int y = 0; y < height; y++)
            {
                Assert.That(rows[y].Length, Is.EqualTo(width), "ragged ascii grid");
                for (int x = 0; x < width; x++)
                    cells[y * width + x] = rows[y][x];
            }

            return new Matrix<char>(width, height, cells);
        }

        public static ITraversalCondition<char> WalkDots(bool allowAllEdges = true)
        {
            return new PredicateTraversalCondition<char>(
                (m, c) => m.Get(c) == '.',
                (m, from, to) => allowAllEdges);
        }

        public static IPathfinder FourWayPathfinder()
        {
            return new AStarPathfinder(new OrthogonalNeighbourhood(), new ManhattanHeuristic());
        }

        public static IPathfinder EightWayPathfinder()
        {
            return new AStarPathfinder(new EightWayNeighbourhood(), new OctileHeuristic());
        }
    }
}
