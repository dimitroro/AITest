using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Matrix.Tests
{
    /// <summary>LSP suite: same contract checks against every INeighbourhood implementation.</summary>
    [TestFixture(typeof(OrthogonalNeighbourhood))]
    [TestFixture(typeof(EightWayNeighbourhood))]
    public class NeighbourhoodLspTests
    {
        private readonly Type _implType;
        private INeighbourhood _neighbourhood;

        public NeighbourhoodLspTests(Type implType)
        {
            _implType = implType;
        }

        [SetUp]
        public void SetUp()
        {
            _neighbourhood = (INeighbourhood)Activator.CreateInstance(_implType);
        }

        [Test]
        public void WhenGetNeighboursWithNonPositiveWidth_ThenThrowsArgumentOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _neighbourhood.GetNeighbours(new MatrixIndex(0, 0), 0, 3));
        }

        [Test]
        public void WhenGetNeighboursWithNonPositiveHeight_ThenThrowsArgumentOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _neighbourhood.GetNeighbours(new MatrixIndex(0, 0), 3, -1));
        }

        [Test]
        public void WhenGetNeighbours_ThenNeverNullAndAllInBounds()
        {
            IReadOnlyList<MatrixIndex> n = _neighbourhood.GetNeighbours(new MatrixIndex(1, 1), 3, 3);
            Assert.That(n, Is.Not.Null);
            foreach (MatrixIndex i in n)
            {
                Assert.That(i.X, Is.GreaterThanOrEqualTo(0).And.LessThan(3));
                Assert.That(i.Y, Is.GreaterThanOrEqualTo(0).And.LessThan(3));
                Assert.That(i, Is.Not.EqualTo(new MatrixIndex(1, 1)));
            }
        }

        [Test]
        public void WhenGetNeighboursAtCorner_ThenOnlyInBoundsNeighbours()
        {
            IReadOnlyList<MatrixIndex> n = _neighbourhood.GetNeighbours(new MatrixIndex(0, 0), 3, 3);
            foreach (MatrixIndex i in n)
            {
                Assert.That(i.X, Is.GreaterThanOrEqualTo(0).And.LessThan(3));
                Assert.That(i.Y, Is.GreaterThanOrEqualTo(0).And.LessThan(3));
            }

            Assert.That(n, Has.No.Member(new MatrixIndex(-1, 0)));
        }

        [Test]
        public void WhenGetNeighboursCalledTwice_ThenOrderIsStable()
        {
            IReadOnlyList<MatrixIndex> a = _neighbourhood.GetNeighbours(new MatrixIndex(2, 2), 5, 5);
            IReadOnlyList<MatrixIndex> b = _neighbourhood.GetNeighbours(new MatrixIndex(2, 2), 5, 5);
            Assert.That(a, Is.EqualTo(b));
        }
    }
}
