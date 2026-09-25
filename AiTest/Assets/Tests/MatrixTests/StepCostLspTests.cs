using System;
using NUnit.Framework;

namespace Matrix.Tests
{
    /// <summary>LSP suite: IStepCost — GetCost returns finite &gt; 0 for adjacent steps.</summary>
    [TestFixture]
    public class StepCostLspTests
    {
        private static readonly IStepCost<int>[] Implementations =
        {
            new ConstantStepCost<int>(1f),
            new OctileStepCost<int>()
        };

        [TestCaseSource(nameof(Implementations))]
        public void WhenGetCostOrthogonalStep_ThenPositiveFinite(IStepCost<int> cost)
        {
            Matrix<int> m = new Matrix<int>(3, 3, 0);
            float c = cost.GetCost(m, new MatrixIndex(1, 1), new MatrixIndex(2, 1));
            Assert.That(float.IsNaN(c) || float.IsInfinity(c), Is.False);
            Assert.That(c, Is.GreaterThan(0f));
        }

        [Test]
        public void WhenConstantStepCostQueried_ThenAlwaysReturnsConfiguredCost()
        {
            ConstantStepCost<int> cost = new ConstantStepCost<int>(2.5f);
            Matrix<int> m = new Matrix<int>(2, 2, 0);
            FloatAssert.AreClose(2.5f, cost.GetCost(m, new MatrixIndex(0, 0), new MatrixIndex(1, 0)));
        }

        [Test]
        public void WhenConstantStepCostNonPositive_ThenThrowsOnConstruction()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ConstantStepCost<int>(0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ConstantStepCost<int>(-1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ConstantStepCost<int>(float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ConstantStepCost<int>(float.PositiveInfinity));
        }

        [Test]
        public void WhenOctileStepCostOrthogonal_ThenReturnsOne()
        {
            OctileStepCost<int> cost = new OctileStepCost<int>();
            Matrix<int> m = new Matrix<int>(3, 3, 0);
            FloatAssert.AreClose(1f, cost.GetCost(m, new MatrixIndex(0, 0), new MatrixIndex(1, 0)));
        }

        [Test]
        public void WhenOctileStepCostDiagonal_ThenReturnsSqrt2()
        {
            OctileStepCost<int> cost = new OctileStepCost<int>();
            Matrix<int> m = new Matrix<int>(3, 3, 0);
            FloatAssert.AreClose(
                (float)Math.Sqrt(2),
                cost.GetCost(m, new MatrixIndex(0, 0), new MatrixIndex(1, 1)));
        }
    }
}
