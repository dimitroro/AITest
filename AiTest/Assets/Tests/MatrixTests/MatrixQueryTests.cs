using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Matrix.Tests
{
    [TestFixture]
    public class MatrixQueryTests
    {
        [Test]
        public void WhenFindValuePresent_ThenReturnsFirstRowMajorMatch()
        {
            Matrix<int> m = new Matrix<int>(2, 2, new[] { 0, 5, 5, 0 });
            bool found = m.Find(5, out MatrixIndex index);
            Assert.That(found, Is.True);
            Assert.That(index, Is.EqualTo(new MatrixIndex(1, 0)));
        }

        [Test]
        public void WhenFindValueAbsent_ThenReturnsFalse()
        {
            Matrix<int> m = new Matrix<int>(2, 2, 0);
            Assert.That(m.Find(1, out _), Is.False);
        }

        [Test]
        public void WhenFindAllByValue_ThenRowMajorOrEmptyWhenNone()
        {
            Matrix<int> m = new Matrix<int>(2, 2, new[] { 1, 2, 1, 3 });
            Assert.That(m.FindAll(1), Is.EqualTo(new[] { new MatrixIndex(0, 0), new MatrixIndex(0, 1) }));
            Assert.That(m.FindAll(9), Is.Empty);
        }

        [Test]
        public void WhenFindAllByPredicate_ThenReturnsRowMajorMatches()
        {
            Matrix<int> m = new Matrix<int>(2, 2, new[] { 1, 2, 3, 4 });
            IReadOnlyList<MatrixIndex> odds = m.FindAll(v => v % 2 == 1);
            Assert.That(odds, Is.EqualTo(new[] { new MatrixIndex(0, 0), new MatrixIndex(0, 1) }));
        }

        [Test]
        public void WhenFindAllWithNullPredicate_ThenThrowsArgumentNull()
        {
            Matrix<int> m = new Matrix<int>(1, 1, 0);
            Assert.Throws<ArgumentNullException>(() => m.FindAll((Func<int, bool>)null));
        }

        [Test]
        public void WhenEnumerate_ThenYieldsAllCellsInRowMajorOrder()
        {
            Matrix<int> m = new Matrix<int>(2, 2, new[] { 1, 2, 3, 4 });
            List<MatrixCell<int>> cells = m.Enumerate().ToList();
            Assert.That(cells.Count, Is.EqualTo(4));
            Assert.That(cells[0].Index, Is.EqualTo(new MatrixIndex(0, 0)));
            Assert.That(cells[0].Value, Is.EqualTo(1));
            Assert.That(cells[1].Index, Is.EqualTo(new MatrixIndex(1, 0)));
            Assert.That(cells[2].Index, Is.EqualTo(new MatrixIndex(0, 1)));
            Assert.That(cells[3].Index, Is.EqualTo(new MatrixIndex(1, 1)));
            Assert.That(cells[3].Value, Is.EqualTo(4));
        }

        [Test]
        public void WhenCountMatchingPredicate_ThenReturnsMatchCount()
        {
            Matrix<int> m = new Matrix<int>(2, 2, new[] { 1, 2, 1, 1 });
            Assert.That(m.Count(v => v == 1), Is.EqualTo(3));
        }

        [Test]
        public void WhenCountWithNullPredicate_ThenThrowsArgumentNull()
        {
            Matrix<int> m = new Matrix<int>(1, 1, 0);
            Assert.Throws<ArgumentNullException>(() => m.Count(null));
        }

        [Test]
        public void WhenAnyMatchExists_ThenReturnsTrue()
        {
            Matrix<int> m = new Matrix<int>(2, 2, new[] { 0, 0, 7, 0 });
            Assert.That(m.Any(v => v == 7), Is.True);
        }

        [Test]
        public void WhenAnyNoMatch_ThenReturnsFalse()
        {
            Matrix<int> m = new Matrix<int>(2, 2, 0);
            Assert.That(m.Any(v => v == 1), Is.False);
        }

        [Test]
        public void WhenAnyWithNullPredicate_ThenThrowsArgumentNull()
        {
            Matrix<int> m = new Matrix<int>(1, 1, 0);
            Assert.Throws<ArgumentNullException>(() => m.Any(null));
        }

        [Test]
        public void WhenAllCellsMatch_ThenReturnsTrue()
        {
            Matrix<int> m = new Matrix<int>(2, 2, 5);
            Assert.That(m.All(v => v == 5), Is.True);
        }

        [Test]
        public void WhenAllHasMismatch_ThenReturnsFalse()
        {
            Matrix<int> m = new Matrix<int>(2, 2, new[] { 5, 5, 5, 0 });
            Assert.That(m.All(v => v == 5), Is.False);
        }

        [Test]
        public void WhenAllWithNullPredicate_ThenThrowsArgumentNull()
        {
            Matrix<int> m = new Matrix<int>(1, 1, 0);
            Assert.Throws<ArgumentNullException>(() => m.All(null));
        }

        [Test]
        public void WhenRegionContainsChecked_ThenInclusiveStartExclusiveEnd()
        {
            MatrixRegion region = new MatrixRegion(1, 1, 2, 2);
            Assert.That(region.Contains(new MatrixIndex(1, 1)), Is.True);
            Assert.That(region.Contains(new MatrixIndex(2, 2)), Is.True);
            Assert.That(region.Contains(new MatrixIndex(3, 1)), Is.False);
            Assert.That(region.Contains(new MatrixIndex(0, 1)), Is.False);
        }
    }
}
