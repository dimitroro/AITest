using System;
using NUnit.Framework;

namespace Matrix.Tests
{
    [TestFixture]
    public class PathHeuristicSpecificTests
    {
        [Test]
        public void WhenManhattanHorizontalMove_ThenEqualsDx()
        {
            FloatAssert.AreClose(4f, new ManhattanHeuristic().Estimate(new MatrixIndex(0, 0), new MatrixIndex(4, 0)));
        }

        [Test]
        public void WhenOctileDiagonalOneStep_ThenEqualsSqrt2()
        {
            FloatAssert.AreClose(
                (float)Math.Sqrt(2),
                new OctileHeuristic().Estimate(new MatrixIndex(0, 0), new MatrixIndex(1, 1)));
        }
    }
}
