using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Matrix.Tests
{
    [TestFixture]
    public class PathfinderTests
    {
        [Test]
        public void WhenFindPathFourWayOpenGrid_ThenOptimalLengthAndCost()
        {
            Matrix<char> grid = GridHelpers.FromAscii("...\n...\n...");
            IPathfinder pf = GridHelpers.FourWayPathfinder();
            PathResult result = pf.FindPath(
                grid,
                new MatrixIndex(0, 0),
                new MatrixIndex(2, 2),
                GridHelpers.WalkDots(),
                new ConstantStepCost<char>(1f));

            Assert.That(result.Success, Is.True);
            Assert.That(result.Path[0], Is.EqualTo(new MatrixIndex(0, 0)));
            Assert.That(result.Path[result.Path.Count - 1], Is.EqualTo(new MatrixIndex(2, 2)));
            Assert.That(result.Path.Count, Is.EqualTo(5));
            FloatAssert.AreClose(4f, result.TotalCost);
            AssertAdjacentOrthogonal(result.Path);
        }

        [Test]
        public void WhenFindPathEightWayOpenGrid_ThenOptimalDiagonalCost()
        {
            Matrix<char> grid = GridHelpers.FromAscii("...\n...\n...");
            IPathfinder pf = GridHelpers.EightWayPathfinder();
            PathResult result = pf.FindPath(
                grid,
                new MatrixIndex(0, 0),
                new MatrixIndex(2, 2),
                new NoCornerCuttingTraversalCondition<char>(GridHelpers.WalkDots()),
                new OctileStepCost<char>());

            Assert.That(result.Success, Is.True);
            Assert.That(result.Path.Count, Is.EqualTo(3));
            FloatAssert.AreClose(2f * (float)Math.Sqrt(2), result.TotalCost);
        }

        [Test]
        public void WhenFindPathAroundWalls_ThenDetoursAndAvoidsBlockedCells()
        {
            Matrix<char> grid = GridHelpers.FromAscii(".#.\n.#.\n...");
            IPathfinder pf = GridHelpers.FourWayPathfinder();
            PathResult result = pf.FindPath(
                grid,
                new MatrixIndex(0, 0),
                new MatrixIndex(2, 0),
                GridHelpers.WalkDots(),
                new ConstantStepCost<char>(1f));

            Assert.That(result.Success, Is.True);
            Assert.That(result.Path, Has.No.Member(new MatrixIndex(1, 0)));
            Assert.That(result.Path, Has.No.Member(new MatrixIndex(1, 1)));
            FloatAssert.AreClose(6f, result.TotalCost);
        }

        [Test]
        public void WhenFindPathUnreachableGoal_ThenReturnsFailure()
        {
            Matrix<char> grid = GridHelpers.FromAscii(".#.\n.#.\n.#.");
            IPathfinder pf = GridHelpers.FourWayPathfinder();
            PathResult result = pf.FindPath(
                grid,
                new MatrixIndex(0, 0),
                new MatrixIndex(2, 0),
                GridHelpers.WalkDots(),
                new ConstantStepCost<char>(1f));

            Assert.That(result.Success, Is.False);
            Assert.That(result.Path, Is.Empty);
            FloatAssert.AreClose(0f, result.TotalCost);
        }

        [Test]
        public void WhenFindPathBlockedStart_ThenReturnsFailure()
        {
            Matrix<char> grid = GridHelpers.FromAscii("#..\n...");
            IPathfinder pf = GridHelpers.FourWayPathfinder();
            PathResult result = pf.FindPath(
                grid,
                new MatrixIndex(0, 0),
                new MatrixIndex(2, 0),
                GridHelpers.WalkDots(),
                new ConstantStepCost<char>(1f));

            Assert.That(result.Success, Is.False);
        }

        [Test]
        public void WhenFindPathBlockedGoal_ThenReturnsFailure()
        {
            Matrix<char> grid = GridHelpers.FromAscii("..#");
            IPathfinder pf = GridHelpers.FourWayPathfinder();
            PathResult result = pf.FindPath(
                grid,
                new MatrixIndex(0, 0),
                new MatrixIndex(2, 0),
                GridHelpers.WalkDots(),
                new ConstantStepCost<char>(1f));

            Assert.That(result.Success, Is.False);
        }

        [Test]
        public void WhenFindPathStartEqualsGoal_ThenSuccessSingleCellCostZero()
        {
            Matrix<char> grid = GridHelpers.FromAscii("...");
            IPathfinder pf = GridHelpers.FourWayPathfinder();
            MatrixIndex start = new MatrixIndex(1, 0);
            PathResult result = pf.FindPath(
                grid,
                start,
                start,
                GridHelpers.WalkDots(),
                new ConstantStepCost<char>(1f));

            Assert.That(result.Success, Is.True);
            Assert.That(result.Path.Count, Is.EqualTo(1));
            Assert.That(result.Path[0], Is.EqualTo(start));
            FloatAssert.AreClose(0f, result.TotalCost);
        }

        [Test]
        public void WhenFindPathCornerCuttingForbiddenWithDecorator_ThenAvoidsWallAndCostsMoreThanTwo()
        {
            Matrix<char> grid = GridHelpers.FromAscii(".#.\n...");
            IPathfinder pf = GridHelpers.EightWayPathfinder();
            NoCornerCuttingTraversalCondition<char> traversal =
                new NoCornerCuttingTraversalCondition<char>(GridHelpers.WalkDots());
            PathResult result = pf.FindPath(
                grid,
                new MatrixIndex(0, 0),
                new MatrixIndex(2, 0),
                traversal,
                new OctileStepCost<char>());

            Assert.That(result.Success, Is.True);
            Assert.That(result.Path, Has.No.Member(new MatrixIndex(1, 0)));
            Assert.That(result.TotalCost, Is.GreaterThan(2f));
        }

        [Test]
        public void WhenFindPathCornerCuttingAllowedWithoutDecorator_ThenUsesDiagonal()
        {
            Matrix<char> grid = GridHelpers.FromAscii(".#.\n...");
            IPathfinder pf = GridHelpers.EightWayPathfinder();
            ITraversalCondition<char> allowCut = new PredicateTraversalCondition<char>(
                (m, c) => m.Get(c) == '.',
                (m, a, b) => true);
            PathResult result = pf.FindPath(
                grid,
                new MatrixIndex(0, 0),
                new MatrixIndex(2, 0),
                allowCut,
                new OctileStepCost<char>());

            Assert.That(result.Success, Is.True);
            FloatAssert.AreClose(2f * (float)Math.Sqrt(2), result.TotalCost);
            Assert.That(result.Path.Count, Is.EqualTo(3));
        }

        [Test]
        public void WhenFindPathInvalidStepCostZero_ThenThrowsArgumentOutOfRange()
        {
            Matrix<char> grid = GridHelpers.FromAscii("..\n..");
            IPathfinder pf = GridHelpers.FourWayPathfinder();
            RecordingStepCost<char> badCost =
                new RecordingStepCost<char>(new ConstantStepCost<char>(1f), overrideCost: 0f);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                pf.FindPath(
                    grid,
                    new MatrixIndex(0, 0),
                    new MatrixIndex(1, 0),
                    GridHelpers.WalkDots(),
                    badCost));
        }

        [Test]
        public void WhenFindPathInvalidStepCostNaN_ThenThrowsArgumentOutOfRange()
        {
            Matrix<char> grid = GridHelpers.FromAscii("..\n..");
            IPathfinder pf = GridHelpers.FourWayPathfinder();
            RecordingStepCost<char> badCost =
                new RecordingStepCost<char>(new ConstantStepCost<char>(1f), overrideCost: float.NaN);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                pf.FindPath(
                    grid,
                    new MatrixIndex(0, 0),
                    new MatrixIndex(1, 0),
                    GridHelpers.WalkDots(),
                    badCost));
        }

        [Test]
        public void WhenFindPathStartOutOfBounds_ThenThrowsMatrixOutOfBounds()
        {
            Matrix<char> grid = GridHelpers.FromAscii("..");
            IPathfinder pf = GridHelpers.FourWayPathfinder();
            Assert.Throws<MatrixOutOfBoundsException>(() =>
                pf.FindPath(
                    grid,
                    new MatrixIndex(-1, 0),
                    new MatrixIndex(0, 0),
                    GridHelpers.WalkDots(),
                    new ConstantStepCost<char>(1f)));
        }

        [Test]
        public void WhenFindPathGoalOutOfBounds_ThenThrowsMatrixOutOfBounds()
        {
            Matrix<char> grid = GridHelpers.FromAscii("..");
            IPathfinder pf = GridHelpers.FourWayPathfinder();
            Assert.Throws<MatrixOutOfBoundsException>(() =>
                pf.FindPath(
                    grid,
                    new MatrixIndex(0, 0),
                    new MatrixIndex(0, 5),
                    GridHelpers.WalkDots(),
                    new ConstantStepCost<char>(1f)));
        }

        [Test]
        public void WhenFindPathNullMatrix_ThenThrowsArgumentNull()
        {
            IPathfinder pf = GridHelpers.FourWayPathfinder();
            Assert.Throws<ArgumentNullException>(() =>
                pf.FindPath<char>(
                    null,
                    new MatrixIndex(0, 0),
                    new MatrixIndex(0, 0),
                    GridHelpers.WalkDots(),
                    new ConstantStepCost<char>(1f)));
        }

        [Test]
        public void WhenFindPathNullTraversal_ThenThrowsArgumentNull()
        {
            Matrix<char> grid = GridHelpers.FromAscii(".");
            IPathfinder pf = GridHelpers.FourWayPathfinder();
            Assert.Throws<ArgumentNullException>(() =>
                pf.FindPath(
                    grid,
                    new MatrixIndex(0, 0),
                    new MatrixIndex(0, 0),
                    null,
                    new ConstantStepCost<char>(1f)));
        }

        [Test]
        public void WhenFindPathNullStepCost_ThenThrowsArgumentNull()
        {
            Matrix<char> grid = GridHelpers.FromAscii(".");
            IPathfinder pf = GridHelpers.FourWayPathfinder();
            Assert.Throws<ArgumentNullException>(() =>
                pf.FindPath(
                    grid,
                    new MatrixIndex(0, 0),
                    new MatrixIndex(0, 0),
                    GridHelpers.WalkDots(),
                    null));
        }

        [Test]
        public void WhenFindPathExpands_ThenIsPassableBeforeCanTraverseAndGetCostOnlyOnAllowedEdges()
        {
            Matrix<char> grid = GridHelpers.FromAscii("..\n.#");
            RecordingTraversalCondition<char> recordingTraversal =
                new RecordingTraversalCondition<char>(GridHelpers.WalkDots());
            RecordingStepCost<char> recordingCost =
                new RecordingStepCost<char>(new ConstantStepCost<char>(1f));
            IPathfinder pf = GridHelpers.FourWayPathfinder();

            PathResult result = pf.FindPath(
                grid,
                new MatrixIndex(0, 0),
                new MatrixIndex(1, 0),
                recordingTraversal,
                recordingCost);

            Assert.That(result.Success, Is.True);
            Assert.That(recordingTraversal.Calls.Any(c => c.StartsWith("IsPassable:")), Is.True);

            HashSet<string> seenPassable = new HashSet<string>();
            foreach (string call in recordingTraversal.Calls)
            {
                if (call.StartsWith("IsPassable:"))
                {
                    seenPassable.Add(call.Substring("IsPassable:".Length));
                }
                else if (call.StartsWith("CanTraverse:"))
                {
                    string toPart = call.Split(new[] { "->" }, StringSplitOptions.None)[1];
                    Assert.That(
                        seenPassable.Contains(toPart),
                        $"CanTraverse for {toPart} without prior IsPassable(to). Calls: {string.Join(", ", recordingTraversal.Calls)}");
                }
            }

            foreach ((MatrixIndex from, MatrixIndex to) in recordingCost.Calls)
            {
                Assert.That(grid.Get(to), Is.EqualTo('.'));
                Assert.That(
                    recordingTraversal.Calls.Contains($"CanTraverse:{from}->{to}"),
                    Is.True);
            }

            Assert.That(recordingCost.Calls.Any(c => c.To.Equals(new MatrixIndex(1, 1))), Is.False);
        }

        [Test]
        public void WhenAStarPathfinderNullNeighbourhood_ThenThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AStarPathfinder(null, new ManhattanHeuristic()));
        }

        [Test]
        public void WhenAStarPathfinderNullHeuristic_ThenThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AStarPathfinder(new OrthogonalNeighbourhood(), null));
        }

        private static void AssertAdjacentOrthogonal(IReadOnlyList<MatrixIndex> path)
        {
            for (int i = 1; i < path.Count; i++)
            {
                int dx = Math.Abs(path[i].X - path[i - 1].X);
                int dy = Math.Abs(path[i].Y - path[i - 1].Y);
                Assert.That(dx + dy, Is.EqualTo(1), $"non-orthogonal step at {i}: {path[i - 1]} -> {path[i]}");
            }
        }
    }
}
