using System;
using NUnit.Framework;

namespace Matrix.Tests
{
    /// <summary>LSP suite: IPathHeuristic — admissible, >= 0, finite.</summary>
    [TestFixture(typeof(ManhattanHeuristic))]
    [TestFixture(typeof(OctileHeuristic))]
    public class PathHeuristicLspTests
    {
        private readonly Type _implType;
        private IPathHeuristic _heuristic;

        public PathHeuristicLspTests(Type implType)
        {
            _implType = implType;
        }

        [SetUp]
        public void SetUp()
        {
            _heuristic = (IPathHeuristic)Activator.CreateInstance(_implType);
        }

        [Test]
        public void WhenEstimateSameCell_ThenReturnsZero()
        {
            FloatAssert.AreClose(0f, _heuristic.Estimate(new MatrixIndex(3, 4), new MatrixIndex(3, 4)));
        }

        [Test]
        public void WhenEstimateAnyPair_ThenNonNegativeFinite()
        {
            float e = _heuristic.Estimate(new MatrixIndex(0, 0), new MatrixIndex(5, 3));
            Assert.That(float.IsNaN(e), Is.False);
            Assert.That(float.IsInfinity(e), Is.False);
            Assert.That(e, Is.GreaterThanOrEqualTo(0f));
        }

        [Test]
        public void WhenEstimateSwappedEndpoints_ThenSymmetric()
        {
            MatrixIndex a = new MatrixIndex(1, 2);
            MatrixIndex b = new MatrixIndex(4, 7);
            FloatAssert.AreClose(_heuristic.Estimate(a, b), _heuristic.Estimate(b, a));
        }

        [Test]
        public void WhenEstimateAgainstMatchingCostModel_ThenEqualsAdmissibleFormula()
        {
            MatrixIndex from = new MatrixIndex(0, 0);
            MatrixIndex to = new MatrixIndex(3, 5);
            float estimate = _heuristic.Estimate(from, to);
            float upper;
            if (_heuristic is ManhattanHeuristic)
            {
                upper = Math.Abs(to.X - from.X) + Math.Abs(to.Y - from.Y);
            }
            else
            {
                int dx = Math.Abs(to.X - from.X);
                int dy = Math.Abs(to.Y - from.Y);
                int max = Math.Max(dx, dy);
                int min = Math.Min(dx, dy);
                upper = max + (float)(Math.Sqrt(2) - 1) * min;
            }

            Assert.That(estimate, Is.LessThanOrEqualTo(upper + FloatAssert.TOLERANCE));
            FloatAssert.AreClose(upper, estimate);
        }
    }
}
