using System;
using NUnit.Framework;

namespace Matrix.Tests
{
    [TestFixture]
    public class PathResultTests
    {
        [Test]
        public void WhenFromPathEmpty_ThenThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => PathResult.FromPath(Array.Empty<MatrixIndex>(), 0f));
        }

        [Test]
        public void WhenFromPathNegativeCost_ThenThrowsArgumentOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => PathResult.FromPath(new[] { new MatrixIndex(0, 0) }, -1f));
        }
    }
}
