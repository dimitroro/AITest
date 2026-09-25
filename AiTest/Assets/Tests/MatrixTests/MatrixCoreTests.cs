using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Matrix.Tests
{
    [TestFixture]
    public class MatrixCoreTests
    {
        [Test]
        public void WhenConstructedWithFillValue_ThenAllCellsEqualFill()
        {
            Matrix<int> m = new Matrix<int>(3, 2, 7);
            Assert.That(m.Width, Is.EqualTo(3));
            Assert.That(m.Height, Is.EqualTo(2));
            Assert.That(m.Get(new MatrixIndex(0, 0)), Is.EqualTo(7));
            Assert.That(m.Get(new MatrixIndex(2, 1)), Is.EqualTo(7));
        }

        [Test]
        public void WhenConstructedFromRowMajor_ThenCellsMatchArray()
        {
            Matrix<int> m = new Matrix<int>(2, 2, new[] { 1, 2, 3, 4 });
            Assert.That(m.Get(new MatrixIndex(0, 0)), Is.EqualTo(1));
            Assert.That(m.Get(new MatrixIndex(1, 0)), Is.EqualTo(2));
            Assert.That(m.Get(new MatrixIndex(0, 1)), Is.EqualTo(3));
            Assert.That(m.Get(new MatrixIndex(1, 1)), Is.EqualTo(4));
        }

        [Test]
        public void WhenConstructedWithZeroWidth_ThenThrowsArgumentOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Matrix<int>(0, 1, 0));
        }

        [Test]
        public void WhenCheckingInBounds_ThenInsideTrueAndOutsideFalse()
        {
            Matrix<int> m = new Matrix<int>(2, 3, 0);
            Assert.That(m.InBounds(new MatrixIndex(0, 0)), Is.True);
            Assert.That(m.InBounds(new MatrixIndex(1, 2)), Is.True);
            Assert.That(m.InBounds(new MatrixIndex(-1, 0)), Is.False);
            Assert.That(m.InBounds(new MatrixIndex(2, 0)), Is.False);
            Assert.That(m.InBounds(new MatrixIndex(0, 3)), Is.False);
        }

        [Test]
        public void WhenGetOutOfBounds_ThenThrowsMatrixOutOfBounds()
        {
            Matrix<int> m = new Matrix<int>(2, 2, 0);
            Assert.Throws<MatrixOutOfBoundsException>(() => m.Get(new MatrixIndex(2, 0)));
        }

        [Test]
        public void WhenSetThenGet_ThenReturnsWrittenValue()
        {
            Matrix<int> m = new Matrix<int>(2, 2, 0);
            m.Set(new MatrixIndex(1, 0), 42);
            Assert.That(m.Get(new MatrixIndex(1, 0)), Is.EqualTo(42));
        }

        [Test]
        public void WhenSetOutOfBounds_ThenThrowsMatrixOutOfBounds()
        {
            Matrix<int> m = new Matrix<int>(2, 2, 0);
            Assert.Throws<MatrixOutOfBoundsException>(() => m.Set(new MatrixIndex(-1, 0), 1));
        }

        [Test]
        public void WhenTryGetInBounds_ThenReturnsTrueAndValue()
        {
            Matrix<int> m = new Matrix<int>(2, 2, new[] { 9, 0, 0, 0 });
            bool ok = m.TryGet(new MatrixIndex(0, 0), out int value);
            Assert.That(ok, Is.True);
            Assert.That(value, Is.EqualTo(9));
        }

        [Test]
        public void WhenTryGetOutOfBounds_ThenReturnsFalseWithoutThrowing()
        {
            Matrix<int> m = new Matrix<int>(2, 2, 5);
            bool ok = m.TryGet(new MatrixIndex(5, 5), out int value);
            Assert.That(ok, Is.False);
            Assert.That(value, Is.EqualTo(default(int)));
        }

        [Test]
        public void WhenTrySetInBounds_ThenWritesAndReturnsTrue()
        {
            Matrix<int> m = new Matrix<int>(2, 2, 0);
            Assert.That(m.TrySet(new MatrixIndex(0, 1), 3), Is.True);
            Assert.That(m.Get(new MatrixIndex(0, 1)), Is.EqualTo(3));
        }

        [Test]
        public void WhenTrySetOutOfBounds_ThenReturnsFalseAndLeavesMatrixUnchanged()
        {
            Matrix<int> m = new Matrix<int>(2, 2, 1);
            Assert.That(m.TrySet(new MatrixIndex(-1, 0), 99), Is.False);
            Assert.That(m.Get(new MatrixIndex(0, 0)), Is.EqualTo(1));
        }

        [Test]
        public void WhenFill_ThenAllCellsBecomeValue()
        {
            Matrix<int> m = new Matrix<int>(2, 2, 0);
            m.Fill(8);
            Assert.That(m.All(v => v == 8), Is.True);
        }

        [Test]
        public void WhenFillRegionValid_ThenOnlyThoseCellsChange()
        {
            Matrix<int> m = new Matrix<int>(3, 3, 0);
            m.FillRegion(new MatrixRegion(1, 1, 2, 1), 5);
            Assert.That(m.Get(new MatrixIndex(1, 1)), Is.EqualTo(5));
            Assert.That(m.Get(new MatrixIndex(2, 1)), Is.EqualTo(5));
            Assert.That(m.Get(new MatrixIndex(0, 0)), Is.EqualTo(0));
            Assert.That(m.Get(new MatrixIndex(1, 0)), Is.EqualTo(0));
        }

        [Test]
        public void WhenFillRegionOutOfParent_ThenThrowsInvalidMatrixRegion()
        {
            Matrix<int> m = new Matrix<int>(2, 2, 0);
            Assert.Throws<InvalidMatrixRegionException>(() => m.FillRegion(new MatrixRegion(1, 0, 2, 1), 1));
        }

        [Test]
        public void WhenFillRegionEmptySize_ThenThrowsInvalidMatrixRegion()
        {
            Matrix<int> m = new Matrix<int>(2, 2, 0);
            Assert.Throws<InvalidMatrixRegionException>(() => m.FillRegion(new MatrixRegion(0, 0, 0, 1), 1));
        }

        [Test]
        public void WhenCloneMutated_ThenSourceRemainsUnchanged()
        {
            Matrix<int> m = new Matrix<int>(2, 2, new[] { 1, 2, 3, 4 });
            IMatrix<int> clone = m.Clone();
            clone.Set(new MatrixIndex(0, 0), 99);
            Assert.That(m.Get(new MatrixIndex(0, 0)), Is.EqualTo(1));
            Assert.That(clone.Get(new MatrixIndex(0, 0)), Is.EqualTo(99));
            Assert.That(clone.Width, Is.EqualTo(m.Width));
            Assert.That(clone.Height, Is.EqualTo(m.Height));
        }

        [Test]
        public void WhenCropValidRegion_ThenReturnsNewMatrixAndSourceUnchanged()
        {
            Matrix<int> m = new Matrix<int>(3, 3, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            IMatrix<int> cropped = m.Crop(new MatrixRegion(1, 1, 2, 2));
            Assert.That(cropped.Width, Is.EqualTo(2));
            Assert.That(cropped.Height, Is.EqualTo(2));
            Assert.That(cropped.Get(new MatrixIndex(0, 0)), Is.EqualTo(5));
            Assert.That(cropped.Get(new MatrixIndex(1, 1)), Is.EqualTo(9));
            Assert.That(m.Get(new MatrixIndex(1, 1)), Is.EqualTo(5));
        }

        [Test]
        public void WhenCropOutOfParent_ThenThrowsInvalidMatrixRegion()
        {
            Matrix<int> m = new Matrix<int>(2, 2, 0);
            Assert.Throws<InvalidMatrixRegionException>(() => m.Crop(new MatrixRegion(0, 0, 3, 1)));
        }

        [Test]
        public void WhenCropNegativeSize_ThenThrowsInvalidMatrixRegion()
        {
            Matrix<int> m = new Matrix<int>(2, 2, 0);
            Assert.Throws<InvalidMatrixRegionException>(() => m.Crop(new MatrixRegion(0, 0, -1, 1)));
        }

        [Test]
        public void WhenExpandWithPositivePadding_ThenFillsNewCellsAndSourceUnchanged()
        {
            Matrix<int> m = new Matrix<int>(1, 1, 9);
            IMatrix<int> expanded = m.Expand(new ExpandPadding(1, 1, 1, 1), 0);
            Assert.That(expanded.Width, Is.EqualTo(3));
            Assert.That(expanded.Height, Is.EqualTo(3));
            Assert.That(expanded.Get(new MatrixIndex(1, 1)), Is.EqualTo(9));
            Assert.That(expanded.Get(new MatrixIndex(0, 0)), Is.EqualTo(0));
            Assert.That(m.Width, Is.EqualTo(1));
        }

        [Test]
        public void WhenExpandWithZeroPadding_ThenSameSizeAndContentPreserved()
        {
            Matrix<int> m = new Matrix<int>(2, 2, new[] { 1, 2, 3, 4 });
            IMatrix<int> expanded = m.Expand(new ExpandPadding(0, 0, 0, 0), -1);
            Assert.That(expanded.Width, Is.EqualTo(2));
            Assert.That(expanded.Height, Is.EqualTo(2));
            Assert.That(expanded.Get(new MatrixIndex(1, 1)), Is.EqualTo(4));
        }

        [Test]
        public void WhenExpandWithNegativePadding_ThenThrowsArgumentOutOfRange()
        {
            Matrix<int> m = new Matrix<int>(1, 1, 0);
            Assert.Throws<ArgumentOutOfRangeException>(() => m.Expand(new ExpandPadding(-1, 0, 0, 0), 0));
        }

        [Test]
        public void WhenExpandToFullContainment_ThenPastesAtOriginAndFillsRest()
        {
            Matrix<int> m = new Matrix<int>(2, 2, new[] { 1, 2, 3, 4 });
            IMatrix<int> result = m.ExpandTo(4, 3, new MatrixIndex(1, 0), 0);
            Assert.That(result.Width, Is.EqualTo(4));
            Assert.That(result.Height, Is.EqualTo(3));
            Assert.That(result.Get(new MatrixIndex(1, 0)), Is.EqualTo(1));
            Assert.That(result.Get(new MatrixIndex(2, 1)), Is.EqualTo(4));
            Assert.That(result.Get(new MatrixIndex(0, 0)), Is.EqualTo(0));
            Assert.That(result.Get(new MatrixIndex(3, 2)), Is.EqualTo(0));
            Assert.That(m.Get(new MatrixIndex(0, 0)), Is.EqualTo(1));
        }

        [Test]
        public void WhenExpandToSameSizeAtOrigin_ThenCopiesEntireSource()
        {
            Matrix<int> m = new Matrix<int>(2, 2, new[] { 5, 6, 7, 8 });
            IMatrix<int> result = m.ExpandTo(2, 2, new MatrixIndex(0, 0), -1);
            Assert.That(result.Get(new MatrixIndex(0, 0)), Is.EqualTo(5));
            Assert.That(result.Get(new MatrixIndex(1, 1)), Is.EqualTo(8));
        }

        [Test]
        public void WhenExpandToShrinkWidth_ThenThrowsArgumentOutOfRange()
        {
            Matrix<int> m = new Matrix<int>(3, 2, 0);
            Assert.Throws<ArgumentOutOfRangeException>(() => m.ExpandTo(2, 2, new MatrixIndex(0, 0), 0));
        }

        [Test]
        public void WhenExpandToNegativeContentOrigin_ThenThrowsArgumentOutOfRange()
        {
            Matrix<int> m = new Matrix<int>(2, 2, 0);
            Assert.Throws<ArgumentOutOfRangeException>(() => m.ExpandTo(3, 3, new MatrixIndex(-1, 0), 0));
        }

        [Test]
        public void WhenExpandToSourceDoesNotFit_ThenThrowsArgumentOutOfRange()
        {
            Matrix<int> m = new Matrix<int>(2, 2, 0);
            Assert.Throws<ArgumentOutOfRangeException>(() => m.ExpandTo(3, 3, new MatrixIndex(2, 0), 0));
        }

        [Test]
        public void WhenGetRowValid_ThenReturnsSnapshotNotLive()
        {
            Matrix<int> m = new Matrix<int>(3, 2, new[] { 1, 2, 3, 4, 5, 6 });
            IReadOnlyList<int> row = m.GetRow(1);
            Assert.That(row, Is.EqualTo(new[] { 4, 5, 6 }));
            m.Set(new MatrixIndex(0, 1), 99);
            Assert.That(row[0], Is.EqualTo(4));
        }

        [Test]
        public void WhenGetRowOutOfBounds_ThenThrowsMatrixOutOfBounds()
        {
            Matrix<int> m = new Matrix<int>(2, 2, 0);
            Assert.Throws<MatrixOutOfBoundsException>(() => m.GetRow(2));
        }

        [Test]
        public void WhenGetColumnValid_ThenReturnsSnapshot()
        {
            Matrix<int> m = new Matrix<int>(2, 3, new[] { 1, 2, 3, 4, 5, 6 });
            Assert.That(m.GetColumn(1), Is.EqualTo(new[] { 2, 4, 6 }));
        }

        [Test]
        public void WhenGetColumnOutOfBounds_ThenThrowsMatrixOutOfBounds()
        {
            Matrix<int> m = new Matrix<int>(2, 2, 0);
            Assert.Throws<MatrixOutOfBoundsException>(() => m.GetColumn(-1));
        }

        [Test]
        public void WhenGetRegionValid_ThenReturnsRowMajorSnapshot()
        {
            Matrix<int> m = new Matrix<int>(3, 3, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            Assert.That(m.GetRegion(new MatrixRegion(1, 0, 2, 2)), Is.EqualTo(new[] { 2, 3, 5, 6 }));
        }

        [Test]
        public void WhenGetRegionInvalid_ThenThrowsInvalidMatrixRegion()
        {
            Matrix<int> m = new Matrix<int>(2, 2, 0);
            Assert.Throws<InvalidMatrixRegionException>(() => m.GetRegion(new MatrixRegion(0, 0, 0, 2)));
        }

        [Test]
        public void WhenGetNeighboursWithNullNeighbourhood_ThenThrowsArgumentNull()
        {
            Matrix<int> m = new Matrix<int>(3, 3, 0);
            Assert.Throws<ArgumentNullException>(() => m.GetNeighbours(new MatrixIndex(1, 1), null));
        }

        [Test]
        public void WhenGetNeighboursWithOutOfBoundsCenter_ThenThrowsMatrixOutOfBounds()
        {
            Matrix<int> m = new Matrix<int>(2, 2, 0);
            Assert.Throws<MatrixOutOfBoundsException>(
                () => m.GetNeighbours(new MatrixIndex(5, 0), new OrthogonalNeighbourhood()));
        }

        [Test]
        public void WhenGetNeighboursOrthogonalAtCenter_ThenReturnsValuesInNeighbourhoodOrder()
        {
            Matrix<int> m = new Matrix<int>(3, 3, new[] { 0, 1, 0, 2, 9, 3, 0, 4, 0 });
            IReadOnlyList<int> values = m.GetNeighbours(new MatrixIndex(1, 1), new OrthogonalNeighbourhood());
            Assert.That(values, Is.EqualTo(new[] { 1, 3, 4, 2 }));
        }
    }
}
