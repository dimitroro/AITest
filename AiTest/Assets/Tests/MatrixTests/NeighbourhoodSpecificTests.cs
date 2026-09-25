using System.Collections.Generic;
using NUnit.Framework;

namespace Matrix.Tests
{
    [TestFixture]
    public class NeighbourhoodSpecificTests
    {
        [Test]
        public void WhenOrthogonalAtCenter_ThenReturnsFourNeighboursNesw()
        {
            OrthogonalNeighbourhood n = new OrthogonalNeighbourhood();
            Assert.That(
                n.GetNeighbours(new MatrixIndex(1, 1), 3, 3),
                Is.EqualTo(new[]
                {
                    new MatrixIndex(1, 0),
                    new MatrixIndex(2, 1),
                    new MatrixIndex(1, 2),
                    new MatrixIndex(0, 1)
                }));
        }

        [Test]
        public void WhenEightWayAtCenter_ThenReturnsEightNeighboursIncludingDiagonals()
        {
            EightWayNeighbourhood n = new EightWayNeighbourhood();
            IReadOnlyList<MatrixIndex> list = n.GetNeighbours(new MatrixIndex(1, 1), 3, 3);
            Assert.That(list.Count, Is.EqualTo(8));
            Assert.That(list, Has.Member(new MatrixIndex(2, 0)));
            Assert.That(list, Has.Member(new MatrixIndex(0, 2)));
        }

        [Test]
        public void WhenEightWayQueried_ThenDoesNotFilterByWalkability()
        {
            EightWayNeighbourhood n = new EightWayNeighbourhood();
            Assert.That(n.GetNeighbours(new MatrixIndex(1, 1), 3, 3).Count, Is.EqualTo(8));
        }
    }
}
