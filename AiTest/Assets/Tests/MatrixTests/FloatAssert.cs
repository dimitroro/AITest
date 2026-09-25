using NUnit.Framework;

namespace Matrix.Tests
{
    internal static class FloatAssert
    {
        public const float TOLERANCE = 1e-5f;

        public static void AreClose(float expected, float actual, float tolerance = TOLERANCE)
        {
            Assert.That(actual, Is.EqualTo(expected).Within(tolerance));
        }
    }
}
