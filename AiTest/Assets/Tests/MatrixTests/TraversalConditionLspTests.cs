using System;
using NUnit.Framework;

namespace Matrix.Tests
{
    /// <summary>LSP suite: ITraversalCondition — Predicate and NoCornerCutting decorator.</summary>
    [TestFixture]
    public class TraversalConditionLspTests
    {
        private static ITraversalCondition<char> PredicateAlwaysPass()
        {
            return new PredicateTraversalCondition<char>((m, c) => true, (m, a, b) => true);
        }

        private static ITraversalCondition<char>[] Implementations()
        {
            ITraversalCondition<char> pred = PredicateAlwaysPass();
            return new ITraversalCondition<char>[]
            {
                pred,
                new NoCornerCuttingTraversalCondition<char>(PredicateAlwaysPass())
            };
        }

        [TestCaseSource(nameof(Implementations))]
        public void WhenIsPassableOnOpenCell_ThenReturnsTrue(ITraversalCondition<char> traversal)
        {
            Matrix<char> m = GridHelpers.FromAscii(".\n.");
            Assert.That(traversal.IsPassable(m, new MatrixIndex(0, 0)), Is.True);
        }

        [Test]
        public void WhenPredicateNullIsPassable_ThenThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new PredicateTraversalCondition<int>(null, (m, a, b) => true));
        }

        [Test]
        public void WhenPredicateNullCanTraverse_ThenThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new PredicateTraversalCondition<int>((m, c) => true, null));
        }

        [Test]
        public void WhenPredicateInvoked_ThenDelegatesToCellAndEdgeFuncs()
        {
            int cellCalls = 0;
            int edgeCalls = 0;
            PredicateTraversalCondition<int> t = new PredicateTraversalCondition<int>(
                (m, c) => { cellCalls++; return true; },
                (m, a, b) => { edgeCalls++; return false; });
            Matrix<int> matrix = new Matrix<int>(2, 2, 0);
            Assert.That(t.IsPassable(matrix, new MatrixIndex(0, 0)), Is.True);
            Assert.That(t.CanTraverse(matrix, new MatrixIndex(0, 0), new MatrixIndex(1, 0)), Is.False);
            Assert.That(cellCalls, Is.EqualTo(1));
            Assert.That(edgeCalls, Is.EqualTo(1));
        }

        [Test]
        public void WhenNoCornerCuttingNullInner_ThenThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => new NoCornerCuttingTraversalCondition<int>(null));
        }

        [Test]
        public void WhenNoCornerCuttingIsPassable_ThenDelegatesToInner()
        {
            PredicateTraversalCondition<char> inner = new PredicateTraversalCondition<char>(
                (m, c) => m.Get(c) == '.',
                (m, a, b) => true);
            NoCornerCuttingTraversalCondition<char> decorated =
                new NoCornerCuttingTraversalCondition<char>(inner);
            Matrix<char> grid = GridHelpers.FromAscii(".#");
            Assert.That(decorated.IsPassable(grid, new MatrixIndex(0, 0)), Is.True);
            Assert.That(decorated.IsPassable(grid, new MatrixIndex(1, 0)), Is.False);
        }

        [Test]
        public void WhenNoCornerCuttingOrthogonalStep_ThenDelegatesToInnerCanTraverse()
        {
            PredicateTraversalCondition<char> inner = new PredicateTraversalCondition<char>(
                (m, c) => true,
                (m, a, b) => false);
            NoCornerCuttingTraversalCondition<char> decorated =
                new NoCornerCuttingTraversalCondition<char>(inner);
            Matrix<char> grid = GridHelpers.FromAscii("..\n..");
            Assert.That(
                decorated.CanTraverse(grid, new MatrixIndex(0, 0), new MatrixIndex(1, 0)),
                Is.False);
        }

        [Test]
        public void WhenNoCornerCuttingDiagonalWithBlockedOrthogonalShare_ThenReturnsFalse()
        {
            Matrix<char> grid = GridHelpers.FromAscii(".#\n#.");
            ITraversalCondition<char> inner = GridHelpers.WalkDots();
            NoCornerCuttingTraversalCondition<char> decorated =
                new NoCornerCuttingTraversalCondition<char>(inner);
            Assert.That(
                decorated.CanTraverse(grid, new MatrixIndex(0, 0), new MatrixIndex(1, 1)),
                Is.False);
        }

        [Test]
        public void WhenNoCornerCuttingDiagonalWithOpenShares_ThenAllowsWhenInnerAllows()
        {
            Matrix<char> grid = GridHelpers.FromAscii("..\n..");
            NoCornerCuttingTraversalCondition<char> decorated =
                new NoCornerCuttingTraversalCondition<char>(GridHelpers.WalkDots());
            Assert.That(
                decorated.CanTraverse(grid, new MatrixIndex(0, 0), new MatrixIndex(1, 1)),
                Is.True);
        }

        [Test]
        public void WhenWithoutDecoratorDiagonalThroughBlockedCorner_ThenCanTraverseStillTrue()
        {
            Matrix<char> grid = GridHelpers.FromAscii(".#\n#.");
            ITraversalCondition<char> openEdges = GridHelpers.WalkDots();
            Assert.That(
                openEdges.CanTraverse(grid, new MatrixIndex(0, 0), new MatrixIndex(1, 1)),
                Is.True);
        }
    }
}
